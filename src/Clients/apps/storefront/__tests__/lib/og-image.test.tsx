import { describe, expect, it, vi } from "vitest";

import { OG_IMAGE_SIZE, createCatalogOgImage } from "@/lib/og-image";

const MockImageResponse = vi.hoisted(() =>
  vi.fn(function (
    this: { element: unknown; options: unknown },
    element: unknown,
    options: unknown,
  ) {
    this.element = element;
    this.options = options;
  }),
);

vi.mock("next/og", () => ({
  ImageResponse: MockImageResponse,
}));

describe("og-image", () => {
  it("exports standard OpenGraph dimensions", () => {
    expect(OG_IMAGE_SIZE).toEqual({ width: 1200, height: 630 });
  });

  it("creates an ImageResponse with title, subtitle, and standard size", () => {
    const response = createCatalogOgImage({
      title: "Browse by Category",
      subtitle: "Fiction • Non-Fiction",
    });

    expect(MockImageResponse).toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ width: 1200, height: 630 }),
    );
    expect(response).toBeInstanceOf(MockImageResponse);
  });
});
