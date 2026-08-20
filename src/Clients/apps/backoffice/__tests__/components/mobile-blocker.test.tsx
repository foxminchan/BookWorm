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

  it.each([
    {
      name: "desktop width (>= 1024px)",
      matches: false,
      shouldRenderChildren: true,
    },
    {
      name: "mobile width (< 1024px)",
      matches: true,
      shouldRenderChildren: false,
    },
    {
      name: "small tablet width (800px)",
      matches: true,
      shouldRenderChildren: false,
    },
    {
      name: "exact desktop breakpoint (1024px)",
      matches: false,
      shouldRenderChildren: true,
    },
  ])("should handle $name", ({ matches, shouldRenderChildren }) => {
    mockMatchMedia(matches);

    render(
      <MobileBlocker>
        <div>Desktop Content</div>
      </MobileBlocker>,
    );

    if (shouldRenderChildren) {
      expect(screen.getByText("Desktop Content")).toBeInTheDocument();
      expect(screen.queryByText("Desktop Only")).not.toBeInTheDocument();
      return;
    }

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
