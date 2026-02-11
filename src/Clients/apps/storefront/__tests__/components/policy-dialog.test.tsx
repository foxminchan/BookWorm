import { screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { PolicyDialog } from "@/components/policy-dialog";

import { renderWithProviders } from "../utils/test-utils";

describe("PolicyDialog", () => {
  const defaultProps = {
    policyType: "privacy" as const,
    isOpen: true,
    onOpenChange: vi.fn(),
    content: "",
  };

  it("should display privacy policy title", () => {
    renderWithProviders(<PolicyDialog {...defaultProps} />);

    expect(screen.getByText("Privacy Policy")).toBeInTheDocument();
  });

  it("should display terms of service title", () => {
    renderWithProviders(<PolicyDialog {...defaultProps} policyType="terms" />);

    expect(screen.getByText("Terms of Service")).toBeInTheDocument();
  });

  it("should render heading from markdown h1", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content="# Main Title" />,
    );

    expect(screen.getByText("Main Title")).toBeInTheDocument();
  });

  it("should render h2 headings", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content="## Section Title" />,
    );

    expect(screen.getByText("Section Title")).toBeInTheDocument();
  });

  it("should render h3 headings", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content="### Subsection" />,
    );

    expect(screen.getByText("Subsection")).toBeInTheDocument();
  });

  it("should render paragraph text", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content="Some paragraph text here." />,
    );

    expect(screen.getByText("Some paragraph text here.")).toBeInTheDocument();
  });

  it("should render list items with bullets", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content="- First item\n- Second item" />,
    );

    expect(screen.getByText(/First item/)).toBeInTheDocument();
    expect(screen.getByText(/Second item/)).toBeInTheDocument();
  });

  it("should render bold text within content", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content="This is **bold** text" />,
    );

    expect(screen.getByText("bold")).toBeInTheDocument();
    expect(screen.getByText("bold").tagName).toBe("STRONG");
  });

  it("should render italic text within content", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content="This is *italic* text" />,
    );

    expect(screen.getByText("italic")).toBeInTheDocument();
    expect(screen.getByText("italic").tagName).toBe("EM");
  });

  it("should not render when closed", () => {
    renderWithProviders(
      <PolicyDialog
        {...defaultProps}
        isOpen={false}
        content="Hidden content"
      />,
    );

    expect(screen.queryByText("Hidden content")).not.toBeInTheDocument();
  });

  it("should skip empty lines", () => {
    renderWithProviders(
      <PolicyDialog {...defaultProps} content={"Line one\n\nLine two"} />,
    );

    expect(screen.getByText("Line one")).toBeInTheDocument();
    expect(screen.getByText("Line two")).toBeInTheDocument();
  });

  it("should render multi-line content correctly", () => {
    const content = [
      "# Privacy Policy",
      "## Data Collection",
      "We collect the following:",
      "- Email address",
      "- Name",
    ].join("\n");

    renderWithProviders(<PolicyDialog {...defaultProps} content={content} />);

    expect(screen.getByText("Data Collection")).toBeInTheDocument();
    expect(screen.getByText("We collect the following:")).toBeInTheDocument();
    expect(screen.getByText("Email address")).toBeInTheDocument();
    expect(screen.getByText("Name")).toBeInTheDocument();
  });
});
