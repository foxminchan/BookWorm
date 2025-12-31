"use client";

import { useState } from "react";
import Link from "next/link";
import { PolicyDialog } from "@/components/policy-dialog";

const PRIVACY_POLICY_CONTENT = `
# Privacy Policy

Last updated: December 30, 2025

This Privacy Policy describes Our policies and procedures on the collection, use and disclosure of Your information when You use the Service and tells You about Your privacy rights and how the law protects You.

We use Your Personal data to provide and improve the Service. By using the Service, You agree to the collection and use of information in accordance with this Privacy Policy.

## Interpretation and Definitions

### Interpretation

The words whose initial letters are capitalized have meanings defined under the following conditions.

### Definitions

For the purposes of this Privacy Policy:

- **Account** means a unique account created for You to access our Service or parts of our Service.
- **Company** refers to BookWorm.
- **Personal Data** is any information that relates to an identified or identifiable individual.
- **Service** refers to the Website.
- **Website** refers to BookWorm.

## Collecting and Using Your Personal Data

### Types of Data Collected

While using Our Service, We may ask You to provide Us with certain personally identifiable information that can be used to contact or identify You. This may include:

- Email address
- First name and last name
- Phone number
- Address, State, Province, ZIP/Postal code, City

### Use of Your Personal Data

The Company may use Personal Data for the following purposes:

- To provide and maintain our Service
- To manage Your Account
- To contact You regarding updates or information
- For business transfers and analysis

## Security of Your Personal Data

The security of Your Personal Data is important to Us, but no method of transmission over the Internet is 100% secure.

## Contact Us

If you have any questions about this Privacy Policy, you can contact us at: nguyenxuannhan407@gmail.com
`;

const TERMS_OF_SERVICE_CONTENT = `
# Terms and Conditions

Last updated: December 30, 2025

Please read these terms and conditions carefully before using Our Service.

## Interpretation and Definitions

### Interpretation

The words whose initial letters are capitalized have meanings defined under the following conditions.

### Definitions

For the purposes of these Terms and Conditions:

- **Company** refers to BookWorm.
- **Service** refers to the Website.
- **Website** refers to BookWorm.
- **You** means the individual accessing or using the Service.

## Acknowledgment

These Terms and Conditions form the entire agreement between You and the Company. By accessing or using the Service, You agree to be bound by these Terms and Conditions.

Your access to and use of the Service is conditioned on Your acceptance of and compliance with these Terms and Conditions.

## Links to Other Websites

Our Service may contain links to third-party web sites or services that are not owned or controlled by the Company. The Company has no control over the content or practices of any third party websites.

## Termination

We may terminate or suspend Your access immediately for any reason whatsoever, including without limitation if You breach these Terms and Conditions.

## Governing Law

The laws of Vietnam shall govern this Terms and Your use of the Service.

## Contact Us

If you have any questions about these Terms and Conditions, you can contact us at: nguyenxuannhan407@gmail.com
`;

export function Footer() {
  const [privacyOpen, setPrivacyOpen] = useState(false);
  const [termsOpen, setTermsOpen] = useState(false);

  return (
    <>
      <footer
        className="hidden md:block border-t bg-secondary py-12"
        role="contentinfo"
      >
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div className="col-span-1 md:col-span-2">
              <Link
                href="/"
                className="text-xl text-foreground font-bold mb-4 block"
              >
                BookWorm
              </Link>
              <p className="text-muted-foreground text-sm max-w-xs leading-relaxed">
                Curating the finest selection of literature and design
                inspiration for the modern reader.
              </p>
            </div>
            <nav aria-label="Quick Links">
              <h3 className="font-bold text-sm mb-4 uppercase tracking-wider">
                Quick Links
              </h3>
              <ul className="space-y-2 text-sm text-muted-foreground">
                <li>
                  <Link href="/shop" className="hover:text-primary">
                    Books
                  </Link>
                </li>
                <li>
                  <Link href="/categories" className="hover:text-primary">
                    Categories
                  </Link>
                </li>
                <li>
                  <Link href="/publishers" className="hover:text-primary">
                    Publishers
                  </Link>
                </li>
              </ul>
            </nav>
            <nav aria-label="Help">
              <h3 className="font-bold text-sm mb-4 uppercase tracking-wider">
                Help
              </h3>
              <ul className="space-y-2 text-sm text-muted-foreground">
                <li>
                  <Link href="/shipping" className="hover:text-primary">
                    Shipping Info
                  </Link>
                </li>
                <li>
                  <Link href="/returns" className="hover:text-primary">
                    Returns
                  </Link>
                </li>
                <li>
                  <Link href="/about" className="hover:text-primary">
                    Our Story
                  </Link>
                </li>
              </ul>
            </nav>
          </div>
          <div className="mt-12 pt-8 border-t border-border flex flex-col md:flex-row justify-between items-center gap-4 text-xs text-muted-foreground">
            <p>© {new Date().getFullYear()} BookWorm. All rights reserved.</p>
            <div className="flex gap-6">
              <button
                onClick={() => setPrivacyOpen(true)}
                className="hover:text-primary transition-colors cursor-pointer"
              >
                Privacy Policy
              </button>
              <button
                onClick={() => setTermsOpen(true)}
                className="hover:text-primary transition-colors cursor-pointer"
              >
                Terms of Service
              </button>
            </div>
          </div>
        </div>
      </footer>

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
