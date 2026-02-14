import type { AxiosRequestConfig } from "./global";

const getBaseURL = (): string => {
  return (
    process.env.NEXT_PUBLIC_GATEWAY_HTTPS ||
    process.env.NEXT_PUBLIC_GATEWAY_HTTP ||
    "http://localhost:5000"
  );
};

const defaultConfig: AxiosRequestConfig = {
  baseURL: getBaseURL(),
  withCredentials: true,
  timeout: 30000,
};

const axiosConfigs: Record<string, AxiosRequestConfig> = {
  default: defaultConfig,
  test: {
    baseURL: getBaseURL(),
    withCredentials: true,
    timeout: 10000,
  },
};

const getAxiosConfig = (): AxiosRequestConfig => {
  const nodeEnv: string = process.env.NODE_ENV || "development";
  return axiosConfigs[nodeEnv] ?? defaultConfig;
};

const axiosConfig = getAxiosConfig();

export default axiosConfig;
