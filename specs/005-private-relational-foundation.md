# spec: private relational foundation — the company integers keep

status: accepted

> **continuity role:** accepted transition
> **owner:** repository owner
> **created:** 2026-11-15
> **accepted:** 2026-11-17
> **supersedes:** none
> **related decisions:** `d-001`, `d-003`, `d-006`, `d-008`, `d-011`, `d-012`, `d-018`

accepted by the repository owner on 2026-11-17. this spec authorizes the
bounded local implementation and verification described below. it does not
authorize migration of the owner's ledger, inspection of owner data, discord
or other provider access, publication, commit, push, or deployment.

## context

polyvalent already treats number identity and number relations as equal parts
of its purpose, but the implemented private instrument currently knows only
one active research object: an integer dossier. that dossier gathers property
states, evidence, profile entries, and publication staging. the public site can
say “who this integer knows,” but those relations are derived exclusively from
shared published property membership. the private schema has no authored
direct-relation or constellation representation.

the missing distinction is epistemic rather than graphical. these are two
different research objects:

- a **direct relation** is a specific proposition involving exactly two
  integers in explicit roles, such as “773 has prime index 137”;
- a **constellation** is an authored company of integers held together by a
  named account and optional member roles.

they are close cousins, not aliases. a direct relation says what passes between
two participants. a constellation says why these participants belong in one
room. a constellation may cite or discuss direct relations, and a direct
relation may motivate a constellation, but neither representation is generated
from the other.

`d-003` and `epistemic-002` already protect constellations from being flattened
into properties or unexplained pairwise edges. this accepted transition extends that
protection to first-class direct relations while preserving the current
property, coverage, publication, and public-relation systems unchanged.

the larger conversation also identified a future private correspondence spool
and discord courier. those are deliberately excluded here. a capture channel
cannot be designed safely until the research objects into which a reviewed tip
might eventually be transformed have stable identities and lifecycle rules.

## owner view

**one-breath change:** give the private ledger and avalonia instrument two new
revisioned research objects—direct relations and constellations—then let an
integer, relation, or constellation occupy the same spacious active-object
room without adding any public projection or automatic inference.

current:

```text
active integer
  -> property states and evidence
  -> contextual profile entries
  -> private drafting and publication staging

public relation
  -> derived from shared published property membership
```

accepted target:

```text
private active object
  -> integer
       -> property states
       -> direct relations
       -> constellation memberships
  -> direct relation
       -> exactly two role-bearing integer participants
       -> authored proposition, warrant, register, sources, revisions
  -> constellation
       -> two or more role-bearing integer members
       -> authored account, register, sources, revisions

public artifact and public relation derivation
  -> unchanged
```

**authority:** `CONTRACTS.md` continues to own epistemic, privacy, history, and
publication invariants; executable migrations remain schema authority;
`DECISIONS.md` owns accepted rationale; this spec owns the bounded
transition. owner acceptance authorizes the listed local implementation and
the explicit contract and decision additions described below, but no use of
the resulting migration against the owner's ledger.

## accepted owner decisions

| decision | recommendation | alternative | consequence of recommendation |
|---|---|---|---|
| relation and constellation identity | model direct relations and constellations as distinct first-class lineages with parallel revision, citation, and lifecycle machinery | use one generic relation object with cardinality and type flags | preserves the difference between a proposition and an authored company instead of making every distinction a presentation convention |
| direct relation shape | require exactly two distinct canonical integers, explicit participant roles, a rendered proposition, and declared directionality | store an unlabeled pair plus prose notes | permits `773` as value and `137` as prime index without pretending every relation is symmetric or semantically interchangeable |
| constellation shape | require a named account and at least two distinct canonical integer members; permit member roles and member notes | treat a constellation as a bag of integers or as all possible pairwise edges | keeps the collective account primary and prevents combinatorial edge manufacture |
| shared wardrobe | give both objects stable lineages, draft and immutable accepted revisions, explicit supersession, epistemic register, private notes, sources, captures, and revision-scoped citations | let each feature invent unrelated evidence and history conventions | reuses the ledger's provenance grammar while keeping the two object types distinct |
| epistemic register | require nonempty owner-authored plain text in this slice; do not establish a closed universal taxonomy | hard-code a supposedly complete enum now | makes the epistemic position visible without prematurely deciding every species of mathematical, historical, interpretive, or synchronistic warrant |
| inference | perform no pairwise expansion, transitive closure, reciprocal synthesis, property conversion, or automatic cousin creation | derive additional records whenever participants overlap | ensures every authored relation and constellation remains attributable to an explicit research act |
| privacy and publication | keep every new object private and absent from staging, publisher dtos, release items, and public bytes | add public selection and rendering now | proves the private ontology before creating a second publication membrane or changing what the public graph means |
| native room | replace the always-visible three-column authoring bench with one active-object field, a held-object rail, an epistemic margin, and contextual actions | add relation and constellation accordions to the present integer-only dashboard | lets the purpose of the instrument govern its geometry and avoids multiplying small controls around the existing layout |
| session navigation | keep held objects and the visit trail in memory only for this slice | persist workspace sessions immediately | provides roaming and return without adding another recovery, migration, or privacy lifecycle |
| migration boundary | add one additive migration and prove it only against disposable or copied fixtures during implementation | mutate an owner ledger as implementation proof | permits an exact schema transition while keeping owner data outside the development and verification boundary |

the owner's 2026-11-17 acceptance selected the recommendations in this table.

## scope

in:

- renderer-independent domain identities for a direct relation, constellation,
  participant or member role, directionality, epistemic register, and revision
  state;
- additive private sqlite persistence for stable direct-relation and
  constellation lineages, revisioned content, participants or members, and
  revision-scoped citations;
- reuse of canonical integer identity, source identity, immutable source
  captures, citation locators, and accepted-revision immutability;
- draft creation and editing, citation attachment, acceptance, withdrawal, and
  explicit superseding revision for both object types;
- private queries from an integer to its accepted and draft direct relations
  and constellation memberships;
- an avalonia active-object model that can open an integer, direct relation, or
  constellation without confusing one representation for another;
- a session-local held-object rail and bounded visit trail with no persisted
  workspace state;
- contextual authoring controls that appear for the active object's current
  lifecycle rather than one permanent bench containing every possible action;
- one complete fictional or public mathematical fixture for each object type,
  plus boundary fixtures proving that overlap creates no automatic record;
- migration, repository, application, view-model, rendered native, privacy,
  backup, restore, and documentation proof;
- an accepted decision entry and normative contract additions only if the
  owner accepts this spec.

out:

- correspondence envelopes, a local tip spool, hot-key capture, discord,
  bots, tokens, channel or user allowlists, attachments, acknowledgements, or
  cursor reconciliation;
- any automatic transition from a suggestion, message, note, property claim,
  shared membership, qualifier, profile entry, direct relation, or
  constellation into another research object;
- public staging, publication selections, release items, artifact fields,
  routes, graph edges, lists, lenses, or public pages for authored direct
  relations or constellations;
- changing the meaning or derivation of current public shared-membership
  relations;
- a freeform graph canvas, force layout, complete-corpus map, or graph-only
  navigation;
- persisted workspaces, saved trails, encounter pings, or session-held integer
  recovery;
- a universal controlled vocabulary for epistemic registers, relation kinds,
  constellation kinds, or cultural traditions;
- automatic validation of arbitrary mathematical prose or general theorem
  proving;
- inspection, migration, backup, repair, or mutation of the owner's actual
  ledger during implementation or automated verification;
- commit, push, provider calls, publication, deployment, or live-state claims.

## governing contracts and decisions

unchanged contracts:

- `surface-001` keeps the avalonia application as the private research and
  curation surface and the public site as a separate projection;
- `privacy-001`, `privacy-002`, `publication-001`, `publication-002`, and
  `publication-003` keep private state excluded by default and publication
  explicit, sanitized, immutable, and separate from deployment;
- `epistemic-001` keeps definitions, attestations, witnesses, sources, and
  registers distinguishable;
- `epistemic-002` already forbids silently converting a constellation into an
  exhaustive property or unexplained pairwise facts;
- `provenance-001` keeps a source identity, capture, and contextual citation
  independently addressable;
- `history-001` keeps accepted revisions immutable and corrections explicit;
- `narrative-001` keeps profile lore attached to one integer and edge-silent;
- `relation-001` continues to govern only published shared-membership
  relations and their bounded visual presentation;
- `fixture-001` keeps private research traces and personal identifiers out of
  tracked fixtures and automated proof.

accepted rationale retained:

- `d-001` separates the private rolodex from the public explorer;
- `d-003` distinguishes reusable properties from authored constellations and
  rejects pairwise flattening;
- `d-006` permits visual kinship between private and public surfaces while
  requiring different interaction models;
- `d-008` establishes sqlite as the canonical private provenance ledger;
- `d-011` and `d-012` keep sources, captures, citations, and accepted evidence
  exact and revision-bound;
- `d-018` keeps the current formspree path as public correspondence delivered
  for human review, never canonical ingestion.

acceptance authorizes one new decision recording that:

1. an authored direct relation and an authored constellation are distinct
   first-class private research objects;
2. they share provenance and revision grammar but not identity or automatic
   conversion;
3. neither object is public until a later explicit publication decision and
   accepted slice establish a safe projection.

acceptance would also authorize adding these normative claims without
weakening any existing contract:

- `relation-002`: an authored direct relation preserves its two canonical
  participants, explicit roles, directionality, proposition, warrant,
  epistemic register, provenance, and revision history as distinguishable
  data;
- `relation-003`: an authored constellation preserves its collective account,
  member roles, provenance, and revision history and never implies every
  possible pairwise relation among its members;
- `relation-004`: direct relations, constellations, and current
  shared-property public relations are separate representations; no overlap,
  traversal, or presentation action creates another representation
  automatically;
- `publication-004`: authored direct relations and constellations remain
  private unless a later explicit publication model selects an exact accepted
  revision and safe evidence.

these additions became current contracts with owner acceptance and bounded
implementation.

## conceptual model

### direct relation

a direct relation has a stable lineage and one or more revisions. one revision
contains:

- exactly two distinct canonical integer participants;
- an explicit role for each participant;
- directionality of `directed` or `symmetric`;
- a concise proposition suitable for display;
- a fuller warrant or account explaining why the relation is asserted;
- a nonempty epistemic register;
- private notes;
- zero or more contextual citations to existing sources and optional captures;
- lifecycle state, revision number, timestamps, and explicit supersession.

the proposition is authored text, not executable mathematics. where a
relation is mechanically checkable, a focused domain validator may later be
added through another accepted slice; this foundation neither claims nor
requires a universal relation-expression language.

illustrative shape only:

```text
direct relation: prime index
participant 1: 773 · role value
participant 2: 137 · role prime index
directionality: directed
proposition: 773 has prime index 137
epistemic register: mathematical derivation
```

the implementation proof must independently verify any mathematical fixture
it uses. the example above does not become canonical research merely by
appearing in this spec.

### constellation

a constellation has a stable lineage and one or more revisions. one revision
contains:

- a name;
- a collective account explaining why the company is being authored;
- at least two distinct canonical integer members;
- an explicit role for every member, with optional member-specific notes;
- a nonempty epistemic register;
- private notes;
- zero or more contextual citations to existing sources and optional captures;
- lifecycle state, revision number, timestamps, and explicit supersession.

cardinality does not decide object type. a two-member constellation remains a
constellation when its research meaning is collective membership rather than a
single proposition between participants. a many-member direct relation is not
permitted in this slice.

an illustrative fictional fixture may group `137`, `173`, `317`, and `713`
under a plainly fictional “permutation rehearsal” account. it demonstrates
member roles and collective authorship without entering the seeded corpus or
asserting a real cultural claim.

### the cousins' shared wardrobe

the two object types share these semantics without sharing tables or ids:

| concern | direct relation | constellation |
|---|---|---|
| stable identity | relation lineage | constellation lineage |
| mutable work | draft revision | draft revision |
| terminal research state | immutable accepted or withdrawn revision | immutable accepted or withdrawn revision |
| correction | explicit superseding revision | explicit superseding revision |
| participants | exactly two role-bearing integers | two or more role-bearing integers |
| authored meaning | proposition plus warrant | collective account |
| evidence | revision-scoped citations | revision-scoped citations |
| privacy | private only | private only |
| automatic inference | none | none |

the same source or capture may be cited contextually by both cousins. citation
records remain specific to the exact revision and do not make the two research
objects interchangeable.

## lifecycle and invariants

each lineage begins with revision `1` in `draft`. a draft may change content,
participants or members, notes, register, and citations. acceptance validates
the complete revision and makes its content, participant or member records,
and citations immutable.

an accepted revision may be corrected only by preparing a new draft that
explicitly names the accepted revision it supersedes. acceptance of the new
revision retires the prior revision from active selection without deleting
history. a draft may be withdrawn. accepted revisions are never edited,
withdrawn, or deleted.

acceptance requires:

- canonical, distinct integer participants or members;
- the required cardinality and role fields;
- a nonempty proposition and warrant for a direct relation, or name and
  collective account for a constellation;
- a nonempty epistemic register;
- no duplicate active lineage slug;
- all citations to resolve to existing source identities and captures;
- an explicit supersession target when the lineage already has an active
  accepted revision.

absence of a direct relation or constellation membership means only that no
such authored object is recorded. it never establishes a covered negative.
coverage remains a property-specific concept.

opening, holding, traversing, or placing two objects next to one another is a
presentation act. it creates no persisted state except when the owner invokes
an explicit draft or lifecycle command.

## persistence shape

the anticipated migration is additive and private. exact names may change
during semantic preflight, but the accepted implementation must preserve these
representations:

- stable direct-relation lineage;
- revisioned direct-relation content;
- exactly two revision-bound participant rows with canonical integer foreign
  keys, roles, and ordinals;
- revision-bound direct-relation citations;
- stable constellation lineage;
- revisioned constellation content;
- two or more revision-bound member rows with canonical integer foreign keys,
  roles, ordinals, and optional member notes;
- revision-bound constellation citations;
- database guards for legal status transitions, accepted-revision
  immutability, citation immutability, and deletion refusal;
- indexes for integer-to-relation and integer-to-constellation lookup;
- no publication-selection, public-release-item, or public-projection table for
  either new object.

participant and member writes must occur inside the same transaction as their
revision mutation. acceptance validation must observe one transactionally
consistent revision. service-layer checks and database guards must not leave a
partially accepted object if validation fails.

the migration must advance the schema version independently of corpus recipe,
source-extract envelope, or public release version. `seed-v6` and
`reference-v6` do not become `v7` merely because private relational tables
exist. exact reference-ledger orientation must be deliberately updated so the
known additive schema does not masquerade as unexplained drift, while authored
relational rows still make a cabinet non-reference in the semantic corpus
predicate.

implementation and automated proof may migrate only disposable fixtures or
explicit test copies. the implementation close must not open, inspect, back
up, or migrate the owner's actual ledger. the owner-operated migration remains
a later deliberate action after review of backup and recovery instructions.

## private application behavior

the application layer exposes renderer-independent operations for:

- opening a direct-relation or constellation dossier by stable identity;
- listing the direct relations and constellation memberships of an integer;
- creating a draft with canonical participants or members;
- editing only a draft;
- attaching a contextual citation only to a draft;
- accepting or withdrawing a draft;
- preparing a superseding draft from an accepted revision;
- returning lifecycle, provenance, and participant or member data without
  presentation-specific labels or geometry.

integer creation follows the existing explicit authoring seam: manually
authoring a draft may ensure a canonical integer identity exists, but it
creates no property claim, coverage fact, profile, public item, relation beyond
the requested draft, or constellation beyond the requested draft.

queries return authored records only. they do not merge these records with the
public shared-membership relation algorithm, and they do not synthesize
reciprocal or transitive results.

## native interaction model

the private gui becomes one research room with several possible occupants,
not three simultaneous dashboards.

### held-object rail

- holds integers, direct relations, and constellations with an explicit type
  mark and human-readable identity;
- supports keyboard and pointer selection, removal, and return;
- never silently evicts the active object;
- is session-local and resets when the process closes;
- does not imply research significance, acceptance, or publication.

### active-object field

- an integer foregrounds identity, property states, authored direct relations,
  and constellation memberships;
- a direct relation foregrounds its proposition, two participants and roles,
  warrant, register, revision state, citations, and private notes;
- a constellation foregrounds its name, collective account, members and roles,
  register, revision state, citations, and private notes;
- every referenced integer, relation, or constellation can be opened without
  losing the current object from the held rail;
- the active object remains available as a complete textual account without a
  graph.

### epistemic margin

- shows object type, lifecycle state, register, authorship status, revision,
  supersession, sources, and citations;
- distinguishes authored assertion, source identity, captured evidence,
  private note, and accepted state through text, not color alone;
- contains the contextual lifecycle actions valid for the active object;
- does not keep unrelated creation forms permanently visible.

### visit trail

- records a bounded session-local sequence of deliberate openings;
- distinguishes returning to an object from creating a relation between
  objects;
- supports back and forward traversal without persisting research data;
- clears on process exit and is never published.

the existing palette may remain, but geometry must follow the research act:
room for one active account, quiet secondary structure, and fewer always-on
controls. visual kinship with other resonance instruments is reference only;
no other checkout, stylesheet, font, or runtime becomes a dependency.

## public and correspondence boundaries

the current publisher and web runtime remain unaware of authored direct
relations and constellations. a ledger containing them must produce exactly
the same public artifact as an otherwise equivalent ledger without them, given
identical existing staging selections.

the current public phrase “who this integer knows” continues to mean shared
published property membership under `relation-001`. it must not silently begin
mixing authored private relations with derived public relations.

the current formspree suggestion path remains unchanged. no public suggestion
or delivered email enters the private ledger automatically. the future local
correspondence spool and discord courier must have their own accepted specs,
secret boundary, idempotence model, and human conversion action.

## likely files and components

- `src/Polyvalent.Domain/KnowledgeModels.cs` or focused domain files for
  relation and constellation identities, directionality, roles, and lifecycle
  validation;
- `src/Polyvalent.Application/ResearchContracts.cs` and
  `ResearchService.cs` for renderer-independent dossiers and commands;
- one new numbered migration under `src/Polyvalent.Data/Migrations/`;
- `src/Polyvalent.Data/PolyvalentStore.cs` for transactional persistence and
  private queries;
- `src/Polyvalent.Data/ReferenceCorpusShape.cs` and ledger-orientation proof
  for the new known schema and authored-state distinction;
- `src/Polyvalent.Desktop/ViewModels/` for active-object, held-object,
  contextual-action, and visit-trail state;
- `src/Polyvalent.Desktop/MainWindow.axaml` and theme resources for the room;
- domain, data, application or desktop tests under `tests/`;
- `CONTRACTS.md` and `DECISIONS.md` only after owner acceptance;
- `SCHEMA.md`, `docs/architecture.md`, `docs/development.md`, `README.md`, and
  `STATUS.md` when implementation changes their owned truth.

no web source, public artifact, deployment workflow, provider configuration,
or external repository should require a behavioral change.

## implementation by invariant

1. **seal the meanings before the tables:** record the accepted direct-relation
   and constellation distinction, add the accepted contracts, and define
   canonical roles, directionality, revision states, and acceptance rules in
   renderer-independent domain proof.
2. **prove the migration without owner data:** add the relational schema,
   lifecycle guards, indexes, schema fingerprint update, backup and restore
   proof, and migrations from a disposable `001` fixture before any gui work.
3. **establish transactional research operations:** implement draft, edit,
   cite, accept, withdraw, supersede, dossier, and integer-neighborhood paths
   with failure rollback and no automatic inference.
4. **prove the publication silence:** demonstrate that authored relational
   state is unreachable from publisher selections, release items, public dtos,
   public artifact bytes, and the web runtime.
5. **teach the room its occupants:** introduce the active-object union, held
   rail, contextual epistemic margin, and visit trail while preserving the
   complete integer property and profile workflows.
6. **render the research distinction:** inspect direct relation,
   constellation, and integer views at wide, minimum, narrow-equivalent, and
   enlarged scales; exercise pointer, keyboard, focus, reduced motion, and
   screen-text equivalence.
7. **close the continuity loop:** reconcile schema, architecture, operation,
   recovery, status, and validation evidence without claiming owner-ledger
   migration, native comfort, publication, or deployment.

## change shape

| dimension | changed? | source of truth | proof |
|---|---:|---|---|
| user-visible behavior | yes | avalonia view models and xaml | focused state tests, rendered native review, keyboard and focus inspection, owner-visible comfort review |
| persisted state or schema | yes | new numbered sqlite migration and repository layer | fresh migration, `001` upgrade, reopen, transaction, guard, backup, restore, and schema-fingerprint tests |
| external or provider-visible behavior | no | public boundary and out-of-scope declaration | no network or provider dependency; unchanged public artifact and built-boundary comparison |
| identifiers, secrets or privacy | yes | canonical integer ids, new private lineages, publication exclusion | identity round trips, private sentinels, dependency scan, artifact byte comparison, no-secret inventory |
| lifecycle or terminal states | yes | relation and constellation revision services plus database guards | transition, immutability, supersession, withdrawal, and failure-rollback tests |
| setup, operation or recovery | yes | database initialization plus development and recovery documentation | disposable old-schema rehearsal, automatic backup proof, restore exercise, owner-ledger exclusion review |
| normative contract or decision | yes | accepted `relation-002` through `relation-004`, `publication-004`, and `d-019` | owner acceptance, contract trace, decision entry, documentation check |
| cross-repository dependency | no | solution and project references | cold full check with no other resonance checkout available |

## risk route and review obligations

the planning artifact is green, but the accepted implementation is red-risk
because it introduces a schema migration, canonical private persistence,
immutable lifecycle transitions, and a privacy boundary adjacent to
publication.

before implementation, an independently scoped semantic preflight must:

- reconstruct every reader and writer of migration state, schema
  fingerprinting, backup, initialization, relation-like qualifiers, dossier
  queries, publication selections, and public relation derivation;
- identify which existing identifiers use “relation” to mean derived public
  shared membership and keep them separate from authored direct relations;
- enumerate the do-not-disturb publication, coverage, seed idempotence,
  ledger-orientation, and owner-data boundaries;
- attack the accepted cardinality, immutability, supersession, and transaction
  rules with counterexamples;
- fingerprint the reviewed source revision and retain unresolved ambiguity
  rather than converting it into implementation authority.

after implementation and ordinary proof, a fresh independent diff adversary
must attack:

- upgrades from `001`, initialization, backup and restore;
- partial participant or member writes and failed acceptance rollback;
- accepted-revision mutation and deletion paths;
- duplicate, self, missing-parent, stale-supersession, and concurrency cases;
- accidental pairwise or transitive synthesis;
- publisher, artifact, source, build, and runtime privacy leaks;
- integer, relation, and constellation navigation, held-object identity,
  keyboard focus, narrow layout, and contextual actions;
- every realized boundary not declared in this change shape.

the implementing agent remains the only writer. review findings retain their
epistemic status and do not become contracts automatically.

## acceptance criteria

implementation may be called complete only when:

1. direct relations and constellations have distinct stable identities,
   revision tables, participant or member records, citation records, and
   application dossiers;
2. a direct-relation revision cannot be accepted with fewer or more than two
   distinct canonical integer participants, missing roles, missing
   directionality, an empty proposition, empty warrant, or empty register;
3. a constellation revision cannot be accepted with fewer than two distinct
   canonical integer members, missing roles, an empty name, empty collective
   account, or empty register;
4. draft content, participants or members, notes, register, and citations are
   editable, while accepted revisions and their children are immutable and
   undeletable;
5. supersession preserves prior accepted revisions, validates one active
   accepted revision per lineage, and rejects stale or cross-lineage targets;
6. failed validation or persistence leaves no partially accepted revision,
   participant set, member set, or citation set;
7. opening an integer returns its authored direct relations and constellation
   memberships without manufacturing a property claim, coverage fact, public
   relation, reciprocal relation, transitive relation, or pairwise
   constellation edge;
8. the same source or capture may support contextual citations on either
   object type while each citation remains bound to exactly one revision;
9. a known-schema migration from a disposable `001` ledger creates a backup,
   preserves every existing claim, witness, citation, profile, staging
   selection, and release record, and reopens successfully;
10. restoring the pre-migration backup recovers the exact prior fixture and no
    test opens or copies the owner's actual ledger;
11. schema version, schema fingerprint, corpus recipe, source-extract envelope,
    and public release version remain distinct axes;
12. an otherwise identical staged ledger generates byte-identical public
    release data before and after private direct relations and constellations
    are authored;
13. no new relational table or field is reachable from the web runtime, public
    artifact, pages workflow, provider boundary, or suggestion adapter;
14. the private gui can open, hold, revisit, and traverse an integer, direct
    relation, and constellation without confusing their ids or creating
    persisted state through navigation alone;
15. every active-object type has a complete textual account, visible lifecycle
    and epistemic register, reachable citations, and contextual actions without
    requiring a graph or hover;
16. existing integer property, evidence, profile, revision, citation, staging,
    ledger-orientation, and navigation workflows remain available and covered
    by focused regressions;
17. wide, minimum-window, enlarged, keyboard-only, focus, reduced-motion, and
    owner-visible native review distinguish the three object types and expose
    no clipped essential controls or document-wide horizontal overflow;
18. the semantic preflight is reconciled before editing, the fresh diff
    adversary is reconciled after implementation, and any unresolved
    ambiguity remains explicit;
19. the project-owned complete check, documentation continuity check, format
    verification, and direct complete-diff review pass;
20. close reporting distinguishes disposable migration proof, automated local
    proof, rendered native inspection, owner comfort, owner-ledger migration,
    public behavior, provider behavior, and deployment rather than inferring
    one from another.

## validation expected after implementation

focused proof:

```text
dotnet test tests/Polyvalent.Domain.Tests/Polyvalent.Domain.Tests.csproj -c Release
dotnet test tests/Polyvalent.Data.Tests/Polyvalent.Data.Tests.csproj -c Release
dotnet test tests/Polyvalent.Desktop.Tests/Polyvalent.Desktop.Tests.csproj -c Release
```

migration and publication-silence proof must use disposable directories and
project-owned test commands added with the implementation. it must include a
copied `001` fixture, backup comparison, restore comparison, authored
relational rows, and byte comparison of public release output. no command in
the spec points at `./var/local` or a platform owner-data directory.

complete proof:

```text
./scripts/check.sh
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --require SCHEMA.md --review-claims
git diff --check
direct review of the complete diff, migration, generated public comparison, and privacy boundaries
```

rendered native inspection is additional proof. automated view-model tests,
headless x11 captures, or screenshots do not establish owner comfort or an
actual owner-ledger migration.

## implementation receipt

implemented locally on 2026-11-17 without owner-ledger access, commit, push,
provider call, publication, or deployment:

- migration `002_private_relational_foundation.sql` adds the two distinct
  lineage families, database-enforced draft entry, immutable revision identity,
  terminal content and child immutability, current-revision supersession,
  globally unique authored lineage slugs, source-matched captures, and integer
  lookup indexes;
- domain, application, repository, and native view-model layers now carry
  exact authored aggregates and tagged integer, direct-relation, and
  constellation identities without calling public relation derivation;
- populated exact-v1 disposable migration proof compares every legacy table
  with the automatic backup, verifies restored-byte identity, and reopens the
  backup as `migration-required`;
- controlled v2 ledgers with and without accepted private relational records
  generate byte-identical public `data.json` and `manifest.json`;
- the fresh independent diff adversary's confirmed terminal-insert,
  draft-reopen, reparenting, cross-type slug, traversal, fixture,
  lifecycle-margin, and reproduction-runbook findings were reconciled and
  covered by focused proof;
- `./scripts/check.sh` passes with `118` .net tests and `38` web tests; the
  documentation continuity checker reports zero errors and zero warnings;
- disposable native inspection covered integer, accepted direct-relation, and
  accepted constellation states at `1600×1000` and the `1060×700` minimum.

owner-visible native comfort, enlarged-text and actual keyboard-only tactile
review, and any deliberate owner-ledger migration remain unclaimed. because
those acceptance boundaries remain open, this spec stays `accepted` rather
than being retired.

## rollback

- before any owner-operated migration, revert the bounded domain,
  application, migration, data, desktop, test, contract, decision, and
  documentation diff;
- implementation proof uses disposable fixtures that may be removed after
  evidence is captured;
- if a future owner-operated migration is attempted, preserve the automatic
  pre-migration backup and verify its path and digest before opening the
  migrated ledger for authoring;
- restoration replaces only the explicitly selected migrated database with
  its verified pre-migration backup while the desktop and publisher are
  stopped; sidecars are handled according to `docs/development.md`;
- rollback never deletes authored relational records from a live ledger one
  table at a time, rewrites accepted history, or treats downgrade sql as a
  substitute for backup restoration;
- no public or provider rollback exists in this slice because no public or
  provider state is authorized to change.

## risks and unknowns

- the word “relation” already names the derived public shared-membership view.
  authored direct relations need identifiers that remain unmistakable across
  domain, application, storage, and gui layers;
- sqlite cannot express every cross-row cardinality rule with a simple check
  constraint. acceptance must be transactional and defended at both service
  and database lifecycle boundaries without pretending a count query is a
  static column constraint;
- a free-text epistemic register preserves openness but cannot yet guarantee
  consistent filtering. a controlled vocabulary is intentionally deferred
  until observed research use supplies a defensible taxonomy;
- visually consolidating the gui may expose hidden dependencies on the current
  selected-claim and selected-profile state. semantic preflight must trace
  command enablement, refresh, and navigation before the room is rewritten;
- adding a known migration changes the exact schema fingerprint and reference
  orientation logic even when the new tables are empty. corpus identity must
  distinguish known empty relational capacity from authored relational state;
- the public artifact byte-comparison claim depends on identical existing
  staging and deterministic generation inputs. proof must control those inputs
  rather than comparing unrelated timestamps or release ids;
- a future correspondence spool may want to refer to numbers not yet present
  in the canonical ledger. this slice does not let that future requirement
  distort manual research authoring or create a hidden ingestion path;
- this spec does not establish whether authored direct relations or
  constellations will ever become public. later publication may require a
  different safe vocabulary rather than exposing private records directly.

## open questions

none inside the accepted slice. acceptance selects the cousin model,
private-only boundary, non-taxonomic register, additive migration, session-only
navigation, contextual native room, explicit contract additions, semantic
preflight, diff adversary, and listed deferrals.

## authorization boundary

the repository owner's acceptance authorizes the bounded local implementation
and verification described above, including
the additive migration against disposable or copied fixtures and the explicit
contract and decision additions. it does not authorize inspection, copy,
backup, migration, repair, or mutation of the owner's actual ledger; discord
or other provider access; secrets or environment handling; public artifact
promotion; commit; push; publication; deployment; or live-state claims.
