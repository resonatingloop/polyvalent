import { describe, expect, it, vi } from "vitest";
import {
  submitSuggestion,
  suggestionEndpoint,
  validateSuggestion,
  validatedSuggestionFormId,
  type SuggestionFetch,
  type SuggestionInput,
} from "./suggestions";

const validInput: SuggestionInput = {
  kind: "profile lore",
  integer: "153",
  proposal: "sonnet 153 has entered the chat.",
  sourceUrl: "https://example.com/receipt",
  name: "a number gossip",
  email: "tipster@example.com",
  creditPermission: true,
  gotcha: "",
};

describe("public suggestion boundary", () => {
  it("accepts only a narrow public form id and constructs one fixed provider endpoint", () => {
    expect(validatedSuggestionFormId("abcdefgh")).toBe("abcdefgh");
    expect(validatedSuggestionFormId(" abc12345 ")).toBe("abc12345");
    expect(validatedSuggestionFormId("https://elsewhere.invalid/form")).toBeNull();
    expect(validatedSuggestionFormId("../escape")).toBeNull();
    expect(validatedSuggestionFormId(undefined)).toBeNull();
    expect(suggestionEndpoint("abc12345")).toBe("https://formspree.io/f/abc12345");
    expect(() => suggestionEndpoint("bad/value")).toThrow("malformed");
  });

  it("normalizes inert fields and preserves credit as a distinct consent", () => {
    const result = validateSuggestion({
      ...validInput,
      integer: " 153 ",
      proposal: "  the allegation  ",
      name: "  anonymous-ish  ",
      creditPermission: false,
    });

    expect(result).toEqual({
      ok: true,
      errors: {},
      payload: {
        ...validInput,
        integer: "153",
        proposal: "the allegation",
        name: "anonymous-ish",
        creditPermission: false,
      },
    });
  });

  it("rejects malformed integers, unsafe urls, empty copy, and overlong fields", () => {
    const result = validateSuggestion({
      ...validInput,
      kind: "hot stock tip",
      integer: "0153",
      proposal: " ",
      sourceUrl: "javascript:alert(1)",
      name: "n".repeat(101),
      email: "not-an-address",
    });

    expect(result.ok).toBe(false);
    if (result.ok) return;
    expect(Object.keys(result.errors).sort()).toEqual([
      "email",
      "integer",
      "kind",
      "name",
      "proposal",
      "sourceUrl",
    ]);
  });

  it("posts only normalized form data and reports provider rejection without reading its body", async () => {
    const validation = validateSuggestion(validInput);
    if (!validation.ok) throw new Error("fixture should validate");
    const request = vi.fn<SuggestionFetch>(async () => ({ ok: true, status: 200 }));

    await submitSuggestion("abc12345", validation.payload, request);

    expect(request).toHaveBeenCalledOnce();
    const [url, init] = request.mock.calls[0]!;
    expect(url).toBe("https://formspree.io/f/abc12345");
    expect(init.method).toBe("POST");
    expect(init.credentials).toBe("omit");
    expect(init.referrerPolicy).toBe("strict-origin-when-cross-origin");
    const body = init.body as FormData;
    expect(Object.fromEntries(body.entries())).toEqual({
      suggestion_kind: "profile lore",
      integer: "153",
      message: "sonnet 153 has entered the chat.",
      source_url: "https://example.com/receipt",
      name: "a number gossip",
      email: "tipster@example.com",
      credit_permission: "yes",
      _gotcha: "",
      subject: "polyvalent numerical gossip",
    });

    const rejected = vi.fn<SuggestionFetch>(async () => ({ ok: false, status: 429 }));
    await expect(submitSuggestion("abc12345", validation.payload, rejected)).rejects.toThrow("(429)");
  });
});
