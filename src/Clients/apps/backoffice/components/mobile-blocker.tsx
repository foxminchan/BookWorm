"use client";

import { useEffect, useState } from "react";

import { Monitor } from "lucide-react";

const MOBILE_BREAKPOINT = 1024;

export function MobileBlocker({ children }: { children: React.ReactNode }) {
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    // Use matchMedia for efficient, threshold-based detection instead of
    // listening to every resize pixel change
    const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`);

    const handleChange = (e: MediaQueryListEvent | MediaQueryList) => {
      setIsMobile(e.matches);
    };

    handleChange(mql);
    mql.addEventListener("change", handleChange);

    return () => mql.removeEventListener("change", handleChange);
  }, []);

  if (isMobile) {
    return (
      <div className="flex h-screen items-center justify-center bg-linear-to-br from-gray-50 to-gray-100 p-6 dark:from-gray-900 dark:to-gray-950">
        <div className="max-w-md text-center">
          <div className="mb-6 flex justify-center">
            <div className="rounded-full bg-blue-100 p-4 dark:bg-blue-900">
              <Monitor className="h-12 w-12 text-blue-600 dark:text-blue-400" />
            </div>
          </div>
          <h1 className="mb-3 text-2xl font-bold text-gray-900 dark:text-gray-100">
            Desktop Only
          </h1>
          <p className="mb-2 text-gray-600 dark:text-gray-400">
            The Admin Portal is only available on desktop devices.
          </p>
          <p className="text-sm text-gray-500 dark:text-gray-500">
            Please access this portal from a device with a screen width of at
            least 1024px for the best experience.
          </p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
