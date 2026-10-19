# polyvalent contracts

> **continuity role:** normative invariants
> **status:** current
> **last verified:** 2026-10-19
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
| active transition | `specs/001-first-slice.md` | accepted 2026-10-18 |

the accepted first slice, explicitly authorized pages-delivery follow-up, and
bounded seed-v5 corpus enrichment now supply implementation and project-owned
proof for their in-scope invariants. deferred features retain prose-only proof
until a later accepted slice gives them a representation.

## invariants

| id | claim | representation owner | current proof |
|---|---|---|---|
| `surface-001` | the private gui is the research and curation surface; the public web ui is an exploratory projection, not an administrative view of the private corpus. | application boundaries | solution dependency graph, desktop view-model proof, and rendered smoke |
| `privacy-001` | public output contains only explicitly selected public claims and safe public evidence. private material is excluded by default. | publication policy and exporter | publication sentinel exclusion and fail-closed tests |
| `privacy-002` | the public runtime never connects to, opens, or receives the canonical private store. | public data adapter and dependency inventory | `web/scripts/check-public-boundary.mjs` and locked runtime dependencies |
| `epistemic-001` | a property definition, an integer-property attestation, its witness or source, and its epistemic register remain distinguishable. | domain model and schema | migration foreign keys, typed records, and seed/dossier tests |
| `epistemic-002` | an authored constellation is not silently converted into an exhaustive property or an unexplained set of pairwise facts. | domain model and gui | explicit review |
| `coverage-001` | absence of a positive attestation means unknown unless a recorded complete coverage claim proves that the integer was evaluated for that property. | evaluator, import, and query layers | dossier state test and mismatch hold test |
| `publication-001` | publishing a claim does not publish attached private notes, rejected sources, private witnesses, or neighboring private interpretations. | publication projection | multi-location private-sentinel test and public dto field scan |
| `submission-001` | a public suggestion is untrusted correspondence for human review and never mutates canonical research data automatically. | submission boundary | explicit review |
| `fixture-001` | tests and demonstrations use fictional or public mathematical fixtures, never private research traces or personal identifiers. | repository fixtures and artifact scan | direct fixture review and tracked-artifact scan |
| `identity-001` | an integer is represented at persistence and public boundaries by one canonical signed-decimal string with arbitrary-precision domain semantics; storage and browser numeric limits are not domain limits. | domain value, sqlite mapping, and public schema | domain, migration round-trip, and web bigint tests |
| `provenance-001` | a source identity, a particular retrieved capture, and a citation's contextual use remain distinct and independently addressable. | provenance schema and repositories | normalized migration schema and seeded source/capture/citation records |
| `evidence-001` | every accepted property claim conforms to the versioned evidence contract of the exact property version it names. | property contract registry and validators | evaluator, witness, draft-acceptance, and publisher validation tests |
| `history-001` | accepted revisions, source captures, terminal runs, and successful releases are immutable; correction preserves lineage through explicit supersession. | schema constraints and lifecycle services | mutation rejection, checksum drift, and staged-supersession tests |
| `narrative-001` | sourced contextual lore is represented as a revisioned profile entry attached to one integer and creates no graph relation unless a later explicit model says otherwise. | narrative model and projection | twenty seeded profiles including multiple biographies for one integer and profile-only `1729`, release projection, and relation derivation that consults claims only |
| `relation-001` | published shared membership remains a graph fact even when its degree is high; the visual graph may show a declared bounded slice, but the semantic list remains complete and exact partner qualifiers may determine salience. | public relation query and renderers | twin-partner ordering test, bounded graph component, complete relation list, and public artifact checks |
| `publication-002` | private edits and staging never change public state; only an explicit successful generation creates a complete immutable release, and generation never implies deployment. | publisher and release ledger | deterministic, fail-closed, overwrite, exact citation-item, and immutable-release tests |
| `publication-003` | the public runtime consumes only a versioned public artifact and has no canonical-database, desktop, private-api, or write capability. | web data adapter and dependency inventory | runtime digest check, artifact scan, and public-boundary check |
| `delivery-001` | public deployment builds only from tracked, reviewed public inputs and uploads only the checked static web output; it never opens the private ledger or runs the publisher. | github pages workflow | private git-boundary check, public artifact/runtime checks, `check-pages-workflow.mjs`, and pages artifact scope |

## conceptual data boundary

the durable conceptual objects are:

- integer;
- property definition and immutable accepted property version;
- stable claim lineage and its draft or immutable accepted revisions;
- typed witness attached to an exact claim revision;
- evaluator, population run, comparison run, and coverage claim;
- source identity, immutable source capture, and contextual citation;
- revisioned profile entry attached to one integer;
- authored constellation and its member roles;
- mutable publication selection, immutable release, and public projection.

these terms do not require one table per object. sqlite is the accepted
canonical local engine; a future schema may normalize or compose the objects
differently only if it preserves the distinctions above.

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
  -> optional explicit staging of an exact accepted revision
  -> sanitization and boundary validation
  -> complete immutable versioned public release
  -> reviewed tracked public artifact
  -> checked static pages artifact
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
- property-specific qualifiers and witnesses must be checked by the validator
  belonging to the exact property version before acceptance;
- the public graph and list are derived views of published attestations and
  relations, never sources of mathematical truth;
- the public artifact must remain inspectable for excluded private sentinels;
- generated public data and deployment are separate operations with separate
  proof;
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

the desktop and publisher resolve the private database from an explicit `--db`,
an explicit `--data-dir`, `POLYVALENT_DATA_DIR`, or the platform-local
application-data directory, in that order. neither application loads `.env`.
`docs/development.md` owns concrete commands, ignored local paths, backup, and
restore procedure.

## required validation

```text
./scripts/check.sh
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --review-claims
```

rendered browser and native desktop review is required in addition to automated
proof when user-visible behavior changes. deployment, provider delivery, and
real owner-data privacy each require their own evidence and must not be inferred
from this local suite.

## supersession

- none.

## intentionally unspecified

- custom domain, email provider, and anti-abuse mechanism;
- private constellation representation and any public lens vocabulary;
- automatic source synchronization or general evaluator plugin system;
- application-level database encryption;
- production monitoring, analytics, and public artifact promotion mechanics.
