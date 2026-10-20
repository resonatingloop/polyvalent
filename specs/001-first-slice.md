# spec: first slice — the one-way integer instrument

status: accepted

> **continuity role:** proposed transition
> **owner:** repository owner
> **created:** 2026-10-17
> **accepted:** 2026-10-18
> **supersedes:** none

accepted by the repository owner on 2026-10-18. this spec authorizes the
bounded implementation described below.

## context

when this slice was accepted, polyvalent was a documentation-only repository.
its accepted product boundary separates a private avalonia research gui from a
public exploratory web ui while preserving one canonical research corpus and a
fail-closed publication boundary.

this slice establishes the smallest end-to-end instrument that can prove those
promises rather than merely scaffold them. it begins with integers `1` through
`2000`, five deliberately different mathematical properties, a local sqlite
provenance ledger, a narrow private rolodex, an explicit publication command,
and a static public profile-and-relation experience.

the target state is not a public deployment or a complete research gui. it is
a locally executable vertical path whose mathematical, epistemic, persistence,
privacy, and presentation seams can all be tested.

## scope

in:

- a `.net 10` solution with renderer-independent domain and application
  layers, direct sqlite persistence, an avalonia desktop application, and a
  publication executable;
- a react, typescript, and vite web application that consumes only a versioned
  public json release;
- arbitrary-precision integer identity represented canonically at persistence
  and web boundaries;
- a migrated canonical sqlite store with claims, immutable accepted revisions,
  typed witnesses, population runs, coverage, sources, captures, citations,
  profile entries, publication staging, and immutable release records;
- reproducible population of integers `1` through `2000` for prime,
  triangular, decimal-palindrome, decimal-narcissistic, and decimal-friedman
  properties;
- a narrow private rolodex workflow for inspecting integer records, evidence,
  coverage, sources, private drafts, profile entries, and publication staging;
- an explicit publisher that emits a sanitized, versioned, inspectable public
  artifact and never publishes merely because private state changed;
- public integer and property pages, explained relation lists, a small
  bipartite relation graph, and a non-graph equivalent;
- project-owned mathematical, migration, lifecycle, privacy, artifact, web,
  and desktop proof;
- development, architecture, attribution, and recovery documentation created
  alongside the implementation.

out:

- deployment, hosting, domains, analytics, or production monitoring;
- the public suggestion form, email provider, anti-abuse controls, or any
  submission-to-inbox integration;
- session-held integers, encounter pings, or persistent visitor state;
- thematic lenses such as an occult lens;
- private constellation authoring and storage;
- explicit integer-to-integer relation authoring beyond relations derived from
  shared published properties;
- property-definition creation in the gui;
- public accounts, administrative routes, editing, or database access;
- automatic or scheduled oeis synchronization;
- a graph of the complete corpus or graph-only navigation;
- application-level database encryption;
- final visual polish or parity with every existing resonance instrument.

## governing contracts and decisions

- `CONTRACTS.md`: `surface-001`, `privacy-001`, `privacy-002`,
  `epistemic-001`, `epistemic-002`, `coverage-001`, `publication-001`,
  `submission-001`, and `fixture-001`;
- `CONTRACTS.md`: the storage, provenance, evidence, history, narrative, and
  publication invariants added with this draft;
- `DECISIONS.md`: `d-001` through `d-012`;
- `PROJECT_BRIEF.md`: the two-surface experience, visual lineage, private
  research boundary, and public-profile intent.

## change shape

| dimension | changed? | source of truth | proof |
|---|---:|---|---|
| user-visible behavior | yes | desktop and web applications | focused view-model tests, browser smoke, and owner-visible native/web review |
| persisted state or schema | yes | sqlite migrations and repository layer | fresh migration, reopen, constraint, lifecycle, and backup/restore tests |
| external or provider-visible behavior | no | scope boundary | no deployment, email delivery, analytics, or automated live-source call exists |
| identifiers, secrets or privacy | yes | domain identity, publication policy, and public schema | arbitrary-precision round trips, private-sentinel exclusion, artifact schema scan, and no-secret inventory |
| lifecycle or terminal states | yes | claim, run, capture, and release state machines | transition and immutability tests |
| setup, operation or recovery | yes | `README.md` and `docs/development.md` | cold-checkout build, seed, run, publish-preview, backup, and restore exercises |
| normative contract or decision | yes | `CONTRACTS.md` and `DECISIONS.md` | documentation check and owner review |
| cross-repository dependency | no | project files and dependency graph | build succeeds without another resonance checkout and dependency inventory contains none |

## proposed architecture

```text
canonical private sqlite store
          │
          ├── avalonia desktop
          │     rolodex, research drafts, evidence, curation
          │
          └── explicit publisher
                selection
                  -> sanitization
                  -> boundary validation
                  -> immutable public release
                            │
                            ▼
                  static react/vite web ui
                  profiles, lists, relation graph
```

the governing implementation rule is **shared domain meaning, separate
runtimes**. `.net` owns canonical mathematical evaluation, evidence validation,
persistence, and publication. the web application queries and renders the
public release; it does not evaluate mathematical membership, access sqlite,
or know how the desktop application works.

### proposed repository shape

```text
src/
  polyvalent.domain/
  polyvalent.application/
  polyvalent.data/
  polyvalent.desktop/
  polyvalent.publisher/

tests/
  polyvalent.domain.tests/
  polyvalent.data.tests/
  polyvalent.publication.tests/

web/
  src/
  public/data/

data/
  source-extracts/schema-v1/

docs/
  architecture.md
  development.md

specs/
  001-first-slice.md
```

the exact casing of generated project filenames may follow `.net` conventions,
but namespace and dependency directions must preserve these boundaries.

### proposed stack

- `.net 10` and `system.numerics.biginteger` for the domain;
- avalonia `12` and `communitytoolkit.mvvm` for the private desktop;
- `microsoft.data.sqlite` with explicit parameterized sql and ordered migration
  files rather than an object-relational mapper;
- `system.text.json` for canonical payload and release serialization;
- xunit-compatible `.net` tests;
- react `19`, typescript, vite, and vitest for the public web application;
- a bounded, accessible svg relation renderer rather than a graph dependency
  in this slice.

exact dependency versions belong to committed project files and lockfiles. no
component may import code, assets, or runtime state from another resonance
repository.

## integer identity

an integer's public identity is its canonical signed decimal representation,
not a sqlite or javascript numeric value.

- domain arithmetic uses `biginteger`;
- sqlite stores the canonical value as unique text behind an internal surrogate
  key;
- public json represents every integer-like quantity as a string;
- accepted forms are `0`, a nonzero unsigned decimal sequence, or `-` followed
  by a nonzero unsigned decimal sequence;
- leading `+`, leading zeroes, whitespace, decimal points, exponents, and `-0`
  are rejected;
- the first populated range is `1` through `2000`, but neither schema nor wire
  format imposes that as a domain ceiling.

ordering and range iteration are performed through domain `biginteger` values,
not lexicographic sqlite text order.

## persistence and schema

the canonical store is sqlite. foreign-key enforcement is enabled on every
connection, writes are transactional, and migrations are ordered, checksummed,
and recorded in `schema_migrations`.

the first schema may normalize names differently, but it must preserve these
logical groups and foreign-key relationships:

### identity and vocabulary

- `integers`: internal identity plus canonical decimal value;
- `properties`: stable property identity and slug;
- `property_versions`: immutable accepted definitions, aliases, graph policy,
  evidence contract, and optional evaluator binding.

### claims and evidence

- `claims`: stable lineage connecting one integer with one property;
- `claim_revisions`: draft or immutable accepted assertion revisions naming an
  exact property version, plus origin kind, provenance link, qualifiers,
  review disposition, and supersession;
- `claim_witnesses`: versioned typed evidence payloads attached to exact claim
  revisions.

there is no dynamic property column on `integers`, universal notes blob, or
entity-attribute-value substitute for evidence.

### provenance and coverage

- `evaluators`: stable compiled evaluator keys and semantic versions;
- `population_runs`: immutable deterministic or bounded-import runs, including
  exact inputs, range, tool version, timestamps, and terminal outcome;
- `comparison_runs`: immutable comparisons between a computed result set and a
  captured external sequence;
- `coverage_claims`: the property version, range, basis, exclusions, and run
  that warrant complete local coverage;
- `sources`: stable intellectual or publication identities;
- `source_captures`: immutable records of a particular retrieval, including
  uri, retrieval time, media type, digest, revision metadata when available,
  and license metadata;
- explicit citation tables linking sources or captures to property versions,
  claims, and profile entries with roles and locators.

citations use real foreign keys rather than a generic target-type and target-id
pair.

### narrative

- `profile_entries`: stable identity for contextual material attached to one
  integer;
- `profile_entry_revisions`: private drafts and immutable accepted revisions
  with title, category, epistemic register, and plain-text body;
- citations attach to exact revisions.

profile entries produce no graph edge by default. first-slice public bodies are
plain text; arbitrary html and markdown execution are not supported.

### publication

- `publication_selections`: mutable staging records that select exact accepted
  claim or profile-entry revisions and individually approved citations;
- `public_releases`: immutable successful release identities, schema versions,
  timestamps, and content digests;
- `public_release_items`: the exact selections included in a release.

there is no ambient `is_public` column. publication is an allowlisted relation
to exact revisions.

## lifecycle and immutability

- private drafts may be edited and may lack complete evidence;
- a draft contributes neither positive public membership nor negative coverage
  and cannot create a public graph relation;
- accepting a claim or profile-entry revision freezes its evidence-bearing
  content;
- correcting accepted content creates a new revision that explicitly
  supersedes the prior revision;
- generated and imported claim content cannot be hand-edited; it is replaced
  through a new run or disputed through a separate research action;
- source captures, terminal population and comparison runs, accepted
  revisions, and successful public releases are immutable;
- population runs progress from prepared to running and then exactly one of
  succeeded, failed, or cancelled;
- a coverage claim may reference only a succeeded run and must state whether
  its basis is deterministic evaluation or source-asserted bounded
  enumeration;
- publication staging is mutable; release generation either produces one
  complete immutable artifact and release record or produces neither;
- editing or staging private data never regenerates, deploys, or otherwise
  changes the public site.

## evidence contracts

each property version carries a versioned declarative evidence contract and a
stable compiled validator key. property-specific qualifiers and witnesses are
stored as json only after validation against that exact contract.

an evidence contract states:

- allowed qualifier fields and values;
- accepted witness kinds and payload versions;
- whether a per-claim witness is required;
- whether a successful deterministic run is sufficient;
- whether an external citation is required;
- whether a qualifier must be derived from or checked against a witness;
- which compiled validator and evaluator versions own executable semantics;
- what kind of coverage the property may establish.

the database stores declarative contracts and normalized payloads, never
executable code. the web receives display-ready public evidence and does not
interpret or execute witness expressions.

## initial property pack

| property | population | evidence | coverage | graph mode |
|---|---|---|---|---|
| prime | deterministic evaluation | successful run; compact deterministic certificate optional | complete `1–2000` | none |
| triangular number | deterministic evaluation | required `sequence_index` witness | complete `1–2000` | shared membership |
| decimal palindrome | deterministic evaluation | required `digit_symmetry` witness | complete `1–2000` | none |
| decimal narcissistic number | deterministic evaluation plus source comparison | required `digit_power_sum` witness | complete `1–2000` by local evaluation | shared membership |
| decimal friedman number | bounded source import plus local expression validation | required `digit_expression` witness; optional validated `orderly` qualifier | source-asserted complete `1–2000` | shared membership |

expected population invariants:

- `303` primes;
- `62` positive triangular numbers, ending with `1953`;
- `118` positive decimal palindromes;
- `14` decimal narcissistic numbers:
  `1–9`, `153`, `370`, `371`, `407`, and `1634`;
- `27` decimal friedman numbers, beginning with `25` and ending with `1827`
  inside this range;
- the orderly/nice friedman members inside the range are `127`, `343`, `736`,
  and `1285`.

the narcissistic evaluator is compared against
[`oeis A005188`](https://oeis.org/A005188) and its captured machine-readable
sequence. the friedman import uses [`oeis A036057`](https://oeis.org/A036057),
[`oeis A080035`](https://oeis.org/A080035), and
[`erich friedman's bounded enumeration`](https://erich-friedman.github.io/mathmagic/0800.html).
source records preserve attribution and license metadata. raw external pages
are not required in tracked source; any shipped bounded extract carries an
explicit third-party notice and source digest.

the canonical vocabulary is `friedman number`; the ordered-digit subtype uses
the canonical qualifier `orderly`, with `nice` and `good` retained as source
aliases. this slice does not use `strong` or `weak` as sourced qualifiers.

if a local evaluation, imported set, supplied witness, or captured source
disagrees, the disagreement is preserved, the affected property is not
publishable, and no source is silently preferred.

## private desktop behavior

the desktop is a compact research rolodex, not a social profile or graph.

the first-slice happy path is:

1. launch against the default or explicitly selected local database;
2. search or move through integers `1–2000`;
3. inspect the selected integer's active claims, origin, witnesses, citations,
   and positive, covered-negative, or unassessed status;
4. expand and collapse evidence-bearing sections without losing the selected
   integer;
5. create and edit a private manual draft against an existing property;
6. create a source record and contextual citation;
7. create and revise a private profile entry;
8. accept or withdraw a draft through the application lifecycle;
9. stage an exact accepted claim or profile-entry revision and individually
   selected public-safe citations for publication;
10. inspect the staged set without generating a release.

generated or imported claims remain read-only. the gui may annotate, dispute,
or request regeneration but may not overwrite their evidence.

the visual baseline follows the resonance plate family through native avalonia
materials: dark matte layers, compact evidence controls, serif display type,
mono operational type, restrained ember and iris accents, and the integer
record occupying the center. graph spectacle and public social-card language do
not enter this surface.

## publication behavior

publication is an explicit command operating on one consistent read snapshot
of the private database.

the publisher must:

1. read only exact staged accepted revisions;
2. traverse only explicitly selected public-safe evidence and citations;
3. map database records into dedicated public projection types rather than
   serializing persistence entities;
4. reject incomplete evidence, disputed or superseded selections, invalid
   source urls, unknown witness renderers, and references to unselected private
   records;
5. generate into a temporary directory;
6. validate the complete artifact schema and scan every emitted byte for
   private sentinel values;
7. atomically finalize `manifest.json` and `data.json` only after all checks
   pass;
8. record one immutable release with the digest of `data.json`;
9. make no network request and perform no deployment.

`manifest.json` identifies the public schema version, release id, generation
time, source database schema version, and sha-256 digest of `data.json`.

`data.json` contains only:

- canonical integer strings;
- selected public property definitions and aliases;
- selected active claims, endpoint qualifiers, and display-ready witnesses;
- selected profile-entry revisions;
- selected citation labels, locators, and safe `http` or `https` urls;
- graph policy and the membership data required to derive explained
  relationships.

it contains no sqlite keys, private notes, raw capture bodies or paths,
unselected citations, draft history, source-review debris, configuration, or
credentials.

one deterministic public mathematical release is committed for cold-checkout
web development. publication tests generate releases in temporary directories.
a later deployment spec decides how owner-curated releases enter hosting and
whether deployed release artifacts are committed or supplied separately.

## public web behavior

the web application has no backend, database driver, private-api client, write
path, or runtime source lookup.

the first-slice happy path is:

1. enter an integer on the landing page;
2. open `/integer/{canonical-value}`;
3. see a public profile with plain-text profile entries, property affiliations,
   witnesses, and selected citations;
4. inspect every related integer and the exact shared property that relates it;
5. switch between the complete relation list and a centered bipartite graph;
6. follow a property node to `/property/{slug}` and inspect its definition,
   sources, coverage statement, and published members.

the graph renders:

```text
center integer -> shared property -> related integer
```

only properties whose published graph mode is `shared membership` participate.
the graph shows a deterministic bounded neighbor selection and reports omitted
counts; the list exposes every published relation. rarity and explicit graph
policy determine salience, so ubiquitous properties such as prime do not form
meaningless clouds.

`explainrelationship(a, b)` intersects accepted published memberships and
returns the shared property plus endpoint qualifiers and witness summaries. it
does not perform mathematical evaluation.

for the reference path:

```text
153 -> friedman number -> 343

153: friedman witness 3 × 51
343: orderly/nice friedman witness (3 + 4)^3
```

all graph information is available in semantic text and links. keyboard use,
focus visibility, reduced motion, readable contrast, and narrow-screen behavior
are acceptance requirements.

the public aesthetic shares cartographic darkness, unequal gravity, and
luminous constellational traces with resonatrix and upward without copying
their components or depending on their repositories.

## configuration, data, and recovery

configuration precedence for the private database is:

1. an explicit command-line database or data-directory option;
2. `POLYVALENT_DATA_DIR` from the process environment;
3. the platform-local application-data directory under `polyvalent`.

the application does not load `.env` files. no secret is introduced by this
slice.

the default database filename is `polyvalent.sqlite3`. repository-local
database state belongs under ignored `./var/`, local source captures under
ignored `./captures/`, and owner-generated release previews under ignored
`./artifacts/`; common database filenames, sidecars, and migration backups are
also ignored as defense in depth. none are used as repository fixtures.

before applying a migration to a nonempty database, the data layer creates a
timestamped sqlite-consistent backup in the same private data root. migrations
run transactionally; failure leaves the prior database usable and reports the
backup path. restore is an explicit documented file operation against a closed
application and never overwrites the current database automatically.

public recovery consists of selecting and serving a previously validated
immutable release. deployment mechanics remain outside this slice.

## likely files and components

- `.net` solution and project files under `src/` and `tests/`;
- ordered sql migrations and repository implementations under
  `src/polyvalent.data/`;
- seed manifests, property contracts, and attributed bounded source data under
  `data/source-extracts/schema-v1/`;
- avalonia views, view models, and theme resources under
  `src/polyvalent.desktop/`;
- publisher commands and public dto/schema definitions under
  `src/polyvalent.publisher/`;
- react application, public schema adapter, relation query, and accessible
  graph under `web/`;
- unified validation helpers under `scripts/`;
- `docs/architecture.md`, `docs/development.md`, and any required third-party
  notice;
- reconciled `README.md`, `STATUS.md`, `AGENTS.md`, `CONTRACTS.md`, and
  `DECISIONS.md` at the implementation checkpoint.

## implementation by invariant: eight chambers

1. establish the foundation and persistence: create the solution, dependency
   direction, web application, test projects, and cold-checkout commands;
   implement canonical integer parsing, `biginteger` round trips, the initial
   sqlite migration, foreign keys, transactional repositories,
   backup-before-migration, and reopen proof; prove no cross-repository runtime
   dependency.
2. establish selective immutability: implement draft, acceptance,
   supersession, run, capture, and release states with forbidden-transition
   tests.
3. establish evidence-bearing knowledge: implement versioned property
   contracts, typed witnesses, sources, captures, contextual citations,
   coverage bases, and positive, covered-negative, and unknown queries.
4. establish reproducible population: run the four deterministic evaluators,
   import and validate bounded friedman evidence, compare narcissistic results
   with the captured source set, and assert exact counts and specimen witnesses.
5. establish the private research path: render the native rolodex, preserve
   selection across collapsed evidence sections, support private drafts and
   profile entries, and stage exact accepted revisions.
6. establish the one-way press: generate the public bundle through dedicated
   projection types, validate it completely, prove sentinel exclusion, and
   record immutable releases without networking or deployment.
7. establish public exploration: render search, number and property profiles,
   complete relation lists, the bounded bipartite graph, evidence explanations,
   and accessible fallback behavior from the release alone.
8. establish operations and continuity: document setup, data locations,
   migration, backup, restore, source attribution, publication preview, and
   exact verification commands; then update the current checkpoint.

## done when

- a cold checkout can restore, build, test, seed, launch the desktop, generate
  a public release, and build the web application through documented commands;
- the initial migration produces a constrained sqlite database and can reopen
  it without mutation;
- canonical integer values larger than both javascript's exact numeric range
  and sqlite's signed integer range round-trip without loss;
- all five initial properties produce the expected `1–2000` counts and the
  witnesses required by their exact evidence contracts;
- absence is reported as covered-negative only inside a valid complete
  coverage claim and remains unknown elsewhere;
- `153` and `343` are related publicly through the friedman property, with
  `343` identified as orderly/nice and neither number labeled through the
  unsupported `strong/weak` vocabulary;
- the desktop can create an incomplete private draft without letting it affect
  coverage, publication, or public relations;
- accepted content cannot be edited in place and corrections preserve explicit
  supersession;
- generated and imported evidence cannot be hand-edited;
- a source capture, terminal population run, and successful release reject
  attempted mutation;
- release generation is deterministic under an injected clock and release id;
- a private-only sentinel placed in notes, source metadata, neighboring claims,
  and draft profile text is absent from every byte of the emitted artifact;
- the web build and browser smoke complete with no database, backend, or
  private-file access;
- every graph relationship has the same textual explanation available without
  using the graph;
- desktop and web surfaces receive owner-visible rendered review in addition to
  automated proof;
- affected documentation accurately distinguishes implemented, tested,
  owner-observed, deferred, and unverified behavior.

## validation expected after implementation

```text
dotnet restore
dotnet build -c release
dotnet test -c release
npm --prefix web ci
npm --prefix web run check
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --require SCHEMA.md --review-claims
git diff --check
direct review of the complete diff and generated public artifact
```

the implementation must replace these provisional commands with the exact
project-owned happy path in `README.md`, `AGENTS.md`, and
`docs/development.md`.

## risks and unknowns

- validating friedman expressions safely is more complex than displaying
  them; the parser must accept only the bounded mathematical grammar and must
  never evaluate arbitrary code;
- source-asserted friedman completeness has a different warrant from local
  deterministic coverage and must remain visibly distinct;
- oeis-derived material requires preserved attribution and license metadata;
- a custom svg graph may need iteration to balance clarity, motion, keyboard
  access, and visual distinctiveness;
- implementing both surfaces in one slice creates schedule pressure, so no
  deferred feature may enter without a spec revision;
- application-level encryption is absent; privacy depends on the local
  operating-system account, restrictive file placement, and the publication
  firewall;
- native desktop appearance, browser interaction, deployment, email delivery,
  and real owner-data privacy cannot be inferred from unit tests alone.

## open questions

- none inside the accepted slice. acceptance includes the direct-sql stack,
  five-property pack, listed deferrals, static release boundary, and
  unencrypted local-store posture.
