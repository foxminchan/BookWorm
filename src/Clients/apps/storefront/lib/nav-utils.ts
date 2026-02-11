/**
 * Check whether a navigation link is active based on the current pathname.
 * Matches the exact path or any nested child route.
 */
export function isNavActive(pathname: string, href: string): boolean {
  return pathname === href || pathname.startsWith(`${href}/`);
}
