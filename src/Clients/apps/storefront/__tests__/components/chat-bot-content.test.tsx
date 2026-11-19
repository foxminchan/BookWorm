import { createRef } from "react";

import { act, fireEvent } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import type { ChatBotRef } from "@/components/chat-bot";
import { ChatBotContent } from "@/components/chat-bot-content";

const mockUseRateLimit = vi.hoisted(() => vi.fn());
const mockUseOfflineQueue = vi.hoisted(() => vi.fn());

vi.mock("@copilotkit/react-core/v2", () => ({
  CopilotSidebar: ({
    agentId,
    defaultOpen,
    labels,
  }: {
    agentId?: string;
    defaultOpen?: boolean;
    labels?: Record<string, string>;
  }) => (
    <div
      data-testid="copilot-sidebar"
      data-agent-id={agentId}
      data-default-open={String(defaultOpen)}
      data-title={labels?.modalHeaderTitle}
      role="dialog"
    >
      <button type="button" data-testid="first-focusable">
        First
      </button>
      <button type="button" data-testid="last-focusable">
        Last
      </button>
    </div>
  ),
}));

vi.mock("@copilotkit/react-core/v2/styles.css", () => ({}));
vi.mock("@/styles/copilot.css", () => ({}));

vi.mock("@/env.mjs", () => ({
  env: {
    NEXT_PUBLIC_COPILOT_AGENT_NAME: "bookworm-agent",
  },
}));

vi.mock("@/hooks/useChatAgentState", () => ({
  useChatAgentState: vi.fn(),
}));
vi.mock("@/hooks/useBasketContext", () => ({
  useBasketContext: vi.fn(),
}));
vi.mock("@/hooks/useUserContext", () => ({
  useUserContext: vi.fn(),
}));
vi.mock("@/hooks/useBookSearchActions", () => ({
  useBookSearchActions: vi.fn(),
}));
vi.mock("@/hooks/useA2UIComponents", () => ({
  useA2UIComponents: vi.fn(),
}));
vi.mock("@/hooks/useBasketActions", () => ({
  useBasketActions: () => ({
    ConfirmationDialog: () => <div data-testid="confirmation-dialog" />,
    liveRegion: <div data-testid="live-region" />,
  }),
}));
vi.mock("@/hooks/useRateLimit", () => ({
  useRateLimit: mockUseRateLimit,
}));
vi.mock("@/hooks/useOfflineQueue", () => ({
  useOfflineQueue: mockUseOfflineQueue,
}));

describe("ChatBotContent", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseRateLimit.mockReturnValue({
      isRateLimited: false,
      isThrottling: false,
      resetIn: 0,
    });
    mockUseOfflineQueue.mockReturnValue({
      isOnline: true,
      queueSize: 0,
      isSyncing: false,
    });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("renders trigger button, confirmation dialog, and live region when closed", () => {
    renderWithProviders(<ChatBotContent />);

    expect(screen.getByTestId("confirmation-dialog")).toBeInTheDocument();
    expect(screen.getByTestId("live-region")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Open BookWorm Literary Guide chat" }),
    ).toBeInTheDocument();
    expect(screen.queryByTestId("copilot-sidebar")).not.toBeInTheDocument();
  });

  it("opens chat dialog when trigger button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ChatBotContent />);

    await user.click(
      screen.getByRole("button", { name: "Open BookWorm Literary Guide chat" }),
    );

    const sidebar = screen.getByTestId("copilot-sidebar");
    expect(sidebar).toBeInTheDocument();
    expect(sidebar).toHaveAttribute("data-agent-id", "bookworm-agent");
    expect(sidebar).toHaveAttribute("data-default-open", "true");
    expect(sidebar).toHaveAttribute("data-title", "BookWorm Literary Guide");
  });

  it("opens chat via imperative ref handle", () => {
    const ref = createRef<ChatBotRef>();
    renderWithProviders(<ChatBotContent ref={ref} />);

    expect(screen.queryByTestId("copilot-sidebar")).not.toBeInTheDocument();

    act(() => {
      ref.current?.openChat();
    });

    expect(screen.getByTestId("copilot-sidebar")).toBeInTheDocument();
  });

  it("traps focus within the open dialog on Tab and Shift+Tab", () => {
    const ref = createRef<ChatBotRef>();
    renderWithProviders(<ChatBotContent ref={ref} />);

    act(() => {
      ref.current?.openChat();
    });

    const firstBtn = screen.getByTestId("first-focusable");
    const lastBtn = screen.getByTestId("last-focusable");

    lastBtn.focus();
    expect(document.activeElement).toBe(lastBtn);

    fireEvent.keyDown(document, { key: "Tab", shiftKey: false });
    expect(document.activeElement).toBe(firstBtn);

    fireEvent.keyDown(document, { key: "Tab", shiftKey: true });
    expect(document.activeElement).toBe(lastBtn);

    // Non-Tab key should not change focus
    fireEvent.keyDown(document, { key: "Escape" });
    expect(document.activeElement).toBe(lastBtn);
  });

  it("renders offline indicator with and without queued messages", () => {
    mockUseOfflineQueue.mockReturnValue({
      isOnline: false,
      queueSize: 0,
      isSyncing: false,
    });

    const ref = createRef<ChatBotRef>();
    const { rerender } = renderWithProviders(<ChatBotContent ref={ref} />);
    act(() => {
      ref.current?.openChat();
    });

    expect(screen.getByLabelText("You are offline")).toBeInTheDocument();
    expect(screen.getByText("Offline")).toBeInTheDocument();

    mockUseOfflineQueue.mockReturnValue({
      isOnline: false,
      queueSize: 3,
      isSyncing: false,
    });
    rerender(<ChatBotContent ref={ref} />);

    expect(
      screen.getByLabelText("You are offline. 3 messages queued"),
    ).toBeInTheDocument();
    expect(screen.getByText("(3 queued)")).toBeInTheDocument();
  });

  it("renders syncing indicator when online and syncing", () => {
    mockUseOfflineQueue.mockReturnValue({
      isOnline: true,
      queueSize: 2,
      isSyncing: true,
    });

    const ref = createRef<ChatBotRef>();
    renderWithProviders(<ChatBotContent ref={ref} />);
    act(() => {
      ref.current?.openChat();
    });

    expect(
      screen.getByLabelText("Sending queued messages"),
    ).toBeInTheDocument();
    expect(screen.getByText("Sending queued messages...")).toBeInTheDocument();
  });

  it("shows and clears rate limit warning after resetIn", () => {
    vi.useFakeTimers();
    mockUseRateLimit.mockReturnValue({
      isRateLimited: true,
      isThrottling: false,
      resetIn: 5000,
    });

    const ref = createRef<ChatBotRef>();
    renderWithProviders(<ChatBotContent ref={ref} />);
    act(() => {
      ref.current?.openChat();
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Rate limit reached. Please wait 5s",
    );

    act(() => {
      vi.advanceTimersByTime(5000);
    });

    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("shows and clears throttling warning after 2000ms", () => {
    vi.useFakeTimers();
    mockUseRateLimit.mockReturnValue({
      isRateLimited: false,
      isThrottling: true,
      resetIn: 0,
    });

    const ref = createRef<ChatBotRef>();
    renderWithProviders(<ChatBotContent ref={ref} />);
    act(() => {
      ref.current?.openChat();
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Please wait before sending another message",
    );

    act(() => {
      vi.advanceTimersByTime(2000);
    });

    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("shows error fallback on copilot/agent ErrorEvent and resets on Retry", async () => {
    const user = userEvent.setup();
    const ref = createRef<ChatBotRef>();
    renderWithProviders(<ChatBotContent ref={ref} />);

    act(() => {
      ref.current?.openChat();
    });

    // Unrelated error should be ignored
    act(() => {
      globalThis.dispatchEvent(
        new ErrorEvent("error", {
          message: "unrelated error",
          error: new Error("unrelated"),
        }),
      );
    });
    expect(screen.queryByText("Something went wrong")).not.toBeInTheDocument();

    // Copilot error should trigger fallback
    act(() => {
      globalThis.dispatchEvent(
        new ErrorEvent("error", {
          message: "copilot connection failed",
          error: new Error("copilot connection failed"),
          cancelable: true,
        }),
      );
    });

    expect(screen.getByText("Something went wrong")).toBeInTheDocument();
    expect(
      screen.getByText("The chat encountered an error. Please try again."),
    ).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Retry" }));

    // Reset closes the chat and returns focus to the trigger button
    const triggerButton = screen.getByRole("button", {
      name: "Open BookWorm Literary Guide chat",
    });
    expect(triggerButton).toBeInTheDocument();
    expect(document.activeElement).toBe(triggerButton);
  });
});
