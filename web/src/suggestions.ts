import { canonicalInteger } from "./integer";

export const suggestionKinds = [
  "property",
  "relation",
  "profile lore",
  "correction",
  "other",
] as const;

export type SuggestionKind = (typeof suggestionKinds)[number];

export interface SuggestionInput {
  kind: string;
  integer: string;
  proposal: string;
  sourceUrl: string;
  name: string;
  email: string;
  creditPermission: boolean;
  gotcha: string;
}

export interface SuggestionPayload {
  kind: SuggestionKind;
  integer: string;
  proposal: string;
  sourceUrl: string;
  name: string;
  email: string;
  creditPermission: boolean;
  gotcha: string;
}

export type SuggestionField = Exclude<keyof SuggestionInput, "creditPermission" | "gotcha">;
export type SuggestionErrors = Partial<Record<SuggestionField, string>>;

export type SuggestionValidation =
  | { ok: true; payload: SuggestionPayload; errors: SuggestionErrors }
  | { ok: false; errors: SuggestionErrors };

export type SuggestionFetch = (
  input: string,
  init: RequestInit,
) => Promise<{ ok: boolean; status: number }>;

const formIdPattern = /^[a-z0-9]{8,32}$/;
const formspreeEndpointPrefix = "https://formspree.io/f/";

export function validatedSuggestionFormId(value: string | undefined): string | null {
  const candidate = value?.trim() ?? "";
  return formIdPattern.test(candidate) ? candidate : null;
}

export function suggestionEndpoint(formId: string): string {
  const validated = validatedSuggestionFormId(formId);
  if (!validated) throw new Error("the public suggestion form id is missing or malformed.");
  return `${formspreeEndpointPrefix}${validated}`;
}

export function validateSuggestion(input: SuggestionInput): SuggestionValidation {
  const errors: SuggestionErrors = {};
  const kind = input.kind.trim();
  const integer = input.integer.trim();
  const proposal = input.proposal.trim();
  const sourceUrl = input.sourceUrl.trim();
  const name = input.name.trim();
  const email = input.email.trim();

  if (!suggestionKinds.includes(kind as SuggestionKind)) {
    errors.kind = "choose what kind of gossip this is.";
  }

  let canonical = "";
  if (integer) {
    try {
      canonical = canonicalInteger(integer);
    } catch {
      errors.integer = "use a canonical signed integer: 153, 0, or -7.";
    }
  }

  if (!proposal) {
    errors.proposal = "give us the gossip before dropping the envelope.";
  } else if (proposal.length > 2000) {
    errors.proposal = "keep the dossier to 2000 characters or fewer.";
  }

  if (sourceUrl.length > 500) {
    errors.sourceUrl = "keep the source url to 500 characters or fewer.";
  } else if (sourceUrl && !isSafeSourceUrl(sourceUrl)) {
    errors.sourceUrl = "use a complete http or https source url.";
  }

  if (name.length > 100) {
    errors.name = "keep the byline to 100 characters or fewer.";
  }

  if (email.length > 254) {
    errors.email = "keep the reply address to 254 characters or fewer.";
  } else if (email && !/^[^@\s]+@[^@\s]+$/.test(email)) {
    errors.email = "enter a usable reply address or leave it blank.";
  }

  if (Object.keys(errors).length > 0) return { ok: false, errors };

  return {
    ok: true,
    errors,
    payload: {
      kind: kind as SuggestionKind,
      integer: canonical,
      proposal,
      sourceUrl,
      name,
      email,
      creditPermission: input.creditPermission,
      gotcha: input.gotcha,
    },
  };
}

export async function submitSuggestion(
  formId: string,
  payload: SuggestionPayload,
  request: SuggestionFetch = (input, init) => fetch(input, init),
): Promise<void> {
  const body = new FormData();
  body.set("suggestion_kind", payload.kind);
  body.set("integer", payload.integer);
  body.set("message", payload.proposal);
  body.set("source_url", payload.sourceUrl);
  body.set("name", payload.name);
  body.set("email", payload.email);
  body.set("credit_permission", payload.creditPermission ? "yes" : "no");
  body.set("_gotcha", payload.gotcha);
  body.set("subject", "polyvalent numerical gossip");

  const response = await request(suggestionEndpoint(formId), {
    method: "POST",
    headers: { Accept: "application/json" },
    body,
    credentials: "omit",
    referrerPolicy: "strict-origin-when-cross-origin",
  });

  if (!response.ok) {
    throw new Error(`the correspondence relay declined the request (${response.status}).`);
  }
}

function isSafeSourceUrl(value: string): boolean {
  try {
    const parsed = new URL(value);
    return (parsed.protocol === "http:" || parsed.protocol === "https:")
      && !parsed.username
      && !parsed.password;
  } catch {
    return false;
  }
}
