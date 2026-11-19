import { createCatalogOgImage } from "@/lib/og-image";

export { OG_IMAGE_SIZE as size } from "@/lib/og-image";
export const runtime = "edge";
export const alt = "BookWorm - Browse by Category";
export const contentType = "image/png";

export default function Image() {
  return createCatalogOgImage({
    title: "Browse by Category",
    subtitle: "Fiction • Non-Fiction • Biography • History & More",
  });
}
