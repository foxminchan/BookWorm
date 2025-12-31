import type React from "react";
import { Geist, Geist_Mono } from "next/font/google";
import type { Metadata, Viewport } from "next";
import { JsonLd } from "@/components/json-ld";
import { generateOrganizationJsonLd, generateWebsiteJsonLd } from "@/lib/seo";
import { Providers } from "@/components/providers";
import "@workspace/ui/globals.css";

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 5,
  userScalable: true,
};

export const metadata: Metadata = {
  title: "BookWorm - Curated Books & Design Inspiration | Online Bookstore",
  description:
    "Discover a carefully curated collection of literature, design books, and inspiration for the modern reader. Shop fiction, non-fiction, design, science, and more.",
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
  creator: "BookWorm",
  manifest: "/manifest.json",
  openGraph: {
    type: "website",
    url: "https://bookworm.com",
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

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const organizationJsonLd = generateOrganizationJsonLd();
  const websiteJsonLd = generateWebsiteJsonLd();

  return (
    <html lang="en">
      <head>
        <JsonLd data={organizationJsonLd} />
        <JsonLd data={websiteJsonLd} />
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{
            __html: JSON.stringify({
              "@context": "https://schema.org",
              "@type": "Organization",
              name: "BookWorm",
              url: "https://bookworm.com",
              logo: "https://bookworm.com/logo.svg",
              description:
                "Curated online bookstore with literature, design, and inspiration books",
              sameAs: [
                "https://twitter.com/bookworm",
                "https://facebook.com/bookworm",
              ],
            }),
          }}
        />
      </head>
      <body className={`font-sans antialiased`}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
