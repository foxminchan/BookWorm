import { NextResponse } from "next/server";

/** Lightweight readiness/liveness endpoint for Aspire and Azure Container Apps health probes. */
export function GET() {
  return NextResponse.json({ status: "ok" });
}
