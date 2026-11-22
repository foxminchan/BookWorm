import type { CopilotSidebarProps } from "@copilotkit/react-core/v2";

/**
 * CopilotKit v2 slot-based theme aligned with BookWorm's shadcn/ui design
 * tokens and AI Elements styling patterns (inspired by CopilotKit/shadify).
 */
export const copilotSidebarTheme: Partial<CopilotSidebarProps> = {
  className: "bg-background text-foreground border-l border-border shadow-xl",
  header: {
    className: "border-b border-border bg-card text-card-foreground",
  },
  messageView: {
    className: "bg-background px-3 py-2",
    toolCallDisplay: "grouped",
    userMessage: {
      className: "items-end",
      messageRenderer: {
        className:
          "bg-primary text-primary-foreground rounded-xl px-3 py-2 text-sm leading-relaxed shadow-xs",
      },
    },
    assistantMessage: {
      className: "text-foreground",
    },
  },
  input: {
    className: "border-t border-border bg-background",
    textArea: {
      className:
        "bg-muted/50 text-foreground placeholder:text-muted-foreground border-input focus-visible:border-ring rounded-xl text-sm",
    },
    sendButton: {
      className:
        "bg-primary text-primary-foreground hover:bg-primary/90 rounded-lg",
    },
  },
  suggestionView: {
    className: "bg-transparent gap-1.5",
    suggestion: {
      className:
        "rounded-full border border-border bg-secondary text-secondary-foreground hover:bg-accent hover:text-accent-foreground text-xs transition-colors",
    },
  },
};
