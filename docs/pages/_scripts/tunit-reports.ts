type TUnitEntry = {
	name: string;
	path: string;
};

function showTunitFallback(loading: HTMLElement | null, fallback: HTMLElement | null) {
	loading?.classList.add("hidden");
	if (fallback) {
		fallback.classList.remove("hidden");
		fallback.classList.add("flex");
	}
}

export function initTunitReports() {
	const sidebar = document.getElementById("tunit-sidebar");
	const list = document.getElementById("tunit-project-list");
	const frame = document.getElementById("tunit-frame") as HTMLIFrameElement | null;
	const loading = document.getElementById("tunit-loading");
	const fallback = document.getElementById("tunit-fallback");
	const standaloneLink = document.getElementById(
		"tunit-standalone-link"
	) as HTMLAnchorElement | null;

	if (!list || !sidebar || list.dataset.initialized === "true") return;
	list.dataset.initialized = "true";

	const base = (frame?.dataset.baseUrl ?? "").replace(/\/$/, "");
	const withBase = (path: string) =>
		path.startsWith("http") || (base && path.startsWith(base)) ? path : `${base}${path}`;

	function selectReport(entry: TUnitEntry, buttons: HTMLButtonElement[]) {
		const reportPath = withBase(entry.path);
		if (frame) {
			frame.src = reportPath;
			frame.classList.remove("hidden");
		}
		loading?.classList.add("hidden");
		if (fallback) {
			fallback.classList.add("hidden");
		}
		if (standaloneLink) {
			standaloneLink.href = reportPath;
			standaloneLink.classList.remove("hidden");
			standaloneLink.classList.add("inline-flex");
		}
		for (const btn of buttons) {
			const active = btn.dataset.path === entry.path;
			btn.className = active
				? "w-full rounded-lg bg-[rgb(var(--ec-button-bg))] px-2.5 py-2 text-left text-xs font-medium text-[rgb(var(--ec-button-text))]"
				: "w-full rounded-lg px-2.5 py-2 text-left text-xs text-[rgb(var(--ec-page-text-muted))] hover:bg-[rgb(var(--ec-content-hover))] hover:text-[rgb(var(--ec-page-text))]";
		}
	}

	fetch(`${base}/reports/tunit/manifest.json`)
		.then((res) => {
			if (!res.ok) throw new Error("Manifest not found");
			return res.json() as Promise<TUnitEntry[]>;
		})
		.then((entries) => {
			if (!list.isConnected) return;
			if (!Array.isArray(entries) || entries.length === 0) {
				showTunitFallback(loading, fallback);
				return;
			}

			sidebar.classList.remove("hidden");
			const buttons: HTMLButtonElement[] = [];

			for (const entry of entries) {
				const li = document.createElement("li");
				const btn = document.createElement("button");
				btn.type = "button";
				btn.dataset.path = entry.path;
				btn.textContent = entry.name;
				btn.addEventListener("click", () => selectReport(entry, buttons));
				li.appendChild(btn);
				list.appendChild(li);
				buttons.push(btn);
			}

			selectReport(entries[0], buttons);
		})
		.catch(() => {
			if (!list.isConnected) return;
			showTunitFallback(loading, fallback);
		});
}

document.addEventListener("astro:page-load", initTunitReports);
if (document.readyState !== "loading") {
	initTunitReports();
}
