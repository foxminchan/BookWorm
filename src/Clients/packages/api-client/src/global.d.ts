export type AxiosRequestConfig = import("axios").AxiosRequestConfig;

export type ApiDataError = {
  type?: string;
  title: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
};

export type AppAxiosError = import("axios").AxiosError<ApiDataError, unknown>;
