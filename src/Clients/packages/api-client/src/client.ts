import axios, {
  type AxiosInstance,
  type AxiosResponse,
  HttpStatusCode,
} from "axios";
import axiosRetry, { exponentialDelay } from "axios-retry";

import axiosConfig from "./config";
import type { AxiosRequestConfig } from "./global";

const DEFAULT_MAX_RETRIES = Number(process.env.NEXT_PUBLIC_MAX_RETRIES) || 5;

/**
 * A function that resolves the current access token, or `null` if unavailable.
 * May be synchronous or asynchronous — the request interceptor awaits the result.
 * Used by the request interceptor to attach `Authorization: Bearer <token>`.
 */
export type AccessTokenProvider = () => Promise<string | null> | string | null;

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
        requestConfig.headers.Authorization = `Bearer ${token}`;
      }
      return requestConfig;
    });

    axiosRetry(this.client, {
      retries: maxRetries,
      retryDelay: exponentialDelay,
      retryCondition: (error) =>
        axiosRetry.isNetworkOrIdempotentRequestError(error) ||
        error.response?.status === HttpStatusCode.TooManyRequests,
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

  public async get<T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.get<T>(url, config);
  }

  public async post<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.post<T>(url, data, config);
  }

  public async put<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.put<T>(url, data, config);
  }

  public async patch<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.patch<T>(url, data, config);
  }

  public async delete<T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.client.delete<T>(url, config);
  }
}

export const apiClient = new ApiClient();
