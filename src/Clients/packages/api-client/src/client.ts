import axios, {
  AxiosHeaders,
  type AxiosInstance,
  type AxiosResponse,
  HttpStatusCode,
} from "axios";
import pRetry from "p-retry";

import axiosConfig from "./config";
import type { AxiosRequestConfig } from "./global";
import { HttpHeaders } from "./headers";

export { HttpHeaders } from "./headers";

const DEFAULT_MAX_RETRIES = Number(process.env.NEXT_PUBLIC_MAX_RETRIES) || 5;
const MAX_RETRY_AFTER_MS = 60_000;
const IDEMPOTENT_HTTP_METHODS = new Set([
  "get",
  "head",
  "options",
  "put",
  "delete",
  "query",
]);

/**
 * A function that resolves the current access token, or `null` if unavailable.
 * May be synchronous or asynchronous — the request interceptor awaits the result.
 * Used by the request interceptor to attach `Authorization: Bearer <token>`.
 */
export type AccessTokenProvider = () => Promise<string | null> | string | null;

export type ConditionalResponse<T> =
  | { data: T; etag?: string; notModified: false }
  | { data: null; etag?: string; notModified: true };

export function formatETag(version?: number | null): string | undefined {
  if (version === undefined || version === null || !Number.isFinite(version)) {
    return undefined;
  }

  return `"${version}"`;
}

export function getHeaderValue(
  headers: AxiosResponse["headers"],
  name: string,
): string | undefined {
  const raw =
    headers instanceof AxiosHeaders
      ? headers.get(name)
      : (headers[name] ?? headers[name.toLowerCase()]);

  if (typeof raw !== "string") {
    return undefined;
  }

  const trimmed = raw.trim();
  return trimmed.length > 0 ? trimmed : undefined;
}

export function extractETag(
  headers: AxiosResponse["headers"],
): string | undefined {
  return getHeaderValue(headers, HttpHeaders.ETag);
}

/**
 * Parses an HTTP `Retry-After` header (delay-seconds or HTTP-date) into
 * milliseconds, clamped to {@link MAX_RETRY_AFTER_MS}.
 */
export function parseRetryAfterMs(
  headers: AxiosResponse["headers"],
  now = Date.now(),
): number | undefined {
  const value = getHeaderValue(headers, HttpHeaders.RetryAfter);
  if (!value) {
    return undefined;
  }

  if (/^\d+$/.test(value)) {
    const seconds = Number(value);
    return Math.min(seconds * 1000, MAX_RETRY_AFTER_MS);
  }

  const retryAtMs = Date.parse(value);
  if (Number.isNaN(retryAtMs)) {
    return undefined;
  }

  return Math.min(Math.max(0, retryAtMs - now), MAX_RETRY_AFTER_MS);
}

function waitForDelay(ms: number, signal?: AbortSignal): Promise<void> {
  if (ms <= 0) {
    return Promise.resolve();
  }

  signal?.throwIfAborted();

  return new Promise((resolve, reject) => {
    const onAbort = () => {
      clearTimeout(timer);
      reject(signal?.reason);
    };

    const timer = setTimeout(() => {
      signal?.removeEventListener("abort", onAbort);
      resolve();
    }, ms);

    signal?.addEventListener("abort", onAbort, { once: true });
  });
}

export function isPreconditionFailedError(error: unknown): boolean {
  return (
    axios.isAxiosError(error) &&
    error.response?.status === HttpStatusCode.PreconditionFailed
  );
}

export default class ApiClient {
  private readonly client: AxiosInstance;
  private readonly maxRetries: number;
  private tokenProvider: AccessTokenProvider | null = null;

  constructor(config = axiosConfig, maxRetries = DEFAULT_MAX_RETRIES) {
    const axiosConfigs = "baseURL" in config ? config : axiosConfig;
    this.maxRetries = maxRetries;

    this.client = axios.create({
      ...axiosConfigs,
      headers: {
        ...(axiosConfigs.headers as Record<string, string>),
      },
    });

    this.client.interceptors.request.use(async (requestConfig) => {
      const token = await this.tokenProvider?.();
      if (token) {
        requestConfig.headers.setAuthorization(`Bearer ${token}`);
      }
      return requestConfig;
    });
  }

  private shouldRetryRequest(error: unknown): boolean {
    if (!axios.isAxiosError(error) || axios.isCancel(error)) {
      return false;
    }

    if (!error.response) {
      return Boolean(error.code) && error.code !== "ECONNABORTED";
    }

    const status = error.response.status;

    if (
      status === HttpStatusCode.RequestTimeout ||
      status === HttpStatusCode.TooManyRequests
    ) {
      return true;
    }

    // Never retry permanent client errors (4xx except 408 and 429)
    if (
      status >= HttpStatusCode.BadRequest &&
      status < HttpStatusCode.InternalServerError
    ) {
      return false;
    }

    const method = error.config?.method?.toLowerCase();
    if (method !== undefined && IDEMPOTENT_HTTP_METHODS.has(method)) {
      return true;
    }

    const headers = error.config?.headers;
    const hasIdempotencyKey =
      headers instanceof AxiosHeaders
        ? Boolean(headers.get(HttpHeaders.RequestId))
        : Boolean(
            headers?.[HttpHeaders.RequestId] ??
            headers?.[HttpHeaders.RequestId.toLowerCase()],
          );

    return hasIdempotencyKey;
  }

  private requestWithRetry<T>(
    operation: () => Promise<AxiosResponse<T>>,
    signal?: AxiosRequestConfig["signal"],
  ): Promise<AxiosResponse<T>> {
    const abortSignal = signal instanceof AbortSignal ? signal : undefined;

    return pRetry(operation, {
      retries: this.maxRetries,
      signal: abortSignal,
      shouldRetry: async ({ error, retryDelay }) => {
        if (!this.shouldRetryRequest(error)) {
          return false;
        }

        if (axios.isAxiosError(error) && error.response) {
          const retryAfterMs = parseRetryAfterMs(error.response.headers);
          if (retryAfterMs !== undefined && retryAfterMs > retryDelay) {
            await waitForDelay(retryAfterMs - retryDelay, abortSignal);
          }
        }

        return true;
      },
    });
  }

  /**
   * Register a provider function that returns the current access token.
   * The provider may be sync or async — the interceptor awaits the result.
   * Called on every request to attach the Bearer header.
   */
  public setTokenProvider(provider: AccessTokenProvider): void {
    this.tokenProvider = provider;
  }

  public async getAccessToken(): Promise<string | null> {
    return (await this.tokenProvider?.()) ?? null;
  }

  public get<T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.requestWithRetry(
      () => this.client.get<T>(url, config),
      config?.signal,
    );
  }

  public async getConditional<T>(
    url: string,
    options?: { ifNoneMatch?: string; params?: unknown; signal?: AbortSignal },
  ): Promise<ConditionalResponse<T>> {
    const headers = new AxiosHeaders({
      [HttpHeaders.CacheControl]: "no-cache",
    });

    if (options?.ifNoneMatch) {
      headers.set(HttpHeaders.IfNoneMatch, options.ifNoneMatch);
    }

    const response = await this.get<T>(url, {
      params: options?.params,
      headers,
      signal: options?.signal,
      validateStatus: (status) =>
        (status >= HttpStatusCode.Ok &&
          status < HttpStatusCode.MultipleChoices) ||
        status === HttpStatusCode.NotModified,
    });

    const etag = extractETag(response.headers);

    if (response.status === HttpStatusCode.NotModified) {
      return { data: null, etag, notModified: true };
    }

    return { data: response.data, etag, notModified: false };
  }

  public query<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.requestWithRetry(
      () =>
        this.client.request<T>({
          ...config,
          method: "QUERY",
          url,
          data,
        }),
      config?.signal,
    );
  }

  public post<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.requestWithRetry(
      () => this.client.post<T>(url, data, config),
      config?.signal,
    );
  }

  public put<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.requestWithRetry(
      () => this.client.put<T>(url, data, config),
      config?.signal,
    );
  }

  public patch<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.requestWithRetry(
      () => this.client.patch<T>(url, data, config),
      config?.signal,
    );
  }

  public delete<T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.requestWithRetry(
      () => this.client.delete<T>(url, config),
      config?.signal,
    );
  }
}

export const apiClient = new ApiClient();
