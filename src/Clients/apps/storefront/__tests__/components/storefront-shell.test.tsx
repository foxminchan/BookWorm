import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { StorefrontShell } from "@workspace/storefront-shell/components/storefront-shell";

vi.mock("@workspace/storefront-shell/components/header", () => ({
  Header: () => <header data-testid="mock-header">Header</header>,
}));

vi.mock("@workspace/storefront-shell/components/footer", () => ({
  Footer: () => <footer data-testid="mock-footer">Footer</footer>,
}));

vi.mock("@workspace/storefront-shell/components/mobile-bottom-nav", () => ({
  MobileBottomNav: () => (
    <nav data-testid="mock-mobile-bottom-nav">Mobile Bottom Nav</nav>
  ),
}));

describe("StorefrontShell", () => {
  it("renders header, main content, footer, and mobile bottom nav", () => {
    const { container } = render(
      <StorefrontShell>
        <div>Test Child</div>
      </StorefrontShell>,
    );

    expect(screen.getByTestId("mock-header")).toBeInTheDocument();
    expect(screen.getByTestId("mock-footer")).toBeInTheDocument();
    expect(screen.getByTestId("mock-mobile-bottom-nav")).toBeInTheDocument();
    const main = container.querySelector("#main-content");
    expect(main).toBeInTheDocument();
    expect(main).toContainElement(screen.getByText("Test Child"));
  });
});
