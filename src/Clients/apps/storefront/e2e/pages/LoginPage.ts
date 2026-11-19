import { AuthRedirectPage } from "./AuthRedirectPage";

/**
 * Login Page Object Model
 * Represents the login page at '/login'
 */
export class LoginPage extends AuthRedirectPage {
  readonly path = "/login";
  protected readonly headingText = "Redirecting to login...";
  protected readonly descriptionText =
    "Please wait while we redirect you to the login page.";

  isOnLoginPage(): boolean {
    return this.page.url().includes(this.path);
  }
}
