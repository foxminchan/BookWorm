import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  mockSession,
  renderWithProviders,
  screen,
} from "@/__tests__/utils/test-utils";
import { HeaderAccountMenu } from "@/components/header-account-menu";

// Hoisted mocks
const mockUseDelayedToggle = vi.hoisted(() => vi.fn());
const mockUseLogout = vi.hoisted(() => vi.fn());
const mockUseSession = vi.hoisted(() => vi.fn());

vi.mock("@/hooks/useDelayedToggle", () => ({
  useDelayedToggle: mockUseDelayedToggle,
}));

vi.mock("@/hooks/useLogout", () => ({
  useLogout: mockUseLogout,
}));

vi.mock("@/lib/auth-client", () => ({
  useSession: mockUseSession,
}));

function setupDelayedToggle(
  overrides: Partial<{
    isOpen: boolean;
    close: () => void;
    handleMouseEnter: () => void;
    handleMouseLeave: () => void;
  }> = {},
) {
  const defaults = {
    isOpen: false,
    open: vi.fn(),
    close: vi.fn(),
    toggle: vi.fn(),
    handleMouseEnter: vi.fn(),
    handleMouseLeave: vi.fn(),
  };
  mockUseDelayedToggle.mockReturnValue({ ...defaults, ...overrides });
  return { ...defaults, ...overrides };
}

describe("HeaderAccountMenu", () => {
  const user = userEvent.setup();

  beforeEach(() => {
    vi.clearAllMocks();
    setupDelayedToggle();
    mockUseLogout.mockReturnValue({ logout: vi.fn(), isLoggingOut: false });
    mockUseSession.mockReturnValue({ data: null, isPending: false });
  });

  it("renders account button", () => {
    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    expect(
      screen.getByRole("button", { name: /account menu/i }),
    ).toBeInTheDocument();
  });

  it("renders navigation with Account label", () => {
    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    expect(
      screen.getByRole("navigation", { name: /account/i }),
    ).toBeInTheDocument();
  });

  it("does not show menu when closed", () => {
    setupDelayedToggle({ isOpen: false });
    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    expect(screen.queryByRole("menu")).not.toBeInTheDocument();
  });

  it("shows guest menu items when not authenticated", () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    expect(
      screen.getByRole("menuitem", { name: /login/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("menuitem", { name: /register/i }),
    ).toBeInTheDocument();
  });

  it("shows authenticated menu items when user is logged in", () => {
    setupDelayedToggle({ isOpen: true });
    mockUseSession.mockReturnValue({
      data: mockSession,
      isPending: false,
    });

    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    expect(
      screen.getByRole("menuitem", { name: /my profile/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("menuitem", { name: /order history/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("menuitem", { name: /logout/i }),
    ).toBeInTheDocument();
  });

  it("does not show login/register when authenticated", () => {
    setupDelayedToggle({ isOpen: true });
    mockUseSession.mockReturnValue({
      data: mockSession,
      isPending: false,
    });

    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    expect(
      screen.queryByRole("menuitem", { name: /login/i }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole("menuitem", { name: /register/i }),
    ).not.toBeInTheDocument();
  });

  it("calls logout when logout button is clicked", async () => {
    const mockLogoutFn = vi.fn().mockResolvedValue(undefined);
    setupDelayedToggle({ isOpen: true });
    mockUseLogout.mockReturnValue({
      logout: mockLogoutFn,
      isLoggingOut: false,
    });
    mockUseSession.mockReturnValue({
      data: mockSession,
      isPending: false,
    });

    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    await user.click(screen.getByRole("menuitem", { name: /logout/i }));

    expect(mockLogoutFn).toHaveBeenCalled();
  });

  it("closes menu on logout", async () => {
    const mockClose = vi.fn();
    const mockLogoutFn = vi.fn().mockResolvedValue(undefined);
    setupDelayedToggle({ isOpen: true, close: mockClose });
    mockUseLogout.mockReturnValue({
      logout: mockLogoutFn,
      isLoggingOut: false,
    });
    mockUseSession.mockReturnValue({
      data: mockSession,
      isPending: false,
    });

    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    await user.click(screen.getByRole("menuitem", { name: /logout/i }));

    expect(mockClose).toHaveBeenCalled();
  });

  it("applies active style when isAccountActive is true", () => {
    renderWithProviders(<HeaderAccountMenu isAccountActive={true} />);

    const button = screen.getByRole("button", { name: /account menu/i });
    expect(button.className).toContain("bg-secondary");
  });

  it("does not apply active style when isAccountActive is false", () => {
    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    const button = screen.getByRole("button", { name: /account menu/i });
    expect(button.className).not.toContain("bg-secondary");
  });

  it("has correct aria attributes", () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    const button = screen.getByRole("button", { name: /account menu/i });
    expect(button).toHaveAttribute("aria-expanded", "true");
    expect(button).toHaveAttribute("aria-haspopup", "true");
  });

  it("has correct links for authenticated menu items", () => {
    setupDelayedToggle({ isOpen: true });
    mockUseSession.mockReturnValue({
      data: mockSession,
      isPending: false,
    });

    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    const profileLink = screen.getByRole("menuitem", { name: /my profile/i });
    expect(profileLink).toHaveAttribute("href", "/account");

    const ordersLink = screen.getByRole("menuitem", { name: /order history/i });
    expect(ordersLink).toHaveAttribute("href", "/account/orders");
  });

  it("has correct links for guest menu items", () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderAccountMenu isAccountActive={false} />);

    const loginLink = screen.getByRole("menuitem", { name: /login/i });
    expect(loginLink).toHaveAttribute("href", "/login");

    const registerLink = screen.getByRole("menuitem", { name: /register/i });
    expect(registerLink).toHaveAttribute("href", "/register");
  });
});
