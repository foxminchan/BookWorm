"use client";

import type React from "react";

import { Component, type ReactNode } from "react";
import { AlertCircle } from "lucide-react";
import { Button } from "@workspace/ui/components/button";

type ErrorBoundaryProps = {
  children: ReactNode;
  fallback?: ReactNode;
};

type ErrorBoundaryState = {
  hasError: boolean;
  error?: Error;
};

export class ErrorBoundary extends Component<
  ErrorBoundaryProps,
  ErrorBoundaryState
> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error("[v0] Error caught by boundary:", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div className="min-h-100 flex items-center justify-center p-8">
          <div className="text-center space-y-4 max-w-md">
            <div className="size-16 bg-destructive/10 rounded-full flex items-center justify-center mx-auto">
              <AlertCircle className="size-8 text-destructive" />
            </div>
            <h2 className="text-2xl font-serif font-medium">
              Something went wrong
            </h2>
            <p className="text-muted-foreground">
              We encountered an error while loading this content. Please try
              again.
            </p>
            <Button
              onClick={() => this.setState({ hasError: false })}
              className="rounded-full"
            >
              Try Again
            </Button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
