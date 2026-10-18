# polyvalent decisions

> **continuity role:** historical rationale and supersession
> **status:** append-only
> **last verified:** 2026-10-18
> **owns:** consequential choices another maintainer should not have to re-derive
> **update when:** a decision introduces, clarifies, or supersedes a durable fact

do not use this file as a second implementation manual. current procedures live
in operational documentation.

## d-001 — one corpus, two deliberately different surfaces

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: none

context:

public exploration and private research need the same domain truth but reward
different interaction grammars.

decision:

keep the private avalonia gui and public web ui in this repository. the private
gui is a rolodex-like research record with collapsible connections; the public
site presents integer profiles and explained relation graphs.

alternatives considered:

- one shared interface model: rejected because it would make research
  theatrical or public exploration administrative;
- separate repositories with duplicated domain concepts: deferred because it
  would create premature synchronization pressure.

consequences:

- domain truth and publication need explicit seams;
- visual kinship does not imply shared components;
- each surface requires its own acceptance proof.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-002 — public data is an explicit projection

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: none

context:

the canonical research corpus may contain private notes, properties,
constellations, uncertain findings, and source debris that must not leak into a
public artifact.

decision:

public output is selected affirmatively from the private corpus. the public
runtime does not access the canonical private store.

alternatives considered:

- publish everything except records marked private: rejected because one
  missing flag could disclose private material;
- connect the public site directly to the private database: rejected because it
  erases the intended trust boundary.

consequences:

- publication requires a versioned, testable boundary;
- private-only sentinel fixtures should prove exclusion;
- public freshness and private canonical truth become distinct states.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-003 — properties and constellations are different epistemic objects

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: none

context:

some relations arise because integers satisfy a reusable property; others are
authored, contextual, many-member interpretations whose meaning cannot be
reduced to an exhaustive predicate.

decision:

represent reusable properties through definitions and attestations. represent
authored relations through private constellations with member roles, provenance,
and an interpretive account.

alternatives considered:

- model every relation as a property: rejected because it implies uniform and
  potentially exhaustive membership;
- flatten every constellation into pairwise edges: rejected because it loses
  the meaning of the group as a whole.

consequences:

- the private gui exposes private properties and private constellations as
  separate research chambers;
- the initial public surface exposes vetted public properties, not private
  constellations;
- future public constellations require a new explicit decision.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-004 — property absence follows open-world semantics

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: none

context:

manually recording a few members of a property must not imply that every other
integer lacks it.

decision:

absence means unknown unless a coverage claim establishes that the relevant
integer was evaluated by a complete computation or defensible bounded import.

alternatives considered:

- treat missing rows as false: rejected because partial population would create
  false negative knowledge;
- require every property to be exhaustive before it can exist: rejected because
  private research needs sparse sightings and draft properties.

consequences:

- queries and gui language must distinguish known positive, evaluated negative,
  and unassessed;
- property onboarding must record population mode and coverage;
- external sequence references aid provenance but do not certify completeness
  on their own.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-005 — public suggestions stop at the inbox

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: none

context:

the expected suggestion volume is tiny and does not justify an account,
moderation, or submission-ingestion system.

decision:

provide a lightweight public form that delivers a proposed property or relation
to the owner's email for analysis. it does not write to the corpus.

alternatives considered:

- direct submission-to-database workflow: rejected as unnecessary complexity
  and an avoidable trust expansion;
- contributor accounts and status tracking: rejected as disproportionate to
  expected use.

consequences:

- the form still requires untrusted-input, privacy, delivery, and abuse proof;
- submitter identity and cited source remain distinct;
- credit permission must be explicit if attribution is later published.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-006 — related instruments share a lineage, not a skin

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: none

context:

the private and public surfaces belong to an existing visual family but serve
different modes of attention.

decision:

the private avalonia gui translates the resonance plate material language into
a dense working instrument. the public web ui shares the cartographic,
constellational atmosphere of resonatrix and upward while remaining a distinct
integer-sociality instrument.

alternatives considered:

- duplicate one component system across desktop and web: rejected because the
  surfaces need different interaction grammars;
- use unrelated aesthetics: rejected because polyvalent belongs to the same
  family of instruments.

consequences:

- shared palette and typographic ideas are translated through native toolkit
  capabilities;
- public motion and graph spectacle do not migrate into the private gui;
- rendered owner-visible comparison is required before visual work is accepted.

evidence:

- owner-confirmed design discussion and grounded reference inspection on
  2026-10-17.

## d-007 — public state is an explicit immutable release

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: `specs/001-first-slice.md` (accepted 2026-10-18)

context:

the public site must be able to explore curated research without acquiring a
write path, a dependency on the desktop application, or ambient visibility
into the live private corpus.

decision:

an explicit publication action produces a versioned, immutable public data
artifact from affirmatively staged material. the public runtime reads that
artifact only. editing or staging private records does not alter public state,
and generating an artifact does not deploy it.

alternatives considered:

- let the public application query the canonical database: rejected because it
  collapses the privacy boundary into application configuration;
- regenerate public output whenever a private record changes: rejected because
  research activity must not become publication by accident;
- use mutable public flags on canonical rows: rejected because they do not pin
  the exact revision and evidence that was reviewed.

consequences:

- private truth, staged publication intent, generated release, and deployed
  state are four distinct representations;
- each successful release needs an identity, schema version, and content
  digest;
- rollback means serving a previously validated release rather than rewriting
  canonical history.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-008 — the canonical store is a hybrid provenance ledger

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: `specs/001-first-slice.md` (accepted 2026-10-18)

context:

polyvalent needs reproducible mathematical populations, bounded source imports,
sparse manual research, and honest knowledge of what has or has not been
checked. none of those warrants can be represented faithfully by a bag of
property tags.

decision:

use sqlite as the canonical local store. materialize positive claims produced
manually, computationally, or by bounded import, together with the run and
evidence that warranted them. infer a negative result only within explicit
recorded coverage. keep a source, a particular capture of that source, and a
contextual citation as separate objects.

alternatives considered:

- store only evaluator code and recompute every view: rejected because a
  historical conclusion needs the exact run, inputs, and version that produced
  it;
- store positive booleans without runs or coverage: rejected because missing
  rows would invite false negatives and irreproducible claims;
- treat a url as source, capture, and citation at once: rejected because
  intellectual identity, retrieved evidence, and local use drift differently.

consequences:

- evaluator code remains the semantic authority for computed properties while
  ledger rows preserve what a specific execution concluded;
- deterministic coverage and source-asserted bounded coverage remain visibly
  different warrants;
- source drift can be recorded without silently changing an accepted claim.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-009 — integer identity is arbitrary-precision canonical text

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: `specs/001-first-slice.md` (accepted 2026-10-18)

context:

the first corpus is bounded to `1` through `2000`, but the instrument is about
integers rather than machine-sized ids. sqlite and javascript numeric limits
must not become an accidental ontology.

decision:

the domain uses arbitrary-precision integers. persistence and public exchange
use one canonical signed-decimal text form behind internal surrogate keys.

alternatives considered:

- use signed 64-bit database keys as the integer itself: rejected because the
  storage limit would become a false domain limit;
- emit json numbers: rejected because browser number parsing can lose integer
  identity;
- preserve user-entered textual variants: rejected because `+1`, `01`, and
  `1` must not become distinct integers.

consequences:

- parsing rejects noncanonical spelling, including leading zeroes and `-0`;
- ordering and arithmetic happen in the domain, not through lexicographic text
  comparison;
- boundaries require lossless round-trip tests beyond sqlite and javascript
  exact numeric ranges.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-010 — accepted evidence is corrected by supersession

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: `specs/001-first-slice.md` (accepted 2026-10-18)

context:

research needs editable uncertainty before acceptance and durable provenance
after acceptance. making everything mutable destroys history; making everything
immutable makes ordinary drafting punitive.

decision:

apply selective immutability. drafts and publication staging are mutable.
accepted claim and profile revisions, source captures, terminal runs, and
successful releases are immutable. corrections create explicit superseding
revisions rather than overwriting accepted evidence.

alternatives considered:

- make every row append-only from creation: rejected because private drafting
  is exploratory work;
- permit accepted records to be edited in place: rejected because citations,
  releases, and conclusions would cease to name stable evidence;
- duplicate corrected records without a lineage: rejected because consumers
  could not tell correction from contradiction.

consequences:

- lifecycle transitions and forbidden mutations require database and
  application proof;
- generated or imported claims are replaced through a new run, not hand-edited;
- a released public artifact remains intelligible after canonical research has
  advanced.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-011 — integer lore is a profile entry, not a forced relation

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: `specs/001-first-slice.md` (accepted 2026-10-18)

context:

some worthwhile material belongs publicly on an integer's profile but is not a
reusable mathematical predicate or an honest edge to another integer. examples
include scientific history, cultural reception, and sourced biographical lore.

decision:

represent such material as a first-class, sourced, revisioned profile entry
attached to one integer. a profile entry carries an epistemic register and
creates no graph relation by default.

alternatives considered:

- torture every vignette into a property or connection: rejected because it
  fabricates graph structure and false exhaustiveness;
- place all prose in an undifferentiated notes field: rejected because it
  cannot be cited, reviewed, revised, or published precisely.

consequences:

- a number's public profile can carry context without polluting relation logic;
- private and publishable revisions remain separable;
- later thematic lenses may select profile entries without redefining them.

evidence:

- owner-confirmed design discussion on 2026-10-17.

## d-012 — property evidence is versioned and typed

- date: 2026-10-17
- status: accepted
- supersedes: none
- related spec: `specs/001-first-slice.md` (accepted 2026-10-18)

context:

prime certificates, sequence indices, digit decompositions, and friedman
expressions do not have the same evidence shape. a universal result string
would hide whether a claim is actually checkable.

decision:

each property version declares its allowed qualifiers, witness kinds,
evidence requirements, coverage capability, and stable executable validator or
evaluator keys. evidence payloads are accepted only after validation against
that exact contract. incomplete private drafts may exist but cannot establish
coverage, public membership, or graph relations.

alternatives considered:

- add arbitrary fields directly to integer records: rejected because sparse
  population would masquerade as comprehensive classification;
- accept untyped json as evidence: rejected because shape without semantics is
  not validation;
- require every private idea to have a complete validator immediately:
  rejected because research needs a protected draft state.

consequences:

- executable semantics stay in versioned code rather than database text;
- public evidence is rendered from validated, display-safe projections;
- new property types require an explicit contract and population warrant, not
  merely a gui field.

evidence:

- owner-confirmed design discussion on 2026-10-17.
