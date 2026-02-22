import type React from "react";

import Image from "next/image";

import { Button } from "@workspace/ui/components/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { Input } from "@workspace/ui/components/input";
import { DEFAULT_BOOK_IMAGE } from "@workspace/utils/constants";

type ImageCardProps = Readonly<{
  bookId?: string;
  bookName?: string;
  imagePreview: string | null;
  onImageChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onImageRemove: () => void;
  onImageError: () => void;
}>;

export function ImageCard({
  bookId,
  bookName,
  imagePreview,
  onImageChange,
  onImageRemove,
  onImageError,
}: ImageCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Image</CardTitle>
        <CardDescription>Upload a book cover image (optional)</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="border-border rounded-lg border-2 border-dashed p-8">
          {imagePreview ? (
            <div className="flex flex-col items-center gap-4">
              <Image
                src={imagePreview || DEFAULT_BOOK_IMAGE}
                alt={
                  bookName
                    ? `Cover image for ${bookName}`
                    : "Book cover preview"
                }
                width={96}
                height={128}
                onError={onImageError}
                className="rounded object-cover"
              />
              <div className="flex w-full flex-col gap-2">
                <label htmlFor="book-image-change" className="sr-only">
                  Change book cover image
                </label>
                <Input
                  id="book-image-change"
                  type="file"
                  accept="image/*"
                  onChange={onImageChange}
                  aria-label="Change book cover image"
                />
                {bookId && (
                  <Button
                    type="button"
                    variant="destructive"
                    size="sm"
                    onClick={onImageRemove}
                    aria-label="Remove current book cover image"
                  >
                    Remove Image
                  </Button>
                )}
              </div>
            </div>
          ) : (
            <div>
              <label htmlFor="book-image-upload" className="sr-only">
                Upload book cover image
              </label>
              <Input
                id="book-image-upload"
                type="file"
                accept="image/*"
                onChange={onImageChange}
                placeholder="Select image"
                aria-label="Upload book cover image"
              />
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
