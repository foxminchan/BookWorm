import { render, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { MobileBlocker } from "@/components/mobile-blocker";

describe("MobileBlocker", () => {
  let originalInnerWidth: number;

  beforeEach(() => {
    originalInnerWidth = window.innerWidth;
    vi.clearAllMocks();
  });

  afterEach(() => {
    // Restore original window size
    Object.defineProperty(window, "innerWidth", {
      writable: true,
      configurable: true,
      value: originalInnerWidth,
    });
  });

  it("should render children on desktop (width >= 1024px)", () => {
    Object.defineProperty(window, "innerWidth", {
      writable: true,
      configurable: true,
      value: 1024,
    });

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Content")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Only")).not.toBeInTheDocument();
  });

  it("should show mobile blocker message on mobile (width < 1024px)", () => {
    Object.defineProperty(window, "innerWidth", {
      writable: true,
      configurable: true,
      value: 768,
    });

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Only")).toBeInTheDocument();
    expect(
      screen.getByText(
        "The Admin Portal is only available on desktop devices.",
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        /Please access this portal from a device with a screen width of at least 1024px/,
      ),
    ).toBeInTheDocument();
    expect(screen.queryByText("Desktop Content")).not.toBeInTheDocument();
  });

  it("should show mobile blocker on small tablet (width = 800px)", () => {
    Object.defineProperty(window, "innerWidth", {
      writable: true,
      configurable: true,
      value: 800,
    });

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Only")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Content")).not.toBeInTheDocument();
  });

  it("should render children on exactly 1024px width", () => {
    Object.defineProperty(window, "innerWidth", {
      writable: true,
      configurable: true,
      value: 1024,
    });

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Content")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Only")).not.toBeInTheDocument();
  });

  it("should show mobile blocker on 1023px width", () => {
    Object.defineProperty(window, "innerWidth", {
      writable: true,
      configurable: true,
      value: 1023,
    });

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Only")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Content")).not.toBeInTheDocument();
  });

  it("should render Monitor icon in mobile view", () => {
    Object.defineProperty(window, "innerWidth", {
      writable: true,
      configurable: true,
      value: 768,
    });

    const { container } = render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    // Monitor icon should be present (lucide-react renders as SVG)
    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });
});
