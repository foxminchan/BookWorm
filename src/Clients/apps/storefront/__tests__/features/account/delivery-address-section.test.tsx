import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import DeliveryAddressSection from "@/features/account/delivery-address-section";

import { renderWithProviders } from "../../utils/test-utils";

// Mock the API hook
vi.mock("@workspace/api-hooks/ordering/buyers/useUpdateBuyerAddress", () => ({
  default: () => ({
    mutateAsync: vi.fn().mockResolvedValue({}),
    isPending: false,
  }),
}));

const mockBuyer = {
  id: "buyer-123",
  name: "John Doe",
  address: "123 Main St, New York, NY",
};

describe("DeliveryAddressSection", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display delivery address heading", () => {
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    expect(screen.getByText("Delivery Address")).toBeInTheDocument();
  });

  it("should display current address", () => {
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    expect(screen.getByText("123 Main St, New York, NY")).toBeInTheDocument();
  });

  it("should display 'No address set' when address is null", () => {
    const buyerWithoutAddress = { ...mockBuyer, address: null };
    renderWithProviders(<DeliveryAddressSection buyer={buyerWithoutAddress} />);

    expect(screen.getByText("No address set")).toBeInTheDocument();
  });

  it("should render edit button", () => {
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    expect(screen.getByRole("button", { name: /edit/i })).toBeInTheDocument();
  });

  it("should display MapPin icon", () => {
    const { container } = renderWithProviders(
      <DeliveryAddressSection buyer={mockBuyer} />,
    );

    const mapPinIcon = container.querySelector("svg");
    expect(mapPinIcon).toBeInTheDocument();
  });

  it("should show edit form when edit button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    const editButton = screen.getByRole("button", { name: /edit/i });
    await user.click(editButton);

    expect(screen.getByLabelText("Street Address")).toBeInTheDocument();
    expect(screen.getByLabelText("City")).toBeInTheDocument();
    expect(screen.getByLabelText("Province")).toBeInTheDocument();
  });

  it("should pre-fill form with existing address parts", async () => {
    const user = userEvent.setup();
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    const editButton = screen.getByRole("button", { name: /edit/i });
    await user.click(editButton);

    await waitFor(() => {
      expect(screen.getByDisplayValue("123 Main St")).toBeInTheDocument();
      expect(screen.getByDisplayValue("New York")).toBeInTheDocument();
      expect(screen.getByDisplayValue("NY")).toBeInTheDocument();
    });
  });

  it("should hide edit button when in edit mode", async () => {
    const user = userEvent.setup();
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    const editButton = screen.getByRole("button", { name: /edit/i });
    await user.click(editButton);

    expect(
      screen.queryByRole("button", { name: /^edit$/i }),
    ).not.toBeInTheDocument();
  });

  it("should render save and cancel buttons in edit mode", async () => {
    const user = userEvent.setup();
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    const editButton = screen.getByRole("button", { name: /edit/i });
    await user.click(editButton);

    expect(
      screen.getByRole("button", { name: /save address/i }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("should close edit form when cancel is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    const editButton = screen.getByRole("button", { name: /edit/i });
    await user.click(editButton);

    const cancelButton = screen.getByRole("button", { name: /cancel/i });
    await user.click(cancelButton);

    expect(screen.queryByLabelText("Street Address")).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: /edit/i })).toBeInTheDocument();
  });

  it("should have rounded corners", () => {
    const { container } = renderWithProviders(
      <DeliveryAddressSection buyer={mockBuyer} />,
    );

    const card = container.querySelector(".rounded-lg");
    expect(card).toBeInTheDocument();
  });

  it("should have hover effects", () => {
    const { container } = renderWithProviders(
      <DeliveryAddressSection buyer={mockBuyer} />,
    );

    const card = container.querySelector(String.raw`.hover\:bg-secondary\/20`);
    expect(card).toBeInTheDocument();
  });

  it("should have transition animations", () => {
    const { container } = renderWithProviders(
      <DeliveryAddressSection buyer={mockBuyer} />,
    );

    const card = container.querySelector(".transition-colors");
    expect(card).toBeInTheDocument();
  });

  it("should render form fields with placeholders", async () => {
    const user = userEvent.setup();
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    const editButton = screen.getByRole("button", { name: /edit/i });
    await user.click(editButton);

    expect(
      screen.getByPlaceholderText("Enter street address"),
    ).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Enter city")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Enter province")).toBeInTheDocument();
  });

  it("should have grid layout for city and province fields", async () => {
    const user = userEvent.setup();
    const { container } = renderWithProviders(
      <DeliveryAddressSection buyer={mockBuyer} />,
    );

    const editButton = screen.getByRole("button", { name: /edit/i });
    await user.click(editButton);

    await waitFor(() => {
      const grid = container.querySelector(".grid-cols-2");
      expect(grid).toBeInTheDocument();
    });
  });

  it("should display serif font for heading", () => {
    renderWithProviders(<DeliveryAddressSection buyer={mockBuyer} />);

    const heading = screen.getByText("Delivery Address");
    expect(heading).toHaveClass("font-serif");
  });
});
