# polyvalent status

> **continuity role:** operational checkpoint
> **status:** current
> **last verified:** 2026-10-18
> **owns:** verified current state, active work, known gaps, and next useful action
> **update when:** a meaningful stopping point changes any of those facts

## Verified checkpoint

the accepted first slice is implemented as one local vertical instrument:

- a .net 10 solution with canonical integer semantics, five property
  evaluators/contracts, a bounded non-executing friedman parser, application
  services, direct sqlite persistence, avalonia desktop, and publisher;
- a migrated provenance ledger with immutable accepted evidence, source and
  capture distinctions, comparisons, coverage, revision lineage, explicit
  staging, privacy sentinels, and immutable release records;
- a deterministic `1–2000` seed containing `524` accepted positive claims and
  one sourced profile entry;
- a committed reference release with `5` properties, `481` materialized integer
  profiles, and sha-256
  `0a0cd360cd1832522634e1b4b14c6d461f53a4f78a48ad75a49547f00b6d830d`;
- a static react/vite public ui with search, number profiles, property dossiers,
  complete explained relation lists, and a bounded accessible bipartite graph;
- project-owned setup, validation, attribution, migration, publication, backup,
  and recovery documentation.

the public runtime verifies release bytes against the manifest before parsing,
has exactly two runtime dependencies and two local artifact fetches, and has no
database, backend, private api, persistence, or visitor-storage capability.

verification performed with the project-owned full check:

```text
./scripts/check.sh
```

evidence:

- private git boundary: `30` representative private paths matched by
  repository-owned ignore rules, `3` intentional public fixtures trackable,
  and no tracked file hidden by an ignore rule;
- solution restore and release build: `0` warnings, `0` errors;
- .net tests: `58` passed across domain (`34`), data (`16`), publication (`7`),
  and desktop view-model (`1`) suites;
- .net formatting verification: succeeded;
- web tests: `13` passed across relation, canonical-integer, coverage, salience,
  and release-digest behavior;
- public artifact: digest matched; `5` properties and `481` integers;
- public boundary: `15` source files, `2` runtime dependencies, and only the `2`
  expected local release fetches;
- production web build: succeeded;
- `git diff --check`: succeeded as the final step of the script.

focused integration proof also exercised a fresh `bootstrap` command, producing
`2000` integers, `524` claims, one profile entry, and a sealed deterministic
release without networking or deployment.

headless chromium renders were inspected at desktop and narrow widths for the
home page and the `153`, `343`, and `137` profiles. an avalonia render was
inspected under x11 after correcting research-column measurement. these are
rendered implementation evidence, not a substitute for the owner's final
interactive browser and native desktop acceptance.

## Working now

- `153` and `343` relate through published decimal friedman membership without
  a stored pairwise edge; `343` is shown as orderly/nice and unsupported
  strong/weak labels are absent;
- graph salience keeps explicitly qualified endpoints such as `343` visible
  within an equally rare property while the list retains every relation;
- `137` carries sourced public profile context without acquiring a graph edge;
- absence is covered-negative only inside succeeded recorded coverage and is
  otherwise unknown;
- an incomplete private draft cannot establish membership, coverage, staging,
  or a public relation;
- manual claim and profile drafts are editable in the desktop, then freeze on
  acceptance unless an explicit superseding revision is opened;
- citations, their public-safe/staged disposition, private notes, and parent
  revision staging are visible in the private dossier; sealed releases pin the
  exact selected citation rows as immutable ledger items;
- accepted corrections preserve supersession, and staging a correction evicts
  the stale staged revision from that lineage;
- a source comparison mismatch is preserved and withholds both coverage and
  publication staging for the affected property;
- migration checksum drift, accepted-record mutation, release mutation,
  target overwrite, unsafe urls, incomplete evidence, and public privacy
  sentinels fail closed.

## Incomplete or broken

- no deployment, hosting, analytics, or production monitoring exists;
- no public suggestion form or email provider exists;
- no session-held integers, encounter pings, public lenses, private
  constellations, or arbitrary property-definition authoring exists;
- no application-level database encryption exists;
- power-loss behavior is documented through orphan-release quarantine but has
  not been fault-injected at the filesystem boundary;
- actual owner-data privacy, a real wayland/native session, and deployed static
  routing remain unverified because those require distinct environments;
- because this is a red-risk schema/privacy slice, independent review remains
  required before any release is promoted or deployed.

## Active work

- accepted spec: `specs/001-first-slice.md`;
- implementation: complete locally against automated and rendered smoke proof;
- remaining acceptance: owner-visible interactive review of both surfaces;
- blockers: none for local use or review.

## Next useful action

run the two launch paths in `README.md`, inspect `153`, `343`, and `137` in the
browser, then work through one private draft and staging action in the desktop.
if the surfaces and workflow are accepted, record that owner observation and
retire the first-slice spec. deployment or public suggestions should begin as a
new bounded spec, not as cleanup inside this one.

## Re-entry notes

- database precedence: `--db`, `--data-dir`, `POLYVALENT_DATA_DIR`, then the
  platform-local application-data directory;
- repository-local development data belongs under ignored `./var/`;
- the tracked public fixture is `web/public/data/`; ordinary previews belong
  under ignored `./artifacts/public/`;
- release ids and output directories are immutable and never overwritten;
- `.env` is neither read nor required;
- recovery procedure: `docs/development.md`;
- no commit, push, deployment, publication, or provider call was performed at
  this checkpoint.
