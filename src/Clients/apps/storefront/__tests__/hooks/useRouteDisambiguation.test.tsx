import { JSX } from "react";

import { render, renderHook } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useRouteDisambiguation } from "@/hooks/useRouteDisambiguation";

type RouteOption = {
  id: string;
  label: string;
  description: string;
};

type RenderProps = {
  args: {
    question?: string;
    suggestedRoute?: string;
    confidence?: number;
    options?: RouteOption[];
  };
  status: "inProgress" | "executing" | "complete";
  respond?: (result: unknown) => void;
};

type RegisteredHook = {
  name: string;
  description: string;
  parameters: unknown;
  render: (props: RenderProps) => JSX.Element;
};

const registeredHooks: RegisteredHook[] = [];

vi.mock("@copilotkit/react-core/v2", () => ({
  useHumanInTheLoop: vi.fn((config: RegisteredHook) => {
    registeredHooks.push(config);
  }),
}));

const sampleOptions: RouteOption[] = [
  {
    id: "book",
    label: "Book Search & Recommendations",
    description: "Find books or get recommendations.",
  },
  {
    id: "qa",
    label: "Store Policies & Support",
    description: "Shipping, returns, or orders.",
  },
  {
    id: "rating",
    label: "Reviews & Ratings",
    description: "Summarize customer reviews.",
  },
];

describe("useRouteDisambiguation", () => {
  beforeEach(() => {
    registeredHooks.length = 0;
    vi.clearAllMocks();
  });

  it("registers the disambiguateRoute human-in-the-loop tool", () => {
    renderHook(() => useRouteDisambiguation());

    expect(registeredHooks).toHaveLength(1);
    expect(registeredHooks[0]?.name).toBe("disambiguateRoute");
    expect(registeredHooks[0]?.description).toContain("BookWorm specialist");
  });

  it("renders empty fragment when status is complete", () => {
    renderHook(() => useRouteDisambiguation());
    const hook = registeredHooks[0]!;

    const { container } = render(
      hook.render({
        args: {
          question: "Which specialist?",
          suggestedRoute: "book",
          confidence: 0.6,
          options: sampleOptions,
        },
        status: "complete",
      }),
    );

    expect(container.innerHTML).toBe("");
  });

  it("renders question, suggested badge, and calls respond on option click when executing", async () => {
    const user = userEvent.setup();
    const respond = vi.fn();

    renderHook(() => useRouteDisambiguation());
    const hook = registeredHooks[0]!;

    const { getByRole, getByText } = render(
      hook.render({
        args: {
          question: "Are you looking for books or order support?",
          suggestedRoute: "qa",
          confidence: 0.55,
          options: sampleOptions,
        },
        status: "executing",
        respond,
      }),
    );

    expect(getByText("Choose a Specialist")).toBeInTheDocument();
    expect(
      getByText("Are you looking for books or order support?"),
    ).toBeInTheDocument();
    expect(
      getByText("Store Policies & Support (Suggested)"),
    ).toBeInTheDocument();
    expect(getByText("Book Search & Recommendations")).toBeInTheDocument();

    const qaButton = getByRole("button", {
      name: /Store Policies & Support \(Suggested\)/i,
    });
    expect(qaButton).toBeEnabled();

    await user.click(qaButton);
    expect(respond).toHaveBeenCalledWith("qa");
  });

  it("uses fallback question, handles missing options, and disables buttons when not executing or missing respond", () => {
    renderHook(() => useRouteDisambiguation());
    const hook = registeredHooks[0]!;

    const { getByText, queryByRole, rerender } = render(
      hook.render({
        args: {
          question: "",
          suggestedRoute: "book",
          confidence: 0.5,
        },
        status: "inProgress",
      }),
    );

    expect(
      getByText("Which area best matches what you need help with?"),
    ).toBeInTheDocument();
    expect(queryByRole("button")).not.toBeInTheDocument();

    rerender(
      hook.render({
        args: {
          question: "Pick one",
          suggestedRoute: "book",
          confidence: 0.5,
          options: sampleOptions,
        },
        status: "inProgress",
        respond: vi.fn(),
      }),
    );

    expect(
      queryByRole("button", { name: /Book Search & Recommendations/i }),
    ).toBeDisabled();
  });
});
