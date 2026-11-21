"use client";

export default function GlobalError({
  reset,
}: Readonly<{
  error: Error & { digest?: string };
  reset: () => void;
}>) {
  return (
    <html lang="en">
      <body className="bg-background text-foreground flex min-h-screen items-center justify-center px-4 font-sans antialiased">
        <div className="w-full max-w-md space-y-6 text-center">
          <h1 className="text-2xl font-bold">Application Error</h1>
          <p className="text-muted-foreground text-sm">
            A critical error occurred. Please reload or try again.
          </p>
          <button
            type="button"
            onClick={reset}
            className="bg-primary text-primary-foreground rounded-md px-4 py-2 text-sm font-medium"
          >
            Try Again
          </button>
        </div>
      </body>
    </html>
  );
}
