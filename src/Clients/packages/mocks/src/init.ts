export async function initMocks(): Promise<void> {
  if (globalThis.window === undefined) {
    return;
  }

  if (process.env.NODE_ENV === "development") {
    const { startMocking } = await import("./browser");
    await startMocking();
  }
}
