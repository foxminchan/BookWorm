import { useCallback, useEffect, useRef, useState } from "react";

type UseDelayedToggleOptions = {
  /** Delay in ms before closing on mouse leave. Defaults to 150. */
  readonly closeDelay?: number;
};

type UseDelayedToggleReturn = {
  readonly isOpen: boolean;
  readonly open: () => void;
  readonly close: () => void;
  readonly toggle: () => void;
  readonly handleMouseEnter: () => void;
  readonly handleMouseLeave: () => void;
};

/**
 * Manages a boolean toggle with a delayed close on mouse leave.
 * Useful for hover-based dropdowns that need a grace period before closing.
 */
export function useDelayedToggle({
  closeDelay = 150,
}: UseDelayedToggleOptions = {}): UseDelayedToggleReturn {
  const [isOpen, setIsOpen] = useState(false);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);

  const clearPendingClose = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
      timeoutRef.current = null;
    }
  }, []);

  const open = useCallback(() => {
    clearPendingClose();
    setIsOpen(true);
  }, [clearPendingClose]);

  const close = useCallback(() => {
    clearPendingClose();
    setIsOpen(false);
  }, [clearPendingClose]);

  const toggle = useCallback(() => {
    setIsOpen((prev) => !prev);
  }, []);

  const handleMouseEnter = useCallback(() => {
    clearPendingClose();
    setIsOpen(true);
  }, [clearPendingClose]);

  const handleMouseLeave = useCallback(() => {
    timeoutRef.current = setTimeout(() => {
      setIsOpen(false);
    }, closeDelay);
  }, [closeDelay]);

  useEffect(() => {
    return clearPendingClose;
  }, [clearPendingClose]);

  return { isOpen, open, close, toggle, handleMouseEnter, handleMouseLeave };
}
