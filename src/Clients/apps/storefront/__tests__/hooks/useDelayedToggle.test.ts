import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { useDelayedToggle } from "@/hooks/useDelayedToggle";

describe("useDelayedToggle", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("should start closed", () => {
    const { result } = renderHook(() => useDelayedToggle());

    expect(result.current.isOpen).toBe(false);
  });

  it("should open immediately", () => {
    const { result } = renderHook(() => useDelayedToggle());

    act(() => result.current.open());

    expect(result.current.isOpen).toBe(true);
  });

  it("should close immediately", () => {
    const { result } = renderHook(() => useDelayedToggle());

    act(() => result.current.open());
    act(() => result.current.close());

    expect(result.current.isOpen).toBe(false);
  });

  it("should toggle state", () => {
    const { result } = renderHook(() => useDelayedToggle());

    act(() => result.current.toggle());
    expect(result.current.isOpen).toBe(true);

    act(() => result.current.toggle());
    expect(result.current.isOpen).toBe(false);
  });

  it("should open on mouse enter without delay", () => {
    const { result } = renderHook(() => useDelayedToggle());

    act(() => result.current.handleMouseEnter());

    expect(result.current.isOpen).toBe(true);
  });

  it("should close after delay on mouse leave", () => {
    const { result } = renderHook(() => useDelayedToggle());

    act(() => result.current.handleMouseEnter());
    expect(result.current.isOpen).toBe(true);

    act(() => result.current.handleMouseLeave());
    // Should still be open before delay expires
    expect(result.current.isOpen).toBe(true);

    act(() => vi.advanceTimersByTime(150));
    expect(result.current.isOpen).toBe(false);
  });

  it("should respect custom close delay", () => {
    const { result } = renderHook(() => useDelayedToggle({ closeDelay: 300 }));

    act(() => result.current.handleMouseEnter());
    act(() => result.current.handleMouseLeave());

    act(() => vi.advanceTimersByTime(150));
    expect(result.current.isOpen).toBe(true);

    act(() => vi.advanceTimersByTime(150));
    expect(result.current.isOpen).toBe(false);
  });

  it("should cancel pending close when mouse re-enters", () => {
    const { result } = renderHook(() => useDelayedToggle());

    act(() => result.current.handleMouseEnter());
    act(() => result.current.handleMouseLeave());

    // Re-enter before delay expires
    act(() => vi.advanceTimersByTime(100));
    act(() => result.current.handleMouseEnter());

    // Even after full delay time, should remain open
    act(() => vi.advanceTimersByTime(200));
    expect(result.current.isOpen).toBe(true);
  });

  it("should cancel pending close when open is called", () => {
    const { result } = renderHook(() => useDelayedToggle());

    act(() => result.current.handleMouseEnter());
    act(() => result.current.handleMouseLeave());

    act(() => result.current.open());

    act(() => vi.advanceTimersByTime(300));
    expect(result.current.isOpen).toBe(true);
  });

  it("should clean up timeout on unmount", () => {
    const { result, unmount } = renderHook(() => useDelayedToggle());

    act(() => result.current.handleMouseEnter());
    act(() => result.current.handleMouseLeave());

    unmount();

    // Should not throw or cause issues when timer fires after unmount
    expect(() => vi.advanceTimersByTime(300)).not.toThrow();
  });
});
