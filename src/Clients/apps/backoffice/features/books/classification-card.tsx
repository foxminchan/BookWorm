import type { UseFormReturn } from "react-hook-form";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { Checkbox } from "@workspace/ui/components/checkbox";
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@workspace/ui/components/form";
import { Label } from "@workspace/ui/components/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import type { CreateBookInput } from "@workspace/validations/catalog/books";

import { ClassificationSkeleton } from "@/components/loading-skeleton";

type ClassificationCardProps = Readonly<{
  form: UseFormReturn<CreateBookInput>;
  categories: { id: string; name: string | null }[];
  publishers: { id: string; name: string | null }[];
  authors: { id: string; name: string | null }[];
  selectedAuthors: string[];
  isLoading: boolean;
  onToggleAuthor: (authorId: string) => void;
}>;

export function ClassificationCard({
  form,
  categories,
  publishers,
  authors,
  selectedAuthors,
  isLoading,
  onToggleAuthor,
}: ClassificationCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Classification</CardTitle>
        <CardDescription>
          Select category, publisher, and authors
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {isLoading ? (
          <ClassificationSkeleton />
        ) : (
          <>
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

            <fieldset>
              <legend className="text-foreground mb-3 block text-sm font-medium">
                Authors
              </legend>
              <div className="space-y-2">
                {authors.map((author) => (
                  <div key={author.id} className="flex items-center gap-2">
                    <Checkbox
                      id={`author-${author.id}`}
                      checked={selectedAuthors.includes(author.id)}
                      onCheckedChange={() => onToggleAuthor(author.id)}
                    />
                    <Label
                      htmlFor={`author-${author.id}`}
                      className="text-foreground cursor-pointer text-sm font-normal"
                    >
                      {author.name}
                    </Label>
                  </div>
                ))}
              </div>
              {selectedAuthors.length === 0 && (
                <p
                  className="text-destructive mt-1 text-sm"
                  role="alert"
                  aria-live="polite"
                >
                  At least one author is required
                </p>
              )}
            </fieldset>
          </>
        )}
      </CardContent>
    </Card>
  );
}
