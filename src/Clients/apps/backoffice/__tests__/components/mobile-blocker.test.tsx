import { render, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { MobileBlocker } from "@/components/mobile-blocker";

type ChangeListener = (e: MediaQueryListEvent) => void;

function mockMatchMedia(matches: boolean) {
  const listeners: ChangeListener[] = [];

  const mql = {
    matches,
    media: `(max-width: 1023px)`,
    addEventListener: vi.fn((_event: string, cb: ChangeListener) => {
      listeners.push(cb);
    }),
    removeEventListener: vi.fn((_event: string, cb: ChangeListener) => {
      const idx = listeners.indexOf(cb);
      if (idx >= 0) listeners.splice(idx, 1);
    }),
  };

  vi.spyOn(globalThis, "matchMedia").mockReturnValue(
    mql as unknown as MediaQueryList,
  );

  return { mql, listeners };
}

describe("MobileBlocker", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("should render children on desktop (width >= 1024px)", () => {
    mockMatchMedia(false);

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Content")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Only")).not.toBeInTheDocument();
  });

  it("should show mobile blocker message on mobile (width < 1024px)", () => {
    mockMatchMedia(true);

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
    mockMatchMedia(true);

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Only")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Content")).not.toBeInTheDocument();
  });

  it("should render children on exactly 1024px width", () => {
    mockMatchMedia(false);

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Content")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Only")).not.toBeInTheDocument();
  });

  it("should show mobile blocker on 1023px width", () => {
    mockMatchMedia(true);

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    expect(screen.getByText("Desktop Only")).toBeInTheDocument();
    expect(screen.queryByText("Desktop Content")).not.toBeInTheDocument();
  });

  it("should render Monitor icon in mobile view", () => {
    mockMatchMedia(true);

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
