import { describe, expect, it } from "vitest";

import { cn } from "@/lib/utils";

describe("cn utility", () => {
  it("should merge class names", () => {
    const result = cn("text-red-500", "bg-blue-500");
    expect(result).toBe("text-red-500 bg-blue-500");
  });

  it("should handle conditional classes", () => {
    const result = cn("base-class", true && "conditional-class");
    expect(result).toBe("base-class conditional-class");
  });

  it("should filter out falsy values", () => {
    const result = cn(
      "text-sm",
      false && "hidden",
      null,
      undefined,
      "font-bold",
    );
    expect(result).toBe("text-sm font-bold");
  });

  it("should merge Tailwind classes correctly", () => {
    const result = cn("p-4", "p-8");
    expect(result).toBe("p-8");
  });

  it("should handle arrays", () => {
    const result = cn(["text-sm", "font-bold"], "text-red-500");
    expect(result).toBe("text-sm font-bold text-red-500");
  });

  it("should handle empty input", () => {
    const result = cn();
    expect(result).toBe("");
  });

  it("should handle objects", () => {
    const result = cn({
      "text-red-500": true,
      "bg-blue-500": false,
      "font-bold": true,
    });
    expect(result).toBe("text-red-500 font-bold");
  });
});
