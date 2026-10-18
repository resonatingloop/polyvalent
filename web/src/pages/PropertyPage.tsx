import { Link } from "../router";
import { qualifierLabel } from "../relations";
import type { CorpusIndex, PublicCitation } from "../types";

export function PropertyPage({ index, slug }: { index: CorpusIndex; slug: string }) {
  const property = index.properties.get(slug);
  if (!property) {
    return <NotFound label={`property ${slug}`} />;
  }
  const members = index.members.get(slug) ?? [];

  return (
    <main id="main" className="property-page">
      <header className="property-masthead">
        <p className="eyebrow">public property dossier / versioned definition</p>
        <h1>{property.name}</h1>
        <p className="property-definition">{property.definition}</p>
        <div className="alias-line">
          {property.aliases.length > 0 ? property.aliases.join(" · ") : "no public aliases"}
        </div>
      </header>

      <div className="property-grid">
        <section aria-labelledby="members-heading">
          <div className="section-heading horizontal">
            <div>
              <p className="eyebrow">published membership</p>
              <h2 id="members-heading">{members.length} integers in this release</h2>
            </div>
            <span className={`graph-badge ${property.graphMode}`}>{property.graphMode}</span>
          </div>
          <ol className="member-grid">
            {members.map((integer) => {
              const claim = integer.claims.find((item) => item.propertySlug === slug)!;
              return (
                <li key={integer.value}>
                  <Link href={`/integer/${encodeURIComponent(integer.value)}`}>
                    <strong>{integer.value}</strong>
                    <span>{qualifierLabel(claim) ?? claim.witness?.displayText ?? "confirmed member"}</span>
                  </Link>
                </li>
              );
            })}
          </ol>
        </section>

        <aside className="property-ledger" aria-labelledby="warrant-heading">
          <p className="eyebrow">warrant / provenance</p>
          <h2 id="warrant-heading">coverage ledger</h2>
          {property.coverage.map((coverage) => (
            <div className="coverage-record" key={`${coverage.minimum}:${coverage.maximum}:${coverage.basis}`}>
              <span>{coverage.minimum}—{coverage.maximum}</span>
              <strong>{coverage.basis.replaceAll("-", " ")}</strong>
              {coverage.exclusions.length > 0 && <small>{coverage.exclusions.length} exclusions</small>}
            </div>
          ))}
          <h3>sources</h3>
          <SourceList citations={property.citations} />
        </aside>
      </div>
    </main>
  );
}

export function NotFound({ label }: { label: string }) {
  return (
    <main id="main" className="not-found">
      <p className="eyebrow">no published record</p>
      <h1>{label}</h1>
      <p>the public release has no such object. absence here may still mean unassessed.</p>
      <Link href="/">return to the index</Link>
    </main>
  );
}

function SourceList({ citations }: { citations: PublicCitation[] }) {
  return (
    <ul className="source-list">
      {citations.map((citation) => (
        <li key={`${citation.url}:${citation.label}`}>
          <a href={citation.url} target="_blank" rel="noreferrer">{citation.label}</a>
          <span>{citation.role}</span>
        </li>
      ))}
    </ul>
  );
}
