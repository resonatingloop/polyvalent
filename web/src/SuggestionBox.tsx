import { useRef, useState, type ChangeEvent, type FormEvent } from "react";
import {
  submitSuggestion,
  suggestionKinds,
  validateSuggestion,
  validatedSuggestionFormId,
  type SuggestionErrors,
  type SuggestionInput,
  type SuggestionPayload,
} from "./suggestions";

export type SubmissionState = "idle" | "submitting" | "succeeded" | "failed";
type SuggestionTransport = (formId: string, payload: SuggestionPayload) => Promise<void>;

const emptySuggestion: SuggestionInput = {
  kind: "profile lore",
  integer: "",
  proposal: "",
  sourceUrl: "",
  name: "",
  email: "",
  creditPermission: false,
  gotcha: "",
};

export function SuggestionBox({
  formId = import.meta.env.VITE_PUBLIC_SUGGESTION_FORM_ID,
  transport = submitSuggestion,
}: {
  formId?: string;
  transport?: SuggestionTransport;
}) {
  const configuredFormId = validatedSuggestionFormId(formId);
  const available = configuredFormId !== null;
  const [input, setInput] = useState<SuggestionInput>(emptySuggestion);
  const [errors, setErrors] = useState<SuggestionErrors>({});
  const [state, setState] = useState<SubmissionState>("idle");
  const submissionInFlight = useRef(false);

  const update = (field: keyof SuggestionInput) => (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
  ) => {
    const value = event.target.type === "checkbox"
      ? (event.target as HTMLInputElement).checked
      : event.target.value;
    setInput((current) => ({ ...current, [field]: value }));
    setErrors((current) => ({ ...current, [field]: undefined }));
    if (state !== "idle") setState("idle");
  };

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!configuredFormId || !canSubmitSuggestion(available, state) || submissionInFlight.current) return;

    const validation = validateSuggestion(input);
    if (!validation.ok) {
      setErrors(validation.errors);
      setState("failed");
      focusFirstError(validation.errors);
      return;
    }

    setErrors({});
    submissionInFlight.current = true;
    setState("submitting");
    try {
      await transport(configuredFormId, validation.payload);
      setInput(emptySuggestion);
      setState("succeeded");
    } catch {
      setState("failed");
    } finally {
      submissionInFlight.current = false;
    }
  };

  const status = suggestionStatusCopy(available, state, Object.keys(errors).length > 0);
  const disabled = !canSubmitSuggestion(available, state);

  return (
    <section id="tip-line" className="tip-line" aria-labelledby="tip-line-heading">
      <div className="tip-line-banner" aria-hidden="true">
        <span>tips</span><span>corrections</span><span>receipts</span><span>numerical lore</span>
      </div>
      <div className="tip-line-plate">
        <div className="tip-line-editorial">
          <p className="eyebrow">numerical gossip / open channel</p>
          <h2 id="tip-line-heading">the integers<br />have <em>gossip.</em></h2>
          <p className="tip-line-deck">
            overheard a property, relation, correction, or suspiciously good story?
            slide it under the door. receipts are adored.
          </p>
          <div className="tip-line-stamp" aria-hidden="true">
            <span>polyvalent</span>
            <strong>tip line</strong>
            <span>human eyes only</span>
          </div>
          <p className="tip-line-boundary">
            this is correspondence, not ingestion. every tip waits for human review;
            nothing enters the corpus or appears publicly by itself.
          </p>
        </div>

        <form className="tip-form" onSubmit={submit} noValidate aria-describedby="tip-privacy tip-status">
          <div className="tip-form-header">
            <span>{available ? "anonymous dispatch accepted" : "dispatch channel unavailable"}</span>
            <span className={available ? "tip-wire is-live" : "tip-wire"}>
              <i aria-hidden="true" />{available ? "wire open" : "wire dormant"}
            </span>
          </div>

          <div className="tip-form-grid">
            <Field label="department" error={errors.kind} id="tip-kind">
              <select
                id="tip-kind"
                name="suggestion_kind"
                value={input.kind}
                onChange={update("kind")}
                disabled={disabled}
                aria-invalid={Boolean(errors.kind)}
                aria-describedby={errors.kind ? "tip-kind-error" : undefined}
              >
                {suggestionKinds.map((kind) => <option key={kind} value={kind}>{kind}</option>)}
              </select>
            </Field>

            <Field label="integer, if implicated" error={errors.integer} id="tip-integer">
              <input
                id="tip-integer"
                name="integer"
                value={input.integer}
                onChange={update("integer")}
                inputMode="numeric"
                autoComplete="off"
                spellCheck={false}
                placeholder="e.g. 153"
                disabled={disabled}
                aria-invalid={Boolean(errors.integer)}
                aria-describedby={errors.integer ? "tip-integer-error" : undefined}
              />
            </Field>

            <Field label="the allegation" error={errors.proposal} id="tip-proposal" wide>
              <textarea
                id="tip-proposal"
                name="message"
                value={input.proposal}
                onChange={update("proposal")}
                maxLength={2000}
                rows={7}
                placeholder="tell me what the number has been getting up to…"
                disabled={disabled}
                required
                aria-invalid={Boolean(errors.proposal)}
                aria-describedby={errors.proposal ? "tip-proposal-error" : undefined}
              />
              <span className="tip-count" aria-hidden="true">{input.proposal.length.toString().padStart(4, "0")} / 2000</span>
            </Field>

            <Field label="source / receipt" error={errors.sourceUrl} id="tip-source" wide>
              <input
                id="tip-source"
                name="source_url"
                type="url"
                value={input.sourceUrl}
                onChange={update("sourceUrl")}
                maxLength={500}
                placeholder="https://…"
                disabled={disabled}
                aria-invalid={Boolean(errors.sourceUrl)}
                aria-describedby={errors.sourceUrl ? "tip-source-error" : undefined}
              />
            </Field>

            <Field label="byline, optional" error={errors.name} id="tip-name">
              <input
                id="tip-name"
                name="name"
                value={input.name}
                onChange={update("name")}
                maxLength={100}
                autoComplete="name"
                placeholder="anonymous"
                disabled={disabled}
                aria-invalid={Boolean(errors.name)}
                aria-describedby={errors.name ? "tip-name-error" : undefined}
              />
            </Field>

            <Field label="reply wire, optional" error={errors.email} id="tip-email">
              <input
                id="tip-email"
                name="email"
                type="email"
                value={input.email}
                onChange={update("email")}
                maxLength={254}
                autoComplete="email"
                placeholder="you@example.com"
                disabled={disabled}
                aria-invalid={Boolean(errors.email)}
                aria-describedby={errors.email ? "tip-email-error" : undefined}
              />
            </Field>
          </div>

          <label className="tip-credit">
            <input
              type="checkbox"
              name="credit_permission"
              checked={input.creditPermission}
              onChange={update("creditPermission")}
              disabled={disabled}
            />
            <span aria-hidden="true" />
            you may credit my byline if this tip is later published
          </label>

          <div className="tip-honeypot" aria-hidden="true">
            <label htmlFor="tip-company">leave this field empty</label>
            <input
              id="tip-company"
              name="_gotcha"
              value={input.gotcha}
              onChange={update("gotcha")}
              tabIndex={-1}
              autoComplete="off"
              disabled={disabled}
            />
          </div>

          <div className="tip-form-dispatch">
            <button type="submit" disabled={disabled}>
              <span>{state === "submitting" ? "crossing the wire" : "drop the dossier"}</span>
              <b aria-hidden="true">↗</b>
            </button>
            <p id="tip-status" className={`tip-status is-${available ? state : "unavailable"}`} aria-live="polite" aria-atomic="true">
              {status}
            </p>
          </div>

          <p id="tip-privacy" className="tip-privacy">
            dispatches pass through formspree and may be retained there under its account settings
            before reaching the private research inbox. no automatic publication. no mailing list.
          </p>
        </form>
      </div>
    </section>
  );
}

function Field({
  label,
  error,
  id,
  wide = false,
  children,
}: {
  label: string;
  error?: string;
  id: string;
  wide?: boolean;
  children: React.ReactNode;
}) {
  const errorId = `${id}-error`;
  return (
    <div className={wide ? "tip-field is-wide" : "tip-field"}>
      <label htmlFor={id}>{label}</label>
      {children}
      <p id={errorId} className="tip-field-error" aria-live="polite">{error}</p>
    </div>
  );
}

function focusFirstError(errors: SuggestionErrors): void {
  const order: Array<keyof SuggestionErrors> = ["kind", "integer", "proposal", "sourceUrl", "name", "email"];
  const ids: Record<keyof SuggestionErrors, string> = {
    kind: "tip-kind",
    integer: "tip-integer",
    proposal: "tip-proposal",
    sourceUrl: "tip-source",
    name: "tip-name",
    email: "tip-email",
  };
  const first = order.find((field) => errors[field]);
  if (first && typeof document !== "undefined") document.getElementById(ids[first])?.focus();
}

export function canSubmitSuggestion(available: boolean, state: SubmissionState): boolean {
  return available && state !== "submitting";
}

export function suggestionStatusCopy(
  available: boolean,
  state: SubmissionState,
  hasValidationErrors: boolean,
): string {
  if (!available) return "the tip line is being tuned. no dispatch can leave this page yet.";
  if (state === "submitting") return "sealing the envelope and crossing the wire…";
  if (state === "succeeded") return "the relay accepted the dossier. it has not yet been read, verified, or published.";
  if (state === "failed" && hasValidationErrors) return "the desk circled a few fields that need attention.";
  if (state === "failed") return "the wire did not take it. your text remains here; try again in a moment.";
  return "the desk is listening.";
}
