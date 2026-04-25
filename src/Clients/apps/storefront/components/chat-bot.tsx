"use client";

import {
  forwardRef,
  useCallback,
  useEffect,
  useImperativeHandle,
  useReducer,
  useState,
} from "react";

import { CopilotSidebar } from "@copilotkit/react-core/v2";
import "@copilotkit/react-core/v2/styles.css";
import { useAtomValue } from "jotai";
import {
  AlertCircle,
  Clock,
  MessageCircle,
  RefreshCw,
  WifiOff,
  X,
} from "lucide-react";
import { match } from "ts-pattern";

import { Button } from "@workspace/ui/components/button";
import { Card, CardContent, CardHeader } from "@workspace/ui/components/card";

import { isCopilotEnabledAtom } from "@/atoms/feature-flags-atom";
import { env } from "@/env.mjs";
import { useBasketActions } from "@/hooks/useBasketActions";
import { useBasketContext } from "@/hooks/useBasketContext";
import { useBookSearchActions } from "@/hooks/useBookSearchActions";
import { useChatAgentState } from "@/hooks/useChatAgentState";
import { useOfflineQueue } from "@/hooks/useOfflineQueue";
import { useRateLimit } from "@/hooks/useRateLimit";
import { useUserContext } from "@/hooks/useUserContext";

export type ChatBotRef = {
  openChat: () => void;
};

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

const ChatBotContent = forwardRef<ChatBotRef>(function ChatBotContent(_, ref) {
  type ChatUIState = {
    hasError: boolean;
    error?: Error;
    rateLimitWarning: string | null;
  };

  type ChatUIAction =
    | { type: "SET_ERROR"; error: Error }
    | { type: "CLEAR_ERROR" }
    | { type: "RESET" }
    | { type: "SET_RATE_LIMIT_WARNING"; warning: string }
    | { type: "CLEAR_RATE_LIMIT_WARNING" };

  const chatUIReducer = useCallback(
    (state: ChatUIState, action: ChatUIAction): ChatUIState => {
      return match(action)
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
    },
    [],
  );

  const [uiState, dispatch] = useReducer(chatUIReducer, {
    hasError: false,
    rateLimitWarning: null,
  });

  // Defer client-only overlay rendering to avoid SSR/client hydration mismatch.
  // Network state (isOnline) is unavailable during SSR and differs on first client render.
  const [mounted, setMounted] = useState(false);
  useEffect(() => {
    setMounted(true);
  }, []);

  // Enable copilot tools and bidirectional agent state
  useChatAgentState();
  useBasketContext();
  useUserContext();
  useBookSearchActions();
  const { ConfirmationDialog, liveRegion } = useBasketActions();

  // Rate limiting and offline support
  const { isRateLimited, isThrottling, resetIn } = useRateLimit({
    maxRequests: 15,
    windowMs: 60000,
    throttleMs: 2000,
  });
  const { isOnline, queueSize, isSyncing } = useOfflineQueue();

  // CopilotSidebar v2 manages its own toggle button; click it programmatically
  useImperativeHandle(ref, () => ({
    openChat: () => {
      const toggleBtn = document.querySelector<HTMLElement>(
        "[data-copilot-chat-trigger]",
      );
      toggleBtn?.click();
    },
  }));

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

  return (
    <>
      {/* Render confirmation dialog for basket actions */}
      <ConfirmationDialog />
      {/* Live region for basket announcements */}
      {liveRegion}

      {/* Offline indicator — client-only to avoid hydration mismatch */}
      {mounted && !isOnline && (
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

      {/* Rate limit warning — client-only */}
      {mounted && uiState.rateLimitWarning && (
        <div
          className="copilot-rate-limit-warning fixed top-20 right-6 z-50"
          role="alert"
          aria-live="assertive"
        >
          <Clock className="h-4 w-4" aria-hidden="true" />
          <span>{uiState.rateLimitWarning}</span>
        </div>
      )}

      {/* Syncing indicator — client-only */}
      {mounted && isSyncing && isOnline && (
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
        />
      )}
    </>
  );
});

const UnavailableChatUI = ({
  isOpen,
  onClose,
  isFeatureDisabled = false,
}: {
  isOpen: boolean;
  onClose: () => void;
  isFeatureDisabled?: boolean;
}) => {
  if (!isOpen) {
    return null;
  }

  return (
    <dialog
      className="fixed inset-0 z-50 m-0 hidden h-auto w-auto max-w-none items-end justify-end border-none bg-transparent p-6 md:flex"
      open
      aria-labelledby="chat-unavailable-title"
    >
      <button
        type="button"
        className="fixed inset-0 cursor-default border-none bg-black/20 backdrop-blur-sm"
        onClick={onClose}
        aria-label="Close chat dialog"
      />
      <Card className="border-secondary/30 animate-in fade-in slide-in-from-bottom-4 relative z-10 flex h-150 w-96 flex-col overflow-hidden rounded-2xl shadow-2xl duration-300 sm:w-105">
        <CardHeader className="from-primary via-primary to-primary/90 text-primary-foreground border-primary-foreground/10 flex flex-row items-center justify-between gap-3 border-b bg-linear-to-br p-6">
          <div className="flex items-center gap-3">
            <div className="bg-primary-foreground/20 flex h-10 w-10 items-center justify-center rounded-full">
              <MessageCircle className="h-5 w-5" aria-hidden="true" />
            </div>
            <div>
              <h2 id="chat-unavailable-title" className="font-semibold">
                BookWorm Literary Guide
              </h2>
              <p className="text-xs opacity-90">
                {isFeatureDisabled ? "Coming Soon" : "Chat unavailable"}
              </p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="icon"
            onClick={onClose}
            className="hover:bg-primary-foreground/20 h-8 w-8 rounded-full"
            aria-label="Close dialog"
          >
            <X className="h-5 w-5" aria-hidden="true" />
          </Button>
        </CardHeader>

        <CardContent className="flex flex-1 flex-col items-center justify-center gap-4 p-6">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-orange-100 dark:bg-orange-950">
            <AlertCircle className="h-8 w-8 text-orange-600 dark:text-orange-400" />
          </div>
          <div className="text-center">
            <h3 className="mb-2 text-lg font-semibold">
              {isFeatureDisabled
                ? "Feature in Development"
                : "Chat Unavailable"}
            </h3>
            <p className="text-muted-foreground text-sm">
              {isFeatureDisabled
                ? "Our AI chat assistant is currently under development and will be available soon. Stay tuned!"
                : "The chat feature is currently unavailable. Please try again later."}
            </p>
          </div>
        </CardContent>
      </Card>
    </dialog>
  );
};

export const ChatBot = forwardRef<ChatBotRef>(function ChatBot(_, ref) {
  const isCopilotEnabled = useAtomValue(isCopilotEnabledAtom);
  const hasGateway = !!(
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP
  );
  const [isUnavailableChatOpen, setIsUnavailableChatOpen] = useState(false);

  const isFeatureDisabled = !isCopilotEnabled;
  const canShowChat = isCopilotEnabled && hasGateway;

  useImperativeHandle(ref, () => ({
    openChat: () => {
      if (canShowChat) {
        const chatButton = document.querySelector(
          "[data-copilot-chat-trigger]",
        );
        if (chatButton instanceof HTMLElement) {
          chatButton.click();
        }
      } else {
        setIsUnavailableChatOpen(true);
      }
    },
  }));

  if (canShowChat) {
    return <ChatBotContent ref={ref} />;
  }

  return (
    <>
      <Button
        onClick={() => setIsUnavailableChatOpen(true)}
        size="icon"
        className="fixed right-6 bottom-6 z-50 hidden h-12 w-12 rounded-full shadow-lg transition-transform hover:scale-110 md:flex"
        aria-label={
          isFeatureDisabled
            ? "Chat feature coming soon - click to learn more"
            : "Open chat - feature currently unavailable"
        }
        title={
          isFeatureDisabled
            ? "Feature in development"
            : "Chat feature is unavailable"
        }
      >
        <MessageCircle className="h-6 w-6" aria-hidden="true" />
      </Button>
      <UnavailableChatUI
        isOpen={isUnavailableChatOpen}
        onClose={() => setIsUnavailableChatOpen(false)}
        isFeatureDisabled={isFeatureDisabled}
      />
    </>
  );
});
