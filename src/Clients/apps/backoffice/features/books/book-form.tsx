"use client";

import type React from "react";
import { useEffect, useState } from "react";

import Image from "next/image";
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
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { Checkbox } from "@workspace/ui/components/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@workspace/ui/components/form";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import { Textarea } from "@workspace/ui/components/textarea";
import {
  type CreateBookInput,
  createBookSchema,
} from "@workspace/validations/catalog/books";

import { BookFormSkeleton } from "@/components/loading-skeleton";
import { DEFAULT_BOOK_IMAGE } from "@/lib/constants";

type BookFormProps = {
  bookId?: string;
};

export function BookForm({ bookId }: BookFormProps) {
  const router = useRouter();
  const [selectedAuthors, setSelectedAuthors] = useState<string[]>([]);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [imageFile, setImageFile] = useState<File | undefined>(undefined);
  const [isRemoveImage, setIsRemoveImage] = useState(false);

  const createMutation = useCreateBook();
  const updateMutation = useUpdateBook();
  const { data: bookData, isLoading: isLoadingBook } = useBook(bookId || "", {
    enabled: !!bookId,
  });

  const { data: authorsData } = useAuthors();
  const { data: categoriesData } = useCategories();
  const { data: publishersData } = usePublishers();

  const authors = authorsData || [];
  const categories = categoriesData || [];
  const publishers = publishersData || [];

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
        name: bookData.name || "",
        description: bookData.description || "",
        price: bookData.price,
        priceSale: bookData.priceSale,
        categoryId: bookData.category?.id || "",
        publisherId: bookData.publisher?.id || "",
        authorIds: bookData.authors.map((a) => a.id),
      });
      setSelectedAuthors(bookData.authors.map((a) => a.id));
      if (bookData.imageUrl) {
        setImagePreview(bookData.imageUrl);
      }
    }
  }, [bookData, bookId, form]);

  const onSubmit = (data: CreateBookInput) => {
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
          onSuccess: () => router.push("/books"),
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
    setSelectedAuthors((prev) =>
      prev.includes(authorId)
        ? prev.filter((id) => id !== authorId)
        : [...prev, authorId],
    );
  };

  if (bookId && isLoadingBook) {
    return <BookFormSkeleton />;
  }

  const isLoading = createMutation.isPending || updateMutation.isPending;

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Book Information</CardTitle>
            <CardDescription>
              Enter the basic details about the book
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Book Name</FormLabel>
                  <FormControl>
                    <Input placeholder="Enter book name" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <Textarea placeholder="Enter book description" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="price"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Price</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="0.01"
                        min="0"
                        placeholder="0.00"
                        {...field}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value
                              ? Number.parseFloat(e.target.value)
                              : undefined,
                          )
                        }
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="priceSale"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Sale Price (Optional)</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="0.01"
                        min="0"
                        placeholder="0.00"
                        {...field}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value
                              ? Number.parseFloat(e.target.value)
                              : null,
                          )
                        }
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Classification</CardTitle>
            <CardDescription>
              Select category, publisher, and authors
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <FormField
              control={form.control}
              name="categoryId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Category</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    defaultValue={field.value}
                    value={field.value}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select a category" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {categories.map((cat) => (
                        <SelectItem key={cat.id} value={cat.id}>
                          {cat.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="publisherId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Publisher</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    defaultValue={field.value}
                    value={field.value}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select a publisher" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {publishers.map((pub) => (
                        <SelectItem key={pub.id} value={pub.id}>
                          {pub.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div>
              <Label className="text-foreground mb-3 block text-sm font-medium">
                Authors
              </Label>
              <div className="space-y-2">
                {authors.map((author) => (
                  <Label
                    key={author.id}
                    className="flex cursor-pointer items-center gap-2"
                  >
                    <Checkbox
                      checked={selectedAuthors.includes(author.id)}
                      onCheckedChange={() => toggleAuthor(author.id)}
                    />
                    <span className="text-foreground">{author.name}</span>
                  </Label>
                ))}
              </div>
              {selectedAuthors.length === 0 && (
                <p className="text-destructive mt-1 text-sm">
                  At least one author is required
                </p>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Image</CardTitle>
            <CardDescription>
              Upload a book cover image (optional)
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="border-border rounded-lg border-2 border-dashed p-8">
              {imagePreview ? (
                <div className="flex flex-col items-center gap-4">
                  <Image
                    src={imagePreview || DEFAULT_BOOK_IMAGE}
                    alt="Preview"
                    width={96}
                    height={128}
                    onError={() => setImagePreview(DEFAULT_BOOK_IMAGE)}
                    className="rounded object-cover"
                  />
                  <div className="flex w-full flex-col gap-2">
                    <Input
                      type="file"
                      accept="image/*"
                      onChange={handleImageChange}
                    />
                    {bookId && (
                      <Button
                        type="button"
                        variant="destructive"
                        size="sm"
                        onClick={() => {
                          setImagePreview(null);
                          setImageFile(undefined);
                          setIsRemoveImage(true);
                        }}
                      >
                        Remove Image
                      </Button>
                    )}
                  </div>
                </div>
              ) : (
                <Input
                  type="file"
                  accept="image/*"
                  onChange={handleImageChange}
                  placeholder="Select image"
                />
              )}
            </div>
          </CardContent>
        </Card>

        <div className="flex gap-4">
          <Button type="button" variant="outline" onClick={() => router.back()}>
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={isLoading || selectedAuthors.length === 0}
          >
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                {bookId ? "Updating..." : "Creating..."}
              </>
            ) : bookId ? (
              "Update Book"
            ) : (
              "Create Book"
            )}
          </Button>
        </div>
      </form>
    </Form>
  );
}
