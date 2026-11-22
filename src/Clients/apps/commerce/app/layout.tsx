import type React from "react";

import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import Link from "next/link";

import { createMetadata } from "@workspace/seo/metadata";
import { APP_CONFIG } from "@workspace/storefront-shell/config/app-config";
import { Button } from "@workspace/ui/components/button";
import "@workspace/ui/globals.css";

import { env } from "@/env.mjs";

import { Providers } from "./providers";

export { STOREFRONT_VIEWPORT as viewport } from "@workspace/storefront-shell/config/app-config";

const appUrl = env.NEXT_PUBLIC_APP_URL || APP_CONFIG.defaultLocalUrl;

export const metadata: Metadata = {
  ...createMetadata({
    title: "BookWorm - Basket, Checkout & Account",
    description:
      "Manage your BookWorm shopping basket, checkout confirmation, profile, and order history.",
    appName: APP_CONFIG.name,
    baseUrl: appUrl,
    creatorName: APP_CONFIG.name,
    publisherName: APP_CONFIG.name,
  }),
  title: {
    default: "BookWorm - Basket, Checkout & Account",
    template: "%s | BookWorm",
  },
  generator: "Next.js",
  robots: {
    index: false,
    follow: false,
  },
};

const geist = Geist({ subsets: ["latin"], variable: "--font-sans" });
const geistMono = Geist_Mono({ subsets: ["latin"], variable: "--font-mono" });

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
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
          <Providers>{children}</Providers>
        </div>
      </body>
    </html>
  );
}
