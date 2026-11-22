import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { CopyrightYear } from "@workspace/storefront-shell/components/copyright-year";

describe("CopyrightYear", () => {
  it("renders the current year", () => {
    render(<CopyrightYear />);

    expect(
      screen.getByText(new Date().getFullYear().toString()),
    ).toBeInTheDocument();
  });
});
