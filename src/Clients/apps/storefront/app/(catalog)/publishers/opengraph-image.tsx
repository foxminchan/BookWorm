import { OG_IMAGE_SIZE, createCatalogOgImage } from "@/lib/og-image";

export const runtime = "edge";
export const alt = "BookWorm - Browse by Publisher";
export const size = OG_IMAGE_SIZE;
export const contentType = "image/png";

export default function Image() {
  return createCatalogOgImage({
    title: "Browse by Publisher",
    subtitle: "Penguin • HarperCollins • Simon & Schuster & More",
  });
}
