"use client";

import {
  type UseMutationOptions,
  type UseMutationResult,
  useMutation,
} from "@tanstack/react-query";

/**
 * Executes `useMutation` while composing internal lifecycle callbacks
 * (such as cache invalidation) with any caller-supplied `options`.
 */
export function useMutationFromOptions<
  TData = unknown,
  TError = Error,
  TVariables = void,
  TOnMutateResult = unknown,
>(
  baseOptions: UseMutationOptions<TData, TError, TVariables, TOnMutateResult>,
  overrides?: UseMutationOptions<TData, TError, TVariables, TOnMutateResult>,
): UseMutationResult<TData, TError, TVariables, TOnMutateResult> {
  return useMutation({
    ...baseOptions,
    ...overrides,
    onSuccess:
      baseOptions.onSuccess || overrides?.onSuccess
        ? (data, variables, onMutateResult, context) => {
            baseOptions.onSuccess?.(data, variables, onMutateResult, context);
            overrides?.onSuccess?.(data, variables, onMutateResult, context);
          }
        : undefined,
    onError:
      baseOptions.onError || overrides?.onError
        ? (error, variables, onMutateResult, context) => {
            baseOptions.onError?.(error, variables, onMutateResult, context);
            overrides?.onError?.(error, variables, onMutateResult, context);
          }
        : undefined,
    onSettled:
      baseOptions.onSettled || overrides?.onSettled
        ? (data, error, variables, onMutateResult, context) => {
            baseOptions.onSettled?.(
              data,
              error,
              variables,
              onMutateResult,
              context,
            );
            overrides?.onSettled?.(
              data,
              error,
              variables,
              onMutateResult,
              context,
            );
          }
        : undefined,
  });
}
