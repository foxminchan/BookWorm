import { JSX } from "react";

import { render } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import booksApiClient from "@workspace/api-client/catalog/books";

import { useBookSearchActions } from "@/hooks/useBookSearchActions";

const registeredActions: Array<{
  name: string;
  description?: string;
  webmcp?: unknown;
  handler: (args: any) => Promise<any>;
  render: (args: any) => JSX.Element;
}> = [];

vi.mock("@copilotkit/react-core/v2", () => ({
  useFrontendTool: vi.fn((config) => {
    registeredActions.push(config);
  }),
}));

describe("useBookSearchActions", () => {
  let listSpy: ReturnType<typeof vi.spyOn>;
  let getSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    registeredActions.length = 0;
    vi.clearAllMocks();
    listSpy = vi.spyOn(booksApiClient, "list");
    getSpy = vi.spyOn(booksApiClient, "get");
  });

  afterEach(() => {
    listSpy.mockRestore();
    getSpy.mockRestore();
  });

  it("registers searchBooks and returns results", async () => {
    listSpy.mockResolvedValue({
      items: [
        { id: "1", title: "Title 1", author: "Author", price: 10 },
        { id: "2", title: "Title 2", author: "Author", price: 12 },
      ],
      totalCount: 2,
    } as never);

    useBookSearchActions();

    const action = registeredActions.find((a) => a.name === "searchBooks");

    expect(action).toBeDefined();
    expect(action?.description).toBeTruthy();
    expect(action?.webmcp).toEqual({
      annotations: {
        readOnlyHint: true,
      },
    });

    const result = await action?.handler({ query: "test", maxResults: 5 });

    expect(listSpy).toHaveBeenCalledWith({
      search: "test",
      pageSize: 5,
    });
    expect(result).toEqual(
      JSON.stringify({
        results: [
          { id: "1", title: "Title 1", author: "Author", price: 10 },
          { id: "2", title: "Title 2", author: "Author", price: 12 },
        ],
        total: 2,
        query: "test",
      }),
    );
  });

  it("renders search results in render function", async () => {
    listSpy.mockResolvedValue({ items: [], totalCount: 0 } as never);

    useBookSearchActions();

    const action = registeredActions.find((a) => a.name === "searchBooks");

    const component = action?.render({
      status: "complete",
      result: JSON.stringify({
        results: [
          { id: "1", name: "Result 1", authors: [{ name: "A" }], price: 8 },
          { id: "2", name: "Result 2", authors: [{ name: "B" }], price: 9 },
        ],
        total: 2,
        query: "query",
      }),
    });

    const { getByText } = render(component ?? <div />);

    expect(getByText(/Found 2 books/i)).toBeInTheDocument();
    expect(getByText(/Result 1/)).toBeInTheDocument();
    expect(getByText(/Result 2/)).toBeInTheDocument();
  });

  it("renders executing, single-result, and idle states", () => {
    useBookSearchActions();

    const action = registeredActions.find((a) => a.name === "searchBooks");

    // Executing state
    const execView = action?.render({ status: "executing" });
    const { getByText, unmount } = render(execView ?? <div />);
    expect(getByText(/Searching for books/i)).toBeInTheDocument();
    unmount();

    // Single result state
    const singleView = action?.render({
      status: "complete",
      result: JSON.stringify({
        results: [
          { id: "1", name: "Solo Book", authors: [{ name: "A" }], price: 12 },
        ],
        total: 1,
        query: "solo",
      }),
    });
    const { getByText: getSingleText, unmount: unmountSingle } = render(
      singleView ?? <div />,
    );
    expect(getSingleText(/Found 1 book$/i)).toBeInTheDocument();
    unmountSingle();

    // Idle state
    const idleView = action?.render({ status: "inProgress" });
    expect(idleView).toBeTruthy();
  });

  it("registers getBookDetails and handles execution and rendering states", async () => {
    const mockBook = {
      id: "book-123",
      name: "The Pragmatic Programmer",
      authors: [{ name: "Andy Hunt" }, { name: "Dave Thomas" }],
      description: "A great book for developers.",
      price: 49.99,
      priceSale: 39.99,
    };

    getSpy.mockResolvedValue(mockBook as never);

    useBookSearchActions();

    const action = registeredActions.find((a) => a.name === "getBookDetails");

    expect(action).toBeDefined();
    expect(action?.description).toBeTruthy();
    expect(action?.webmcp).toEqual({
      annotations: {
        readOnlyHint: true,
      },
    });

    // Handler execution
    const result = await action?.handler({ bookId: "book-123" });
    expect(getSpy).toHaveBeenCalledWith("book-123");
    expect(result).toEqual(JSON.stringify(mockBook));

    // Executing render state
    const execView = action?.render({ status: "executing" });
    const { getByText: getExecText, unmount: unmountExec } = render(
      execView ?? <div />,
    );
    expect(getExecText(/Loading book details/i)).toBeInTheDocument();
    unmountExec();

    // Complete render state with sale price and description
    const completeView = action?.render({
      status: "complete",
      result: JSON.stringify(mockBook),
    });
    const { getByText: getCompleteText, unmount: unmountComplete } = render(
      completeView ?? <div />,
    );
    expect(getCompleteText("The Pragmatic Programmer")).toBeInTheDocument();
    expect(getCompleteText("Andy Hunt, Dave Thomas")).toBeInTheDocument();
    expect(getCompleteText(/A great book for developers/)).toBeInTheDocument();
    expect(getCompleteText("$39.99")).toBeInTheDocument();
    unmountComplete();

    // Complete render state with fallback price and no description
    const minimalBook = {
      id: "book-456",
      name: "Minimal Book",
      authors: [{ name: "Solo Author" }, { name: "" }],
      price: 25.0,
    };
    const minimalView = action?.render({
      status: "complete",
      result: JSON.stringify(minimalBook),
    });
    const { getByText: getMinimalText, unmount: unmountMinimal } = render(
      minimalView ?? <div />,
    );
    expect(getMinimalText("Minimal Book")).toBeInTheDocument();
    expect(getMinimalText("Solo Author")).toBeInTheDocument();
    expect(getMinimalText("$25.00")).toBeInTheDocument();
    unmountMinimal();

    // Idle state
    const idleView = action?.render({ status: "inProgress" });
    expect(idleView).toBeTruthy();
  });
});
