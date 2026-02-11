import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { useRateLimit } from "@/hooks/useRateLimit";

type RateLimitResult =
  | { success: true; data: string }
  | { success: false; error: string; resetIn?: number };

describe("useRateLimit", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(0);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("allows request under limits", async () => {
    const { result } = renderHook(() =>
      useRateLimit({ maxRequests: 2, windowMs: 1000, throttleMs: 0 }),
    );

    const fn = vi.fn().mockResolvedValue("ok");
    let response: RateLimitResult | undefined;

    await act(async () => {
      response = await result.current.attemptRequest(fn);
    });

    expect(response).toEqual({ success: true, data: "ok" });
    expect(fn).toHaveBeenCalled();
  });

  it("throttles rapid consecutive requests", async () => {
    const { result } = renderHook(() =>
      useRateLimit({ maxRequests: 5, windowMs: 1000, throttleMs: 500 }),
    );

    const fn = vi.fn().mockResolvedValue("ok");

    await act(async () => {
      await result.current.attemptRequest(fn);
    });

    let blocked: RateLimitResult | undefined;

    await act(async () => {
      blocked = await result.current.attemptRequest(fn);
    });

    expect(blocked?.success).toBe(false);
    if (blocked?.success === false) {
      expect(blocked.error).toContain("Please wait");
    }

    await act(async () => {
      vi.advanceTimersByTime(600);
    });

    let allowed: RateLimitResult | undefined;

    await act(async () => {
      allowed = await result.current.attemptRequest(fn);
    });

    expect(allowed?.success).toBe(true);
  });

  it("blocks when rate limit exceeded", async () => {
    const { result } = renderHook(() =>
      useRateLimit({ maxRequests: 1, windowMs: 1000, throttleMs: 0 }),
    );

    const fn = vi.fn().mockResolvedValue("ok");

    await act(async () => {
      await result.current.attemptRequest(fn);
    });

    let blocked: RateLimitResult | undefined;

    await act(async () => {
      blocked = await result.current.attemptRequest(fn);
    });

    expect(blocked?.success).toBe(false);
    if (blocked?.success === false) {
      expect(blocked.error).toContain("Rate limit exceeded");
    }
    expect(result.current.isRateLimited).toBe(true);
  });
});
