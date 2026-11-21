import axios, {
  AxiosHeaders,
  type AxiosInstance,
  type AxiosResponse,
  HttpStatusCode,
} from "axios";
import axiosRetry, { exponentialDelay } from "axios-retry";

import axiosConfig from "./config";
import type { AxiosRequestConfig } from "./global";
import { HttpHeaders } from "./headers";

export { HttpHeaders } from "./headers";

const DEFAULT_MAX_RETRIES = Number(process.env.NEXT_PUBLIC_MAX_RETRIES) || 5;

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

export function isPreconditionFailedError(error: unknown): boolean {
  return (
    axios.isAxiosError(error) &&
    error.response?.status === HttpStatusCode.PreconditionFailed
  );
}

export default class ApiClient {
  private readonly client: AxiosInstance;
  private tokenProvider: AccessTokenProvider | null = null;

  constructor(config = axiosConfig, maxRetries = DEFAULT_MAX_RETRIES) {
    const axiosConfigs = "baseURL" in config ? config : axiosConfig;

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

    axiosRetry(this.client, {
      retries: maxRetries,
      retryDelay: exponentialDelay,
      retryCondition: (error) => {
        if (!error.response) {
          return axiosRetry.isNetworkError(error);
        }

        const status = error.response.status;

        // Never retry permanent client errors (4xx except 408 and 429)
        if (
          status >= HttpStatusCode.BadRequest &&
          status < HttpStatusCode.InternalServerError &&
          status !== HttpStatusCode.RequestTimeout &&
          status !== HttpStatusCode.TooManyRequests
        ) {
          return false;
        }

        return (
          axiosRetry.isNetworkOrIdempotentRequestError(error) ||
          status === HttpStatusCode.RequestTimeout ||
          status === HttpStatusCode.TooManyRequests
        );
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
    return this.client.get<T>(url, config);
  }

  public async getConditional<T>(
    url: string,
    options?: { ifNoneMatch?: string; params?: unknown },
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
    return this.client.request<T>({
      ...config,
      method: "QUERY",
      url,
      data,
    });
  }

  public post<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.post<T>(url, data, config);
  }

  public put<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.put<T>(url, data, config);
  }

  public patch<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.patch<T>(url, data, config);
  }

  public delete<T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.delete<T>(url, config);
  }
}

export const apiClient = new ApiClient();
