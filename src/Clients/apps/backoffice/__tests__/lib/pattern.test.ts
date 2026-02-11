import { describe, expect, it } from "vitest";

import {
  canCancelOrder,
  canCompleteOrder,
  getOrderStatusStyle,
  isOrderEditable,
  isOrderNew,
} from "@/lib/pattern";

describe("getOrderStatusStyle", () => {
  it("should return blue style for New status", () => {
    expect(getOrderStatusStyle("New")).toBe("bg-blue-100 text-blue-800");
  });

  it("should return green style for Completed status", () => {
    expect(getOrderStatusStyle("Completed")).toBe(
      "bg-green-100 text-green-800",
    );
  });

  it("should return red style for Cancelled status", () => {
    expect(getOrderStatusStyle("Cancelled")).toBe("bg-red-100 text-red-800");
  });
});

describe("isOrderNew", () => {
  it("should return true for New status", () => {
    expect(isOrderNew("New")).toBe(true);
  });

  it("should return false for Completed status", () => {
    expect(isOrderNew("Completed")).toBe(false);
  });

  it("should return false for Cancelled status", () => {
    expect(isOrderNew("Cancelled")).toBe(false);
  });
});

describe("canCompleteOrder", () => {
  it("should be true for New orders", () => {
    expect(canCompleteOrder("New")).toBe(true);
  });

  it("should be false for Completed orders", () => {
    expect(canCompleteOrder("Completed")).toBe(false);
  });

  it("should be false for Cancelled orders", () => {
    expect(canCompleteOrder("Cancelled")).toBe(false);
  });
});

describe("canCancelOrder", () => {
  it("should be true for New orders", () => {
    expect(canCancelOrder("New")).toBe(true);
  });

  it("should be false for Completed orders", () => {
    expect(canCancelOrder("Completed")).toBe(false);
  });

  it("should be false for Cancelled orders", () => {
    expect(canCancelOrder("Cancelled")).toBe(false);
  });
});

describe("isOrderEditable", () => {
  it("should be true for New orders", () => {
    expect(isOrderEditable("New")).toBe(true);
  });

  it("should be false for Completed orders", () => {
    expect(isOrderEditable("Completed")).toBe(false);
  });

  it("should be false for Cancelled orders", () => {
    expect(isOrderEditable("Cancelled")).toBe(false);
  });
});
