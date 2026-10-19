# architecture

> **continuity role:** implemented representation map
> **status:** current for the accepted first slice, pages delivery, and seed-v3 enrichment
> **last verified:** 2026-10-19
> **owns:** dependency direction, data boundaries, lifecycle, and publication seam
> **update when:** a layer, durable representation, or trust boundary changes

## governing shape

polyvalent shares meaning, not runtime state, across its two surfaces.

```text
Polyvalent.Domain
      ▲
      │
Polyvalent.Application
      ▲              ▲
      │              │
Polyvalent.Data      │
   ▲        ▲        │
   │        │        │
desktop   publisher  │
             │        │
             ▼        │
       public json    │
             │        │
             ▼        │
       react web ─────┘ meaning only; no code reference
             │
             ▼
      pages artifact
             │
             ▼
       github pages
```

the horizontal meaning-only arrow is conceptual: the web application has types
for the public wire format but no .net reference. its only runtime input is
`data/manifest.json` plus `data/data.json` beneath the active deployment base.

## project responsibilities

| component | owns | must not own |
|---|---|---|
| `Polyvalent.Domain` | canonical arbitrary-precision integer identity, evaluators, evidence contracts, bounded friedman expression validation | sqlite, ui, network, publication selection |
| `Polyvalent.Application` | research-facing contracts and orchestration vocabulary | sql or renderer details |
| `Polyvalent.Data` | migrations, repositories, population ledger, sources, coverage, staging, and public projection | desktop controls or web rendering |
| `Polyvalent.Desktop` | private rolodex interaction over application services | graph spectacle, public artifact generation, deployment |
| `Polyvalent.Publisher` | explicit command boundary for initialization, seed, and release generation | network access or deployment |
| `web` | static public search, profiles, property dossiers, relation derivation, and accessible svg | sqlite, private api, authoring, mathematical evaluation |
| pages workflow | checked static build and github pages delivery | publisher execution, database access, repository secrets, or non-web artifact upload |

the dependency graph is intentionally one-way. another resonance repository is
neither a package dependency nor a runtime data source.

## integer identity

an integer is canonical signed-decimal text at sqlite and json boundaries and a
`System.Numerics.BigInteger` inside the domain. the parser accepts `0`, a
nonzero unsigned decimal, or a minus sign followed by a nonzero unsigned
decimal. it rejects leading plus signs, leading zeroes, whitespace, decimal
points, exponents, and `-0`.

sqlite surrogate keys identify rows; they never replace mathematical identity.
the browser uses `BigInt` only for comparison and coverage checks and never
round-trips integer identity through javascript `number`.

## canonical ledger

the first migration separates six kinds of thing that would otherwise collapse
into a tag table:

1. integer and versioned property vocabulary;
2. stable claim lineages, revisions, qualifiers, and typed witnesses;
3. evaluators, population runs, comparisons, and coverage warrants;
4. source identities, immutable captures, and contextual citations;
5. revisioned profile entries for sourced lore that makes no graph edge;
6. mutable publication staging and immutable sealed release records.

accepted revisions, source captures, terminal runs, comparison results,
coverage claims, and sealed releases are protected by sqlite triggers. a
correction becomes a new revision with `supersedes_revision_id`; staging the new
revision evicts a stale selection from the same lineage. generated and imported
claims cannot be accepted, edited, or withdrawn through the manual research
service.

## open-world knowledge

positive claims are materialized. a missing claim is not false by default.
`PolyvalentStore` reports one of:

- `positive`: an active accepted revision exists;
- `covered negative`: no accepted revision exists and a succeeded run records
  complete coverage containing the integer;
- `unknown`: no positive claim and no applicable coverage warrant.

the eight deterministic properties record deterministic coverage over `1–2000`.
friedman coverage is explicitly `source-asserted-bounded`. a mismatched source
comparison is preserved as a comparison record and the affected property is
withheld from coverage and publication staging.

## evidence and expressions

each property version stores a declarative evidence contract, validator key,
graph policy, aliases, and optional evaluator key. executable semantics remain
in versioned domain code.

friedman witnesses pass through a hand-written arithmetic parser. its grammar
is limited to decimal digits, parentheses, unary signs, `+`, `-`, `*`, `/`, and
`^`; it bounds expression length, tokens, exponent, and intermediate magnitude;
uses exact rational arithmetic; consumes every source digit exactly once; and
requires a nontrivial operation. it never executes source text.

the current `seed-v3` reference corpus retains the perfect-square, perfect-cube,
fibonacci, and twin-prime evaluators added beside the original five-property
pack. power and sequence witnesses record exact constructive identities. twin-prime claims
record their lower and higher partners as checked qualifiers, allowing the
public renderer to distinguish an exact partner from ambient shared membership.
prime is graphable in this version; density is controlled by presentation, not
by deleting the relation from the published ontology.

the same seed stages seven cited profile entries. profile entries may cause an
otherwise claimless integer such as `86`, `93`, or `146` to appear in the public
corpus, but relation derivation still consults claims only. a composite profile
may place distinct attested registers beside one another, as the geometry and
scripture attached to `153` do, without asserting causal or encoded identity.

## explicit publication firewall

publication is allowlisted at exact-revision granularity:

```text
accepted private revision
  -> explicit staging
  -> consistent read snapshot
  -> dedicated public dto mapping
  -> evidence, url, and sentinel validation
  -> temporary directory
  -> atomic directory move
  -> immutable release ledger transaction
```

the projection exposes only canonical values, selected property definitions,
coverage, active selected claims, display-ready witnesses, selected profile
entries, and selected public-safe citation labels, locators, roles, and http(s)
urls. persistence ids, private notes, raw capture metadata, local paths, draft
history, and unselected neighbors have no field in the public dto.
the sealed release ledger separately pins the exact property, claim, profile,
and citation rows represented by those bytes, so later staging changes cannot
rewrite what a historical release selected.

generation never overwrites a target, makes no network request, and performs no
deployment. an ordinary failure removes temporary output; a failure while
recording the release removes the newly finalized target. a machine crash can
leave an orphan directory, but it is not a valid release because it lacks a
sealed ledger row. recovery quarantines that directory rather than treating it
as published.

the browser fetches both release files, verifies the sha-256 digest before
parsing `data.json`, and then checks release and schema identity. the committed
artifact also receives an independent node-based digest and forbidden-field
scan during every web check.

## static delivery boundary

github pages delivery begins from tracked repository state, not from the private
ledger or an owner preview directory. a read-only build job verifies the git
privacy boundary, public release digest, public dependency inventory, static
fetch allowlist, types, and tests. it then builds against the base path reported
by github pages, adds an `index.html`-equivalent `404.html` client-routing shell,
and uploads only `web/dist`.

a second job has pages and oidc write authority but no checkout step. it can
deploy only the static artifact produced by the successful build job. this
keeps release generation, tracked-artifact review, repository push, and live
deployment as separately observable transitions.

## public relation derivation

the public artifact stores memberships, never pairwise social edges. the web
derives a relationship by intersecting the two integers' accepted published
claims and retaining only property versions whose graph mode is
`shared-membership`.

the complete semantic list is authoritative. the svg is a bounded bipartite
view:

```text
center integer -> shared property -> related integer
```

rarer properties sort first; an exact twin-prime partner sorts ahead of ambient
members within that property; explicitly qualified endpoints then receive
salience, and canonical integer order breaks remaining ties. this makes both
the orderly `343` relation around `153` and the exact `139` relation around
`137` visible without hard-coding either pair. high-degree properties such as
prime remain honest graph facts: the svg shows a deterministic bounded slice,
reports the omitted count, and leaves the complete relation list authoritative.

## private desktop boundary

the avalonia surface opens the canonical ledger directly because it is the
private research tool. it presents a dense rolodex, positive/covered/unknown
states, evidence, profile entries, authoring benches, and exact-revision staging.
it contains no public graph or social card. collapsing research sections does
not change the selected integer because selection lives in the view model.

## configuration and secrets

the database path resolver accepts an explicit database, an explicit data
directory, `POLYVALENT_DATA_DIR`, or the platform-local application-data
directory in that order. neither executable reads `.env`. this slice introduces
no application secret. the pages workflow uses only github's job-scoped token
with per-job least privilege; no provider client enters either application.

the local database is permission-restricted on unix but is not encrypted at the
application layer. the operating-system account and publication firewall are
therefore part of the privacy model.

## deferred boundaries

the following require later accepted slices rather than opportunistic schema or
ui growth: public suggestion delivery, session-held integers, thematic lenses,
private constellations, arbitrary property-definition authoring, custom-domain
operation, analytics, and encrypted local storage.
