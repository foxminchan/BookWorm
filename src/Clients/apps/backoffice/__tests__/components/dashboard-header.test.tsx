import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { userEvent } from "@/__tests__/utils/test-utils";
import { DashboardHeader } from "@/components/dashboard-header";

// Mock next-themes
const mockSetTheme = vi.fn();
const mockTheme = vi.fn();
vi.mock("next-themes", () => ({
  useTheme: () => ({
    theme: mockTheme(),
    setTheme: mockSetTheme,
  }),
}));

// Mock useUserContext
const mockUseUserContext = vi.hoisted(() => vi.fn());
vi.mock("@/hooks/useUserContext", () => ({
  useUserContext: () => mockUseUserContext(),
}));

// Mock useLogout
const mockLogout = vi.hoisted(() => vi.fn());
vi.mock("@/hooks/useLogout", () => ({
  useLogout: () => ({
    logout: mockLogout,
  }),
}));

describe("DashboardHeader", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockTheme.mockReturnValue("light");
    mockUseUserContext.mockReturnValue({
      user: {
        name: "Test User",
        email: "test@example.com",
      },
    });
  });

  it("should render header with title and welcome message", () => {
    render(<DashboardHeader />);

    expect(screen.getByText("Admin Portal")).toBeInTheDocument();
    expect(
      screen.getByText("Welcome back to your dashboard"),
    ).toBeInTheDocument();
  });

  it("should render theme toggle button", () => {
    render(<DashboardHeader />);

    const themeButton = screen.getByRole("button", {
      name: /switch to dark mode/i,
    });
    expect(themeButton).toBeInTheDocument();
  });

  it("should toggle theme when theme button is clicked", async () => {
    const user = userEvent.setup();
    render(<DashboardHeader />);

    const themeButton = screen.getByRole("button", {
      name: /switch to dark mode/i,
    });
    await user.click(themeButton);

    expect(mockSetTheme).toHaveBeenCalledWith("dark");
  });

  it("should toggle to light theme when current theme is dark", async () => {
    mockTheme.mockReturnValue("dark");
    const user = userEvent.setup();
    render(<DashboardHeader />);

    const themeButton = screen.getByRole("button", {
      name: /switch to light mode/i,
    });
    await user.click(themeButton);

    expect(mockSetTheme).toHaveBeenCalledWith("light");
  });

  it("should render user menu button", () => {
    render(<DashboardHeader />);

    const userMenuButton = screen.getByRole("button", { name: /user menu/i });
    expect(userMenuButton).toBeInTheDocument();
  });

  it("should display user information in dropdown menu", async () => {
    const user = userEvent.setup();
    render(<DashboardHeader />);

    const userMenuButton = screen.getByRole("button", { name: /user menu/i });
    await user.click(userMenuButton);

    await waitFor(() => {
      expect(screen.getByText("Test User")).toBeInTheDocument();
      expect(screen.getByText("test@example.com")).toBeInTheDocument();
    });
  });

  it("should display default user name when user name is not available", async () => {
    mockUseUserContext.mockReturnValue({
      user: {
        name: null,
        email: "test@example.com",
      },
    });

    const user = userEvent.setup();
    render(<DashboardHeader />);

    const userMenuButton = screen.getByRole("button", { name: /user menu/i });
    await user.click(userMenuButton);

    await waitFor(() => {
      expect(screen.getByText("Admin User")).toBeInTheDocument();
    });
  });

  it("should call logout when logout menu item is clicked", async () => {
    const user = userEvent.setup();
    render(<DashboardHeader />);

    const userMenuButton = screen.getByRole("button", { name: /user menu/i });
    await user.click(userMenuButton);

    const logoutItem = await screen.findByRole("menuitem", {
      name: /log out/i,
    });
    await user.click(logoutItem);

    expect(mockLogout).toHaveBeenCalled();
  });

  it("should render logout option in dropdown menu", async () => {
    const user = userEvent.setup();
    render(<DashboardHeader />);

    const userMenuButton = screen.getByRole("button", { name: /user menu/i });
    await user.click(userMenuButton);

    await waitFor(() => {
      expect(screen.getByText("Log out")).toBeInTheDocument();
    });
  });

  it("should have proper accessibility attributes for theme button", () => {
    render(<DashboardHeader />);

    const themeButton = screen.getByRole("button", {
      name: /switch to dark mode/i,
    });
    expect(themeButton).toHaveAttribute("aria-label");
  });

  it("should handle missing user email gracefully", async () => {
    mockUseUserContext.mockReturnValue({
      user: {
        name: "Test User",
        email: null,
      },
    });

    const user = userEvent.setup();
    render(<DashboardHeader />);

    const userMenuButton = screen.getByRole("button", { name: /user menu/i });
    await user.click(userMenuButton);

    await waitFor(() => {
      expect(screen.getByText("Test User")).toBeInTheDocument();
    });
  });
});
