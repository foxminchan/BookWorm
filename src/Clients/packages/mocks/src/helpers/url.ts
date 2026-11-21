import { DEFAULT_GATEWAY_URL } from "@workspace/utils/constants";

/** Build a full mock gateway URL for an API endpoint path. */
export function mockUrl(path: string): string {
  return `${DEFAULT_GATEWAY_URL}${path}`;
}
