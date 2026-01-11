import type React from "react";

import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import Link from "next/link";

import { Button } from "@workspace/ui/components/button";
import "@workspace/ui/globals.css";

import { JsonLd } from "@/components/json-ld";
import { env } from "@/env.mjs";
import { generateOrganizationJsonLd, generateWebsiteJsonLd } from "@/lib/seo";

import { Providers } from "./providers";

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 5,
  userScalable: true,
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#fbf8f1" },
    { media: "(prefers-color-scheme: dark)", color: "#1a1a1a" },
  ],
};

export const metadata: Metadata = {
  metadataBase: new URL(env.NEXT_PUBLIC_APP_URL || "http://localhost:3000"),
  title: {
    default: "BookWorm - Curated Books & Design Inspiration | Online Bookstore",
    template: "%s | BookWorm",
  },
  description:
    "Discover a carefully curated collection of literature, design books, and inspiration for the modern reader. Shop fiction, non-fiction, design, science, and more.",
  generator: "Next.js",
  applicationName: "BookWorm",
  keywords: [
    "books",
    "online bookstore",
    "literature",
    "design books",
    "fiction",
    "non-fiction",
    "book shop",
  ],
  authors: [{ name: "BookWorm Team" }],
  creator: "BookWorm",
  publisher: "BookWorm",
  formatDetection: {
    email: false,
    address: false,
    telephone: false,
  },
  manifest: "/manifest.json",
  openGraph: {
    type: "website",
    url: env.NEXT_PUBLIC_APP_URL || "http://localhost:3000",
    title: "BookWorm - Curated Books & Design Inspiration",
    description:
      "Discover a carefully curated collection of literature and design books for the modern reader.",
    siteName: "BookWorm",
    locale: "en_US",
  },
  twitter: {
    card: "summary_large_image",
    title: "BookWorm - Curated Books & Design Inspiration",
    description:
      "Discover a carefully curated collection of literature and design books.",
  },
  icons: {
    icon: [
      {
        url: "/favicon.svg",
        type: "image/svg+xml",
      },
      {
        url: "/favicon-96x96.png",
        sizes: "96x96",
        type: "image/png",
      },
      {
        url: "/favicon.ico",
        sizes: "any",
      },
    ],
    apple: "/apple-touch-icon.png",
  },
  appleWebApp: {
    capable: true,
    statusBarStyle: "black-translucent",
    title: "BookWorm",
  },
};

const _geist = Geist({ subsets: ["latin"] });
const _geistMono = Geist_Mono({ subsets: ["latin"] });

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const organizationJsonLd = generateOrganizationJsonLd();
  const websiteJsonLd = generateWebsiteJsonLd();
  const isCopilotEnabled = env.NEXT_PUBLIC_COPILOT_ENABLED;

  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`font-sans antialiased`}>
        <Button
          asChild
          variant="outline"
          className="sr-only focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-50"
        >
          <Link href="#main-content">Skip to main content</Link>
        </Button>
        <div id="main-content">
          <Providers isCopilotEnabled={isCopilotEnabled}>{children}</Providers>
        </div>
        <JsonLd data={organizationJsonLd} />
        <JsonLd data={websiteJsonLd} />
      </body>
    </html>
  );
}
