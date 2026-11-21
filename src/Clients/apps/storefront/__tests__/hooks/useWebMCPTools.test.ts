import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useWebMCPTools } from "@/hooks/useWebMCPTools";
import { getBookDetailsTool, searchBooksTool } from "@/lib/webmcp-tools";

const { mockDispose, mockCoreConstructor } = vi.hoisted(() => ({
  mockDispose: vi.fn(),
  mockCoreConstructor: vi.fn(),
}));

vi.mock("@copilotkit/react-core", () => ({
  CopilotKitCore: vi.fn(function MockCopilotKitCore(config: unknown) {
    mockCoreConstructor(config);
    return {
      dispose: mockDispose,
    };
  }),
}));

describe("useWebMCPTools", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("does not initialize CopilotKitCore when disabled", () => {
    renderHook(() => useWebMCPTools(false));

    expect(mockCoreConstructor).not.toHaveBeenCalled();
    expect(mockDispose).not.toHaveBeenCalled();
  });

  it("registers searchBooksTool via CopilotKitCore when enabled and disposes on unmount", async () => {
    const { unmount } = renderHook(() => useWebMCPTools(true));

    await waitFor(() => {
      expect(mockCoreConstructor).toHaveBeenCalledTimes(1);
    });
    expect(mockCoreConstructor).toHaveBeenCalledWith({
      tools: [searchBooksTool, getBookDetailsTool],
    });
    expect(mockDispose).not.toHaveBeenCalled();

    unmount();

    expect(mockDispose).toHaveBeenCalledTimes(1);
  });

  it("disposes CopilotKitCore when enabled changes from true to false", async () => {
    const { rerender } = renderHook(
      ({ enabled }: { enabled: boolean }) => useWebMCPTools(enabled),
      { initialProps: { enabled: true } },
    );

    await waitFor(() => {
      expect(mockCoreConstructor).toHaveBeenCalledTimes(1);
    });

    rerender({ enabled: false });

    expect(mockDispose).toHaveBeenCalledTimes(1);
  });
});
