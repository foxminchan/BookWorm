"use client";

import { useEffect } from "react";

import {
  Form,
  Field as FormischField,
  getInput,
  reset,
  setInput,
  useForm,
} from "@formisch/react";
import { Loader2, Send, Star } from "lucide-react";
import * as v from "valibot";

import { Button } from "@workspace/ui/components/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@workspace/ui/components/field";
import { Input } from "@workspace/ui/components/input";
import { Textarea } from "@workspace/ui/components/textarea";
import { cn } from "@workspace/ui/lib/utils";
import { createFeedbackSchema } from "@workspace/validations/rating";

const reviewFormSchema = v.object({
  ...v.omit(createFeedbackSchema, ["bookId"]).entries,
  firstName: v.pipe(v.string(), v.minLength(1, "First name is required")),
  lastName: v.pipe(v.string(), v.minLength(1, "Last name is required")),
  rating: v.pipe(
    v.number(),
    v.integer("Rating must be an integer"),
    v.minValue(1, "Please select a rating"),
    v.maxValue(5, "Rating must be between 1 and 5"),
  ),
});

type ReviewFormProps = {
  firstName: string;
  lastName: string;
  comment: string;
  rating: number;
  isSubmitting: boolean;
  onChange: (field: string, value: string | number) => void;
  onSubmit: () => void;
};

export default function ReviewForm({
  firstName,
  lastName,
  comment,
  rating,
  isSubmitting,
  onChange,
  onSubmit,
}: Readonly<ReviewFormProps>) {
  const form = useForm({
    schema: reviewFormSchema,
    initialInput: {
      firstName,
      lastName,
      comment: comment || "",
      rating,
    },
  });

  useEffect(() => {
    const current = getInput(form);
    const normalizedComment = comment || "";
    if (
      current.firstName !== firstName ||
      current.lastName !== lastName ||
      (current.comment ?? "") !== normalizedComment ||
      current.rating !== rating
    ) {
      reset(form, {
        initialInput: {
          firstName,
          lastName,
          comment: normalizedComment,
          rating,
        },
      });
    }
  }, [comment, firstName, form, lastName, rating]);

  const isFormValid = v.safeParse(reviewFormSchema, getInput(form)).success;

  return (
    <div className="bg-background border-border space-y-4 rounded-2xl border p-6 shadow-sm">
      <h3 className="font-serif text-lg font-medium">Your Review</h3>
      <Form of={form} onSubmit={() => onSubmit()} className="space-y-3">
        <FieldGroup className="gap-3">
          <FormischField of={form} path={["rating"]}>
            {(field) => {
              const currentRating = field.input ?? 0;
              return (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="review-rating">Rating</FieldLabel>
                  <div id="review-rating" className="flex gap-1">
                    {[1, 2, 3, 4, 5].map((star) => (
                      <Button
                        key={star}
                        type="button"
                        variant="ghost"
                        size="icon"
                        aria-label={`Rate ${star} out of 5 stars`}
                        aria-pressed={star <= currentRating}
                        onClick={() => {
                          setInput(form, { path: ["rating"], input: star });
                          onChange("rating", star);
                        }}
                        className="h-8 w-8 p-0 hover:bg-transparent focus-visible:ring-0"
                      >
                        <Star
                          aria-hidden="true"
                          className={cn(
                            "size-6",
                            star <= currentRating
                              ? "fill-primary text-primary"
                              : "text-muted-foreground/30",
                          )}
                        />
                      </Button>
                    ))}
                  </div>
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              );
            }}
          </FormischField>

          <div className="grid grid-cols-2 gap-3">
            <FormischField of={form} path={["firstName"]}>
              {(field) => (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="review-first-name">
                    First Name
                  </FieldLabel>
                  <Input
                    {...field.props}
                    id="review-first-name"
                    placeholder="First Name"
                    className="bg-secondary/50 rounded-lg border-none"
                    value={field.input ?? ""}
                    onChange={(e) => {
                      field.props.onChange(e);
                      onChange("firstName", e.target.value);
                    }}
                    aria-invalid={!!field.errors}
                  />
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              )}
            </FormischField>

            <FormischField of={form} path={["lastName"]}>
              {(field) => (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="review-last-name">Last Name</FieldLabel>
                  <Input
                    {...field.props}
                    id="review-last-name"
                    placeholder="Last Name"
                    className="bg-secondary/50 rounded-lg border-none"
                    value={field.input ?? ""}
                    onChange={(e) => {
                      field.props.onChange(e);
                      onChange("lastName", e.target.value);
                    }}
                    aria-invalid={!!field.errors}
                  />
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              )}
            </FormischField>
          </div>

          <FormischField of={form} path={["comment"]}>
            {(field) => (
              <Field data-invalid={!!field.errors}>
                <FieldLabel htmlFor="review-comment">Comment</FieldLabel>
                <Textarea
                  {...field.props}
                  id="review-comment"
                  placeholder="Share your thoughts..."
                  rows={4}
                  className="bg-secondary/50 resize-none rounded-lg border-none"
                  value={field.input ?? ""}
                  onChange={(e) => {
                    field.props.onChange(e);
                    onChange("comment", e.target.value);
                  }}
                  aria-invalid={!!field.errors}
                />
                <FieldError
                  errors={field.errors?.map((message) => ({ message }))}
                />
              </Field>
            )}
          </FormischField>
        </FieldGroup>

        <Button
          type="submit"
          className="w-full gap-2 rounded-full"
          disabled={isSubmitting || !isFormValid}
        >
          {isSubmitting ? (
            <>
              <Loader2 className="size-4 animate-spin" aria-hidden="true" />
              Submitting...
            </>
          ) : (
            <>
              <Send className="size-4" aria-hidden="true" /> Submit Review
            </>
          )}
        </Button>
      </Form>
    </div>
  );
}
