export async function initMocks() {
  if (typeof window === "undefined") {
    return;
  }

  if (process.env.NODE_ENV === "development") {
    const { startMocking } = await import("@workspace/mocks/browser");
    await startMocking();
  }
}
