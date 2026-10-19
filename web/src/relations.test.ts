import { describe, expect, it } from "vitest";
import referenceData from "../public/data/data.json";
import { canonicalInteger } from "./integer";
import { explainRelationship, getInteger, getRelations, indexCorpus, knowledgeStatus, qualifierLabel } from "./relations";
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

  it("ranks 139 first in the actual reference-v2 field around 137", () => {
    const reference = referenceData as unknown as PublicCorpus;
    const relations = getRelations(indexCorpus(reference), "137");

    expect(relations).toHaveLength(302);
    expect(relations[0].integer.value).toBe("139");
    expect(relations[0].reasons.map((reason) => reason.property.slug))
      .toEqual(["prime", "twin-prime"]);
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

  it("puts an exact twin-prime partner before ambient shared memberships", () => {
    const expanded: PublicCorpus = structuredClone(corpus);
    expanded.properties.find((property) => property.slug === "prime")!.graphMode = "shared-membership";
    expanded.properties.push({
      slug: "twin-prime",
      name: "twin-prime member",
      definition: "fixture definition",
      aliases: [],
      graphMode: "shared-membership",
      coverage: [{ minimum: "1", maximum: "2000", basis: "deterministic", exclusions: [] }],
      citations: [],
    });
    expanded.integers.push(
      twinPrimeInteger("107", { higherTwin: "109" }),
      twinPrimeInteger("137", { higherTwin: "139" }),
      twinPrimeInteger("139", { lowerTwin: "137" }),
    );

    const relations = getRelations(indexCorpus(expanded), "137");

    expect(relations.map((relation) => relation.integer.value)).toEqual(["139", "107"]);
    expect(qualifierLabel(expanded.integers.find((integer) => integer.value === "137")!.claims[1]))
      .toBe("twin with 139");
  });
});

function twinPrimeInteger(value: string, qualifier: Record<string, string>) {
  return {
    value,
    claims: [
      { propertySlug: "prime", qualifier: {}, witness: null, citations: [] },
      {
        propertySlug: "twin-prime",
        qualifier,
        witness: { kind: "prime_partners", schemaVersion: 1, displayText: "fixture witness" },
        citations: [],
      },
    ],
    profileEntries: [],
  };
}
