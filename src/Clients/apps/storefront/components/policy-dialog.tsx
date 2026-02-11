"use client";

import { type ReactNode, useMemo } from "react";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@workspace/ui/components/dialog";

type PolicyDialogProps = {
  policyType: "privacy" | "terms";
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  content: string;
};

type ParsedElement = {
  key: number;
  tag: keyof React.JSX.IntrinsicElements;
  content: ReactNode;
  className: string;
};

const POLICY_TITLES: Record<PolicyDialogProps["policyType"], string> = {
  privacy: "Privacy Policy",
  terms: "Terms of Service",
};

function parseInlineFormatting(text: string): ReactNode {
  const regex = /\*\*([^*]+)\*\*|\*([^*]+)\*/g;
  const parts: ReactNode[] = [];
  let lastIndex = 0;
  let key = 0;
  let match;

  while ((match = regex.exec(text)) !== null) {
    if (match.index > lastIndex) {
      parts.push(text.slice(lastIndex, match.index));
    }

    if (match[1]) {
      parts.push(
        <strong key={key++} className="font-semibold">
          {match[1]}
        </strong>,
      );
    } else if (match[2]) {
      parts.push(
        <em key={key++} className="italic">
          {match[2]}
        </em>,
      );
    }

    lastIndex = regex.lastIndex;
  }

  if (lastIndex < text.length) {
    parts.push(text.slice(lastIndex));
  }

  return parts.length <= 1 ? (parts[0] ?? text) : parts;
}

function parseLine(line: string, lineIndex: number): ParsedElement | null {
  if (line.startsWith("### ")) {
    return {
      key: lineIndex,
      tag: "h3",
      content: parseInlineFormatting(line.slice(4)),
      className: "text-lg font-serif font-semibold mt-4 mb-2",
    };
  }

  if (line.startsWith("## ")) {
    return {
      key: lineIndex,
      tag: "h2",
      content: parseInlineFormatting(line.slice(3)),
      className: "text-2xl font-serif font-semibold mt-5 mb-3",
    };
  }

  if (line.startsWith("# ")) {
    return {
      key: lineIndex,
      tag: "h1",
      content: parseInlineFormatting(line.slice(2)),
      className: "text-3xl font-serif font-bold mt-6 mb-4",
    };
  }

  if (line.startsWith("- ")) {
    return {
      key: lineIndex,
      tag: "div",
      content: (
        <span className="flex gap-2">
          <span aria-hidden="true">•</span>
          <span>{parseInlineFormatting(line.slice(2))}</span>
        </span>
      ),
      className: "ml-4 text-sm",
    };
  }

  if (line.trim()) {
    return {
      key: lineIndex,
      tag: "p",
      content: parseInlineFormatting(line),
      className: "text-sm mb-3 leading-relaxed",
    };
  }

  return null;
}

function parseMarkdown(content: string): ParsedElement[] {
  return content
    .split("\n")
    .map((line, index) => parseLine(line, index))
    .filter((item): item is ParsedElement => item !== null);
}

export function PolicyDialog({
  policyType,
  isOpen,
  onOpenChange,
  content,
}: Readonly<PolicyDialogProps>) {
  const parsedContent = useMemo(() => parseMarkdown(content), [content]);

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[80vh] max-w-2xl overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="font-serif text-2xl">
            {POLICY_TITLES[policyType]}
          </DialogTitle>
        </DialogHeader>
        <div className="space-y-2">
          {parsedContent.map((item) => {
            const Tag = item.tag;
            return (
              <Tag key={item.key} className={item.className}>
                {item.content}
              </Tag>
            );
          })}
        </div>
      </DialogContent>
    </Dialog>
  );
}
