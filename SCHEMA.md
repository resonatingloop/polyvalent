# database schema

> **continuity role:** explanatory current reference
> **status:** current for migration `001_initial.sql`
> **last verified:** 2026-10-20
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

there is no pairwise integer-relation table. public relations are derived from
shared, accepted, staged property memberships whose property version has
`graph_mode = 'shared-membership'`.

## table families

| family | tables | purpose |
|---|---|---|
| migration ledger | `schema_migrations` | records each ordered migration, its checksum, and application time; created by `SqliteDatabase` before migrations run |
| identity and vocabulary | `integers`, `properties`, `property_versions` | canonical signed-decimal integer identity and revisioned property meaning |
| evaluation and coverage | `evaluators`, `population_runs`, `comparison_runs`, `coverage_claims` | records how a bounded population was produced, compared, and warranted complete |
| claims and evidence | `claims`, `claim_revisions`, `claim_witnesses` | keeps a stable integer-property lineage distinct from its revisioned assertion and typed witness |
| provenance | `sources`, `source_captures`, `property_version_citations`, `claim_revision_citations` | separates a source identity, an immutable retrieval, and each contextual use |
| profile lore | `profile_entries`, `profile_entry_revisions`, `profile_entry_revision_citations` | stores sourced integer biography without creating a graph edge |
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
- source captures may be cited by property, claim, or profile revisions, but a
  citation also retains the stable source identity and its role at that site.
- staging selects exact revisions and exact public-safe citations. release item
  tables then freeze that selection beneath one `public_releases` identity.

## lifecycle guards

sqlite triggers enforce the adult boundaries of the ledger:

- accepted property versions cannot be updated or deleted;
- claim and profile content can change only while draft, and revisions are
  withdrawn or superseded rather than deleted;
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
| database migration | `001_initial.sql` / schema version `1` | sqlite tables, constraints, indexes, and triggers |
| corpus recipe | `seed-v5` | the idempotent code path that assembles a fresh private reference ledger |
| source-extract envelope | `data/source-extracts/schema-v1/` / json `schemaVersion: 1` | the tracked format for bounded source-derived value lists |
| public release | `reference-v5` | one reviewed immutable `manifest.json` plus `data.json` projection |

these numbers do not march in lockstep. a new corpus recipe may reuse the same
database and extract schemas; a new release may reflect new selected data
without a sql migration. draft spec `specs/003-network-ignition.md` proposes
`seed-v6` and `reference-v6` while deliberately requiring no database migration.

## change rule

never edit an applied migration in place. add the next ordered migration,
preserve the checksum ledger, and follow the backup and recovery procedure in
`docs/development.md`. changing a tracked source extract is a provenance change,
not a schema migration, and still requires focused comparison and corpus proof.
