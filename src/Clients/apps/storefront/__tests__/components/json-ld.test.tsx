import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { JsonLd } from "@/components/json-ld";

describe("JsonLd", () => {
  it("should render a script tag with type application/ld+json", () => {
    const data = { "@context": "https://schema.org", "@type": "Book" };
    const { container } = render(<JsonLd data={data} />);

    const script = container.querySelector(
      'script[type="application/ld+json"]',
    );
    expect(script).not.toBeNull();
  });

  it("should serialize data as JSON inside the script tag", () => {
    const data = {
      "@context": "https://schema.org",
      "@type": "Book",
      name: "Clean Code",
    };

    const { container } = render(<JsonLd data={data} />);

    const script = container.querySelector(
      'script[type="application/ld+json"]',
    );
    const parsed = JSON.parse(script?.innerHTML ?? "");

    expect(parsed).toEqual(data);
  });

  it("should handle complex nested data", () => {
    const data = {
      "@context": "https://schema.org",
      "@type": "Book",
      author: [
        { "@type": "Person", name: "Author One" },
        { "@type": "Person", name: "Author Two" },
      ],
      offers: {
        "@type": "Offer",
        price: 19.99,
        priceCurrency: "USD",
      },
    };

    const { container } = render(<JsonLd data={data} />);

    const script = container.querySelector(
      'script[type="application/ld+json"]',
    );
    const parsed = JSON.parse(script?.innerHTML ?? "");

    expect(parsed.author).toHaveLength(2);
    expect(parsed.offers.price).toBe(19.99);
  });

  it("should handle empty object", () => {
    const { container } = render(<JsonLd data={{}} />);

    const script = container.querySelector(
      'script[type="application/ld+json"]',
    );
    expect(JSON.parse(script?.innerHTML ?? "")).toEqual({});
  });
});
