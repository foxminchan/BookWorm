"use client";

import { useCallback, useEffect, useRef } from "react";

export type DebounceOptions = {
  leading?: boolean;
  trailing?: boolean;
};

/**
 * Returns a debounced version of `callback` supporting `leading` and `trailing`
 * invocation modes. Pending timers are cleared on unmount.
 */
export function useDebounceCallback<T extends (...args: never[]) => unknown>(
  callback: T,
  delay: number,
  options: DebounceOptions = {},
): (...args: Parameters<T>) => void {
  const { leading = false, trailing = true } = options;
  const callbackRef = useRef(callback);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const lastArgsRef = useRef<Parameters<T> | null>(null);

  useEffect(() => {
    callbackRef.current = callback;
  }, [callback]);

  useEffect(() => {
    return () => {
      if (timerRef.current !== null) {
        clearTimeout(timerRef.current);
      }
    };
  }, []);

  return useCallback(
    (...args: Parameters<T>) => {
      const isInvokingLeading = leading && timerRef.current === null;
      lastArgsRef.current = args;

      if (timerRef.current !== null) {
        clearTimeout(timerRef.current);
      }

      if (isInvokingLeading) {
        callbackRef.current(...args);
        lastArgsRef.current = null;
      }

      timerRef.current = setTimeout(() => {
        timerRef.current = null;
        if (trailing && lastArgsRef.current !== null) {
          const pendingArgs = lastArgsRef.current;
          lastArgsRef.current = null;
          callbackRef.current(...pendingArgs);
        }
      }, delay);
    },
    [delay, leading, trailing],
  );
}
