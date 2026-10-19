import { qualifierLabel } from "./relations";
import type { IntegerRelation, RelationReason } from "./types";

export type GraphFocus =
  | { kind: "property"; value: string }
  | { kind: "integer"; value: string }
  | null;

export interface GraphTrace {
  propertySlugs: ReadonlySet<string>;
  integerValues: ReadonlySet<string>;
  kicker: string;
  title: string;
  body: string;
}

export function describeGraphTrace(
  centerValue: string,
  relations: IntegerRelation[],
  focus: GraphFocus,
): GraphTrace {
  if (focus?.kind === "property") {
    const connected = relations.filter((relation) =>
      relation.reasons.some((reason) => reason.property.slug === focus.value));
    const property = connected
      .flatMap((relation) => relation.reasons)
      .find((reason) => reason.property.slug === focus.value)?.property;

    if (property) {
      return {
        propertySlugs: new Set([property.slug]),
        integerValues: new Set(connected.map((relation) => relation.integer.value)),
        kicker: `property channel / ${connected.length} visible mutual${connected.length === 1 ? "" : "s"}`,
        title: `${centerValue} → ${property.name}`,
        body: `this published affiliation carries ${connected.length} of the integers currently drawn. open the violet station for its definition, coverage, and sources.`,
      };
    }
  }

  if (focus?.kind === "integer") {
    const relation = relations.find((candidate) => candidate.integer.value === focus.value);
    if (relation) {
      return {
        propertySlugs: new Set(relation.reasons.map((reason) => reason.property.slug)),
        integerValues: new Set([relation.integer.value]),
        kicker: `mutual acquired / ${relation.reasons.length} shared channel${relation.reasons.length === 1 ? "" : "s"}`,
        title: `${centerValue} ↔ ${relation.integer.value}`,
        body: relation.reasons
          .map((reason) => describeReason(centerValue, relation.integer.value, reason))
          .join(" / "),
      };
    }
  }

  return {
    propertySlugs: new Set(),
    integerValues: new Set(),
    kicker: "trace console / awaiting contact",
    title: `the ${centerValue} field is listening.`,
    body: "hover or tab through a node to isolate the published reason that joins it to the center.",
  };
}

function describeReason(centerValue: string, relatedValue: string, reason: RelationReason): string {
  const details = [reason.property.name];
  if (reason.centerClaim.witness) {
    details.push(`${centerValue}: ${reason.centerClaim.witness.displayText}`);
  }
  if (reason.relatedClaim.witness) {
    details.push(`${relatedValue}: ${reason.relatedClaim.witness.displayText}`);
  }
  const qualifier = qualifierLabel(reason.relatedClaim);
  if (qualifier) details.push(qualifier);
  return details.join(" · ");
}
