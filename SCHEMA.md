# database schema

> **continuity role:** explanatory current reference
> **status:** current through migration `002_private_relational_foundation.sql`
> **last verified:** 2026-11-17
> **owns:** a compact map of persisted tables, relationships, guards, and version axes
> **update when:** a migration changes the database or a version axis changes meaning

polyvalent's canonical store is one private sqlite database. this document is a
map, not a second schema definition: the ordered sql migrations under
`src/Polyvalent.Data/Migrations/` are executable truth, while `CONTRACTS.md`
owns the invariants the schema must preserve.

## shape at a glance

```text
integers ── claims ── claim_revisions ── claim_witnesses
    │          │              └────────── claim_revision_citations ── sources
    │          └── properties ── property_versions ────────────────┐     │
    │                              │             │                  │     │
    │                              │             ├── population_runs│     │
    │                              │             │        ├── comparison_runs
    │                              │             │        └── coverage_claims
    │                              │             └── property_version_citations
    │                              │                                      │
    └── profile_entries ── profile_entry_revisions                        │
                               └── profile_entry_revision_citations ──────┘
                                                                          │
sources ── source_captures ────────────────────────────────────────────────┘

accepted revisions ── publication selections ── sealed public releases
```

there is no public pairwise integer-edge table. public relations are derived from
shared, accepted, staged property memberships whose property version has
`graph_mode = 'shared-membership'`. private authored direct relations are a
separate revisioned representation and never enter that derivation.

## table families

| family | tables | purpose |
|---|---|---|
| migration ledger | `schema_migrations` | records each ordered migration, its checksum, and application time; created by `SqliteDatabase` before migrations run |
| identity and vocabulary | `integers`, `properties`, `property_versions` | canonical signed-decimal integer identity and revisioned property meaning |
| evaluation and coverage | `evaluators`, `population_runs`, `comparison_runs`, `coverage_claims` | records how a bounded population was produced, compared, and warranted complete |
| claims and evidence | `claims`, `claim_revisions`, `claim_witnesses` | keeps a stable integer-property lineage distinct from its revisioned assertion and typed witness |
| provenance | `sources`, `source_captures`, `property_version_citations`, `claim_revision_citations` | separates a source identity, an immutable retrieval, and each contextual use |
| profile lore | `profile_entries`, `profile_entry_revisions`, `profile_entry_revision_citations` | stores sourced integer biography without creating a graph edge |
| authored direct relations | `direct_relations`, `direct_relation_revisions`, `direct_relation_participants`, `direct_relation_revision_citations` | stores one authored proposition between exactly two role-bearing integers, with stable lineage and revision-bound evidence |
| authored constellations | `constellations`, `constellation_revisions`, `constellation_members`, `constellation_revision_citations` | stores a collective account for two or more role-bearing integers without manufacturing pairwise edges |
| publication staging | `publication_property_selections`, `publication_property_citation_selections`, `publication_claim_selections`, `publication_claim_citation_selections`, `publication_profile_selections`, `publication_profile_citation_selections` | allowlists exact accepted revisions and public-safe citations for the next release |
| privacy proof | `privacy_sentinels` | supplies private marker values that publication tests must never emit |
| release ledger | `public_releases`, `public_release_property_items`, `public_release_claim_items`, `public_release_profile_items`, `public_release_property_citation_items`, `public_release_claim_citation_items`, `public_release_profile_citation_items` | pins the exact rows represented by immutable release bytes |

## important relationships

- `claims` is unique per integer and property. history belongs in
  `claim_revisions`, not duplicate claim rows.
- each claim revision names the exact `property_versions` row whose definition
  and evidence contract governed acceptance.
- `claim_witnesses` is one-to-one with a claim revision. citations are
  one-to-many and remain separate from the witness.
- `coverage_claims` requires a succeeded population run. without applicable
  coverage, absence of a positive claim means unknown, not false.
- profile entries attach to one integer and carry their own revision and
  citation history; they do not imply mathematical membership.
- direct-relation and constellation lineages each point to at most one current
  accepted revision. superseding acceptance advances that pointer only from
  the exact current accepted revision.
- direct-relation participants and constellation members bind canonical
  integer identities to exact revisions. they are mutable only while the
  parent is draft and are frozen with the accepted revision.
- source captures may be cited by property, claim, or profile revisions, but a
  citation also retains the stable source identity and its role at that site.
- staging selects exact revisions and exact public-safe citations. release item
  tables then freeze that selection beneath one `public_releases` identity.

## lifecycle guards

sqlite triggers enforce the adult boundaries of the ledger:

- accepted property versions cannot be updated or deleted;
- claim and profile content can change only while draft, and revisions are
  withdrawn or superseded rather than deleted;
- direct-relation and constellation content, participants, members, and
  citations can change only while draft; acceptance validates cardinality,
  roles, authored content, register, and current supersession atomically;
- accepted relational revisions and every child row reject update, delete, and
  reparenting; lineages are never deleted;
- a citation capture must belong to the source identity named at the same
  relational citation site;
- citations and witnesses can be attached only while their owner is draft;
- source captures, comparison results, and coverage claims are immutable;
  population runs follow only `prepared → running → terminal` and are never
  deleted;
- only active accepted revisions may enter publication staging, and only
  public-safe citations belonging to the selected revision may accompany them;
- release items may be added only while a release is building; sealing is the
  sole permitted release transition, after which the release and its items are
  immutable.

## four independent version axes

| axis | current value | what it versions |
|---|---|---|
| database migration | through `002_private_relational_foundation.sql` / schema version `2` | sqlite tables, constraints, indexes, and triggers |
| corpus recipe | `seed-v6` | the idempotent code path that assembles a fresh private reference ledger |
| source-extract envelope | `data/source-extracts/schema-v1/` / json `schemaVersion: 1` | the tracked format for bounded source-derived value lists |
| public release | `reference-v6` | one reviewed immutable `manifest.json` plus `data.json` projection |

these numbers do not march in lockstep. a new corpus recipe may reuse the same
database and extract schemas; a new release may reflect new selected data
without a sql migration. accepted spec `specs/003-network-ignition.md` supplies
`seed-v6` and `reference-v6` while deliberately requiring no database migration.
accepted spec `specs/005-private-relational-foundation.md` advances only the
private database schema axis; it does not rename the corpus recipe, source
extract envelope, or public release.
ledger orientation adds no persisted version field: the current recipe is
recognized from exact semantic corpus, provenance, and staging state. creation
timestamps and immutable `public_releases` history are excluded from recipe
identity; schema compatibility remains governed by the migration ledger.

## change rule

never edit an applied migration in place. add the next ordered migration,
preserve the checksum ledger, and follow the backup and recovery procedure in
`docs/development.md`. changing a tracked source extract is a provenance change,
not a schema migration, and still requires focused comparison and corpus proof.
