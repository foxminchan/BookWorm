"use client";

import type React from "react";
import { useEffect, useState } from "react";

import { useRouter } from "next/navigation";

import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useForm } from "react-hook-form";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import useBook from "@workspace/api-hooks/catalog/books/useBook";
import useCreateBook from "@workspace/api-hooks/catalog/books/useCreateBook";
import useUpdateBook from "@workspace/api-hooks/catalog/books/useUpdateBook";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
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

  const createMutation = useCreateBook();
  const updateMutation = useUpdateBook();
  const { data: bookData, isLoading: isLoadingBook } = useBook(bookId ?? "", {
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
    }
  }, [bookData, bookId, form]);

  const onSubmit = async (data: CreateBookInput) => {
    if (bookId) {
      updateMutation.mutate(
        {
          request: {
            id: bookId,
            ...data,
            authorIds: selectedAuthors,
            image: imageFile,
            isRemoveImage,
          },
        },
        {
          onSuccess: () => router.push("/books"),
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
        },
      );
    }
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result as string);
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
