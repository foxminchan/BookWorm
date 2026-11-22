import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import ProfileSection from "@/features/account/profile-section";

import { renderWithProviders } from "../../utils/test-utils";

const mockBuyer = {
  id: faker.string.uuid(),
  name: faker.person.fullName(),
  address: faker.location.streetAddress(true),
};

describe("ProfileSection", () => {
  it("should display buyer name", () => {
    const buyer = { ...mockBuyer, name: "John Doe" };
    renderWithProviders(<ProfileSection buyer={buyer} />);

    expect(screen.getByText("John Doe")).toBeInTheDocument();
  });

  it("should not display internal customer ID", () => {
    const buyer = { ...mockBuyer, id: "buyer-123" };
    renderWithProviders(<ProfileSection buyer={buyer} />);

    expect(screen.queryByText(/customer id:/i)).not.toBeInTheDocument();
    expect(screen.queryByText("buyer-123")).not.toBeInTheDocument();
  });

  it("should render user icon", () => {
    const { container } = renderWithProviders(
      <ProfileSection buyer={mockBuyer} />,
    );

    const userIcon = container.querySelector("svg");
    expect(userIcon).toBeInTheDocument();
  });

  it("should display 'No Name' when name is null", () => {
    const buyerWithoutName = { ...mockBuyer, name: null };
    renderWithProviders(<ProfileSection buyer={buyerWithoutName} />);

    expect(screen.getByText("No Name")).toBeInTheDocument();
  });

  it("should display 'No Name' when name is empty string", () => {
    const buyerWithEmptyName = { ...mockBuyer, name: "" };
    renderWithProviders(<ProfileSection buyer={buyerWithEmptyName} />);

    expect(screen.getByText("No Name")).toBeInTheDocument();
  });

  it("should have rounded corners", () => {
    const { container } = renderWithProviders(
      <ProfileSection buyer={mockBuyer} />,
    );

    const card = container.querySelector(".rounded-lg");
    expect(card).toBeInTheDocument();
  });

  it("should have border styling", () => {
    const { container } = renderWithProviders(
      <ProfileSection buyer={mockBuyer} />,
    );

    const border = container.querySelector(String.raw`.border-border\/40`);
    expect(border).toBeInTheDocument();
  });

  it("should display name with serif font", () => {
    const buyer = { ...mockBuyer, name: "John Doe" };
    renderWithProviders(<ProfileSection buyer={buyer} />);

    const name = screen.getByText("John Doe");
    expect(name).toHaveClass("font-serif");
  });

  it("should have circular user icon container", () => {
    const { container } = renderWithProviders(
      <ProfileSection buyer={mockBuyer} />,
    );

    const iconContainer = container.querySelector(".rounded-full");
    expect(iconContainer).toBeInTheDocument();
  });

  it("should have proper padding", () => {
    const { container } = renderWithProviders(
      <ProfileSection buyer={mockBuyer} />,
    );

    const card = container.querySelector(".p-6");
    expect(card).toBeInTheDocument();
  });

  it("should render flex layout", () => {
    const { container } = renderWithProviders(
      <ProfileSection buyer={mockBuyer} />,
    );

    const flexContainer = container.querySelector(".flex.items-center.gap-4");
    expect(flexContainer).toBeInTheDocument();
  });
});
