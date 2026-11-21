import type { Metadata } from "next";

export type MetadataImage = {
  url: string;
  alt?: string;
  width?: number;
  height?: number;
};

export type MetadataGenerator = Omit<Metadata, "description" | "title"> & {
  title: string;
  description: string;
  appName?: string;
  baseUrl?: string;
  image?: string | MetadataImage;
  creatorName?: string;
  publisherName?: string;
  appendAppName?: boolean;
};

/**
 * Generates consistent Next.js page metadata with Open Graph and Twitter defaults.
 */
export function createMetadata({
  title,
  description,
  image,
  appName = "BookWorm",
  baseUrl,
  creatorName = "BookWorm",
  publisherName = "BookWorm",
  appendAppName = false,
  openGraph,
  twitter,
  ...properties
}: MetadataGenerator): Metadata {
  const parsedTitle =
    appendAppName && !title.includes(appName) ? `${title} | ${appName}` : title;
  const socialTitle = title.includes(appName) ? title : `${title} | ${appName}`;
  const images = image
    ? [
        typeof image === "string"
          ? { url: image, width: 1200, height: 630, alt: title }
          : image,
      ]
    : undefined;

  return {
    title: parsedTitle,
    description,
    applicationName: appName,
    ...(baseUrl ? { metadataBase: new URL(baseUrl) } : {}),
    creator: creatorName,
    publisher: publisherName,
    formatDetection: {
      email: false,
      address: false,
      telephone: false,
    },
    openGraph: {
      type: "website",
      title: socialTitle,
      description,
      siteName: appName,
      locale: "en_US",
      ...(images ? { images } : {}),
      ...openGraph,
    },
    twitter: {
      card: "summary_large_image",
      title: socialTitle,
      description,
      ...(images ? { images } : {}),
      ...twitter,
    },
    ...properties,
  };
}
