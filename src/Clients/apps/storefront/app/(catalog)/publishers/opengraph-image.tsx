import { createCatalogOgImage } from "@/lib/og-image";

export { OG_IMAGE_SIZE as size } from "@/lib/og-image";
export const runtime = "edge";
export const alt = "BookWorm - Browse by Publisher";
export const contentType = "image/png";

export default function Image() {
  return createCatalogOgImage({
    title: "Browse by Publisher",
    subtitle: "Penguin • HarperCollins • Simon & Schuster & More",
  });
}
