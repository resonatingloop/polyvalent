# spec: ledger re-entry — the cabinet knows what year it is

status: accepted
implementation: complete; owner-visible native acceptance pending

> **continuity role:** implemented transition awaiting owner-visible seal
> **owner:** repository owner
> **created:** 2026-11-13
> **accepted:** 2026-11-13
> **implemented:** 2026-11-13
> **supersedes:** none
> **related decisions:** `d-001`, `d-008`, `d-014`, `d-015`, `d-016`, `d-017`

accepted by the repository owner on 2026-11-13. this spec authorizes only the
bounded local implementation and verification described below. it does not
authorize inspection or mutation of the owner's real private ledger, a schema
migration, commit, push, publication, provider calls, or deployment.

## context

the documented quickest-success path currently seeds and then opens
`./var/local` as two independent commands. that path is unambiguous in a cold
checkout, but `./var/local` can survive from an earlier reference corpus or
contain authored research. the current seeder correctly refuses to rewrite a
populated ledger that does not exactly match `seed-v6`, exits nonzero, and
leaves it untouched. the desktop is a separate executable, so it can still
open that ledger successfully after the refused seed.

the combined re-entry experience can therefore look successful while showing
an older or authored cabinet. the terminal reported the refusal, the desktop
opened a valid database, and no invariant was violated, but the returning owner
was not given a durable answer to three ordinary questions:

1. which database did i open?
2. does it exactly match the current reference recipe?
3. what is the safe next command if i only wanted today's reference cabinet?

the problem is orientation, not corruption. an existing private ledger may be
historical, authored, deliberately divergent, or some combination. this slice
must not convert resemblance into a false provenance claim or turn re-entry
into an automatic upgrade path.

repository evidence also contradicts one current operational sentence:
`STATUS.md` says `reference-v6` was not committed or pushed, while the clean
local `main`, `origin/main`, the current commit, and the tracked manifest place
`reference-v6` in repository history. this slice may reconcile that repository
fact at close, but it must not infer current live deployment from git.

## owner view

**one-breath change:** add a read-only way to classify a selected ledger's
relationship to the current reference recipe, surface that classification in
the publisher and private desktop, and give the readme a versioned fresh
reference path without moving or rewriting any existing cabinet.

current:

```text
seed ./var/local
  -> exact seed-v6 ledger: succeed idempotently
  -> other populated ledger: refuse safely

desktop ./var/local
  -> open whichever compatible ledger is present
```

proposed:

```text
inspect selected path without creating or modifying it
  -> missing
  -> empty compatible ledger
  -> exact current reference: seed-v6
  -> compatible non-current or authored ledger
  -> unreadable or incompatible: explicit error

fresh demonstration path -> ./var/reference/seed-v6
existing owner cabinet    -> remains at its existing selected path
```

**authority:** `CONTRACTS.md` continues to own privacy, history, and
publication invariants; executable migrations remain schema authority;
`CorpusSeeder.SeedVersion` and the exact reference-shape predicate own the
current corpus recipe; this draft owns only the proposed transition.

## owner decisions proposed by this draft

| decision | recommendation | alternative | consequence of recommendation |
|---|---|---|---|
| language for a populated mismatch | say `non-current or authored ledger`; do not assign a historical seed version without proof | infer `seed-v1` through `seed-v5` from counts or run names | preserves uncertainty and owner authorship rather than laundering a software guess into provenance |
| treatment of existing ledgers | keep them usable through the existing desktop workflow and disable only automatic reseeding | force read-only mode or block opening | preserves the private gui as a research surface while keeping the seed refusal intact |
| fresh reference location | document `./var/reference/seed-v6` for the current demonstration cabinet | continue using `./var/local` as though it were always fresh | makes corpus-recipe identity visible in the path and avoids colliding with surviving local state |
| terminal orientation | add a non-mutating publisher `inspect` command | rely only on a gui badge after launch | lets a cold owner establish what is present before opening or mutating any application path |
| desktop orientation | show a quiet persistent ledger-status plate, not a blocking modal | warn only through the status strip or terminal | keeps the cabinet's temporal status visible without treating older research as an error |
| coupling of documented commands | make desktop launch contingent on successful current-reference seeding in the demonstration recipe | retain two visually adjacent but independent commands | prevents a seed refusal from visually dissolving into a successful launch |

accepting this spec accepts the recommendations in this table.

## scope

in:

- a read-only ledger-orientation model with explicit, non-overlapping outcomes;
- one shared exact-current predicate used by both orientation and the seeder,
  without weakening the seeder's refusal behavior;
- a publisher `inspect` command that does not create a directory, database,
  migration ledger, journal, wal file, backup, or application record;
- publisher success and refusal output that names the selected absolute
  database path, current corpus recipe, observed orientation, and one safe next
  action;
- a private desktop plate that presents the selected path's orientation and
  current recipe without exposing private record content;
- a versioned fresh-reference path and success-dependent launch sequence in
  `README.md` and `docs/development.md`;
- focused data, publisher, and desktop proof using temporary fictional or
  public mathematical fixtures only;
- reconciliation of operational documentation at the implemented checkpoint,
  including the repository-visible `reference-v6` state and one concrete
  re-entry action.

out:

- automatic upgrade, migration, regeneration, merge, copy, rename, move,
  repair, or deletion of an existing ledger;
- inspection of the owner's real `./var/local` contents during implementation
  or automated verification;
- assigning a precise historical seed identity when exact evidence does not
  establish it;
- changing the sqlite schema or adding a persisted corpus-version field;
- making non-current ledgers read-only or blocking their existing desktop
  authoring workflow;
- auto-seeding from the desktop;
- a general database management, comparison, or reconciliation interface;
- public artifact regeneration, commit, push, deployment, provider calls, or
  live-state claims;
- broader private gui redesign.

## governing contracts and decisions

- `surface-001` keeps the avalonia application as the private research and
  curation surface rather than a public administrative view;
- `fixture-001` requires fictional or public mathematical fixtures and
  prohibits private research traces in tests and demonstrations;
- `history-001` preserves immutable accepted research history and explicit
  supersession;
- `privacy-001`, `privacy-002`, `publication-001`, `publication-002`, and
  `publication-003` remain unchanged. orientation never becomes publication;
- `d-001` keeps the private rolodex and public explorer deliberately distinct;
- `d-008` establishes sqlite as the canonical private provenance ledger;
- `d-014`, `d-015`, `d-016`, and `d-017` repeatedly require populated older
  ledgers to remain untouched until an explicit revision workflow exists;
- `SCHEMA.md` keeps database schema, corpus recipe, source-extract envelope,
  and public release as four independent version axes.

no normative contract or historical decision change is proposed.

## orientation contract

orientation describes a selected path's relationship to the code's current
reference recipe. it does not certify the ledger's complete history, ownership,
quality, or fitness for publication.

| outcome | minimum evidence | display meaning | automatic seed disposition |
|---|---|---|---|
| `missing` | no filesystem entry exists at the resolved database path | no ledger exists at this path | a separately requested seed may create the current reference ledger |
| `empty` | a readable compatible ledger exists with no property vocabulary or authored corpus state | compatible empty cabinet | a separately requested seed may populate the current reference ledger |
| `current-reference` | the ledger passes the same complete exact-shape predicate used for `seed-v6` idempotence | exact current reference corpus · `seed-v6` | a separately requested seed may succeed idempotently |
| `non-current-or-authored` | the ledger is readable and compatible but populated state fails the exact current-reference predicate | valid cabinet that does not exactly match today's reference recipe; historical or authored status remains unresolved | automatic seed remains refused and the ledger remains untouched |

an unreadable file, unknown migration, migration checksum mismatch, or
incompatible sqlite structure is an explicit inspection error rather than one
of the four valid outcomes. inspection must not attempt repair or migration.

the orientation model may report bounded structural evidence needed to explain
its result, such as schema version, current recipe id, property count, claim
count, profile count, or a failed exact-current check. it must not emit notes,
profile bodies, source contents, citation labels, witnesses, private sentinel
values, or arbitrary row samples.

## command behavior

the proposed command is:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- inspect --data-dir PATH
```

`inspect` resolves the database path through the existing precedence rules but
does not call `SqliteDatabase.InitializeAsync`. it opens an existing database
through an explicitly read-only connection only after filesystem checks. a
missing path is reported without creating its parent directory.

successful output includes:

- resolved absolute database path;
- schema version when a compatible ledger exists;
- current corpus recipe, presently `seed-v6`;
- one orientation outcome and plain-language meaning;
- whether a separately requested current-reference seed would be allowed,
  idempotent, or refused;
- one safe next action.

inspection returns zero for the four valid outcomes, including
`non-current-or-authored`, because that outcome is not corruption. unreadable
or incompatible input returns nonzero with no repair attempt.

the existing `seed` command continues to return nonzero for a populated
non-current or authored ledger. its refusal adds the resolved path, current
recipe, and a copy-ready fresh-reference command using
`./var/reference/seed-v6`. it never launches the desktop itself.

## desktop behavior

the private desktop presents orientation near the existing database-path and
staging context. exact copy and material treatment remain subject to the
native design pass, but the semantic labels are fixed:

- `reference corpus · seed-v6`;
- `empty ledger`;
- `non-current or authored ledger · automatic reseeding disabled`.

the plate is persistent, keyboard- and screen-reader-reachable text. it is not
color-only, hover-only, a transient toast, or a blocking modal. it does not
change which integers can be opened, which drafts can be authored, or which
existing revision operations are available.

the desktop retains its existing initialization and migration contract. this
slice does not silently redefine desktop launch as a read-only operation. the
new strict no-write guarantee belongs specifically to `publisher inspect`.

## operational re-entry behavior

the documented reference walkthrough uses a versioned ignored directory:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- seed --data-dir ./var/reference/seed-v6 &&
dotnet run --project src/Polyvalent.Desktop/Polyvalent.Desktop.csproj -c Release -- --data-dir ./var/reference/seed-v6
```

the second command runs only after successful creation or exact-current
idempotence. documentation separately shows how to inspect and deliberately
open an existing owner-selected ledger. `./var/local` is neither moved nor
relabelled by implementation; it simply stops serving as the supposedly fresh
reference path in the front-door example.

when a corpus recipe later advances, the versioned demonstration directory,
expected labels, and operational documentation advance together. this is a
corpus-recipe transition, not a database-schema migration.

## likely files and components

- `src/Polyvalent.Data/CorpusSeeder.cs` for the existing exact-current
  predicate and unchanged seed refusal;
- a focused data-layer orientation type and inspector beside the sqlite data
  boundary;
- `src/Polyvalent.Application/ResearchService.cs` and application-facing view
  records for desktop consumption;
- `src/Polyvalent.Publisher/Program.cs` for `inspect`, exit behavior, and
  actionable seed output;
- `src/Polyvalent.Desktop/ViewModels/MainWindowViewModel.cs` and
  `src/Polyvalent.Desktop/MainWindow.axaml` for persistent orientation copy;
- data and desktop tests under `tests/`, plus a focused
  `tests/Polyvalent.Publisher.Tests/` project for command parsing, exit code,
  stdout, stderr, and non-mutation behavior without shell-dependent assertions;
- `README.md`, `STATUS.md`, `docs/development.md`, `docs/architecture.md`, and
  cli help for implemented operation and re-entry truth;
- `SCHEMA.md` only if explanatory wording must clarify that orientation adds no
  fifth version axis. the four-axis model itself does not change.

## implementation by invariant

1. **preflight the no-touch boundary.** before product edits, an independent
   read-only reviewer reconstructs every database open, initialization,
   migration, directory-creation, wal, backup, seed-shape, and desktop-startup
   path implicated by the slice. the receipt distinguishes confirmed contracts,
   inferred dependencies, unresolved ambiguity, and demonstrated
   counterexamples.
2. **establish one orientation vocabulary.** add the four valid outcomes and
   explicit inspection errors without storing new metadata or claiming
   historical lineage.
3. **make exact-current judgment single-sourced.** extract or encapsulate the
   current seed-shape predicate so read-only orientation and idempotent seeding
   consult the same logic. seeding retains all current validations and refusal
   semantics.
4. **prove non-mutation before adding presentation.** exercise missing, empty,
   exact-current, compatible-non-current, deliberately divergent, unknown
   migration, checksum mismatch, and non-sqlite paths. prove that `inspect`
   creates no file or directory and leaves existing bytes and sidecar inventory
   unchanged.
5. **establish terminal re-entry.** add `inspect`, structured human-readable
   output, exit-code proof, and actionable seed refusal. do not introduce a
   shell parser contract or machine-readable format in this slice.
6. **establish persistent native orientation.** surface the shared result in
   the desktop without changing research operations, publication staging, or
   private record rendering. inspect fresh and non-current fictional ledgers in
   a native render.
7. **reconcile cold-entry truth.** replace the ambiguous front-door path,
   document deliberate existing-ledger inspection/opening, reconcile current
   repository state without inferring live deployment, and leave one concrete
   next action in `STATUS.md`.
8. **attack the realized boundary.** after implementation, a fresh read-only
   diff adversary attempts to find accidental writes, predicate drift, false
   provenance claims, private-content disclosure, invalid exit semantics,
   version-path drift, and changes outside the accepted slice. reconcile its
   findings before close.

## change shape

| dimension | changed? | source of truth | proof |
|---|---:|---|---|
| user-visible behavior | yes | publisher output and private desktop orientation plate | command tests, view-model tests, native wide/narrow render, keyboard and screen-reader-label review |
| persisted state or schema | no | existing migration set and sqlite ledger | migration inventory, missing-path non-creation, existing-file digest and sidecar comparison, seed refusal regression |
| external or provider-visible behavior | no | existing public runtime and provider boundaries | no network path, endpoint, public artifact, or deployment diff; boundary scan remains green |
| identifiers, secrets or privacy | yes, local orientation only | resolved local path, corpus recipe id, bounded structural evidence, existing privacy contracts | output allowlist review, private-sentinel fixture, no arbitrary row reads or output |
| lifecycle or terminal states | yes | orientation result model and publisher exit contract | exhaustive outcome and error tests |
| setup, operation or recovery | yes | `README.md`, `docs/development.md`, cli help, and `STATUS.md` | cold fresh-reference rehearsal, existing-ledger inspect/open rehearsal, documentation check |
| normative contract or decision | no | existing `CONTRACTS.md` and decisions `d-001`, `d-008`, `d-014` through `d-017` | contract trace and direct documentation review |
| cross-repository dependency | no | solution and repository boundary | project reference inventory and complete local check |

## acceptance criteria

implementation may be called complete only when:

1. `inspect` on a missing path reports `missing`, returns zero, and creates no
   directory, database, journal, wal, shm, backup, or other file;
2. `inspect` distinguishes an empty compatible ledger, an exact unchanged
   `seed-v6` ledger, and a compatible populated ledger that does not match the
   current recipe;
3. unreadable, non-sqlite, unknown-migration, and checksum-mismatch inputs fail
   explicitly and remain untouched;
4. the exact-current result is derived from the same complete predicate used
   for seeder idempotence rather than from a looser count or version-label
   shortcut;
5. inspection of every existing fixture leaves the database digest, length,
   timestamps where the platform permits reliable assertion, and sidecar file
   inventory unchanged; semantic row-count comparison supplements rather than
   replaces filesystem proof;
6. publisher output contains only the resolved path, schema and recipe ids,
   bounded structural evidence, outcome, seed disposition, and safe next
   action; a sentinel in private notes, profile bodies, sources, citations, or
   witnesses never appears;
7. `seed` remains idempotent on exact `seed-v6`, refuses compatible non-current
   or authored state, returns nonzero on refusal, and does not weaken any
   existing corpus-shape validation;
8. seed refusal names the preserved path and supplies a copy-ready command for
   a fresh `./var/reference/seed-v6` cabinet without creating it;
9. the desktop displays one fixed semantic orientation label, keeps that label
   reachable at wide and constrained window sizes, and does not change
   navigation, authoring, revision, citation, or staging behavior;
10. opening a non-current fictional ledger remains possible through the
    existing desktop path and never claims a specific historical seed version;
11. the readme's fresh-reference launch is success-dependent and uses
    `./var/reference/seed-v6`; deliberate inspection and opening of an existing
    ledger are separate documented actions;
12. no test or review opens, copies, hashes, or otherwise inspects the owner's
    real `./var/local` database;
13. `STATUS.md` reports repository-visible `reference-v6` truth separately from
    unverified live deployment and names one concrete re-entry action;
14. focused data, publisher, and desktop tests pass, followed by the complete
    project check, documentation checker, direct diff review, semantic
    preflight reconciliation, and fresh diff-adversary reconciliation;
15. owner-visible native review confirms that current and non-current cabinet
    labels are legible, calm, and not easily mistaken for publication or
    corruption status.

## validation expected after implementation

focused proof:

```text
dotnet test tests/Polyvalent.Data.Tests/Polyvalent.Data.Tests.csproj -c Release
dotnet test tests/Polyvalent.Desktop.Tests/Polyvalent.Desktop.Tests.csproj -c Release
dotnet test tests/Polyvalent.Publisher.Tests/Polyvalent.Publisher.Tests.csproj -c Release
```

manual no-touch rehearsal against disposable fixtures:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- inspect --data-dir /tmp/polyvalent-ledger-reentry-missing
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- inspect --data-dir /tmp/polyvalent-ledger-reentry-current
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- inspect --data-dir /tmp/polyvalent-ledger-reentry-noncurrent
```

complete proof:

```text
./scripts/check.sh
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --require SCHEMA.md --review-claims
git diff --check
direct review of the complete diff and generated cli output
```

native desktop rendering, keyboard traversal, and owner comfort remain distinct
owner-visible proof. they are not inferred from view-model tests.

## rollback

- before implementation, delete or reject this draft; no product state has
  changed;
- during implementation, revert the bounded code, tests, and documentation
  diff. no database rollback should exist because the slice adds no persisted
  state and its inspection path is non-mutating;
- remove only disposable temporary fixtures and generated advisory review
  surfaces created for this slice;
- do not remove, restore over, rename, copy, or edit any pre-existing ledger;
- no public release or deployment rollback belongs to this slice.

## risks and unknowns

- the exact-current checks presently live inside the seeder and may assume a
  connection opened through write-capable initialization. extraction must not
  accidentally create a weaker parallel predicate or reuse a helper that
  performs writes;
- sqlite read-only intent is insufficient by assertion alone. connection mode,
  directory creation, pragmas, migrations, journals, wal/shm sidecars, backups,
  timestamps, and error paths all require direct proof;
- a populated compatible ledger may contain a pristine historical seed,
  extensive authored research, or both. the orientation label must retain that
  ambiguity;
- counts are useful explanatory evidence but cannot establish exact-current
  identity by themselves;
- the versioned demonstration path duplicates the current recipe identity in
  operational prose. every later corpus-recipe transition must update and test
  that path rather than silently reusing it;
- the desktop currently initializes the selected database before opening the
  main window. the strict `inspect` no-write contract must not be carelessly
  generalized to ordinary desktop startup;
- `STATUS.md` contains stale repository-transition prose while live deployment
  is temporally unstable. close may reconcile git-visible facts, but live state
  requires separate direct verification or an explicit unknown label;
- the new publisher test project must exercise a testable command runner
  directly rather than treating terminal text as an unstructured subprocess
  side effect. it adds no new runtime dependency or provider boundary.

## open questions

none inside the proposed slice. acceptance adopts the orientation vocabulary,
non-mutating `inspect` command, persistent desktop plate, versioned reference
path, continued usability of non-current ledgers, and explicit deferrals above.

## authorization boundary

if the repository owner marks this exact spec revision accepted, it authorizes
only the bounded local implementation and verification described above. it
does not authorize codex to inspect or mutate the owner's real private ledger,
add a schema migration, reconcile authored data, commit, push, publish, deploy,
contact a provider, or make an unverified claim about live state.

## implementation receipt

implemented locally on 2026-11-13 without inspecting the owner's real ledger,
changing the schema, regenerating the public artifact, committing, pushing,
publishing, deploying, or contacting a provider.

- the independent semantic preflight found that the former seed gate admitted
  extra authored state; the implementation replaced it with one shared exact
  schema-and-corpus fingerprint used by seed idempotence and orientation;
- immutable inspection distinguishes the four accepted outcomes, refuses
  incompatible schema, active wal, and active rollback-journal states, and
  checks database/sidecar metadata again after reading;
- publisher refusal, bounded output, private-sentinel exclusion, desktop
  refresh, accessibility copy, and versioned re-entry commands have focused
  proof;
- the project-owned check passed with `101` .net and `38` web tests, and the
  documentation checker reported `0` errors and `0` warnings;
- a fresh read-only diff adversary reported no remaining blocker or high
  finding after remediation;
- native current-reference and non-current/authored renders passed agent visual
  inspection at wide and minimum window sizes. owner-visible acceptance of
  those two plates remains the sole close condition.
