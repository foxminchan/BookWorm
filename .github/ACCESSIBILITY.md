# Accessibility statement

BookWorm aims to make its documentation, source code, and client applications
usable by as many people as possible. The client applications target the Web
Content Accessibility Guidelines (WCAG) 2.1 Level AA.

Accessibility is an ongoing practice. We welcome people with disabilities and
contributors who use assistive technology to share their experience and help
shape improvements.

## Accessibility priorities

We prioritize:

- Keyboard access to all interactive functionality
- Clear focus indicators and logical focus order
- Semantic HTML and compatibility with assistive technologies
- Sufficient color contrast and non-color indicators
- Text alternatives for meaningful images and other non-text content
- Clear labels, instructions, and error messages
- Responsive layouts and readable content at increased text sizes

BookWorm is a demonstration project and may contain accessibility barriers.
The WCAG 2.1 AA target is an ongoing goal, not a claim that every part of the
project currently conforms.

## Contributor expectations

Contributors should help prevent accessibility regressions:

- For user-interface changes, perform an automated accessibility check when
  an appropriate tool is available.
- Perform a keyboard-only pass for changes involving interactive controls,
  including checking focus visibility, tab order, keyboard operation, and
  escape routes from dialogs or menus.
- Use semantic HTML and native controls where possible.
- Use descriptive link text and meaningful alternative text for images.
- Provide captions and transcripts for newly added video or audio content.
- Do not use color as the only way to communicate information.
- Keep headings, lists, tables, and code examples structured and readable.

Pull requests that change the user interface should describe the accessibility
checks performed and any known limitations.

## Supported environments

BookWorm's accessible user interfaces are web applications. We prioritize
modern, supported desktop and mobile browsers, keyboard-only navigation, and
common screen-reader workflows. Actual behavior can vary by browser, operating
system, assistive technology, and device; support for third-party embeds and
other dependencies is best effort.

If a particular combination does not work as expected, please include it in
your report so that we can reproduce the issue.

## Known limitations

There are no known accessibility barriers formally tracked on this page at
this time. This section will be updated as barriers are confirmed and
prioritized. Some features may depend on third-party services or components
whose accessibility is outside BookWorm's direct control.

## Report an accessibility barrier

Please use the
[accessibility issue template](https://github.com/foxminchan/BookWorm/issues/new?template=accessibility.yml)
to report an accessibility barrier or suggest an improvement. Include:

1. The page, feature, or documentation affected
2. A description of what happened and what you expected to happen
3. The steps needed to reproduce the barrier
4. The browser, operating system, device, and assistive technology involved,
   including versions, if applicable
5. The impact or severity of the barrier
6. Any screenshots, recordings, or other details that help explain the issue

Please do not include passwords, private data, or security-sensitive
information in a public issue. For security vulnerabilities, follow the
instructions in the [Security Policy](SECURITY.md) instead.

If you cannot use GitHub Issues, email
[nguyenxuannhan407@gmail.com](mailto:nguyenxuannhan407@gmail.com). You do not
need to disclose a diagnosis or other personal information to report a barrier.

We use the following informal severity guidance:

- **Critical:** A core task cannot be completed.
- **Serious:** Completing a task is significantly difficult, but a workaround
  exists.
- **Moderate:** The barrier causes a recurring or inconsistent difficulty.
- **Minor:** The barrier has a small impact on usability.

## Response and maintenance

Project maintainers will acknowledge reports as soon as practical, investigate
confirmed barriers, and prioritize fixes based on severity, impact, and scope.
Where possible, we will explain next steps, provide a workaround, and ask the
reporter to verify a fix.

Accessibility improvements may be made through code, documentation, tests, or
design changes. Project maintainers own accessibility triage and keep this
statement current as practices, supported environments, or known limitations
change.
