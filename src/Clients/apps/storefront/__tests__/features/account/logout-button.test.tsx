import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import LogoutButton from "@/features/account/logout-button";

import { renderWithProviders } from "../../utils/test-utils";

// Mock the useLogout hook
const mockLogout = vi.fn();
vi.mock("@/hooks/useLogout", () => ({
  useLogout: () => ({
    logout: mockLogout,
    isLoggingOut: false,
  }),
}));

describe("LogoutButton", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render logout button", () => {
    renderWithProviders(<LogoutButton />);

    expect(screen.getByRole("button", { name: /logout/i })).toBeInTheDocument();
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

  it("should have full width", () => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass("w-full");
  });

  it("should have border styling", () => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass("border-border/40");
  });

  it("should have transparent background", () => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass("bg-transparent");
  });

  it("should have hover effects", () => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass("hover:bg-secondary/20");
  });

  it("should have medium font weight", () => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass("font-medium");
  });

  it("should center content", () => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass("justify-center");
  });

  it("should have proper height", () => {
    renderWithProviders(<LogoutButton />);

    const button = screen.getByRole("button", { name: /logout/i });
    expect(button).toHaveClass("h-12");
  });
});
