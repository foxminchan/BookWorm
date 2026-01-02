"use client";

import { useMemo } from "react";

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

function parseMarkdown(content: string) {
  return content
    .split("\n")
    .map((line) => {
      // Headings
      if (line.startsWith("# ")) {
        return {
          tag: "h1",
          text: line.replace("# ", ""),
          className: "text-3xl font-serif font-bold mt-6 mb-4",
        };
      }
      if (line.startsWith("## ")) {
        return {
          tag: "h2",
          text: line.replace("## ", ""),
          className: "text-2xl font-serif font-semibold mt-5 mb-3",
        };
      }
      if (line.startsWith("### ")) {
        return {
          tag: "h3",
          text: line.replace("### ", ""),
          className: "text-lg font-serif font-semibold mt-4 mb-2",
        };
      }
      // List items
      if (line.startsWith("- ")) {
        return {
          tag: "li",
          text: line.replace("- ", ""),
          className: "ml-4 text-sm",
        };
      }
      // Bold and italic
      if (line.trim()) {
        const processedText = line
          .replace(
            /\*\*([^*]+)\*\*/g,
            '<strong class="font-semibold">$1</strong>',
          )
          .replace(/\*([^*]+)\*/g, '<em class="italic">$1</em>');
        return {
          tag: "p",
          text: processedText,
          className: "text-sm mb-3 leading-relaxed",
        };
      }
      return null;
    })
    .filter(Boolean);
}

export function PolicyDialog({
  policyType,
  isOpen,
  onOpenChange,
  content,
}: PolicyDialogProps) {
  const parsedContent = useMemo(() => parseMarkdown(content), [content]);
  const title =
    policyType === "privacy" ? "Privacy Policy" : "Terms of Service";

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[80vh] max-w-2xl overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="font-serif text-2xl">{title}</DialogTitle>
        </DialogHeader>
        <div className="space-y-2">
          {parsedContent.map((item, idx) => {
            if (!item) return null;
            const Element = item.tag as any;
            return item.tag === "li" ? (
              <Element key={idx} className={item.className}>
                <div className="flex gap-2">
                  <span>•</span>
                  <span>{item.text}</span>
                </div>
              </Element>
            ) : (
              <Element
                key={idx}
                className={item.className}
                dangerouslySetInnerHTML={{ __html: item.text }}
              />
            );
          })}
        </div>
      </DialogContent>
    </Dialog>
  );
}
