function showFallback(loading: HTMLElement | null, fallback: HTMLElement | null) {
	loading?.classList.add("hidden");
	if (fallback) {
		fallback.classList.remove("hidden");
		fallback.classList.add("flex");
	}
}

function normalizeReportPath(pathname: string) {
	return pathname.replace(/\/index\.html$/i, "").replace(/\/+$/, "");
}

function keepNavigationInsideIframe(
	frame: HTMLIFrameElement,
	initialReportUrl: string,
	backButton: HTMLButtonElement | null,
	standaloneLink: HTMLAnchorElement | null
) {
	const initialPath = normalizeReportPath(
		new URL(initialReportUrl, window.location.origin).pathname
	);

	frame.addEventListener("load", () => {
		try {
			const win = frame.contentWindow;
			const doc = frame.contentDocument;
			if (!win || !doc) return;

			const currentUrl = new URL(win.location.href);
			const isAtSummary = normalizeReportPath(currentUrl.pathname) === initialPath;

			if (backButton) {
				backButton.classList.toggle("hidden", isAtSummary);
				backButton.classList.toggle("inline-flex", !isAtSummary);
			}
			if (standaloneLink) {
				standaloneLink.href = currentUrl.href;
			}

			doc.addEventListener(
				"click",
				(event) => {
					if (
						event.defaultPrevented ||
						event.button !== 0 ||
						event.metaKey ||
						event.ctrlKey ||
						event.shiftKey ||
						event.altKey
					) {
						return;
					}

					const anchor = (event.target as Element | null)?.closest?.(
						"a[href]"
					) as HTMLAnchorElement | null;
					if (!anchor || anchor.target !== "_blank") return;

					const resolvedUrl = new URL(anchor.href, doc.baseURI);
					if (resolvedUrl.origin === window.location.origin) {
						anchor.target = "_self";
					}
				},
				true
			);

			const originalOpen = win.open.bind(win);
			win.open = (targetUrl?: string | URL, target?: string, features?: string) => {
				if (targetUrl) {
					try {
						const resolvedUrl = new URL(targetUrl.toString(), doc.baseURI);
						if (resolvedUrl.origin === window.location.origin) {
							win.location.assign(resolvedUrl.href);
							return win;
						}
					} catch {
						// Fall through to default window.open
					}
				}
				return originalOpen(targetUrl, target, features);
			};
		} catch {
			// Ignore cross-origin access errors
		}
	});

	backButton?.addEventListener("click", () => {
		frame.src = initialReportUrl;
	});
}

export function initAllureReport() {
	const frame = document.getElementById("allure-frame") as HTMLIFrameElement | null;
	const fallback = document.getElementById("allure-fallback");
	const loading = document.getElementById("allure-loading");
	const backButton = document.getElementById("allure-back-summary") as HTMLButtonElement | null;
	const standaloneLink = document.getElementById(
		"allure-standalone-link"
	) as HTMLAnchorElement | null;
	const url = frame?.dataset.reportUrl;

	if (!frame || !fallback || !url || frame.dataset.initialized === "true") return;
	frame.dataset.initialized = "true";

	keepNavigationInsideIframe(frame, url, backButton, standaloneLink);

	fetch(url)
		.then(async (res) => {
			if (!frame.isConnected) return;
			if (!res.ok) {
				showFallback(loading, fallback);
				return;
			}
			const html = await res.text();
			if (!frame.isConnected) return;
			if (html.includes("404: Not found") || html.includes("data-not-found")) {
				showFallback(loading, fallback);
				return;
			}
			frame.src = url;
			frame.classList.remove("hidden");
			loading?.classList.add("hidden");
			fallback.classList.add("hidden");
		})
		.catch(() => {
			if (!frame.isConnected) return;
			showFallback(loading, fallback);
		});
}

document.addEventListener("astro:page-load", initAllureReport);
if (document.readyState !== "loading") {
	initAllureReport();
}
