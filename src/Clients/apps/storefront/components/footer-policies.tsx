"use client";

import { useState } from "react";

import { Button } from "@workspace/ui/components/button";

import { PolicyDialog } from "@/components/policy-dialog";
import {
  PRIVACY_POLICY_CONTENT,
  TERMS_OF_SERVICE_CONTENT,
} from "@/lib/policy-content";

export function FooterPolicies() {
  const [privacyOpen, setPrivacyOpen] = useState(false);
  const [termsOpen, setTermsOpen] = useState(false);

  return (
    <>
      <div className="flex gap-6">
        <Button
          type="button"
          variant="ghost"
          onClick={() => setPrivacyOpen(true)}
          className="hover:text-primary h-auto cursor-pointer p-0 font-normal transition-colors"
          aria-label="Open privacy policy"
        >
          Privacy Policy
        </Button>
        <Button
          type="button"
          variant="ghost"
          onClick={() => setTermsOpen(true)}
          className="hover:text-primary h-auto cursor-pointer p-0 font-normal transition-colors"
          aria-label="Open terms of service"
        >
          Terms of Service
        </Button>
      </div>
      <PolicyDialog
        policyType="privacy"
        isOpen={privacyOpen}
        onOpenChange={setPrivacyOpen}
        content={PRIVACY_POLICY_CONTENT}
      />
      <PolicyDialog
        policyType="terms"
        isOpen={termsOpen}
        onOpenChange={setTermsOpen}
        content={TERMS_OF_SERVICE_CONTENT}
      />
    </>
  );
}
