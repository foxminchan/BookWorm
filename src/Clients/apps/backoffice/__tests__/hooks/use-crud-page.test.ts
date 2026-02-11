import type { UseMutationResult, UseQueryResult } from "@tanstack/react-query";
import { act, renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useCrudPage } from "@/hooks/use-crud-page";

// Mock sonner toast
vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

type TestItem = { id: string; name: string };

function createMockMutation<TResult, TRequest>(
  overrides: Partial<UseMutationResult<TResult, Error, TRequest>> = {},
): UseMutationResult<TResult, Error, TRequest> {
  return {
    mutateAsync: vi.fn().mockResolvedValue(undefined),
    mutate: vi.fn(),
    isPending: false,
    isIdle: true,
    isSuccess: false,
    isError: false,
    data: undefined,
    error: null,
    status: "idle",
    variables: undefined,
    context: undefined,
    failureCount: 0,
    failureReason: null,
    isPaused: false,
    reset: vi.fn(),
    submittedAt: 0,
    ...overrides,
  } as unknown as UseMutationResult<TResult, Error, TRequest>;
}

function createMockListQuery(
  overrides: Partial<UseQueryResult<TestItem[]>> = {},
): UseQueryResult<TestItem[]> {
  return {
    data: [],
    isLoading: false,
    isError: false,
    error: null,
    status: "success",
    isFetching: false,
    isSuccess: true,
    refetch: vi.fn(),
    ...overrides,
  } as unknown as UseQueryResult<TestItem[]>;
}

function createDefaultOptions() {
  return {
    entityName: "Author",
    listQuery: createMockListQuery(),
    createMutation: createMockMutation<{ id: string }, { name: string }>(),
    updateMutation: createMockMutation<void, { id: string; name: string }>(),
    deleteMutation: createMockMutation<void, string>(),
    buildCreateRequest: (name: string) => ({ name }),
    buildUpdateRequest: (id: string, name: string) => ({ id, name }),
  };
}

describe("useCrudPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should return items from listQuery data", () => {
    const items: TestItem[] = [
      { id: "1", name: "Item 1" },
      { id: "2", name: "Item 2" },
    ];

    const options = createDefaultOptions();
    options.listQuery = createMockListQuery({ data: items });

    const { result } = renderHook(() => useCrudPage(options));

    expect(result.current.items).toEqual(items);
  });

  it("should return empty array when listQuery has no data", () => {
    const options = createDefaultOptions();
    options.listQuery = createMockListQuery({ data: undefined });

    const { result } = renderHook(() => useCrudPage(options));

    expect(result.current.items).toEqual([]);
  });

  it("should expose isLoading from listQuery", () => {
    const options = createDefaultOptions();
    options.listQuery = createMockListQuery({ isLoading: true });

    const { result } = renderHook(() => useCrudPage(options));

    expect(result.current.isLoading).toBe(true);
  });

  it("should initialize dialog as closed", () => {
    const options = createDefaultOptions();

    const { result } = renderHook(() => useCrudPage(options));

    expect(result.current.isDialogOpen).toBe(false);
  });

  it("should toggle dialog open state", () => {
    const options = createDefaultOptions();

    const { result } = renderHook(() => useCrudPage(options));

    act(() => {
      result.current.setIsDialogOpen(true);
    });

    expect(result.current.isDialogOpen).toBe(true);

    act(() => {
      result.current.setIsDialogOpen(false);
    });

    expect(result.current.isDialogOpen).toBe(false);
  });

  it("should call createMutation and close dialog on handleCreate", async () => {
    const { toast } = await import("sonner");
    const options = createDefaultOptions();
    const mockMutateAsync = vi.fn().mockResolvedValue({ id: "new-id" });
    options.createMutation = createMockMutation({
      mutateAsync: mockMutateAsync,
    });

    const { result } = renderHook(() => useCrudPage(options));

    // Open dialog first
    act(() => {
      result.current.setIsDialogOpen(true);
    });

    expect(result.current.isDialogOpen).toBe(true);

    await act(async () => {
      await result.current.handleCreate("New Author");
    });

    expect(mockMutateAsync).toHaveBeenCalledWith({ name: "New Author" });
    expect(result.current.isDialogOpen).toBe(false);
    expect(toast.success).toHaveBeenCalledWith("Author has been created");
  });

  it("should call updateMutation on handleUpdate", async () => {
    const { toast } = await import("sonner");
    const options = createDefaultOptions();
    const mockMutateAsync = vi.fn().mockResolvedValue(undefined);
    options.updateMutation = createMockMutation({
      mutateAsync: mockMutateAsync,
    });

    const { result } = renderHook(() => useCrudPage(options));

    await act(async () => {
      await result.current.handleUpdate("id-1", "Updated Author");
    });

    expect(mockMutateAsync).toHaveBeenCalledWith({
      id: "id-1",
      name: "Updated Author",
    });
    expect(toast.success).toHaveBeenCalledWith("Author has been updated");
  });

  it("should call deleteMutation on handleDelete", async () => {
    const { toast } = await import("sonner");
    const options = createDefaultOptions();
    const mockMutateAsync = vi.fn().mockResolvedValue(undefined);
    options.deleteMutation = createMockMutation({
      mutateAsync: mockMutateAsync,
    });

    const { result } = renderHook(() => useCrudPage(options));

    await act(async () => {
      await result.current.handleDelete("id-1");
    });

    expect(mockMutateAsync).toHaveBeenCalledWith("id-1");
    expect(toast.success).toHaveBeenCalledWith("Author has been deleted");
  });

  it("should compute isSubmitting when any mutation is pending", () => {
    const options = createDefaultOptions();
    options.createMutation = createMockMutation({ isPending: true });

    const { result } = renderHook(() => useCrudPage(options));

    expect(result.current.isSubmitting).toBe(true);
  });

  it("should not be submitting when all mutations are idle", () => {
    const options = createDefaultOptions();

    const { result } = renderHook(() => useCrudPage(options));

    expect(result.current.isSubmitting).toBe(false);
  });

  it("should expose isCreatePending from createMutation", () => {
    const options = createDefaultOptions();
    options.createMutation = createMockMutation({ isPending: true });

    const { result } = renderHook(() => useCrudPage(options));

    expect(result.current.isCreatePending).toBe(true);
  });
});
