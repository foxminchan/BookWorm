import {
  type FormStore,
  Field as FormischField,
  setInput,
} from "@formisch/react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { Checkbox } from "@workspace/ui/components/checkbox";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@workspace/ui/components/field";
import { Label } from "@workspace/ui/components/label";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import type { createBookSchema } from "@workspace/validations/catalog/books";

import { ClassificationSkeleton } from "@/components/loading-skeleton";

type ClassificationCardProps = Readonly<{
  form: FormStore<typeof createBookSchema>;
  categories: { id: string; name: string | null }[];
  publishers: { id: string; name: string | null }[];
  authors: { id: string; name: string | null }[];
  isLoading: boolean;
}>;

export function ClassificationCard({
  form,
  categories,
  publishers,
  authors,
  isLoading,
}: ClassificationCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Classification</CardTitle>
        <CardDescription>
          Select category, publisher, and authors
        </CardDescription>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <ClassificationSkeleton />
        ) : (
          <FieldGroup className="gap-4">
            <FormischField of={form} path={["categoryId"]}>
              {(field) => (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="book-category">Category</FieldLabel>
                  <Select
                    value={field.input ?? ""}
                    onValueChange={(value) =>
                      setInput(form, {
                        path: ["categoryId"],
                        input: value ?? "",
                      })
                    }
                  >
                    <SelectTrigger
                      id="book-category"
                      className="w-full"
                      aria-invalid={!!field.errors}
                    >
                      <SelectValue placeholder="Select a category" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        {categories.map((cat) => (
                          <SelectItem key={cat.id} value={cat.id}>
                            {cat.name}
                          </SelectItem>
                        ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              )}
            </FormischField>

            <FormischField of={form} path={["publisherId"]}>
              {(field) => (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="book-publisher">Publisher</FieldLabel>
                  <Select
                    value={field.input ?? ""}
                    onValueChange={(value) =>
                      setInput(form, {
                        path: ["publisherId"],
                        input: value ?? "",
                      })
                    }
                  >
                    <SelectTrigger
                      id="book-publisher"
                      className="w-full"
                      aria-invalid={!!field.errors}
                    >
                      <SelectValue placeholder="Select a publisher" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectGroup>
                        {publishers.map((pub) => (
                          <SelectItem key={pub.id} value={pub.id}>
                            {pub.name}
                          </SelectItem>
                        ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              )}
            </FormischField>

            <FormischField of={form} path={["authorIds"]}>
              {(field) => {
                const selectedAuthors = field.input ?? [];
                const selectedAuthorSet = new Set(selectedAuthors);
                const authorErrors =
                  field.errors ??
                  (selectedAuthors.length === 0
                    ? ["At least one author is required"]
                    : null);

                const handleToggleAuthor = (authorId: string) => {
                  const nextAuthors = selectedAuthorSet.has(authorId)
                    ? selectedAuthors.filter((id) => id !== authorId)
                    : [...selectedAuthors, authorId];
                  setInput(form, {
                    path: ["authorIds"],
                    input: nextAuthors,
                  });
                };

                return (
                  <FieldSet className="gap-3">
                    <FieldLegend variant="label" className="mb-0">
                      Authors
                    </FieldLegend>
                    <div className="space-y-2">
                      {authors.map((author) => (
                        <div
                          key={author.id}
                          className="flex items-center gap-2"
                        >
                          <Checkbox
                            id={`author-${author.id}`}
                            checked={selectedAuthorSet.has(author.id)}
                            onCheckedChange={() =>
                              handleToggleAuthor(author.id)
                            }
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
                    <FieldError
                      errors={authorErrors?.map((message) => ({ message }))}
                    />
                  </FieldSet>
                );
              }}
            </FormischField>
          </FieldGroup>
        )}
      </CardContent>
    </Card>
  );
}
