import { JSX } from "react";

import { render } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useBookSearchActions } from "@/hooks/useBookSearchActions";

const registeredActions: Array<{
  name: string;
  handler: (args: any) => Promise<any>;
  render: (args: any) => JSX.Element;
}> = [];

const { mockList } = vi.hoisted(() => ({
  mockList: vi.fn(),
}));

vi.mock("@copilotkit/react-core", () => ({
  useCopilotAction: vi.fn((config) => {
    registeredActions.push(config);
  }),
}));

vi.mock("@workspace/api-client/catalog/books", () => ({
  default: {
    list: mockList,
  },
}));

describe("useBookSearchActions", () => {
  beforeEach(() => {
    registeredActions.length = 0;
    vi.clearAllMocks();
  });

  it("registers searchBooks and returns results", async () => {
    mockList.mockResolvedValue({
      items: [
        { id: "1", title: "Title 1", author: "Author", price: 10 },
        { id: "2", title: "Title 2", author: "Author", price: 12 },
      ],
      totalCount: 2,
    });

    useBookSearchActions();

    const action = registeredActions.find((a) => a.name === "searchBooks");

    expect(action).toBeDefined();

    const result = await action?.handler({ query: "test", maxResults: 5 });

    expect(mockList).toHaveBeenCalledWith({
      search: "test",
      pageSize: 5,
    });
    expect(result).toEqual({
      results: [
        { id: "1", title: "Title 1", author: "Author", price: 10 },
        { id: "2", title: "Title 2", author: "Author", price: 12 },
      ],
      total: 2,
      query: "test",
    });
  });

  it("renders search results in render function", async () => {
    mockList.mockResolvedValue({ items: [], totalCount: 0 });

    useBookSearchActions();

    const action = registeredActions.find((a) => a.name === "searchBooks");

    const component = action?.render({
      status: "complete",
      result: {
        results: [
          { id: "1", title: "Result 1", author: "A", price: 8 },
          { id: "2", title: "Result 2", author: "B", price: 9 },
        ],
        total: 2,
        query: "query",
      },
    });

    const { getByText } = render(component ?? <div />);

    expect(getByText(/Found 2 books/i)).toBeInTheDocument();
    expect(getByText(/Result 1/)).toBeInTheDocument();
    expect(getByText(/Result 2/)).toBeInTheDocument();
  });
});
