"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";

type RateLimitConfig = {
  maxRequests: number;
  windowMs: number;
  throttleMs?: number;
};

type RateLimitState = {
  requests: number[];
  isThrottling: boolean;
  nextAllowedTime: number;
};

const DEFAULT_CONFIG: RateLimitConfig = {
  maxRequests: 10, // 10 requests
  windowMs: 60000, // per minute
  throttleMs: 2000, // minimum 2s between requests
};

export function useRateLimit(config: Partial<RateLimitConfig> = {}) {
  const fullConfig = useMemo(
    () => ({ ...DEFAULT_CONFIG, ...config }),
    [config.maxRequests, config.windowMs, config.throttleMs],
  );
  const [state, setState] = useState<RateLimitState>({
    requests: [],
    isThrottling: false,
    nextAllowedTime: 0,
  });
  const throttleTimeoutRef = useRef<NodeJS.Timeout | undefined>(undefined);

  // Clean up old requests outside the time window
  const cleanupOldRequests = useCallback(
    (now: number, requests: number[]) => {
      return requests.filter(
        (timestamp) => now - timestamp < fullConfig.windowMs,
      );
    },
    [fullConfig.windowMs],
  );

  const checkRateLimit = useCallback((): {
    allowed: boolean;
    reason?: "rate-limit" | "throttle";
    remainingRequests?: number;
    resetIn?: number;
  } => {
    const now = Date.now();
    const activeRequests = cleanupOldRequests(now, state.requests);

    // Check throttling
    if (fullConfig.throttleMs && now < state.nextAllowedTime) {
      return {
        allowed: false,
        reason: "throttle",
        resetIn: state.nextAllowedTime - now,
      };
    }

    // Check rate limit
    if (activeRequests.length >= fullConfig.maxRequests) {
      const oldestRequest = Math.min(...activeRequests);
      const resetIn = fullConfig.windowMs - (now - oldestRequest);
      return {
        allowed: false,
        reason: "rate-limit",
        remainingRequests: 0,
        resetIn,
      };
    }

    return {
      allowed: true,
      remainingRequests: fullConfig.maxRequests - activeRequests.length,
    };
  }, [
    cleanupOldRequests,
    fullConfig.maxRequests,
    fullConfig.throttleMs,
    fullConfig.windowMs,
    state.nextAllowedTime,
    state.requests,
  ]);

  const recordRequest = useCallback(() => {
    const now = Date.now();
    const activeRequests = cleanupOldRequests(now, state.requests);

    setState({
      requests: [...activeRequests, now],
      isThrottling: true,
      nextAllowedTime: fullConfig.throttleMs ? now + fullConfig.throttleMs : 0,
    });

    // Reset throttling flag after throttle period
    if (fullConfig.throttleMs) {
      if (throttleTimeoutRef.current) {
        clearTimeout(throttleTimeoutRef.current);
      }
      throttleTimeoutRef.current = setTimeout(() => {
        setState((prev) => ({ ...prev, isThrottling: false }));
      }, fullConfig.throttleMs);
    }
  }, [cleanupOldRequests, fullConfig.throttleMs, state.requests]);

  const attemptRequest = useCallback(
    async <T>(
      fn: () => Promise<T>,
    ): Promise<
      | { success: true; data: T }
      | { success: false; error: string; resetIn?: number }
    > => {
      const limitCheck = checkRateLimit();

      if (!limitCheck.allowed) {
        const message =
          limitCheck.reason === "throttle"
            ? `Please wait ${Math.ceil((limitCheck.resetIn || 0) / 1000)}s before sending another message`
            : `Rate limit exceeded. Try again in ${Math.ceil((limitCheck.resetIn || 0) / 1000)}s`;

        return {
          success: false,
          error: message,
          resetIn: limitCheck.resetIn,
        };
      }

      try {
        recordRequest();
        const data = await fn();
        return { success: true, data };
      } catch (error) {
        return {
          success: false,
          error: error instanceof Error ? error.message : "Request failed",
        };
      }
    },
    [checkRateLimit, recordRequest],
  );

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (throttleTimeoutRef.current) {
        clearTimeout(throttleTimeoutRef.current);
      }
    };
  }, []);

  const status = checkRateLimit();

  return {
    attemptRequest,
    checkRateLimit,
    isRateLimited: !status.allowed && status.reason === "rate-limit",
    isThrottling: state.isThrottling,
    remainingRequests: status.remainingRequests || 0,
    resetIn: status.resetIn,
  };
}
