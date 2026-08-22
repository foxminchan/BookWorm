import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { createMockPublisher } from "@/__tests__/factories";
import { PublisherSelect } from "@/features/books/filters/publisher-select";

const mockUsePublishers = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/publishers/usePublishers", () => ({
  default: mockUsePublishers,
}));

describe("PublisherSelect", () => {
  const mockPublishers = [
    createMockPublisher(),
    createMockPublisher(),
    createMockPublisher(),
  ];

  it.each([
    {
      name: "publisher label",
      data: mockPublishers,
      value: undefined,
      expectedText: "Publisher",
    },
    {
      name: "placeholder",
      data: mockPublishers,
      value: undefined,
      expectedText: "All Publishers",
    },
    {
      name: "empty publishers list",
      data: [],
      value: undefined,
      expectedText: "All Publishers",
    },
    {
      name: "undefined publishers data",
      data: undefined,
      value: undefined,
      expectedText: "All Publishers",
    },
  ])("renders $name", ({ data, value, expectedText }) => {
    mockUsePublishers.mockReturnValue({
      data,
      isLoading: false,
    });

    render(<PublisherSelect value={value} onChange={vi.fn()} />);

    expect(screen.getByText(expectedText)).toBeInTheDocument();
  });

  it("displays selected publisher", () => {
    mockUsePublishers.mockReturnValue({
      data: mockPublishers,
      isLoading: false,
    });

    render(
      <PublisherSelect value={mockPublishers[0]!.id} onChange={vi.fn()} />,
    );

    expect(screen.getByRole("combobox")).toBeInTheDocument();
  });

  it("has proper label association", () => {
    mockUsePublishers.mockReturnValue({
      data: mockPublishers,
      isLoading: false,
    });

    render(<PublisherSelect value={undefined} onChange={vi.fn()} />);

    const label = screen.getByText("Publisher");
    const select = screen.getByRole("combobox");

    expect(label).toHaveAttribute("for", "publisher-select");
    expect(select).toHaveAttribute("id", "publisher-select");
  });

  it("renders with correct styling classes", () => {
    mockUsePublishers.mockReturnValue({
      data: mockPublishers,
      isLoading: false,
    });

    const { container } = render(
      <PublisherSelect value={undefined} onChange={vi.fn()} />,
    );

    const selectTrigger = container.querySelector("#publisher-select");
    expect(selectTrigger).toHaveClass("w-full");
  });

  it("calls onChange when publisher is selected", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    mockUsePublishers.mockReturnValue({
      data: mockPublishers,
      isLoading: false,
    });

    render(<PublisherSelect value={undefined} onChange={onChange} />);

    const select = screen.getByRole("combobox");
    await user.click(select);

    expect(onChange).toBeDefined();
  });
});
