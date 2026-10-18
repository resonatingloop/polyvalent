import { describe, expect, it } from "vitest";
import { canonicalInteger } from "./integer";
import { explainRelationship, getInteger, getRelations, indexCorpus, knowledgeStatus } from "./relations";
import type { PublicCorpus } from "./types";

const corpus: PublicCorpus = {
  schemaVersion: "1",
  releaseId: "fixture-v1",
  generatedAtUtc: "2026-10-18T00:00:00.0000000+00:00",
  properties: [
    {
      slug: "decimal-friedman-number",
      name: "decimal friedman number",
      definition: "fixture definition",
      aliases: [],
      graphMode: "shared-membership",
      coverage: [{ minimum: "1", maximum: "2000", basis: "source-asserted-bounded", exclusions: [] }],
      citations: [],
    },
    {
      slug: "prime",
      name: "prime",
      definition: "fixture definition",
      aliases: [],
      graphMode: "none",
      coverage: [{ minimum: "1", maximum: "2000", basis: "deterministic", exclusions: [] }],
      citations: [],
    },
  ],
  integers: [
    {
      value: "153",
      claims: [{
        propertySlug: "decimal-friedman-number",
        qualifier: { orderly: false },
        witness: { kind: "digit_expression", schemaVersion: 1, displayText: "3 × 51" },
        citations: [],
      }],
      profileEntries: [],
    },
    {
      value: "343",
      claims: [{
        propertySlug: "decimal-friedman-number",
        qualifier: { orderly: true },
        witness: { kind: "digit_expression", schemaVersion: 1, displayText: "(3 + 4) ^ 3" },
        citations: [],
      }],
      profileEntries: [],
    },
  ],
};

describe("integer identity", () => {
  it("preserves arbitrary-precision canonical text", () => {
    expect(canonicalInteger("900719925474099312345678901234567890")).toBe("900719925474099312345678901234567890");
  });

  it.each(["+1", "01", "-0", " 1", "1.0", "1e3"])("rejects noncanonical input %s", (value) => {
    expect(() => canonicalInteger(value)).toThrow();
  });
});

describe("public relation derivation", () => {
  const index = indexCorpus(corpus);

  it("explains 153 to 343 through the shared property and endpoint witnesses", () => {
    const reasons = explainRelationship(index, "153", "343");

    expect(reasons).toHaveLength(1);
    expect(reasons[0].property.name).toBe("decimal friedman number");
    expect(reasons[0].centerClaim.witness?.displayText).toBe("3 × 51");
    expect(reasons[0].relatedClaim.qualifier.orderly).toBe(true);
  });

  it("derives a relation without stored pairwise edges", () => {
    const relations = getRelations(index, "153");

    expect(relations.map((relation) => relation.integer.value)).toEqual(["343"]);
  });

  it("distinguishes a covered negative from an unassessed value", () => {
    const within = getInteger(index, "154");
    const outside = getInteger(index, "2001");
    const prime = index.properties.get("prime")!;

    expect(knowledgeStatus(index, within, prime)).toBe("covered-negative");
    expect(knowledgeStatus(index, outside, prime)).toBe("unassessed");
  });

  it("gives an explicitly qualified endpoint salience within an equally rare property", () => {
    const expanded: PublicCorpus = structuredClone(corpus);
    expanded.integers.push({
      value: "25",
      claims: [{
        propertySlug: "decimal-friedman-number",
        qualifier: {},
        witness: { kind: "digit_expression", schemaVersion: 1, displayText: "5 ^ 2" },
        citations: [],
      }],
      profileEntries: [],
    });

    expect(getRelations(indexCorpus(expanded), "153").map((relation) => relation.integer.value)).toEqual(["343", "25"]);
  });
});
