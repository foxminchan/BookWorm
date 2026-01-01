"use client";

import { Star, Send, Loader2 } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@workspace/ui/components/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@workspace/ui/components/form";
import { Input } from "@workspace/ui/components/input";
import { Textarea } from "@workspace/ui/components/textarea";
import { createFeedbackSchema } from "@workspace/validations/rating";
import cn from "classnames";

const reviewFormSchema = createFeedbackSchema.omit({ bookId: true }).extend({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  rating: z
    .number()
    .int("Rating must be an integer")
    .min(1, "Please select a rating")
    .max(5, "Rating must be between 1 and 5"),
});

type ReviewFormValues = z.infer<typeof reviewFormSchema>;

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
}: ReviewFormProps) {
  const form = useForm<ReviewFormValues>({
    resolver: zodResolver(reviewFormSchema),
    values: {
      firstName,
      lastName,
      comment: comment || "",
      rating,
    },
  });

  const handleSubmit = form.handleSubmit(() => {
    onSubmit();
  });

  return (
    <div className="bg-background border border-border p-6 rounded-2xl shadow-sm space-y-4">
      <h3 className="font-serif font-medium text-lg">Your Review</h3>
      <Form {...form}>
        <form onSubmit={handleSubmit} className="space-y-3">
          <FormField
            control={form.control}
            name="rating"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Rating</FormLabel>
                <FormControl>
                  <div className="flex gap-1">
                    {[1, 2, 3, 4, 5].map((star) => (
                      <button
                        key={star}
                        type="button"
                        onClick={() => {
                          field.onChange(star);
                          onChange("rating", star);
                        }}
                        className="focus:outline-none transition-transform active:scale-90"
                      >
                        <Star
                          className={cn(
                            "size-6",
                            star <= field.value
                              ? "fill-primary text-primary"
                              : "text-muted-foreground/30",
                          )}
                        />
                      </button>
                    ))}
                  </div>
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="grid grid-cols-2 gap-3">
            <FormField
              control={form.control}
              name="firstName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>First Name</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="First Name"
                      className="bg-secondary/50 border-none rounded-lg"
                      {...field}
                      onChange={(e) => {
                        field.onChange(e);
                        onChange("firstName", e.target.value);
                      }}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="lastName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Last Name</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Last Name"
                      className="bg-secondary/50 border-none rounded-lg"
                      {...field}
                      onChange={(e) => {
                        field.onChange(e);
                        onChange("lastName", e.target.value);
                      }}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <FormField
            control={form.control}
            name="comment"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Comment</FormLabel>
                <FormControl>
                  <Textarea
                    placeholder="Share your thoughts..."
                    rows={4}
                    className="bg-secondary/50 border-none rounded-lg resize-none"
                    {...field}
                    value={field.value || ""}
                    onChange={(e) => {
                      field.onChange(e);
                      onChange("comment", e.target.value);
                    }}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <Button
            type="submit"
            className="w-full rounded-full gap-2"
            disabled={isSubmitting || !form.formState.isValid}
          >
            {isSubmitting ? (
              <>
                <Loader2 className="size-4 animate-spin" />
                Submitting...
              </>
            ) : (
              <>
                <Send className="size-4" /> Submit Review
              </>
            )}
          </Button>
        </form>
      </Form>
    </div>
  );
}
