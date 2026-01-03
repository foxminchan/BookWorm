import axios, { type AxiosInstance, type AxiosResponse } from "axios";

import axiosConfig from "./config";
import { AxiosRequestConfig } from "./global";

export default class ApiClient {
  private readonly client: AxiosInstance;

  constructor(config = axiosConfig) {
    const axiosConfigs = "baseURL" in config ? config : axiosConfig;

    const instance = axios.create({
      ...axiosConfigs,
      headers: {
        ...(axiosConfigs.headers as Record<string, string>),
      },
    });

    this.client = this.setupInterceptors(instance);
  }

  private setupInterceptors(instance: AxiosInstance): AxiosInstance {
    instance.interceptors.request.use(
      async (config) => config,
      (error) => {
        console.error(`[request error] [${JSON.stringify(error)}]`);
        return Promise.reject(new Error(error));
      },
    );
    instance.interceptors.response.use(
      async (response) => response,
      (error) => {
        return Promise.reject(new Error(error));
      },
    );

    return instance;
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
