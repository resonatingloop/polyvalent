import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { canSubmitSuggestion, SuggestionBox, suggestionStatusCopy } from "./SuggestionBox";

describe("suggestion box semantics", () => {
  it("fails closed without a configured form id", () => {
    const markup = renderToStaticMarkup(<SuggestionBox formId={undefined} />);

    expect(markup).toContain('id="tip-line"');
    expect(markup).toContain("dispatch channel unavailable");
    expect(markup).not.toContain("anonymous dispatch accepted");
    expect(markup).toContain("wire dormant");
    expect(markup).toContain("no dispatch can leave this page yet");
    expect(markup).toContain("disabled");
    expect(markup).toContain('aria-live="polite"');
  });

  it("renders labeled fields and the provider disclosure when configured", () => {
    const markup = renderToStaticMarkup(<SuggestionBox formId="abc12345" />);

    expect(markup).toContain("wire open");
    expect(markup).toContain('for="tip-proposal"');
    expect(markup).toContain('name="credit_permission"');
    expect(markup).toContain('name="_gotcha"');
    expect(markup).toContain("dispatches pass through formspree");
    expect(markup).toContain("nothing enters the corpus");
  });

  it("blocks duplicate dispatch while one request is in flight and keeps success claims narrow", () => {
    expect(canSubmitSuggestion(true, "idle")).toBe(true);
    expect(canSubmitSuggestion(true, "submitting")).toBe(false);
    expect(canSubmitSuggestion(false, "idle")).toBe(false);
    expect(suggestionStatusCopy(true, "succeeded", false)).toContain("relay accepted");
    expect(suggestionStatusCopy(true, "succeeded", false)).toContain("not yet been read");
  });
});
