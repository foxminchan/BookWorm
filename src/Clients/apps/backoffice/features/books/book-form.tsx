"use client";

import type React from "react";
import { useEffect, useState } from "react";

import { useRouter } from "next/navigation";

import { zodResolver } from "@hookform/resolvers/zod";
import { AlertCircle, Loader2, RefreshCw } from "lucide-react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

import {
  formatETag,
  isPreconditionFailedError,
} from "@workspace/api-client/client";
import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import useBook from "@workspace/api-hooks/catalog/books/useBook";
import useCreateBook from "@workspace/api-hooks/catalog/books/useCreateBook";
import useUpdateBook from "@workspace/api-hooks/catalog/books/useUpdateBook";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@workspace/ui/components/alert";
import { Button } from "@workspace/ui/components/button";
import { Form } from "@workspace/ui/components/form";
import { DEFAULT_BOOK_IMAGE } from "@workspace/utils/constants";
import {
  type CreateBookInput,
  createBookSchema,
} from "@workspace/validations/catalog/books";

import { BookFormSkeleton } from "@/components/loading-skeleton";

import { BookInfoCard } from "./book-info-card";
import { ClassificationCard } from "./classification-card";
import { ImageCard } from "./image-card";

type BookFormProps = Readonly<{
  bookId?: string;
}>;

export function BookForm({ bookId }: BookFormProps) {
  const router = useRouter();
  const [selectedAuthors, setSelectedAuthors] = useState<string[]>([]);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [imageFile, setImageFile] = useState<File | undefined>(undefined);
  const [isRemoveImage, setIsRemoveImage] = useState(false);
  const [hasVersionConflict, setHasVersionConflict] = useState(false);

  const createMutation = useCreateBook();
  const updateMutation = useUpdateBook();
  const {
    data: bookData,
    isLoading: isLoadingBook,
    etag: bookEtag,
    refetch: refetchBook,
  } = useBook(bookId ?? "", {
    enabled: !!bookId,
  });

  const { data: authorsData, isLoading: isLoadingAuthors } = useAuthors();
  const { data: categoriesData, isLoading: isLoadingCategories } =
    useCategories();
  const { data: publishersData, isLoading: isLoadingPublishers } =
    usePublishers();

  const authors = authorsData ?? [];
  const categories = categoriesData ?? [];
  const publishers = publishersData ?? [];

  const form = useForm<CreateBookInput>({
    resolver: zodResolver(createBookSchema),
    defaultValues: {
      name: "",
      description: "",
      price: undefined,
      priceSale: null,
      categoryId: "",
      publisherId: "",
      authorIds: [],
    },
  });

  useEffect(() => {
    if (bookData && bookId) {
      form.reset({
        name: bookData.name ?? "",
        description: bookData.description ?? "",
        price: bookData.price,
        priceSale: bookData.priceSale,
        categoryId: bookData.category?.id ?? "",
        publisherId: bookData.publisher?.id ?? "",
        authorIds: bookData.authors.map((a) => a.id),
      });
      setSelectedAuthors(bookData.authors.map((a) => a.id));
      if (bookData.imageUrl) {
        setImagePreview(bookData.imageUrl);
      }
      setHasVersionConflict(false);
    }
  }, [bookData, bookId, form]);

  const handleReloadLatest = () => {
    setHasVersionConflict(false);
    void refetchBook?.();
  };

  const onSubmit = (data: CreateBookInput) => {
    if (bookId) {
      const ifMatch = bookEtag ?? formatETag(bookData?.version);
      updateMutation.mutate(
        {
          request: {
            id: bookId,
            ...data,
            authorIds: selectedAuthors,
            image: imageFile,
            isRemoveImage,
          },
          ...(ifMatch && { ifMatch }),
        },
        {
          onSuccess: () => {
            setHasVersionConflict(false);
            router.push("/books");
          },
          onError: (error) => {
            if (isPreconditionFailedError(error)) {
              setHasVersionConflict(true);
              toast.error(
                "This book was modified by another user. Reload the latest version to save your changes.",
              );
            } else {
              toast.error("Failed to update book");
            }
          },
        },
      );
    } else {
      createMutation.mutate(
        {
          ...data,
          authorIds: selectedAuthors,
          image: imageFile,
        },
        {
          onSuccess: (response) => router.push(`/books?new=${response.id}`),
          onError: () => {
            toast.error("Failed to create book");
          },
        },
      );
    }
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const MAX_IMAGE_BYTES = 5 * 1024 * 1024;
      if (!file.type.startsWith("image/")) {
        toast.error("Please select a valid image file");
        return;
      }
      if (file.size > MAX_IMAGE_BYTES) {
        toast.error("Image size must be 5 MB or smaller");
        return;
      }
      const reader = new FileReader();
      reader.onloadend = () => {
        if (typeof reader.result === "string") {
          setImagePreview(reader.result);
        }
      };
      reader.readAsDataURL(file);
      setImageFile(file);
      setIsRemoveImage(false);
    }
  };

  const toggleAuthor = (authorId: string) => {
    const newAuthors = selectedAuthors.includes(authorId)
      ? selectedAuthors.filter((id) => id !== authorId)
      : [...selectedAuthors, authorId];
    setSelectedAuthors(newAuthors);
    form.setValue("authorIds", newAuthors, { shouldValidate: true });
  };

  if (bookId && isLoadingBook) {
    return <BookFormSkeleton />;
  }

  const isLoading = createMutation.isPending || updateMutation.isPending;
  const isLoadingClassificationData =
    isLoadingCategories || isLoadingPublishers || isLoadingAuthors;

  const submitLabel = bookId ? "Update Book" : "Create Book";
  const submittingLabel = bookId ? "Updating..." : "Creating...";

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        {hasVersionConflict && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>Version conflict detected</AlertTitle>
            <AlertDescription className="flex flex-wrap items-center justify-between gap-3">
              <span>
                This book was updated elsewhere since you opened it. Reload the
                latest version before saving your changes.
              </span>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handleReloadLatest}
              >
                <RefreshCw className="mr-1.5 h-3.5 w-3.5" />
                Reload latest version
              </Button>
            </AlertDescription>
          </Alert>
        )}

        <BookInfoCard form={form} />

        <ClassificationCard
          form={form}
          categories={categories}
          publishers={publishers}
          authors={authors}
          selectedAuthors={selectedAuthors}
          isLoading={isLoadingClassificationData}
          onToggleAuthor={toggleAuthor}
        />

        <ImageCard
          bookId={bookId}
          bookName={bookData?.name ?? undefined}
          imagePreview={imagePreview}
          onImageChange={handleImageChange}
          onImageRemove={() => {
            setImagePreview(null);
            setImageFile(undefined);
            setIsRemoveImage(true);
          }}
          onImageError={() => setImagePreview(DEFAULT_BOOK_IMAGE)}
        />

        <div className="flex gap-4">
          <Button type="button" variant="outline" onClick={() => router.back()}>
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={
              isLoading ||
              selectedAuthors.length === 0 ||
              !form.watch("categoryId") ||
              !form.watch("publisherId")
            }
          >
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                {submittingLabel}
              </>
            ) : (
              submitLabel
            )}
          </Button>
        </div>
      </form>
    </Form>
  );
}
