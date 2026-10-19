# development and recovery

> **continuity role:** operational runbook
> **status:** current for the accepted first slice, pages delivery, and seed-v4 enrichment
> **last verified:** 2026-10-19
> **owns:** setup, local data, seed, launch, publication preview, validation, and recovery
> **update when:** any command, path, prerequisite, migration, or recovery step changes

## prerequisites

- .net sdk `10.0.112`, selected by `global.json`;
- node `^22.12.0`, `^24.0.0`, or `>=26.0.0`, as recorded in the web package;
- npm and git;
- a graphical desktop session for owner-visible avalonia review.

the implementation was verified on linux. no `.env` file is read or required.

## restore, build, and test

the project-owned full check is:

```text
./scripts/check.sh
```

its constituent commands are:

```text
./scripts/check-private-boundary.sh
node ./scripts/check-pages-workflow.mjs
dotnet restore Polyvalent.slnx
dotnet build Polyvalent.slnx -c Release --no-restore
dotnet format Polyvalent.slnx --verify-no-changes --no-restore
dotnet test Polyvalent.slnx -c Release --no-build
npm --prefix web ci
npm --prefix web run check
git diff --check
```

in a restricted environment whose default nuget advisory cache is read-only,
set `NUGET_HTTP_CACHE_PATH` to a writable temporary directory. `scripts/check.sh`
already defaults that cache to `/tmp/polyvalent-nuget-http` and the npm cache to
`/tmp/polyvalent-npm-cache` without changing normal package locations.

the documentation continuity check is separate because it belongs to the local
codex skill installation:

```text
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --review-claims
```

## database configuration

both the publisher and desktop resolve the private database in this order:

1. `--db <absolute-or-relative-file>`;
2. `--data-dir <directory>` plus `polyvalent.sqlite3`;
3. `POLYVALENT_DATA_DIR` plus `polyvalent.sqlite3`;
4. the platform-local application-data directory plus `polyvalent/polyvalent.sqlite3`.

use `--db` or `--data-dir`, never both. repository-local development state,
including an explicitly named database, belongs under ignored `./var/`; raw
source capture files belong under ignored `./captures/`. common `.db`,
`.sqlite`, and `.sqlite3` filenames are also ignored as defense in depth. the
application does not discover or parse `.env`.

## initialize and seed

create or migrate an empty development database:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- init --data-dir ./var/local
```

populate the deterministic first corpus and its publication staging:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- seed --data-dir ./var/local
```

`seed-v4` is idempotent when the existing ledger has its expected successful
population runs, graph policy, claim counts, and twenty staged profile entries.
a clean result reports `2000`
integers and `717` accepted claims distributed as
`303` prime, `62` triangular, `44` perfect square, `12` perfect cube, `16`
fibonacci, `121` twin-prime, `118` decimal palindrome, `14` decimal
narcissistic, and `27` decimal friedman claims.

the same seed stages twenty sourced profile entries across `36`, `86`, `88`,
`93`, `108`, `137`, `146`, `153`, `156`, `163`, `231`, `318`, `343`, `404`,
`418`, `616`, `808`, and `1729`. `137` and `153` each carry two separately
cited entries. profile entries remain narrative records rather than property
claims, so the claim count and relation ontology do not change.

the command deliberately refuses to rewrite a populated seed-v1, seed-v2,
seed-v3, or divergent ledger. accepted property versions and claim revisions are immutable, so
upgrading authored research requires a separately reviewed revision workflow.
use a fresh `--data-dir` when the desired result is the current reference
corpus.

the narcissistic run is compared with the bounded a005188 extract. disagreement
is retained in `comparison_runs`; that property receives no coverage or staging,
and the command reports the publication hold. friedman expressions are parsed
and checked locally before their imported claims can be accepted.

## private desktop

after seeding, run:

```text
dotnet run --project src/Polyvalent.Desktop/Polyvalent.Desktop.csproj -c Release -- --data-dir ./var/local
```

the first-slice workflow can open canonical integers, inspect property states
and evidence, create manual claim drafts, create sources and citations, create
or revise profile entries, accept or withdraw drafts, and stage exact accepted
revisions. the staging count is visible in the top bar. none of these operations
generates a release.

generated and imported claims are read-only. a new manual claim for the same
integer and property becomes an explicit superseding draft; when the accepted
replacement is staged, the stale staged revision from that lineage is removed.

## public site

the site uses the reviewed artifact already committed under `web/public/data`:

```text
npm --prefix web run dev
```

the development server prints its local address. useful routes are:

- `/` for integer search and the three reference apertures;
- `/integer/153` for the dense relation graph and complete list;
- `/integer/343` for the cube and orderly/nice friedman witnesses;
- `/integer/137` for sourced profile lore plus prime and twin-prime relations;
- `/integer/93` and `/integer/146` for profile-only public records;
- `/integer/616` for source-critical textual lore beside its palindrome claim;
- `/integer/1729` for a profile-only mathematical biography;
- `/property/decimal-friedman-number` for the property dossier.

the production build is written to ignored `web/dist/`:

```text
npm --prefix web run build
```

the build is static. root-hosted local builds use `/`; deployment supplies its
own pages base path without changing local routes.

## github pages deployment

the repository-owned workflow is `.github/workflows/deploy-pages.yml`. before
its first run, open repository settings → pages and set **source** to **github
actions**. that repository setting is an owner-operated external transition;
the workflow does not attempt to enable pages itself.

the workflow runs after a push to `main` that changes `web/`, the workflow, its
boundary checks, or `.gitignore`. it can also be started manually from the
actions tab. its two jobs preserve a narrow authority split:

1. `build` checks out tracked source without persistent git credentials, reads
   the configured pages base path, verifies the private git boundary and public
   artifact/runtime boundary, proves the workflow still uses pinned actions and
   a `web/dist`-only upload, installs the locked web dependencies, and builds the
   static site;
2. the build copies `index.html` to `404.html` so a direct integer or property
   url can boot the client router on github pages;
3. only `web/dist` is uploaded as the pages artifact;
4. `deploy` receives pages and oidc write authority but never checks out the
   repository or opens the private ledger.

the action uses no repository secret and never runs `Polyvalent.Publisher`.
github's reported base path keeps assets, release fetches, links, and client
routing valid both at `/polyvalent/` and at a later configured custom domain.

deployment is proven only by a successful github actions run followed by direct
browser checks of `/`, `/integer/153`, and
`/property/decimal-friedman-number`. local builds do not prove repository
settings, pages availability, tls, or the live route fallback. rollback means a
fresh pages deployment from a reviewed commit containing the desired immutable
public artifact; it never edits the canonical ledger or a sealed release.

## generate a publication preview

publication reads only the exact staged accepted revisions and individually
selected safe citations. choose a new release id and a path that does not exist:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- publish --data-dir ./var/local --output ./artifacts/public/local-v1 --release-id local-v1
```

inject `--generated-at 2026-10-19T00:00:00Z` when deterministic bytes are
required for a fixture or comparison. the publisher writes to a sibling
temporary directory, validates all bytes, moves the directory into place, and
then seals the release ledger record with the exact selected revision and
citation ids. it refuses target or release-id reuse.

generation does not copy output into `web/public/data`, contact a host, or
deploy. review the new manifest, digest, data, source attribution, and privacy
scan before any later artifact-promotion or deployment change.

## reproduce the committed reference artifact

use a fresh database and fresh output outside the tracked tree:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- bootstrap --db /tmp/polyvalent-reference-v4.sqlite3 --output /tmp/polyvalent-reference-v4 --release-id reference-v4 --generated-at 2026-10-19T00:00:00Z
diff -ru web/public/data /tmp/polyvalent-reference-v4
```

both `/tmp` targets must be absent before the command. an empty `diff` proves
byte-for-byte reproduction. refreshing the tracked artifact is an explicit,
reviewed change; do not make a development publish overwrite it.

## migrations and backups

ordered sql migrations are embedded in `Polyvalent.Data`. every applied version
and sha-256 checksum is recorded in `schema_migrations`; reopening rejects an
unknown version or changed checksum.

before applying any pending migration to a nonempty database, initialization
creates a sqlite-consistent sibling backup named:

```text
polyvalent.sqlite3.backup-<utc timestamp>
```

the backup and database receive user-only permissions on unix. migrations run
transactionally and the command reports the backup path.

## restore a private database

restore is deliberately manual and recoverable:

1. close every polyvalent desktop and publisher process using the database;
2. copy the database, `-wal`, and `-shm` files to a separate quarantine
   directory if they exist;
3. copy the chosen `.backup-<timestamp>` file to a new recovery filename;
4. launch first with `--db <recovery-filename>` and verify the schema and
   dossiers;
5. only after verification, decide whether to make that recovered file the
   configured canonical database.

never restore over an open database and never delete the quarantined copy as
part of the restore command.

## recover publication state

a release is valid only when its directory is complete, its manifest digest
matches `data.json`, and the private ledger contains the matching sealed release
row. quarantine a complete-looking directory without a sealed row; it may be an
orphan from a machine interruption. do not infer deployment from generation.

host rollback means selecting a previously reviewed immutable release, not
editing its files or rewriting canonical history. for github pages, deploy a
reviewed commit containing that release through the same workflow.

## source and seed maintenance

tracked bounded extracts live in `data/seed/v1/`; attribution lives in
`THIRD_PARTY_NOTICES.md`. changing an extract, witness, property contract,
evaluator version, or expected count is a data/provenance change. it requires a
new accepted slice or explicit bounded authorization, focused mathematical and
comparison tests, regenerated reference bytes, and documentation reconciliation.

do not commit raw private captures, owner notes, personal identifiers,
credentials, local database files, or `.env` contents.
