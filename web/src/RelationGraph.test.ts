import { describe, expect, it } from "vitest";
import { describeGraphTrace } from "./graphTrace";
import type { IntegerRelation, PublicClaim, PublicProperty } from "./types";

const propertyA = property("property-a", "property a");
const propertyB = property("property-b", "property b");
const centerA = claim("property-a", "witness a");
const centerB = claim("property-b", "witness b");
const relations: IntegerRelation[] = [
  {
    integer: { value: "20", claims: [], profileEntries: [] },
    reasons: [
      {
        property: propertyA,
        centerClaim: centerA,
        relatedClaim: claim("property-a", "witness a", { orderly: true }),
      },
      {
        property: propertyB,
        centerClaim: centerB,
        relatedClaim: claim("property-b", "witness b"),
      },
    ],
  },
  {
    integer: { value: "30", claims: [], profileEntries: [] },
    reasons: [{
      property: propertyA,
      centerClaim: centerA,
      relatedClaim: claim("property-a", "witness a"),
    }],
  },
  {
    integer: { value: "40", claims: [], profileEntries: [] },
    reasons: [{
      property: propertyB,
      centerClaim: centerB,
      relatedClaim: claim("property-b", "witness b"),
    }],
  },
];

describe("relation graph tracing", () => {
  it("leaves the full field available while awaiting contact", () => {
    const trace = describeGraphTrace("10", relations, null);

    expect([...trace.propertySlugs]).toEqual([]);
    expect([...trace.integerValues]).toEqual([]);
    expect(trace.title).toBe("the 10 field is listening.");
  });

  it("illuminates every drawn integer carried by a focused property", () => {
    const trace = describeGraphTrace("10", relations, { kind: "property", value: "property-a" });

    expect([...trace.propertySlugs]).toEqual(["property-a"]);
    expect([...trace.integerValues]).toEqual(["20", "30"]);
    expect(trace.kicker).toContain("2 visible mutuals");
    expect(trace.body).toContain("definition, coverage, and sources");
  });

  it("isolates one mutual and retains every shared reason and witness", () => {
    const trace = describeGraphTrace("10", relations, { kind: "integer", value: "20" });

    expect([...trace.propertySlugs]).toEqual(["property-a", "property-b"]);
    expect([...trace.integerValues]).toEqual(["20"]);
    expect(trace.kicker).toContain("2 shared channels");
    expect(trace.body).toContain("property a · 10: witness a · 20: witness a · orderly / nice");
    expect(trace.body).toContain("property b · 10: witness b · 20: witness b");
  });
});

function property(slug: string, name: string): PublicProperty {
  return {
    slug,
    name,
    definition: "fixture definition",
    aliases: [],
    graphMode: "shared-membership",
    coverage: [],
    citations: [],
  };
}

function claim(
  propertySlug: string,
  displayText: string,
  qualifier: Record<string, boolean | string | number> = {},
): PublicClaim {
  return {
    propertySlug,
    qualifier,
    witness: { kind: "fixture", schemaVersion: 1, displayText },
    citations: [],
  };
}
