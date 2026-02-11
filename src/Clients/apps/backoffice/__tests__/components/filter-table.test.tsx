import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { userEvent } from "@/__tests__/utils/test-utils";
import { FilterTable } from "@/components/filter-table";

type TestData = {
  id: string;
  name: string;
  value: number;
};

describe("FilterTable", () => {
  const mockOnPaginationChange = vi.fn();
  const mockOnSortingChange = vi.fn();

  const columns = [
    {
      accessorKey: "name",
      header: "Name",
    },
    {
      accessorKey: "value",
      header: "Value",
    },
  ];

  const testData: TestData[] = [
    { id: "1", name: "Item 1", value: 100 },
    { id: "2", name: "Item 2", value: 200 },
    { id: "3", name: "Item 3", value: 300 },
  ];

  const defaultProps = {
    columns,
    data: testData,
    title: "Test Table",
    description: "Test Description",
    totalCount: 10,
    pageIndex: 0,
    pageSize: 5,
    isLoading: false,
    onPaginationChange: mockOnPaginationChange,
    onSortingChange: mockOnSortingChange,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render table with title and description", () => {
    render(<FilterTable {...defaultProps} />);

    expect(screen.getByText("Test Table")).toBeInTheDocument();
    expect(
      screen.getAllByText("Test Description").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should render table headers", () => {
    render(<FilterTable {...defaultProps} />);

    expect(screen.getByText("Name")).toBeInTheDocument();
    expect(screen.getByText("Value")).toBeInTheDocument();
  });

  it("should render table data", () => {
    render(<FilterTable {...defaultProps} />);

    expect(screen.getByText("Item 1")).toBeInTheDocument();
    expect(screen.getByText("Item 2")).toBeInTheDocument();
    expect(screen.getByText("Item 3")).toBeInTheDocument();
    expect(screen.getByText("100")).toBeInTheDocument();
    expect(screen.getByText("200")).toBeInTheDocument();
  });

  it("should show loading skeleton when isLoading is true", () => {
    render(<FilterTable {...defaultProps} isLoading={true} />);

    const status = screen.getByRole("status");
    expect(status).toBeInTheDocument();
    expect(screen.getByText("Loading filtered table...")).toBeInTheDocument();
  });

  it("should display row count information", () => {
    render(<FilterTable {...defaultProps} />);

    expect(screen.getByText("3 of 10 row(s) total.")).toBeInTheDocument();
  });

  it("should display page information", () => {
    render(<FilterTable {...defaultProps} />);

    expect(screen.getByText("Page 1 of 2")).toBeInTheDocument();
  });

  it("should have previous and next buttons", () => {
    render(<FilterTable {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /go to previous page/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /go to next page/i }),
    ).toBeInTheDocument();
  });

  it("should disable previous button on first page", () => {
    render(<FilterTable {...defaultProps} pageIndex={0} />);

    const prevButton = screen.getByRole("button", {
      name: /go to previous page/i,
    });
    expect(prevButton).toBeDisabled();
  });

  it("should disable next button on last page", () => {
    render(<FilterTable {...defaultProps} pageIndex={1} totalCount={10} />);

    const nextButton = screen.getByRole("button", { name: /go to next page/i });
    expect(nextButton).toBeDisabled();
  });

  it("should call onPaginationChange when clicking next button", async () => {
    const user = userEvent.setup();
    render(<FilterTable {...defaultProps} />);

    const nextButton = screen.getByRole("button", { name: /go to next page/i });
    await user.click(nextButton);

    expect(mockOnPaginationChange).toHaveBeenCalledWith(1, 5);
  });

  it("should call onPaginationChange when clicking previous button", async () => {
    const user = userEvent.setup();
    render(<FilterTable {...defaultProps} pageIndex={1} />);

    const prevButton = screen.getByRole("button", {
      name: /go to previous page/i,
    });
    await user.click(prevButton);

    expect(mockOnPaginationChange).toHaveBeenCalledWith(0, 5);
  });

  it("should render page size selector", () => {
    render(<FilterTable {...defaultProps} />);

    expect(screen.getByText("Rows per page")).toBeInTheDocument();
    expect(screen.getByRole("combobox")).toBeInTheDocument();
  });

  // Note: Radix UI Select interactions have known issues with happy-dom
  // Testing the selector rendering and accessibility is sufficient

  it("should show empty state when no data", () => {
    render(<FilterTable {...defaultProps} data={[]} />);

    expect(screen.getByText("No data found")).toBeInTheDocument();
  });

  it("should not show pagination when data is empty", () => {
    render(<FilterTable {...defaultProps} data={[]} totalCount={0} />);

    expect(screen.queryByText(/row\(s\) total/i)).not.toBeInTheDocument();
  });

  it("should highlight row when highlightedId matches", () => {
    const getRowId = (row: TestData) => row.id;
    render(
      <FilterTable {...defaultProps} highlightedId="2" getRowId={getRowId} />,
    );

    const rows = screen.getAllByRole("row");
    const highlightedRow = rows.find((row) =>
      row.textContent?.includes("Item 2"),
    );
    expect(highlightedRow).toHaveClass("bg-green-50");
  });

  it("should render without description when not provided", () => {
    const { description, ...propsWithoutDescription } = defaultProps;
    render(<FilterTable {...propsWithoutDescription} />);

    expect(screen.getAllByText("Test Table").length).toBeGreaterThanOrEqual(1);
    expect(screen.queryByText("Test Description")).not.toBeInTheDocument();
  });

  it("should calculate total pages correctly", () => {
    render(<FilterTable {...defaultProps} totalCount={25} pageSize={5} />);

    expect(screen.getByText("Page 1 of 5")).toBeInTheDocument();
  });

  it("should handle single page scenario", () => {
    render(
      <FilterTable
        {...defaultProps}
        totalCount={3}
        pageSize={10}
        data={testData}
      />,
    );

    expect(screen.getByText("Page 1 of 1")).toBeInTheDocument();

    const prevButton = screen.getByRole("button", {
      name: /go to previous page/i,
    });
    const nextButton = screen.getByRole("button", { name: /go to next page/i });

    expect(prevButton).toBeDisabled();
    expect(nextButton).toBeDisabled();
  });

  it("should have proper accessibility for pagination controls", () => {
    render(<FilterTable {...defaultProps} pageIndex={1} />);

    const prevButton = screen.getByRole("button", {
      name: /go to previous page/i,
    });
    const nextButton = screen.getByRole("button", {
      name: /go to next page/i,
    });

    expect(prevButton).toBeInTheDocument();
    expect(nextButton).toBeInTheDocument();
  });

  it("should have proper accessibility for page size selector", () => {
    render(<FilterTable {...defaultProps} />);

    const select = screen.getByRole("combobox");
    expect(select).toHaveAttribute("aria-labelledby", "rows-per-page-label");
  });
});
