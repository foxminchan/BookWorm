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

  it("does not initialize CopilotKitCore if unmounted before dynamic imports resolve", async () => {
    const { unmount } = renderHook(() => useWebMCPTools(true));
    unmount();

    await new Promise((resolve) => setTimeout(resolve, 0));

    expect(mockCoreConstructor).not.toHaveBeenCalled();
    expect(mockDispose).not.toHaveBeenCalled();
  });

  it("logs an error when initializing CopilotKitCore fails", async () => {
    const error = new Error("init failed");
    mockCoreConstructor.mockImplementationOnce(() => {
      throw error;
    });
    const consoleErrorSpy = vi
      .spyOn(console, "error")
      .mockImplementation(() => {});

    renderHook(() => useWebMCPTools(true));

    await waitFor(() => {
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        "Failed to initialize WebMCP tools:",
        error,
      );
    });

    consoleErrorSpy.mockRestore();
  });
});
