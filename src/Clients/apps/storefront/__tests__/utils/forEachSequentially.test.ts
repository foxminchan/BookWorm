import { describe, expect, it, vi } from "vitest";

import { forEachSequentially } from "../../e2e/utils/forEachSequentially";

describe("forEachSequentially", () => {
  it("runs each action in order and waits for it to finish", async () => {
    const order: number[] = [];
    const action = vi.fn(async (value: number) => {
      await Promise.resolve();
      order.push(value);
    });

    await forEachSequentially([1, 2, 3], action);

    expect(order).toEqual([1, 2, 3]);
    expect(action).toHaveBeenCalledTimes(3);
  });

  it("stops processing when an action rejects", async () => {
    const action = vi.fn(async (value: number) => {
      if (value === 2) {
        throw new Error("Action failed");
      }
    });

    await expect(forEachSequentially([1, 2, 3], action)).rejects.toThrow(
      "Action failed",
    );
    expect(action).toHaveBeenCalledTimes(2);
  });
});
