import React from "react";

import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import { HeaderSearch, MobileSearch } from "@/components/header-search";

// Hoisted mocks
const mockPush = vi.hoisted(() => vi.fn());
const mockUseDelayedToggle = vi.hoisted(() => vi.fn());

vi.mock("next/navigation", async () => {
  const actual =
    await vi.importActual<typeof import("next/navigation")>("next/navigation");
  return {
    ...actual,
    useRouter: () => ({
      push: mockPush,
      replace: vi.fn(),
      prefetch: vi.fn(),
      back: vi.fn(),
    }),
    useSearchParams: () => new URLSearchParams(),
    usePathname: () => "/",
  };
});

vi.mock("@/hooks/useDelayedToggle", () => ({
  useDelayedToggle: mockUseDelayedToggle,
}));

// Mock Radix-based form components to avoid Slot single-child issues in test env
vi.mock("@workspace/ui/components/form", async () => {
  const actual = await vi.importActual<
    typeof import("@workspace/ui/components/form")
  >("@workspace/ui/components/form");
  return {
    ...actual,
    FormControl: ({ children }: { children: React.ReactNode }) => (
      <div data-slot="form-control">{children}</div>
    ),
  };
});

function setupDelayedToggle(
  overrides: Partial<{
    isOpen: boolean;
    close: () => void;
    toggle: () => void;
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

describe("HeaderSearch", () => {
  const user = userEvent.setup();

  beforeEach(() => {
    vi.clearAllMocks();
    setupDelayedToggle();
  });

  it("renders search button", () => {
    renderWithProviders(<HeaderSearch />);

    expect(
      screen.getByRole("button", { name: /search books/i }),
    ).toBeInTheDocument();
  });

  it("does not show search dropdown when closed", () => {
    setupDelayedToggle({ isOpen: false });
    renderWithProviders(<HeaderSearch />);

    expect(
      screen.queryByPlaceholderText("Search books..."),
    ).not.toBeInTheDocument();
  });

  it("shows search input when open", () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderSearch />);

    expect(screen.getByPlaceholderText("Search books...")).toBeInTheDocument();
  });

  it("calls toggle when search button is clicked", async () => {
    const { toggle } = setupDelayedToggle();
    renderWithProviders(<HeaderSearch />);

    await user.click(screen.getByRole("button", { name: /search books/i }));

    expect(toggle).toHaveBeenCalled();
  });

  it("has correct aria attributes on search button when open", () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderSearch />);

    const button = screen.getByRole("button", { name: /search books/i });
    expect(button).toHaveAttribute("aria-expanded", "true");
    expect(button).toHaveAttribute("aria-haspopup", "true");
  });

  it("has correct aria attributes on search button when closed", () => {
    setupDelayedToggle({ isOpen: false });
    renderWithProviders(<HeaderSearch />);

    const button = screen.getByRole("button", { name: /search books/i });
    expect(button).toHaveAttribute("aria-expanded", "false");
  });

  it("has search role on wrapper", () => {
    renderWithProviders(<HeaderSearch />);

    expect(screen.getByRole("search")).toBeInTheDocument();
  });

  it("navigates to shop page on search submit", async () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderSearch />);

    const input = screen.getByPlaceholderText("Search books...");
    await user.type(input, "fantasy novels{enter}");

    expect(mockPush).toHaveBeenCalledWith(
      `/shop?search=${encodeURIComponent("fantasy novels")}`,
    );
  });

  it("does not navigate on empty search", async () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderSearch />);

    const input = screen.getByPlaceholderText("Search books...");
    await user.click(input);
    await user.keyboard("{enter}");

    expect(mockPush).not.toHaveBeenCalled();
  });

  it("does not navigate on whitespace-only search", async () => {
    setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderSearch />);

    const input = screen.getByPlaceholderText("Search books...");
    await user.type(input, "   {enter}");

    expect(mockPush).not.toHaveBeenCalled();
  });

  it("closes and resets form after successful search", async () => {
    const { close } = setupDelayedToggle({ isOpen: true });
    renderWithProviders(<HeaderSearch />);

    const input = screen.getByPlaceholderText("Search books...");
    await user.type(input, "test{enter}");

    expect(close).toHaveBeenCalled();
    expect(input).toHaveValue("");
  });
});

describe("MobileSearch", () => {
  const user = userEvent.setup();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders mobile search form with search role", () => {
    renderWithProviders(<MobileSearch />);

    expect(
      screen.getByRole("search", { name: /search books/i }),
    ).toBeInTheDocument();
  });

  it("renders search input", () => {
    renderWithProviders(<MobileSearch />);

    expect(screen.getByPlaceholderText("Search books...")).toBeInTheDocument();
  });

  it("navigates to shop on submit with search term", async () => {
    renderWithProviders(<MobileSearch />);

    const input = screen.getByPlaceholderText("Search books...");
    await user.type(input, "mystery{enter}");

    expect(mockPush).toHaveBeenCalledWith(
      `/shop?search=${encodeURIComponent("mystery")}`,
    );
  });

  it("does not navigate on empty submit", async () => {
    renderWithProviders(<MobileSearch />);

    const input = screen.getByPlaceholderText("Search books...");
    await user.click(input);
    await user.keyboard("{enter}");

    expect(mockPush).not.toHaveBeenCalled();
  });

  it("resets form after successful search", async () => {
    renderWithProviders(<MobileSearch />);

    const input = screen.getByPlaceholderText("Search books...");
    await user.type(input, "novels{enter}");

    expect(input).toHaveValue("");
  });

  it("has sr-only label for accessibility", () => {
    renderWithProviders(<MobileSearch />);

    const labels = screen.getAllByText("Search books");
    expect(labels.length).toBeGreaterThan(0);
  });
});
