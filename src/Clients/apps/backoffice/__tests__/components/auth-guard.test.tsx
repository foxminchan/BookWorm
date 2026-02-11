import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { AuthGuard } from "@/components/auth-guard";

// Mock useUserContext hook
const mockUseUserContext = vi.hoisted(() => vi.fn());
vi.mock("@/hooks/use-user-context", () => ({
  useUserContext: () => mockUseUserContext(),
}));

// Mock signIn from auth-client
vi.mock("@/lib/auth-client", () => ({
  signIn: {
    social: vi.fn(),
  },
}));

describe("AuthGuard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should show loading state when authentication is loading", () => {
    mockUseUserContext.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
    });

    render(
      <AuthGuard>
        <div>Protected Content</div>
      </AuthGuard>,
    );

    expect(screen.getByText("Loading Dashboard")).toBeInTheDocument();
    expect(
      screen.getByText("Please wait while we verify your credentials..."),
    ).toBeInTheDocument();
    expect(screen.getByRole("status")).toBeInTheDocument();
    expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
  });

  it("should trigger social sign-in when not authenticated and not loading", async () => {
    mockUseUserContext.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
    });

    const { signIn } = await import("@/lib/auth-client");

    render(
      <AuthGuard>
        <div>Protected Content</div>
      </AuthGuard>,
    );

    await waitFor(() => {
      expect(signIn.social).toHaveBeenCalledWith({
        provider: "keycloak",
        callbackURL: "/",
      });
    });
  });

  it("should render children when authenticated", () => {
    mockUseUserContext.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
    });

    render(
      <AuthGuard>
        <div>Protected Content</div>
      </AuthGuard>,
    );

    expect(screen.getByText("Protected Content")).toBeInTheDocument();
    expect(screen.queryByText("Loading Dashboard")).not.toBeInTheDocument();
  });

  it("should have proper accessibility attributes in loading state", () => {
    mockUseUserContext.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
    });

    render(
      <AuthGuard>
        <div>Protected Content</div>
      </AuthGuard>,
    );

    const statusCard = screen.getByRole("status");
    expect(statusCard).toHaveAttribute("aria-live", "polite");
    expect(statusCard).toHaveAttribute("aria-busy", "true");
    expect(screen.getByText("Loading Dashboard")).toBeInTheDocument();
  });

  it("should not call signIn when already loading", async () => {
    mockUseUserContext.mockReturnValue({
      isAuthenticated: false,
      isLoading: true,
    });

    const { signIn } = await import("@/lib/auth-client");

    render(
      <AuthGuard>
        <div>Protected Content</div>
      </AuthGuard>,
    );

    expect(signIn.social).not.toHaveBeenCalled();
  });

  it("should not call signIn when already authenticated", async () => {
    mockUseUserContext.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
    });

    const { signIn } = await import("@/lib/auth-client");

    render(
      <AuthGuard>
        <div>Protected Content</div>
      </AuthGuard>,
    );

    expect(signIn.social).not.toHaveBeenCalled();
  });
});
