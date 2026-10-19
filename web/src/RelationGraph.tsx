import { useEffect, useState, type CSSProperties } from "react";
import { describeGraphTrace, type GraphFocus } from "./graphTrace";
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
  value: string;
  href: string;
  x: number;
  y: number;
  kind: "property" | "integer";
}

interface GraphGeometry {
  width: number;
  height: number;
  centerX: number;
  centerY: number;
  propertyRadius: number;
  relationRadiusInner: number;
  relationRadiusOuter: number;
}

const wideGeometry: GraphGeometry = {
  width: 900,
  height: 520,
  centerX: 450,
  centerY: 260,
  propertyRadius: 125,
  relationRadiusInner: 228,
  relationRadiusOuter: 250,
};

const compactGeometry: GraphGeometry = {
  width: 430,
  height: 520,
  centerX: 215,
  centerY: 260,
  propertyRadius: 100,
  relationRadiusInner: 178,
  relationRadiusOuter: 195,
};

export function RelationGraph({ index, center, relations, limit = 18 }: RelationGraphProps) {
  const [hovered, setHovered] = useState<GraphFocus>(null);
  const [focused, setFocused] = useState<GraphFocus>(null);
  const compact = useCompactGraph();
  const geometry = compact ? compactGeometry : wideGeometry;
  const activeFocus = hovered ?? focused;
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
      value: slug,
      href: `/property/${encodeURIComponent(slug)}`,
      x: geometry.centerX + Math.cos(angle) * geometry.propertyRadius,
      y: geometry.centerY + Math.sin(angle) * geometry.propertyRadius,
      kind: "property",
    };
  });
  const relationNodes = shownRelations.map((relation, position): PositionedNode => {
    const angle = (Math.PI * 2 * position) / Math.max(shownRelations.length, 1) - Math.PI / 2;
    const radius = position % 2 === 0 ? geometry.relationRadiusInner : geometry.relationRadiusOuter;
    return {
      key: `integer:${relation.integer.value}`,
      label: relation.integer.value,
      value: relation.integer.value,
      href: `/integer/${encodeURIComponent(relation.integer.value)}`,
      x: geometry.centerX + Math.cos(angle) * radius,
      y: geometry.centerY + Math.sin(angle) * radius,
      kind: "integer",
    };
  });
  const propertyBySlug = new Map(propertyNodes.map((node) => [node.value, node]));
  const integerByValue = new Map(relationNodes.map((node) => [node.value, node]));
  const trace = describeGraphTrace(center.value, shownRelations, activeFocus);
  const tracing = activeFocus !== null;
  let edgeOrder = 0;

  return (
    <div className={`graph-shell${tracing ? " is-tracing" : ""}`}>
      <svg
        className={`relation-graph${compact ? " is-compact" : ""}`}
        viewBox={`0 0 ${geometry.width} ${geometry.height}`}
        role="group"
        aria-labelledby="graph-title graph-description"
      >
        <title id="graph-title">relations around integer {center.value}</title>
        <desc id="graph-description">
          a bounded graph from {center.value} through shared property nodes to related integers.
          hover or focus a node to illuminate its route. the complete equivalent appears in the relation list.
        </desc>
        <g className="graph-lines" aria-hidden="true">
          {propertyNodes.map((node) => {
            const active = tracing && trace.propertySlugs.has(node.value);
            return (
              <GraphEdge
                key={`center:${node.key}`}
                x1={geometry.centerX}
                y1={geometry.centerY}
                x2={node.x}
                y2={node.y}
                order={edgeOrder++}
                active={active}
                dimmed={tracing && !active}
              />
            );
          })}
          {shownRelations.flatMap((relation) => relation.reasons.map((reason) => {
            const property = propertyBySlug.get(reason.property.slug)!;
            const integer = integerByValue.get(relation.integer.value)!;
            const active = tracing
              && trace.propertySlugs.has(reason.property.slug)
              && trace.integerValues.has(relation.integer.value);
            return (
              <GraphEdge
                key={`${reason.property.slug}:${relation.integer.value}`}
                x1={property.x}
                y1={property.y}
                x2={integer.x}
                y2={integer.y}
                order={edgeOrder++}
                active={active}
                dimmed={tracing && !active}
              />
            );
          }))}
        </g>
        <g className="graph-center" aria-hidden="true">
          <circle className="graph-center-orbit" cx={geometry.centerX} cy={geometry.centerY} r="61" pathLength="100" />
          <circle className="graph-center-core" cx={geometry.centerX} cy={geometry.centerY} r="50" />
          <circle className="graph-center-glint" cx={geometry.centerX} cy={geometry.centerY} r="43" />
          <text x={geometry.centerX} y={geometry.centerY + 7} textAnchor="middle">{center.value}</text>
        </g>
        {propertyNodes.map((node, position) => {
          const active = tracing && trace.propertySlugs.has(node.value);
          const target: GraphFocus = { kind: "property", value: node.value };
          return (
            <g
              key={node.key}
              className={nodeClass("graph-node graph-property-node", tracing, active)}
              style={{ "--node-order": position } as CSSProperties}
              onPointerEnter={() => setHovered(target)}
              onPointerLeave={() => setHovered(null)}
              onFocus={() => setFocused(target)}
              onBlur={() => setFocused(null)}
            >
              <Link href={node.href} ariaLabel={`property ${node.label}`}>
                <g className="graph-property">
                  <rect className="graph-node-halo" x={node.x - 61} y={node.y - 23} width="122" height="46" rx="3" />
                  <rect x={node.x - 55} y={node.y - 17} width="110" height="34" rx="2" />
                  <text x={node.x} y={node.y + 4} textAnchor="middle">{shorten(node.label, 17)}</text>
                </g>
              </Link>
            </g>
          );
        })}
        {relationNodes.map((node, position) => {
          const active = tracing && trace.integerValues.has(node.value);
          const target: GraphFocus = { kind: "integer", value: node.value };
          return (
            <g
              key={node.key}
              className={nodeClass("graph-node graph-integer-node", tracing, active)}
              style={{ "--node-order": propertyNodes.length + position } as CSSProperties}
              onPointerEnter={() => setHovered(target)}
              onPointerLeave={() => setHovered(null)}
              onFocus={() => setFocused(target)}
              onBlur={() => setFocused(null)}
            >
              <Link href={node.href} ariaLabel={`integer ${node.label}`}>
                <g className="graph-integer">
                  <circle className="graph-node-halo" cx={node.x} cy={node.y} r={compact ? 34 : 29} />
                  <circle cx={node.x} cy={node.y} r={compact ? 27 : 22} />
                  <text x={node.x} y={node.y + 4} textAnchor="middle">{node.label}</text>
                </g>
              </Link>
            </g>
          );
        })}
      </svg>
      <div className={`graph-trace-readout${tracing ? " is-engaged" : ""}`}>
        <span className="graph-trace-lamp" aria-hidden="true" />
        <div>
          <p className="graph-trace-kicker">{trace.kicker}</p>
          <strong>{trace.title}</strong>
          <p>{trace.body}</p>
        </div>
      </div>
      <p className="graph-caption">
        showing {shownRelations.length} of {relations.length} related integers
        {relations.length > shownRelations.length ? ` · ${relations.length - shownRelations.length} remain in the complete list` : ""}
        <span>hover or focus a node to trace its route</span>
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

function GraphEdge({ x1, y1, x2, y2, order, active, dimmed }: {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  order: number;
  active: boolean;
  dimmed: boolean;
}) {
  return (
    <g
      className={`graph-edge-group${active ? " is-active" : ""}${dimmed ? " is-dimmed" : ""}`}
      style={{ "--edge-order": order } as CSSProperties}
    >
      <line className="graph-edge" x1={x1} y1={y1} x2={x2} y2={y2} pathLength="1" />
      <line className="graph-signal" x1={x1} y1={y1} x2={x2} y2={y2} pathLength="100" />
    </g>
  );
}

function nodeClass(base: string, tracing: boolean, active: boolean): string {
  return `${base}${active ? " is-active" : ""}${tracing && !active ? " is-dimmed" : ""}`;
}

function useCompactGraph(): boolean {
  const query = "(max-width: 720px)";
  const [compact, setCompact] = useState(() =>
    typeof window !== "undefined" && window.matchMedia(query).matches);

  useEffect(() => {
    const media = window.matchMedia(query);
    const update = () => setCompact(media.matches);
    update();
    media.addEventListener("change", update);
    return () => media.removeEventListener("change", update);
  }, []);

  return compact;
}

function shorten(value: string, limit: number): string {
  return value.length <= limit ? value : `${value.slice(0, limit - 1)}…`;
}
