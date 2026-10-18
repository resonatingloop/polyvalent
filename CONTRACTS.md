# polyvalent contracts

> **continuity role:** normative invariants
> **status:** current
> **last verified:** 2026-10-17
> **owns:** confirmed promises implementation must not drift from casually
> **update when:** an authorized decision changes an invariant or its proof

changing a contract requires an explicit decision. a failing implementation is
not, by itself, authorization to weaken the contract.

## current sources of truth

| fact | owner | current proof or inventory |
|---|---|---|
| durable purpose and product boundary | `PROJECT_BRIEF.md` | owner-confirmed review |
| current implemented state | `STATUS.md` | repository inventory and validation output |
| normative epistemic and privacy behavior | `CONTRACTS.md` | explicit contract review |
| accepted rationale and supersession | `DECISIONS.md` | append-only decision entries |
| active transition | accepted file under `specs/` | none exists yet |

implementation, schema, and project-owned tests must replace prose-only proof at
their respective layers once those representations exist.

## invariants

| id | claim | future representation owner | current proof |
|---|---|---|---|
| `surface-001` | the private gui is the research and curation surface; the public web ui is an exploratory projection, not an administrative view of the private corpus. | application boundaries | explicit review |
| `privacy-001` | public output contains only explicitly selected public claims and safe public evidence. private material is excluded by default. | publication policy and exporter | explicit review |
| `privacy-002` | the public runtime never connects to, opens, or receives the canonical private store. | deployment and public data adapter | explicit review |
| `epistemic-001` | a property definition, an integer-property attestation, its witness or source, and its epistemic register remain distinguishable. | domain model and schema | explicit review |
| `epistemic-002` | an authored constellation is not silently converted into an exhaustive property or an unexplained set of pairwise facts. | domain model and gui | explicit review |
| `coverage-001` | absence of a positive attestation means unknown unless a recorded complete coverage claim proves that the integer was evaluated for that property. | evaluator, import, and query layers | explicit review |
| `publication-001` | publishing a claim does not publish attached private notes, rejected sources, private witnesses, or neighboring private interpretations. | publication projection | explicit review |
| `submission-001` | a public suggestion is untrusted correspondence for human review and never mutates canonical research data automatically. | submission boundary | explicit review |
| `fixture-001` | tests and demonstrations use fictional or public mathematical fixtures, never private research traces or personal identifiers. | repository fixtures and artifact scan | explicit review |

## conceptual data boundary

the durable conceptual objects are:

- integer;
- property definition;
- integer-property attestation;
- witness or source;
- coverage claim;
- authored constellation and its member roles;
- publication state and public projection.

these terms do not require one table per object and do not select a database
engine. a future schema may normalize or compose them differently only if it
preserves the distinctions above.

properties may be populated through at least three epistemically distinct
modes:

- deterministic computation with a recorded evaluator and complete range;
- bounded import with a recorded source and defensible coverage limit;
- sparse manual attestation whose unrecorded cases remain unknown.

an external sequence identifier or source link provides provenance; it does
not, by itself, establish complete local coverage.

## publication and submission lifecycle

```text
private research claim
  -> optional explicit publication selection
  -> sanitization and boundary validation
  -> versioned public projection
  -> public web consumption

public suggestion
  -> email delivery
  -> human review
  -> optional private research action
```

there is no automatic transition from a public suggestion to a property,
attestation, constellation, or public record.

## interfaces and boundaries

- desktop and web consume shared domain meanings through an interface selected
  by an accepted spec;
- deterministic property evaluation must not be reimplemented independently in
  presentation components;
- the public graph and list are derived views of published attestations and
  relations, never sources of mathematical truth;
- the public artifact must remain inspectable for excluded private sentinels;
- cross-repository integration requires a separately accepted interface.

## privacy and secrets

- `.env` files, credentials, inbox addresses requiring concealment, and private
  datasets are never read casually, committed, copied into fixtures, or emitted
  in public builds;
- private properties, notes, sources, and constellations remain local unless an
  explicit, reviewed publication selection says otherwise;
- configuration intent, generated public data, deployed behavior, and received
  email are separate representations and require separate proof;
- a public repository or static artifact is not a secrecy boundary.

## configuration

no runtime configuration exists yet. the first accepted implementation spec
must name configuration sources, precedence, secret handling, data locations,
and recovery behavior for every introduced component.

## required validation

until implementation exists:

```text
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --review-claims
git diff --check
```

future accepted specs must add project-owned proof for mathematical evaluation,
coverage semantics, private-to-public exclusion, schema migration, submission
sanitization, accessible fallback behavior, and complete artifact inspection.

## supersession

- none.

## intentionally unspecified

- implementation languages and frameworks beyond the accepted avalonia desktop
  requirement;
- physical database engine and schema;
- whether the public artifact is static, server-rendered, or backed by a public
  service;
- evaluator and import plugin shape;
- initial property list and external sequence sources;
- graph rendering library and exact visual layout;
- deployment host, email provider, and anti-abuse mechanism;
- migration, backup, recovery, and public artifact version formats.
