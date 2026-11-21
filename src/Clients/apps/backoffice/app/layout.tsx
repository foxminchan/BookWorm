import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";

import { createMetadata } from "@workspace/seo/metadata";
import "@workspace/ui/globals.css";

import { APP_CONFIG } from "@/config/app-config";
import { env } from "@/env.mjs";

import { Providers } from "./providers";

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 5,
  userScalable: true,
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#f8f8f8" },
    { media: "(prefers-color-scheme: dark)", color: "#0a0a0a" },
  ],
};

export const metadata: Metadata = {
  ...createMetadata({
    title: "BookWorm Admin - Dashboard",
    description: APP_CONFIG.description,
    appName: APP_CONFIG.name,
    baseUrl: env.NEXT_PUBLIC_APP_URL || APP_CONFIG.defaultUrl,
    creatorName: APP_CONFIG.brandName,
    publisherName: APP_CONFIG.brandName,
  }),
  title: {
    default: "BookWorm Admin - Dashboard",
    template: "%s | BookWorm Admin",
  },
  generator: "Next.js",
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
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
