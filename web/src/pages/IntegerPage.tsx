import { useMemo, useState } from "react";
import { RelationGraph } from "../RelationGraph";
import { Link } from "../router";
import { SearchForm } from "../SearchForm";
import { getInteger, getRelations, knowledgeStatus, qualifierLabel } from "../relations";
import type { CorpusIndex, PublicCitation } from "../types";

export function IntegerPage({ index, value }: { index: CorpusIndex; value: string }) {
  const integer = getInteger(index, value);
  const relations = useMemo(() => getRelations(index, integer.value), [index, integer.value]);
  const [view, setView] = useState<"graph" | "list">("graph");
  const positiveSlugs = new Set(integer.claims.map((claim) => claim.propertySlug));
  const coveredNegative = index.corpus.properties.filter((property) =>
    !positiveSlugs.has(property.slug) && knowledgeStatus(index, integer, property) === "covered-negative",
  );
  const unassessed = index.corpus.properties.filter((property) =>
    knowledgeStatus(index, integer, property) === "unassessed",
  );

  return (
    <main id="main" className="number-page">
      <section className="number-masthead">
        <div className="number-seal" aria-hidden="true"><span>№</span></div>
        <div>
          <p className="eyebrow">public integer profile / canonical decimal</p>
          <h1>{integer.value}</h1>
          <p className="profile-status">
            {integer.claims.length} published affiliations · {relations.length} related integers · {integer.profileEntries.length} profile entries
          </p>
        </div>
        <SearchForm compact initial={integer.value} />
      </section>

      <div className="number-layout">
        <section className="profile-column" aria-labelledby="affiliations-heading">
          <div className="section-heading">
            <p className="eyebrow">affiliations / evidence-bearing</p>
            <h2 id="affiliations-heading">what {integer.value} is known to be</h2>
          </div>

          {integer.claims.length === 0 ? (
            <div className="empty-register">
              <strong>no positive affiliations are published for this integer.</strong>
              <p>{coveredNegative.length > 0 ? "it is still inside the current examined range." : "it currently falls beyond the published coverage envelope."}</p>
            </div>
          ) : (
            <div className="affiliation-stack">
              {integer.claims.map((claim, position) => {
                const property = index.properties.get(claim.propertySlug)!;
                const qualifier = qualifierLabel(claim);
                return (
                  <article className="affiliation" key={claim.propertySlug} style={{ "--order": position } as React.CSSProperties}>
                    <span className="affiliation-index">{String(position + 1).padStart(2, "0")}</span>
                    <div>
                      <p className="eyebrow">{qualifier ?? "confirmed member"}</p>
                      <h3><Link href={`/property/${encodeURIComponent(property.slug)}`}>{property.name}</Link></h3>
                      <p>{property.definition}</p>
                      {claim.witness && <code className="witness">{claim.witness.displayText}</code>}
                      <CitationLine citations={mergeCitations(property.citations, claim.citations)} />
                    </div>
                  </article>
                );
              })}
            </div>
          )}

          {integer.profileEntries.length > 0 && (
            <section className="lore-section" aria-labelledby="lore-heading">
              <p className="eyebrow">profile entries / sourced context</p>
              <h2 id="lore-heading">what gathers around {integer.value}</h2>
              {integer.profileEntries.map((entry) => (
                <article className="lore-entry" key={entry.slug}>
                  <p className="lore-register">{entry.category} · {entry.epistemicRegister}</p>
                  <h3>{entry.title}</h3>
                  <p>{entry.body}</p>
                  <CitationLine citations={entry.citations} />
                </article>
              ))}
            </section>
          )}

          <details className="coverage-drawer">
            <summary>coverage ledger</summary>
            <div>
              <p><strong>{coveredNegative.length}</strong> evaluated negatives in the current property pack</p>
              <ul>{coveredNegative.map((property) => <li key={property.slug}>{property.name}</li>)}</ul>
              {unassessed.length > 0 && <p><strong>{unassessed.length}</strong> properties remain unassessed at this integer.</p>}
            </div>
          </details>
        </section>

        <section className="relations-column" aria-labelledby="relations-heading">
          <div className="relations-header">
            <div>
              <p className="eyebrow">mutuals / explained relations</p>
              <h2 id="relations-heading">who {integer.value} knows</h2>
            </div>
            <div className="view-switch" aria-label="relation view">
              <button type="button" aria-pressed={view === "graph"} onClick={() => setView("graph")}>graph</button>
              <button type="button" aria-pressed={view === "list"} onClick={() => setView("list")}>list</button>
            </div>
          </div>

          {relations.length === 0 ? (
            <div className="empty-register">
              <strong>no graphable mutuals in this release.</strong>
              <p>that is a bounded publication fact, not a verdict on the integer.</p>
            </div>
          ) : view === "graph" ? (
            <RelationGraph index={index} center={integer} relations={relations} />
          ) : (
            <RelationList center={integer.value} relations={relations} />
          )}
        </section>
      </div>
    </main>
  );
}

function RelationList({ center, relations }: { center: string; relations: ReturnType<typeof getRelations> }) {
  return (
    <ol className="relation-list">
      {relations.map((relation) => (
        <li key={relation.integer.value}>
          <Link href={`/integer/${encodeURIComponent(relation.integer.value)}`} className="relation-number">
            {relation.integer.value}
          </Link>
          <div>
            {relation.reasons.map((reason) => (
              <div className="relation-reason" key={reason.property.slug}>
                <p>
                  <span>{center}</span> and <span>{relation.integer.value}</span> share{" "}
                  <Link href={`/property/${encodeURIComponent(reason.property.slug)}`}>
                    {reason.property.name}
                  </Link>
                  {qualifierLabel(reason.relatedClaim) ? ` · ${qualifierLabel(reason.relatedClaim)}` : ""}.
                </p>
                {(reason.centerClaim.witness || reason.relatedClaim.witness) && (
                  <p className="relation-evidence">
                    {reason.centerClaim.witness && <code>{center}: {reason.centerClaim.witness.displayText}</code>}
                    {reason.relatedClaim.witness && <code>{relation.integer.value}: {reason.relatedClaim.witness.displayText}</code>}
                  </p>
                )}
              </div>
            ))}
          </div>
        </li>
      ))}
    </ol>
  );
}

function CitationLine({ citations }: { citations: PublicCitation[] }) {
  if (citations.length === 0) return null;
  return (
    <p className="citations">
      {citations.map((citation, index) => (
        <span key={`${citation.url}:${citation.label}`}>
          {index > 0 && " · "}
          <a href={citation.url} target="_blank" rel="noreferrer">{citation.label}</a>
          {citation.locator ? `, ${citation.locator}` : ""}
        </span>
      ))}
    </p>
  );
}

function mergeCitations(...groups: PublicCitation[][]): PublicCitation[] {
  const seen = new Set<string>();
  return groups.flat().filter((citation) => {
    const key = [citation.url, citation.label, citation.role, citation.locator ?? ""].join("\u0000");
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}
