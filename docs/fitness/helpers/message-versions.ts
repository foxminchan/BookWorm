import { coerce, satisfies, valid, validRange } from "semver";

type MessageReference = {
	id: string;
	version?: string;
};

type LatestMessage = {
	id: string;
	version: string;
};

export function referencesLatestMessage(
	reference: MessageReference,
	message: LatestMessage
): boolean {
	if (reference.id !== message.id) return false;
	if (reference.version === undefined || reference.version === "latest") return true;

	const selector = reference.version;
	if (typeof selector !== "string" || !selector.trim() || typeof message.version !== "string") {
		throw new Error(`Invalid version selector for message "${message.id}".`);
	}
	if (selector === message.version) return true;

	const comparableVersion =
		valid(message.version) ??
		(/^[vV]?\d+(?:\.\d+){0,2}$/.test(message.version)
			? coerce(message.version)?.version
			: undefined);
	const range = validRange(selector) ?? validRange(selector.replace(/V(?=\d)/g, "v"));

	if (!comparableVersion || !range) {
		throw new Error(
			`Cannot compare version "${message.version}" of message "${message.id}" with selector "${selector}".`
		);
	}

	return satisfies(comparableVersion, range);
}
