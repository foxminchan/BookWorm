import { type FormStore, Field as FormischField } from "@formisch/react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@workspace/ui/components/field";
import { Input } from "@workspace/ui/components/input";
import { Textarea } from "@workspace/ui/components/textarea";
import type { createBookSchema } from "@workspace/validations/catalog/books";

type BookInfoCardProps = Readonly<{
  form: FormStore<typeof createBookSchema>;
}>;

export function BookInfoCard({ form }: BookInfoCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Book Information</CardTitle>
        <CardDescription>
          Enter the basic details about the book
        </CardDescription>
      </CardHeader>
      <CardContent>
        <FieldGroup className="gap-4">
          <FormischField of={form} path={["name"]}>
            {(field) => (
              <Field data-invalid={!!field.errors}>
                <FieldLabel htmlFor="book-name">Book Name</FieldLabel>
                <Input
                  {...field.props}
                  id="book-name"
                  placeholder="Enter book name"
                  value={field.input ?? ""}
                  aria-invalid={!!field.errors}
                />
                <FieldError
                  errors={field.errors?.map((message) => ({ message }))}
                />
              </Field>
            )}
          </FormischField>

          <FormischField of={form} path={["description"]}>
            {(field) => (
              <Field data-invalid={!!field.errors}>
                <FieldLabel htmlFor="book-description">Description</FieldLabel>
                <Textarea
                  {...field.props}
                  id="book-description"
                  placeholder="Enter book description"
                  value={field.input ?? ""}
                  aria-invalid={!!field.errors}
                />
                <FieldError
                  errors={field.errors?.map((message) => ({ message }))}
                />
              </Field>
            )}
          </FormischField>

          <div className="grid grid-cols-2 gap-4">
            <FormischField of={form} path={["price"]}>
              {(field) => (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="book-price">Price</FieldLabel>
                  <Input
                    {...field.props}
                    id="book-price"
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    value={field.input ?? ""}
                    onChange={(e) =>
                      field.onChange(
                        e.target.value
                          ? Number.parseFloat(e.target.value)
                          : undefined,
                      )
                    }
                    aria-invalid={!!field.errors}
                  />
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              )}
            </FormischField>

            <FormischField of={form} path={["priceSale"]}>
              {(field) => (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="book-price-sale">
                    Sale Price (Optional)
                  </FieldLabel>
                  <Input
                    {...field.props}
                    id="book-price-sale"
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    value={field.input ?? ""}
                    onChange={(e) =>
                      field.onChange(
                        e.target.value
                          ? Number.parseFloat(e.target.value)
                          : null,
                      )
                    }
                    aria-invalid={!!field.errors}
                  />
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              )}
            </FormischField>
          </div>
        </FieldGroup>
      </CardContent>
    </Card>
  );
}
