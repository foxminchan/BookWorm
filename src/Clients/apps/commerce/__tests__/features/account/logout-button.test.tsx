import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import LogoutButton from "@/features/account/logout-button";

import { renderWithProviders } from "../../utils/test-utils";

// Mock the useLogout hook
const mockLogout = vi.fn();
const mockUseLogout = vi.fn(() => ({
  logout: mockLogout,
  isLoggingOut: false,
}));
vi.mock("@workspace/storefront-shell/hooks/useLogout", () => ({
  useLogout: () => mockUseLogout(),
}));

describe("LogoutButton", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseLogout.mockReturnValue({
      logout: mockLogout,
      isLoggingOut: false,
    });
  });

  it("should render logout button", () => {
    renderWithProviders(<LogoutButton />);

    expect(screen.getByRole("button", { name: /logout/i })).toBeInTheDocument();
  });

  it("should show loading state and disable button when logging out", () => {
    mockUseLogout.mockReturnValue({
      logout: mockLogout,
      isLoggingOut: true,
    });

    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logging out/i });
    expect(button).toBeDisabled();
  });

  it("should call logout function when clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    await user.click(button);

    await waitFor(() => {
      expect(mockLogout).toHaveBeenCalledTimes(1);
    });
  });

  it.each([
    ["full width", "w-full"],
    ["border styling", "border-border/40"],
    ["transparent background", "bg-transparent"],
    ["hover effects", "hover:bg-secondary/20"],
    ["medium font weight", "font-medium"],
    ["center content", "justify-center"],
    ["proper height", "h-12"],
  ])("should have %s", (_label, className) => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass(className);
  });
});
