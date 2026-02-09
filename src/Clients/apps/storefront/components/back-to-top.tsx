"use client";

import { useEffect, useState } from "react";

import { ArrowUp } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

export function BackToTop() {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const toggleVisibility = () => {
      setIsVisible(window.scrollY > 300);
    };

    window.addEventListener("scroll", toggleVisibility, { passive: true });
    return () => window.removeEventListener("scroll", toggleVisibility);
  }, []);

  const scrollToTop = () => {
    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  if (!isVisible) return null;

  return (
    <Button
      onClick={scrollToTop}
      className="border-primary group fixed right-6 bottom-24 z-40 hidden h-12 w-12 items-center justify-center rounded-full border-2 bg-transparent shadow-xl transition-transform duration-300 hover:scale-110 md:flex"
      aria-label="Scroll to top of page"
      title="Back to top"
      type="button"
    >
      <ArrowUp
        className="text-primary h-5 w-5 transition-colors duration-300 group-hover:text-white"
        aria-hidden="true"
      />
      <span className="sr-only">Scroll to top</span>
    </Button>
  );
}
