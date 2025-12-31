"use client";

import { useEffect, useState } from "react";
import { ArrowUp } from "lucide-react";
import { Button } from "@workspace/ui/components/button";

export function BackToTop() {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const toggleVisibility = () => {
      if (window.scrollY > 300) {
        setIsVisible(true);
      } else {
        setIsVisible(false);
      }
    };

    window.addEventListener("scroll", toggleVisibility);
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
      className="fixed bottom-24 right-6 w-12 h-12 rounded-full shadow-xl hover:scale-110 transition-transform duration-300 bg-transparent border-2 border-primary z-40 group hidden md:flex items-center justify-center"
      aria-label="Scroll to top of page"
      title="Back to top"
    >
      <ArrowUp
        className="w-5 h-5 text-primary group-hover:text-white transition-colors duration-300"
        aria-hidden="true"
      />
    </Button>
  );
}
