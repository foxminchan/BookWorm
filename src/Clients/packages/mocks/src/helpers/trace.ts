import { v7 as uuidv7 } from "uuid";

export const generateTraceId = (): string => {
  const traceId = uuidv7().replace(/-/g, "");
  const parentId = uuidv7().substring(0, 16).replace(/-/g, "");
  return `00-${traceId}-${parentId}-01`;
};
