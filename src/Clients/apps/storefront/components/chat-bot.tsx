"use client";

import { forwardRef, useImperativeHandle, useState } from "react";

import dynamic from "next/dynamic";
import Link from "next/link";

import { useAtomValue } from "jotai";
import { AlertCircle, LogIn, MessageCircle, X } from "lucide-react";
import { match } from "ts-pattern";

import { Button } from "@workspace/ui/components/button";
import { Card, CardContent, CardHeader } from "@workspace/ui/components/card";

import { isCopilotEnabledAtom } from "@/atoms/feature-flags-atom";
import { env } from "@/env.mjs";
import { useSession } from "@/lib/auth-client";

const ChatBotContent = dynamic(
  () => import("./chat-bot-content").then((m) => m.ChatBotContent),
  { ssr: false },
);

export type ChatBotRef = {
  openChat: () => void;
};

type UnavailableReason = "disabled" | "unauthenticated" | "unavailable";

const UnavailableChatUI = ({
  isOpen,
  onClose,
  reason = "unavailable",
}: {
  isOpen: boolean;
  onClose: () => void;
  reason?: UnavailableReason;
}) => {
  if (!isOpen) {
    return null;
  }

  const isFeatureDisabled = reason === "disabled";
  const isUnauthenticated = reason === "unauthenticated";

  const subtitle = match(reason)
    .with("disabled", () => "Coming Soon")
    .with("unauthenticated", () => "Sign in required")
    .with("unavailable", () => "Chat unavailable")
    .exhaustive();

  const heading = match(reason)
    .with("disabled", () => "Feature in Development")
    .with("unauthenticated", () => "Sign in to Chat")
    .with("unavailable", () => "Chat Unavailable")
    .exhaustive();

  const description = match(reason)
    .with(
      "disabled",
      () =>
        "Our AI chat assistant is currently under development and will be available soon. Stay tuned!",
    )
    .with(
      "unauthenticated",
      () =>
        "Please sign in to your account to chat with our AI literary assistant.",
    )
    .with(
      "unavailable",
      () =>
        "The chat feature is currently unavailable. Please try again later.",
    )
    .exhaustive();

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
              <p className="text-xs opacity-90">{subtitle}</p>
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
            {isUnauthenticated ? (
              <LogIn className="h-8 w-8 text-orange-600 dark:text-orange-400" />
            ) : (
              <AlertCircle className="h-8 w-8 text-orange-600 dark:text-orange-400" />
            )}
          </div>
          <div className="text-center">
            <h3 className="mb-2 text-lg font-semibold">{heading}</h3>
            <p className="text-muted-foreground text-sm">{description}</p>
            {isUnauthenticated && !isFeatureDisabled && (
              <Button asChild size="sm" className="mt-4 gap-2">
                <Link href="/login">
                  <LogIn className="h-4 w-4" aria-hidden="true" />
                  Sign In
                </Link>
              </Button>
            )}
          </div>
        </CardContent>
      </Card>
    </dialog>
  );
};

export const ChatBot = forwardRef<ChatBotRef>(function ChatBot(_, ref) {
  const isCopilotEnabled = useAtomValue(isCopilotEnabledAtom);
  const { data: session } = useSession();
  const isAuthenticated = !!session?.user;
  const hasGateway = !!(
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP
  );
  const [isUnavailableChatOpen, setIsUnavailableChatOpen] = useState(false);

  const isFeatureDisabled = !isCopilotEnabled;
  const canShowChat = isCopilotEnabled && hasGateway && isAuthenticated;
  let unavailableReason: UnavailableReason = "unavailable";
  if (isFeatureDisabled) {
    unavailableReason = "disabled";
  } else if (hasGateway && !isAuthenticated) {
    unavailableReason = "unauthenticated";
  }

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
        aria-label={match(unavailableReason)
          .with(
            "disabled",
            () => "Chat feature coming soon - click to learn more",
          )
          .with("unauthenticated", () => "Open chat - sign in required")
          .with(
            "unavailable",
            () => "Open chat - feature currently unavailable",
          )
          .exhaustive()}
        title={match(unavailableReason)
          .with("disabled", () => "Feature in development")
          .with("unauthenticated", () => "Sign in to use chat")
          .with("unavailable", () => "Chat feature is unavailable")
          .exhaustive()}
      >
        <MessageCircle className="h-6 w-6" aria-hidden="true" />
      </Button>
      <UnavailableChatUI
        isOpen={isUnavailableChatOpen}
        onClose={() => setIsUnavailableChatOpen(false)}
        reason={unavailableReason}
      />
    </>
  );
});
