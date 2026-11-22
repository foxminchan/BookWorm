import type React from "react";

import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import Link from "next/link";

import { createMetadata } from "@workspace/seo/metadata";
import { STOREFRONT_VIEWPORT } from "@workspace/storefront-shell/config/app-config";
import { Button } from "@workspace/ui/components/button";
import "@workspace/ui/globals.css";

import { JsonLd } from "@/components/json-ld";
import { APP_CONFIG } from "@/config/app-config";
import { env } from "@/env.mjs";
import { generateOrganizationJsonLd, generateWebsiteJsonLd } from "@/lib/seo";

import { Providers } from "./providers";

export const viewport = STOREFRONT_VIEWPORT;

const appUrl = env.NEXT_PUBLIC_APP_URL || APP_CONFIG.defaultLocalUrl;

export const metadata: Metadata = {
  ...createMetadata({
    title: "BookWorm - Curated Books & Design Inspiration | Online Bookstore",
    description:
      "Discover a carefully curated collection of literature, design books, and inspiration for the modern reader. Shop fiction, non-fiction, design, science, and more.",
    appName: APP_CONFIG.name,
    baseUrl: appUrl,
    creatorName: APP_CONFIG.name,
    publisherName: APP_CONFIG.name,
  }),
  title: {
    default: "BookWorm - Curated Books & Design Inspiration | Online Bookstore",
    template: "%s | BookWorm",
  },
  generator: "Next.js",
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
  manifest: "/manifest.json",
  openGraph: {
    type: "website",
    url: appUrl,
    title: "BookWorm - Curated Books & Design Inspiration",
    description:
      "Discover a carefully curated collection of literature and design books for the modern reader.",
    siteName: APP_CONFIG.name,
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
    title: APP_CONFIG.name,
  },
};

const geist = Geist({ subsets: ["latin"], variable: "--font-sans" });
const geistMono = Geist_Mono({ subsets: ["latin"], variable: "--font-mono" });

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const organizationJsonLd = generateOrganizationJsonLd();
  const websiteJsonLd = generateWebsiteJsonLd();
  const isCopilotEnabled = env.NEXT_PUBLIC_COPILOT_ENABLED;

  return (
    <html
      lang="en"
      className={`${geist.variable} ${geistMono.variable}`}
      suppressHydrationWarning
    >
      <body className="font-sans antialiased">
        <Button
          render={<Link href="#main-content">Skip to main content</Link>}
          nativeButton={false}
          variant="outline"
          className="sr-only focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-50"
        />
        <div>
          <Providers isCopilotEnabled={isCopilotEnabled}>{children}</Providers>
        </div>
        <JsonLd data={organizationJsonLd} />
        <JsonLd data={websiteJsonLd} />
      </body>
    </html>
  );
}
