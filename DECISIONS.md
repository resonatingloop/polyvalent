# polyvalent decisions

> **continuity role:** historical rationale and supersession
> **status:** append-only
> **last verified:** 2026-10-17
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
