"use client";

import { forwardRef, useImperativeHandle, useState } from "react";
import { CopilotChat } from "@copilotkit/react-ui";
import "@copilotkit/react-ui/styles.css";
import { AlertCircle, Copy, MessageCircle, Paperclip, X } from "lucide-react";
import { Card, CardHeader, CardContent } from "@workspace/ui/components/card";
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
      <button
        onClick={() => setIsOpen(true)}
        className="fixed bottom-6 right-6 z-50 hidden md:flex w-12 h-12 rounded-full bg-primary text-primary-foreground shadow-lg hover:bg-primary/90 transition-colors items-center justify-center"
        aria-label="Open chat"
      >
        <MessageCircle className="w-6 h-6" aria-hidden="true" />
      </button>
    );
  }

  return (
    <div className="fixed inset-0 z-50 hidden md:flex items-end justify-end p-6">
      <div
        className="fixed inset-0 bg-black/20"
        onClick={() => setIsOpen(false)}
      />
      <div className="relative z-10">
        <Card className="w-96 sm:w-105 h-150 flex flex-col shadow-2xl border-secondary/30 rounded-2xl overflow-hidden animate-in fade-in slide-in-from-bottom-4 duration-300">
          <CardHeader className="bg-linear-to-br from-primary via-primary to-primary/90 text-primary-foreground p-6 flex flex-row items-center justify-between gap-3 border-b border-primary-foreground/10">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-primary-foreground/20 flex items-center justify-center">
                <MessageCircle className="w-5 h-5" />
              </div>
              <h2 className="font-semibold">BookWorm Literary Guide</h2>
            </div>
            <button
              onClick={() => setIsOpen(false)}
              className="p-1 hover:bg-primary-foreground/20 rounded-full transition-colors"
              aria-label="Close chat"
            >
              <X className="w-5 h-5" />
            </button>
          </CardHeader>
          <CardContent className="flex-1 overflow-auto">
            <CopilotChat
              labels={{
                title: "BookWorm Literary Guide",
                placeholder: "Ask about a book...",
              }}
              icons={{
                activityIcon: <AlertCircle className="w-6 h-6" />,
                copyIcon: <Copy className="w-4 h-4" />,
                headerCloseIcon: <X className="w-5 h-5" />,
                sendIcon: <Paperclip className="w-5 h-5 rotate-45" />,
                closeIcon: <X className="w-5 h-5" />,
                openIcon: <MessageCircle className="w-6 h-6" />,
                stopIcon: <X className="w-4 h-4" />,
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
}: {
  isOpen: boolean;
  onClose: () => void;
}) => {
  if (!isOpen) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-50 hidden md:flex items-end justify-end p-6">
      <div className="fixed inset-0 bg-black/20" onClick={onClose} />
      <Card className="w-96 sm:w-105 h-150 flex flex-col shadow-2xl border-secondary/30 rounded-2xl overflow-hidden animate-in fade-in slide-in-from-bottom-4 duration-300 relative z-10">
        <CardHeader className="bg-linear-to-br from-primary via-primary to-primary/90 text-primary-foreground p-6 flex flex-row items-center justify-between gap-3 border-b border-primary-foreground/10">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-primary-foreground/20 flex items-center justify-center">
              <MessageCircle className="w-5 h-5" />
            </div>
            <div>
              <h2 className="font-semibold">BookWorm Literary Guide</h2>
              <p className="text-xs opacity-90">Chat unavailable</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 hover:bg-primary-foreground/20 rounded-full transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </CardHeader>

        <CardContent className="flex-1 flex flex-col items-center justify-center p-6 gap-4">
          <div className="w-16 h-16 rounded-full bg-red-100 flex items-center justify-center">
            <AlertCircle className="w-8 h-8 text-red-600" />
          </div>
          <div className="text-center">
            <h3 className="font-semibold text-lg mb-2">Chat Unavailable</h3>
            <p className="text-sm text-muted-foreground">
              The chat feature is currently unavailable. Please try again later.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export const ChatBot = forwardRef<ChatBotRef>(function ChatBot(_, ref) {
  const hasGateway = !!(
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP
  );
  const [isUnavailableChatOpen, setIsUnavailableChatOpen] = useState(false);

  useImperativeHandle(ref, () => ({
    openChat: () => {
      if (!hasGateway) {
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

  if (!hasGateway) {
    return (
      <>
        <button
          onClick={() => setIsUnavailableChatOpen(true)}
          className="fixed bottom-6 right-6 z-50 hidden md:flex w-12 h-12 rounded-full bg-primary text-primary-foreground shadow-lg hover:bg-primary/90 transition-colors items-center justify-center"
          aria-label="Open chat - feature currently unavailable"
          title="Chat feature is unavailable"
        >
          <MessageCircle className="w-6 h-6" aria-hidden="true" />
        </button>
        <UnavailableChatUI
          isOpen={isUnavailableChatOpen}
          onClose={() => setIsUnavailableChatOpen(false)}
        />
      </>
    );
  }

  return <ChatBotContent ref={ref} />;
});
