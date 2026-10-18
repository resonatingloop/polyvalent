# architecture

> **continuity role:** implemented representation map
> **status:** current for the accepted first slice
> **last verified:** 2026-10-18
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
```

the final arrow is conceptual: the web application has types for the public
wire format but no .net reference. its only runtime input is
`/data/manifest.json` plus `/data/data.json`.

## project responsibilities

| component | owns | must not own |
|---|---|---|
| `Polyvalent.Domain` | canonical arbitrary-precision integer identity, evaluators, evidence contracts, bounded friedman expression validation | sqlite, ui, network, publication selection |
| `Polyvalent.Application` | research-facing contracts and orchestration vocabulary | sql or renderer details |
| `Polyvalent.Data` | migrations, repositories, population ledger, sources, coverage, staging, and public projection | desktop controls or web rendering |
| `Polyvalent.Desktop` | private rolodex interaction over application services | graph spectacle, public artifact generation, deployment |
| `Polyvalent.Publisher` | explicit command boundary for initialization, seed, and release generation | network access or deployment |
| `web` | static public search, profiles, property dossiers, relation derivation, and accessible svg | sqlite, private api, authoring, mathematical evaluation |

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

the deterministic properties record deterministic coverage over `1–2000`.
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

rarer properties sort first; explicitly qualified endpoints receive salience
within an equally rare property; canonical integer order breaks remaining ties.
this makes the orderly `343` relation visible around `153` without hard-coding
that pair.

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
no secret and no provider integration.

the local database is permission-restricted on unix but is not encrypted at the
application layer. the operating-system account and publication firewall are
therefore part of the privacy model.

## deferred boundaries

the following require later accepted slices rather than opportunistic schema or
ui growth: public suggestion delivery, session-held integers, thematic lenses,
private constellations, arbitrary property-definition authoring, deployment,
analytics, and encrypted local storage.
