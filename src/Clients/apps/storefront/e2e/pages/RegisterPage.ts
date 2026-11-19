import { AuthRedirectPage } from "./AuthRedirectPage";

/**
 * Register Page Object Model
 * Represents the registration page at '/register'
 */
export class RegisterPage extends AuthRedirectPage {
  readonly path = "/register";
  protected readonly headingText = "Redirecting to registration...";
  protected readonly descriptionText =
    "Please wait while we redirect you to the registration page.";

  isOnRegisterPage(): boolean {
    return this.page.url().includes(this.path);
  }
}
