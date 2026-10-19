import { canonicalInteger, compareIntegers } from "./integer";
import type {
  CorpusIndex,
  IntegerRelation,
  KnowledgeStatus,
  PublicClaim,
  PublicCorpus,
  PublicInteger,
  PublicProperty,
  RelationReason,
} from "./types";

export function indexCorpus(corpus: PublicCorpus): CorpusIndex {
  const properties = new Map(corpus.properties.map((property) => [property.slug, property]));
  const integers = new Map(corpus.integers.map((integer) => [integer.value, integer]));
  const members = new Map<string, PublicInteger[]>();

  for (const integer of corpus.integers) {
    canonicalInteger(integer.value);
    for (const claim of integer.claims) {
      if (!properties.has(claim.propertySlug)) {
        throw new Error(`integer ${integer.value} references missing property ${claim.propertySlug}.`);
      }
      const propertyMembers = members.get(claim.propertySlug) ?? [];
      propertyMembers.push(integer);
      members.set(claim.propertySlug, propertyMembers);
    }
  }

  for (const propertyMembers of members.values()) {
    propertyMembers.sort((left, right) => compareIntegers(left.value, right.value));
  }

  return { corpus, properties, integers, members };
}

export function getInteger(index: CorpusIndex, value: string): PublicInteger {
  const canonical = canonicalInteger(value);
  return index.integers.get(canonical) ?? {
    value: canonical,
    claims: [],
    profileEntries: [],
  };
}

export function knowledgeStatus(
  index: CorpusIndex,
  integer: PublicInteger,
  property: PublicProperty,
): KnowledgeStatus {
  if (integer.claims.some((claim) => claim.propertySlug === property.slug)) {
    return "positive";
  }

  const value = BigInt(integer.value);
  const covered = property.coverage.some((coverage) => {
    const excluded = new Set(coverage.exclusions);
    return value >= BigInt(coverage.minimum)
      && value <= BigInt(coverage.maximum)
      && !excluded.has(integer.value);
  });
  return covered ? "covered-negative" : "unassessed";
}

export function explainRelationship(
  index: CorpusIndex,
  leftValue: string,
  rightValue: string,
): RelationReason[] {
  const left = getInteger(index, leftValue);
  const right = getInteger(index, rightValue);
  const rightClaims = new Map(right.claims.map((claim) => [claim.propertySlug, claim]));
  const reasons: RelationReason[] = [];

  for (const centerClaim of left.claims) {
    const property = index.properties.get(centerClaim.propertySlug);
    const relatedClaim = rightClaims.get(centerClaim.propertySlug);
    if (property?.graphMode === "shared-membership" && relatedClaim) {
      reasons.push({ property, centerClaim, relatedClaim });
    }
  }

  return reasons.sort((a, b) => a.property.name.localeCompare(b.property.name));
}

export function getRelations(index: CorpusIndex, centerValue: string): IntegerRelation[] {
  const center = getInteger(index, centerValue);
  const related = new Map<string, { integer: PublicInteger; reasons: RelationReason[] }>();

  for (const centerClaim of center.claims) {
    const property = index.properties.get(centerClaim.propertySlug);
    if (!property || property.graphMode !== "shared-membership") {
      continue;
    }

    for (const integer of index.members.get(property.slug) ?? []) {
      if (integer.value === center.value) {
        continue;
      }

      const relatedClaim = integer.claims.find((claim) => claim.propertySlug === property.slug);
      if (!relatedClaim) {
        continue;
      }

      const item = related.get(integer.value) ?? { integer, reasons: [] };
      item.reasons.push({ property, centerClaim, relatedClaim });
      related.set(integer.value, item);
    }
  }

  return [...related.values()].sort((left, right) => {
    const leftRarity = Math.min(...left.reasons.map((reason) => index.members.get(reason.property.slug)?.length ?? Infinity));
    const rightRarity = Math.min(...right.reasons.map((reason) => index.members.get(reason.property.slug)?.length ?? Infinity));
    const leftQualifierSalience = Math.max(...left.reasons.map((reason) => qualifierSalience(reason.relatedClaim)));
    const rightQualifierSalience = Math.max(...right.reasons.map((reason) => qualifierSalience(reason.relatedClaim)));
    const leftDirectTwin = directTwinSalience(left.reasons, left.integer.value);
    const rightDirectTwin = directTwinSalience(right.reasons, right.integer.value);
    return leftRarity - rightRarity
      || rightDirectTwin - leftDirectTwin
      || rightQualifierSalience - leftQualifierSalience
      || compareIntegers(left.integer.value, right.integer.value);
  });
}

export function qualifierLabel(claim: PublicClaim): string | null {
  if (claim.qualifier.orderly === true) {
    return "orderly / nice";
  }

  const lowerTwin = claim.qualifier.lowerTwin;
  const higherTwin = claim.qualifier.higherTwin;
  const twins = [lowerTwin, higherTwin].filter((value): value is string => typeof value === "string");
  if (twins.length === 1) {
    return `twin with ${twins[0]}`;
  }
  if (twins.length === 2) {
    return `twins with ${twins[0]} and ${twins[1]}`;
  }

  return null;
}

function directTwinSalience(reasons: RelationReason[], relatedValue: string): number {
  return reasons.some((reason) =>
    reason.property.slug === "twin-prime"
    && (reason.centerClaim.qualifier.lowerTwin === relatedValue
      || reason.centerClaim.qualifier.higherTwin === relatedValue)) ? 1 : 0;
}

function qualifierSalience(claim: PublicClaim): number {
  return Object.values(claim.qualifier).some((value) => value === true || (typeof value === "string" && value.length > 0)) ? 1 : 0;
}
