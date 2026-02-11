"use client";

import { useEffect, useState } from "react";

import { Monitor } from "lucide-react";

const MOBILE_BREAKPOINT = 1024;

export function MobileBlocker({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const mql = globalThis.matchMedia(
      `(max-width: ${MOBILE_BREAKPOINT - 1}px)`,
    );

    const handleChange = (e: MediaQueryListEvent | MediaQueryList) => {
      setIsMobile(e.matches);
    };

    handleChange(mql);
    mql.addEventListener("change", handleChange);

    return () => mql.removeEventListener("change", handleChange);
  }, []);

  if (isMobile) {
    return (
      <div className="from-background to-muted/20 flex h-screen items-center justify-center bg-linear-to-br p-6">
        <div className="max-w-md text-center">
          <div className="mb-6 flex justify-center">
            <div className="bg-primary/10 rounded-full p-4">
              <Monitor className="text-primary h-12 w-12" />
            </div>
          </div>
          <h1 className="text-foreground mb-3 text-2xl font-bold">
            Desktop Only
          </h1>
          <p className="text-muted-foreground mb-2">
            The Admin Portal is only available on desktop devices.
          </p>
          <p className="text-muted-foreground/70 text-sm">
            Please access this portal from a device with a screen width of at
            least {MOBILE_BREAKPOINT}px for the best experience.
          </p>
        </div>
      </div>
    );
  }

  return children;
}
