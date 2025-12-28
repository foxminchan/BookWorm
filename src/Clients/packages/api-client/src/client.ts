import axios, { type AxiosInstance } from "axios";
import type { AxiosRequestConfig } from "./global";
import axiosConfig from "./config";

export type ApiClientConfig = {
  baseURL: string;
  timeout?: number;
  headers?: Record<string, string>;
};

export class ApiClient {
  private readonly client: AxiosInstance;

  constructor(config: ApiClientConfig | AxiosRequestConfig = axiosConfig) {
    const axiosConfigs = "baseURL" in config ? config : axiosConfig;

    const instance = axios.create({
      ...axiosConfigs,
      headers: {
        "Content-Type": "application/json",
        ...(axiosConfigs.headers as Record<string, string>),
      },
    });

    this.client = this.setupInterceptors(instance);
  }

  private setupInterceptors(instance: AxiosInstance): AxiosInstance {
    instance.interceptors.request.use(
      async (config) => {
        return config;
      },
      (error) => {
        console.error(`[request error] [${JSON.stringify(error)}]`);
        return Promise.reject(new Error(error));
      },
    );
    instance.interceptors.response.use(
      async (response) => {
        return response.data;
      },
      (error) => {
        return Promise.reject(new Error(error));
      },
    );

    return instance;
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.client.get<T>(url, config) as Promise<T>;
  }

  async post<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.client.post<T>(url, data, config) as Promise<T>;
  }

  async put<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.client.put<T>(url, data, config) as Promise<T>;
  }

  async patch<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    return this.client.patch<T>(url, data, config) as Promise<T>;
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.client.delete<T>(url, config) as Promise<T>;
  }

  getClient(): AxiosInstance {
    return this.client;
  }
}
