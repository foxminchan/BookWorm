"use client";

import { forwardRef, useImperativeHandle, useState } from "react";

import { CopilotChat } from "@copilotkit/react-ui";
import "@copilotkit/react-ui/styles.css";
import { useAtomValue } from "jotai";
import { AlertCircle, Copy, MessageCircle, Paperclip, X } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import { Card, CardContent, CardHeader } from "@workspace/ui/components/card";

import { isCopilotEnabledAtom } from "@/atoms/feature-flags-atom";
import { env } from "@/env.mjs";

export type ChatBotRef = {
  openChat: () => void;
};

const ChatBotContent = forwardRef<ChatBotRef>(function ChatBotContent(_, ref) {
  const [isOpen, setIsOpen] = useState(false);

  useImperativeHandle(ref, () => ({
    openChat: () => {
      setIsOpen(true);
    },
  }));

  if (!isOpen) {
    return (
      <Button
        onClick={() => setIsOpen(true)}
        size="icon"
        className="fixed right-6 bottom-6 z-50 hidden h-12 w-12 rounded-full shadow-lg md:flex"
        aria-label="Open chat"
      >
        <MessageCircle className="h-6 w-6" aria-hidden="true" />
      </Button>
    );
  }

  return (
    <div className="fixed inset-0 z-50 hidden items-end justify-end p-6 md:flex">
      <div
        className="fixed inset-0 bg-black/20"
        onClick={() => setIsOpen(false)}
      />
      <div className="relative z-10">
        <Card className="border-secondary/30 animate-in fade-in slide-in-from-bottom-4 flex h-150 w-96 flex-col overflow-hidden rounded-2xl shadow-2xl duration-300 sm:w-105">
          <CardHeader className="from-primary via-primary to-primary/90 text-primary-foreground border-primary-foreground/10 flex flex-row items-center justify-between gap-3 border-b bg-linear-to-br p-6">
            <div className="flex items-center gap-3">
              <div className="bg-primary-foreground/20 flex h-10 w-10 items-center justify-center rounded-full">
                <MessageCircle className="h-5 w-5" />
              </div>
              <h2 className="font-semibold">BookWorm Literary Guide</h2>
            </div>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setIsOpen(false)}
              className="hover:bg-primary-foreground/20 h-8 w-8 rounded-full"
              aria-label="Close chat"
            >
              <X className="h-5 w-5" />
            </Button>
          </CardHeader>
          <CardContent className="flex-1 overflow-auto">
            <CopilotChat
              labels={{
                title: "BookWorm Literary Guide",
                placeholder: "Ask about a book...",
              }}
              icons={{
                activityIcon: <AlertCircle className="h-6 w-6" />,
                copyIcon: <Copy className="h-4 w-4" />,
                headerCloseIcon: <X className="h-5 w-5" />,
                sendIcon: <Paperclip className="h-5 w-5 rotate-45" />,
                closeIcon: <X className="h-5 w-5" />,
                openIcon: <MessageCircle className="h-6 w-6" />,
                stopIcon: <X className="h-4 w-4" />,
              }}
            />
          </CardContent>
        </Card>
      </div>
    </div>
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
    <div className="fixed inset-0 z-50 hidden items-end justify-end p-6 md:flex">
      <div className="fixed inset-0 bg-black/20" onClick={onClose} />
      <Card className="border-secondary/30 animate-in fade-in slide-in-from-bottom-4 relative z-10 flex h-150 w-96 flex-col overflow-hidden rounded-2xl shadow-2xl duration-300 sm:w-105">
        <CardHeader className="from-primary via-primary to-primary/90 text-primary-foreground border-primary-foreground/10 flex flex-row items-center justify-between gap-3 border-b bg-linear-to-br p-6">
          <div className="flex items-center gap-3">
            <div className="bg-primary-foreground/20 flex h-10 w-10 items-center justify-center rounded-full">
              <MessageCircle className="h-5 w-5" />
            </div>
            <div>
              <h2 className="font-semibold">BookWorm Literary Guide</h2>
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
          >
            <X className="h-5 w-5" />
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
    </div>
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
      if (!canShowChat) {
        setIsUnavailableChatOpen(true);
      } else {
        const chatButton = document.querySelector(
          "[data-copilot-chat-trigger]",
        );
        if (chatButton instanceof HTMLElement) {
          chatButton.click();
        }
      }
    },
  }));

  if (!canShowChat) {
    return (
      <>
        <Button
          onClick={() => setIsUnavailableChatOpen(true)}
          size="icon"
          className="fixed right-6 bottom-6 z-50 hidden h-12 w-12 rounded-full shadow-lg md:flex"
          aria-label={
            isFeatureDisabled
              ? "Chat feature coming soon"
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
  }

  return <ChatBotContent ref={ref} />;
});
