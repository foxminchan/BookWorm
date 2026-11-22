"use client";

import { useHumanInTheLoop } from "@copilotkit/react-core/v2";
import { toStandardJsonSchema } from "@valibot/to-json-schema";
import { HelpCircle } from "lucide-react";
import * as v from "valibot";

import { Button } from "@workspace/ui/components/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

const routeOptionSchema = v.object({
  id: v.string(),
  label: v.string(),
  description: v.string(),
});

const disambiguateRouteParameters = toStandardJsonSchema(
  v.object({
    question: v.pipe(
      v.string(),
      v.description("Clarifying question to present to the user"),
    ),
    suggestedRoute: v.pipe(
      v.string(),
      v.description("The top-scoring candidate route identifier"),
    ),
    confidence: v.pipe(
      v.number(),
      v.description("Router confidence score for the suggested route"),
    ),
    options: v.pipe(
      v.array(routeOptionSchema),
      v.description("Available specialist route options"),
    ),
  }),
);

export function useRouteDisambiguation() {
  useHumanInTheLoop({
    name: "disambiguateRoute",
    description:
      "Ask the user to choose which BookWorm specialist should handle an ambiguous request.",
    parameters: disambiguateRouteParameters,
    render: ({ args, status, respond }) => {
      if (status === "complete") {
        return <></>;
      }

      const options = args.options ?? [];

      return (
        <Card className="border-amber-200 bg-amber-50/50 dark:border-amber-900 dark:bg-amber-950/30">
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-semibold">
              <HelpCircle className="size-4 text-amber-600 dark:text-amber-400" />
              Choose a Specialist
            </CardTitle>
            <CardDescription className="text-xs">
              {args.question ||
                "Which area best matches what you need help with?"}
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-2">
            {options.map((option) => {
              const isSuggested = option.id === args.suggestedRoute;
              return (
                <Button
                  key={option.id}
                  type="button"
                  variant={isSuggested ? "default" : "outline"}
                  className="h-auto flex-col items-start gap-0.5 px-3 py-2 text-left whitespace-normal"
                  disabled={!respond || status !== "executing"}
                  onClick={() => respond?.(option.id)}
                >
                  <span className="text-xs font-semibold">
                    {option.label}
                    {isSuggested ? " (Suggested)" : ""}
                  </span>
                  <span className="text-[11px] font-normal opacity-80">
                    {option.description}
                  </span>
                </Button>
              );
            })}
          </CardContent>
        </Card>
      );
    },
  });
}
