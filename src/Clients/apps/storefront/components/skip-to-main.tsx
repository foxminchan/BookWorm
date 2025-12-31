export function SkipToMain() {
  return (
    <a
      href="#main-content"
      className="sr-only focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-999 focus:p-3 focus:bg-primary focus:text-primary-foreground focus:rounded focus:font-semibold"
    >
      Skip to main content
    </a>
  );
}
