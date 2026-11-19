"use client";

import {
  forwardRef,
  useCallback,
  useEffect,
  useImperativeHandle,
  useReducer,
  useRef,
} from "react";

import { CopilotSidebar } from "@copilotkit/react-core/v2";
import "@copilotkit/react-core/v2/styles.css";
import { AlertCircle, Clock, RefreshCw, WifiOff } from "lucide-react";
import { match } from "ts-pattern";

import { Button } from "@workspace/ui/components/button";
import { Card, CardContent } from "@workspace/ui/components/card";

import { env } from "@/env.mjs";
import { useA2UIComponents } from "@/hooks/useA2UIComponents";
import { useBasketActions } from "@/hooks/useBasketActions";
import { useBasketContext } from "@/hooks/useBasketContext";
import { useBookSearchActions } from "@/hooks/useBookSearchActions";
import { useChatAgentState } from "@/hooks/useChatAgentState";
import { useOfflineQueue } from "@/hooks/useOfflineQueue";
import { useRateLimit } from "@/hooks/useRateLimit";
import { useUserContext } from "@/hooks/useUserContext";
import "@/styles/copilot.css";

import type { ChatBotRef } from "./chat-bot";

const ChatBotErrorFallback = ({ onReset }: { onReset: () => void }) => (
  <div className="flex h-full flex-col items-center justify-center gap-4 p-6">
    <div className="flex h-16 w-16 items-center justify-center rounded-full bg-red-100 dark:bg-red-950">
      <AlertCircle className="h-8 w-8 text-red-600 dark:text-red-400" />
    </div>
    <div className="text-center">
      <h3 className="mb-2 text-lg font-semibold">Something went wrong</h3>
      <p className="text-muted-foreground mb-4 text-sm">
        The chat encountered an error. Please try again.
      </p>
      <Button onClick={onReset} size="sm" className="gap-2">
        <RefreshCw className="h-4 w-4" />
        Retry
      </Button>
    </div>
  </div>
);

type ChatUIState = {
  isOpen: boolean;
  hasError: boolean;
  error?: Error;
  rateLimitWarning: string | null;
};

type ChatUIAction =
  | { type: "OPEN" }
  | { type: "CLOSE" }
  | { type: "SET_OPEN"; open: boolean }
  | { type: "SET_ERROR"; error: Error }
  | { type: "CLEAR_ERROR" }
  | { type: "RESET" }
  | { type: "SET_RATE_LIMIT_WARNING"; warning: string }
  | { type: "CLEAR_RATE_LIMIT_WARNING" };

function chatUIReducer(state: ChatUIState, action: ChatUIAction): ChatUIState {
  return match(action)
    .with({ type: "OPEN" }, () => ({
      ...state,
      isOpen: true,
      hasError: false,
    }))
    .with({ type: "CLOSE" }, () => ({ ...state, isOpen: false }))
    .with({ type: "SET_OPEN" }, ({ open }) => ({ ...state, isOpen: open }))
    .with({ type: "SET_ERROR" }, ({ error }) => ({
      ...state,
      hasError: true,
      error,
    }))
    .with({ type: "CLEAR_ERROR" }, () => ({
      ...state,
      hasError: false,
      error: undefined,
    }))
    .with({ type: "RESET" }, () => ({
      ...state,
      hasError: false,
      error: undefined,
      isOpen: false,
    }))
    .with({ type: "SET_RATE_LIMIT_WARNING" }, ({ warning }) => ({
      ...state,
      rateLimitWarning: warning,
    }))
    .with({ type: "CLEAR_RATE_LIMIT_WARNING" }, () => ({
      ...state,
      rateLimitWarning: null,
    }))
    .exhaustive();
}

export const ChatBotContent = forwardRef<ChatBotRef>(
  function ChatBotContent(_, ref) {
    const [uiState, dispatch] = useReducer(chatUIReducer, {
      isOpen: false,
      hasError: false,
      rateLimitWarning: null,
    });

    const triggerButtonRef = useRef<HTMLButtonElement>(null);

    // Enable copilot tools, A2UI components, and bidirectional agent state
    useChatAgentState();
    useBasketContext();
    useUserContext();
    useBookSearchActions();
    useA2UIComponents();
    const { ConfirmationDialog, liveRegion } = useBasketActions();

    // Rate limiting and offline support
    const { isRateLimited, isThrottling, resetIn } = useRateLimit({
      maxRequests: 15,
      windowMs: 60000,
      throttleMs: 2000,
    });
    const { isOnline, queueSize, isSyncing } = useOfflineQueue();

    useImperativeHandle(ref, () => ({
      openChat: () => {
        dispatch({ type: "OPEN" });
      },
    }));

    // Focus management: return focus to trigger button when dialog closes
    useEffect(() => {
      if (!uiState.isOpen && triggerButtonRef.current) {
        triggerButtonRef.current.focus();
      }
    }, [uiState.isOpen]);

    // Focus trap: keep focus within dialog when open
    useEffect(() => {
      if (!uiState.isOpen) return;

      const handleKeyDown = (e: KeyboardEvent) => {
        if (e.key === "Tab") {
          const dialog = document.querySelector('[role="dialog"]');
          if (!dialog) return;

          const focusableElements = dialog.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])',
          );
          const firstElement = focusableElements[0] as HTMLElement;
          const lastElement = focusableElements[
            focusableElements.length - 1
          ] as HTMLElement;

          if (e.shiftKey) {
            if (document.activeElement === firstElement) {
              e.preventDefault();
              lastElement?.focus();
            }
          } else if (document.activeElement === lastElement) {
            e.preventDefault();
            firstElement?.focus();
          }
        }
      };

      document.addEventListener("keydown", handleKeyDown);
      return () => document.removeEventListener("keydown", handleKeyDown);
    }, [uiState.isOpen]);

    useEffect(() => {
      const handleError = (event: ErrorEvent) => {
        if (
          event.message.includes("copilot") ||
          event.message.includes("agent")
        ) {
          dispatch({ type: "SET_ERROR", error: event.error });
          event.preventDefault();
        }
      };

      globalThis.addEventListener("error", handleError);
      return () => globalThis.removeEventListener("error", handleError);
    }, []);

    // Show rate limit warning
    useEffect(() => {
      if (isRateLimited && resetIn) {
        dispatch({
          type: "SET_RATE_LIMIT_WARNING",
          warning: `Rate limit reached. Please wait ${Math.ceil(resetIn / 1000)}s`,
        });
        const timer = setTimeout(
          () => dispatch({ type: "CLEAR_RATE_LIMIT_WARNING" }),
          resetIn,
        );
        return () => clearTimeout(timer);
      } else if (isThrottling) {
        dispatch({
          type: "SET_RATE_LIMIT_WARNING",
          warning: "Please wait before sending another message",
        });
        const timer = setTimeout(
          () => dispatch({ type: "CLEAR_RATE_LIMIT_WARNING" }),
          2000,
        );
        return () => clearTimeout(timer);
      }
    }, [isRateLimited, isThrottling, resetIn]);

    const handleReset = useCallback(() => {
      dispatch({ type: "RESET" });
    }, []);

    if (!uiState.isOpen) {
      return (
        <>
          {/* Keep confirmation dialog and live region mounted for WebMCP calls when chat is closed */}
          <ConfirmationDialog />
          {liveRegion}
          <Button
            ref={triggerButtonRef}
            onClick={() => dispatch({ type: "OPEN" })}
            size="icon"
            className="fixed right-6 bottom-6 z-50 hidden h-12 w-12 rounded-full shadow-lg transition-transform hover:scale-110 md:flex"
            aria-label="Open BookWorm Literary Guide chat"
            aria-haspopup="dialog"
            data-copilot-chat-trigger
          >
            <svg
              className="h-6 w-6"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <path d="M7.9 20A9 9 0 1 0 4 16.1L2 22Z" />
            </svg>
          </Button>
        </>
      );
    }

    return (
      <dialog
        className="fixed inset-0 z-50 m-0 h-auto w-auto max-w-none border-none bg-transparent p-0 md:block"
        open
        aria-label="BookWorm Literary Guide Chat"
      >
        {/* Render confirmation dialog for basket actions */}
        <ConfirmationDialog />
        {/* Live region for basket announcements */}
        {liveRegion}

        {/* Offline indicator */}
        {!isOnline && (
          <output
            className="copilot-offline-badge"
            aria-live="polite"
            aria-label={
              queueSize > 0
                ? `You are offline. ${queueSize} messages queued`
                : "You are offline"
            }
          >
            <WifiOff className="h-3 w-3" aria-hidden="true" />
            <span>Offline</span>
            {queueSize > 0 && <span>({queueSize} queued)</span>}
          </output>
        )}

        {/* Rate limit warning */}
        {uiState.rateLimitWarning && (
          <div
            className="copilot-rate-limit-warning fixed top-20 right-6 z-50"
            role="alert"
            aria-live="assertive"
          >
            <Clock className="h-4 w-4" aria-hidden="true" />
            <span>{uiState.rateLimitWarning}</span>
          </div>
        )}

        {/* Syncing indicator */}
        {isSyncing && isOnline && (
          <output
            className="fixed top-20 right-6 z-50 flex items-center gap-2 rounded-lg border bg-blue-50 px-3 py-2 text-sm dark:bg-blue-950"
            aria-live="polite"
            aria-label="Sending queued messages"
          >
            <div
              className="h-3 w-3 animate-spin rounded-full border-2 border-blue-600 border-t-transparent"
              aria-hidden="true"
            />
            <span>Sending queued messages...</span>
          </output>
        )}

        {uiState.hasError ? (
          <div className="fixed inset-0 flex items-center justify-center bg-black/20 backdrop-blur-sm">
            <Card className="m-6 w-full max-w-md">
              <CardContent className="p-6">
                <ChatBotErrorFallback onReset={handleReset} />
              </CardContent>
            </Card>
          </div>
        ) : (
          <CopilotSidebar
            agentId={env.NEXT_PUBLIC_COPILOT_AGENT_NAME}
            labels={{
              modalHeaderTitle: "BookWorm Literary Guide",
              welcomeMessageText:
                "Hi! I'm your literary assistant. I can help you find books, manage your basket, and answer questions about our collection. What would you like to explore today?",
              chatInputPlaceholder:
                "Ask about books, search for titles, or manage your basket...",
            }}
            defaultOpen={uiState.isOpen}
          />
        )}
      </dialog>
    );
  },
);
