import { describe, expect, it } from "vitest";

import { isNavActive } from "@/lib/nav-utils";

describe("isNavActive", () => {
  it("should return true for exact path match", () => {
    expect(isNavActive("/shop", "/shop")).toBe(true);
  });

  it("should return true for child route", () => {
    expect(isNavActive("/shop/book-1", "/shop")).toBe(true);
  });

  it("should return true for deeply nested child route", () => {
    expect(isNavActive("/account/orders/123", "/account")).toBe(true);
  });

  it("should return false for different paths", () => {
    expect(isNavActive("/account", "/shop")).toBe(false);
  });

  it("should return false for partial prefix match without slash", () => {
    expect(isNavActive("/shopping", "/shop")).toBe(false);
  });

  it("should return true for root exact match", () => {
    expect(isNavActive("/", "/")).toBe(true);
  });

  it("should return false when pathname is shorter than href", () => {
    expect(isNavActive("/a", "/account")).toBe(false);
  });
});
