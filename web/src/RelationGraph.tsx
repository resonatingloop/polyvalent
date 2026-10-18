import { Link } from "./router";
import type { CorpusIndex, IntegerRelation, PublicInteger } from "./types";

interface RelationGraphProps {
  index: CorpusIndex;
  center: PublicInteger;
  relations: IntegerRelation[];
  limit?: number;
}

interface PositionedNode {
  key: string;
  label: string;
  href: string;
  x: number;
  y: number;
  kind: "property" | "integer";
}

export function RelationGraph({ index, center, relations, limit = 18 }: RelationGraphProps) {
  const shownRelations = relations.slice(0, limit);
  const propertySlugs = [...new Set(
    shownRelations.flatMap((relation) => relation.reasons.map((reason) => reason.property.slug)),
  )];
  const propertyNodes = propertySlugs.map((slug, position): PositionedNode => {
    const angle = (Math.PI * 2 * position) / Math.max(propertySlugs.length, 1) - Math.PI / 2;
    const property = index.properties.get(slug)!;
    return {
      key: `property:${slug}`,
      label: property.name,
      href: `/property/${encodeURIComponent(slug)}`,
      x: 450 + Math.cos(angle) * 125,
      y: 260 + Math.sin(angle) * 125,
      kind: "property",
    };
  });
  const relationNodes = shownRelations.map((relation, position): PositionedNode => {
    const angle = (Math.PI * 2 * position) / Math.max(shownRelations.length, 1) - Math.PI / 2;
    const radius = position % 2 === 0 ? 228 : 250;
    return {
      key: `integer:${relation.integer.value}`,
      label: relation.integer.value,
      href: `/integer/${encodeURIComponent(relation.integer.value)}`,
      x: 450 + Math.cos(angle) * radius,
      y: 260 + Math.sin(angle) * radius,
      kind: "integer",
    };
  });
  const propertyBySlug = new Map(propertyNodes.map((node) => [node.key.slice("property:".length), node]));
  const integerByValue = new Map(relationNodes.map((node) => [node.key.slice("integer:".length), node]));

  return (
    <div className="graph-shell">
      <svg
        className="relation-graph"
        viewBox="0 0 900 520"
        role="group"
        aria-labelledby="graph-title graph-description"
      >
        <title id="graph-title">relations around integer {center.value}</title>
        <desc id="graph-description">
          a bounded graph from {center.value} through shared property nodes to related integers.
          the complete equivalent appears in the relation list.
        </desc>
        <g className="graph-lines" aria-hidden="true">
          {propertyNodes.map((node) => (
            <line key={`center:${node.key}`} x1="450" y1="260" x2={node.x} y2={node.y} />
          ))}
          {shownRelations.flatMap((relation) => relation.reasons.map((reason) => {
            const property = propertyBySlug.get(reason.property.slug)!;
            const integer = integerByValue.get(relation.integer.value)!;
            return (
              <line
                key={`${reason.property.slug}:${relation.integer.value}`}
                x1={property.x}
                y1={property.y}
                x2={integer.x}
                y2={integer.y}
              />
            );
          }))}
        </g>
        <g className="graph-center" aria-hidden="true">
          <circle cx="450" cy="260" r="50" />
          <text x="450" y="267" textAnchor="middle">{center.value}</text>
        </g>
        {propertyNodes.map((node) => (
          <Link key={node.key} href={node.href} ariaLabel={`property ${node.label}`}>
            <g className="graph-property">
              <rect x={node.x - 55} y={node.y - 17} width="110" height="34" rx="2" />
              <text x={node.x} y={node.y + 4} textAnchor="middle">{shorten(node.label, 17)}</text>
            </g>
          </Link>
        ))}
        {relationNodes.map((node) => (
          <Link key={node.key} href={node.href} ariaLabel={`integer ${node.label}`}>
            <g className="graph-integer">
              <circle cx={node.x} cy={node.y} r="22" />
              <text x={node.x} y={node.y + 4} textAnchor="middle">{node.label}</text>
            </g>
          </Link>
        ))}
      </svg>
      <p className="graph-caption">
        showing {shownRelations.length} of {relations.length} related integers
        {relations.length > shownRelations.length ? ` · ${relations.length - shownRelations.length} remain in the complete list` : ""}
      </p>
      <ul className="sr-only">
        {shownRelations.map((relation) => (
          <li key={relation.integer.value}>
            {center.value} relates to {relation.integer.value} through {relation.reasons.map((reason) => reason.property.name).join(", ")}.
          </li>
        ))}
      </ul>
    </div>
  );
}

function shorten(value: string, limit: number): string {
  return value.length <= limit ? value : `${value.slice(0, limit - 1)}…`;
}
