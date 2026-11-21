import { Suspense } from "react";

import Link from "next/link";

import { CopyrightYear } from "@/components/copyright-year";
import { FooterPolicies } from "@/components/footer-policies";
import { APP_CONFIG } from "@/config/app-config";
import {
  STOREFRONT_FOOTER_HELP_LINKS,
  STOREFRONT_FOOTER_QUICK_LINKS,
} from "@/config/nav-config";

type FooterNavProps = {
  readonly label: string;
  readonly links: ReadonlyArray<{
    readonly href: string;
    readonly label: string;
  }>;
};

function FooterNav({ label, links }: FooterNavProps) {
  return (
    <nav aria-label={label}>
      <h3 className="mb-4 text-sm font-bold tracking-wider uppercase">
        {label}
      </h3>
      <ul className="text-muted-foreground space-y-2 text-sm">
        {links.map((link) => (
          <li key={link.href}>
            <Link href={link.href} className="hover:text-primary">
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}

export function Footer() {
  return (
    <footer className="bg-secondary hidden border-t py-12 md:block">
      <div className="container mx-auto px-4">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-4">
          <div className="col-span-1 md:col-span-2">
            <Link
              href="/"
              className="text-foreground mb-4 block text-xl font-bold"
            >
              {APP_CONFIG.name}
            </Link>
            <p className="text-muted-foreground max-w-xs text-sm leading-relaxed">
              Curating the finest selection of literature and design inspiration
              for the modern reader.
            </p>
          </div>
          <FooterNav
            label="Quick Links"
            links={STOREFRONT_FOOTER_QUICK_LINKS}
          />
          <FooterNav label="Help" links={STOREFRONT_FOOTER_HELP_LINKS} />
        </div>
        <div className="border-border text-muted-foreground mt-12 flex flex-col items-center justify-between gap-4 border-t pt-8 text-xs md:flex-row">
          <p>
            &copy;{" "}
            <Suspense fallback={<span>2026</span>}>
              <CopyrightYear />
            </Suspense>{" "}
            {APP_CONFIG.name}. All rights reserved.
          </p>
          <FooterPolicies />
        </div>
      </div>
    </footer>
  );
}
