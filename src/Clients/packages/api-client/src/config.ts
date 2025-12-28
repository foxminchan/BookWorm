import type { AxiosRequestConfig } from "./global";

const getBaseURL = (): string => {
  return (
    process.env.GATEWAY_HTTPS ||
    process.env.GATEWAY_HTTP ||
    "http://localhost:5000"
  );
};

const axiosConfigs: Record<string, AxiosRequestConfig> = {
  development: {
    baseURL: getBaseURL(),
    withCredentials: true,
    timeout: 30000,
  },
  production: {
    baseURL: getBaseURL(),
    withCredentials: true,
    timeout: 30000,
  },
  test: {
    baseURL: getBaseURL(),
    withCredentials: true,
    timeout: 10000,
  },
};

const getAxiosConfig = (): AxiosRequestConfig => {
  const nodeEnv: string = process.env.NODE_ENV || "development";
  return (axiosConfigs[nodeEnv] ||
    axiosConfigs.development) as AxiosRequestConfig;
};

const axiosConfig = getAxiosConfig();

export default axiosConfig;
