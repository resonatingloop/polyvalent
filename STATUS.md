# polyvalent status

> **continuity role:** operational checkpoint
> **status:** current
> **last verified:** 2026-10-18
> **owns:** verified current state, active work, known gaps, and next useful action
> **update when:** a meaningful stopping point changes any of those facts

## Verified checkpoint

the accepted first slice and bounded public-delivery follow-up are implemented
locally as one vertical instrument:

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
- a least-privilege github pages workflow that verifies tracked public inputs,
  builds for the configured pages base path, and uploads only the static web
  output;
- project-owned setup, validation, attribution, migration, publication, backup,
  deployment, and recovery documentation.

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
- pages workflow boundary: all `5` actions pinned to full commit hashes,
  `web/dist` is the sole upload path, and the deploy job cannot check out source
  or run shell commands;
- solution restore and release build: `0` warnings, `0` errors;
- .net tests: `58` passed across domain (`34`), data (`16`), publication (`7`),
  and desktop view-model (`1`) suites;
- .net formatting verification: succeeded;
- web tests: `17` passed across relation, canonical-integer, coverage, salience,
  release-digest, and deployment-base routing behavior;
- public artifact: digest matched; `5` properties and `481` integers;
- public boundary: `16` source files, `2` runtime dependencies, and only the `2`
  expected local release fetches;
- root and `/polyvalent/` production web builds: succeeded; the project-path
  build emitted matching `index.html` and `404.html` routing shells;
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
  sentinels fail closed;
- the public router, links, assets, and release fetches honor the deployment base
  path while root-hosted development remains unchanged;
- relevant pushes to `main` or an explicit manual dispatch can build and deploy
  only `web/dist` through separate github pages build and deploy jobs.

## Incomplete or broken

- the github pages workflow exists locally, but pages repository settings, a
  workflow run, the live url, tls, and direct-route behavior remain unverified;
- no custom domain, analytics, or production monitoring exists;
- no public suggestion form or email provider exists;
- no session-held integers, encounter pings, public lenses, private
  constellations, or arbitrary property-definition authoring exists;
- no application-level database encryption exists;
- power-loss behavior is documented through orphan-release quarantine but has
  not been fault-injected at the filesystem boundary;
- actual owner-data privacy and a real wayland/native session remain unverified
  because those require distinct environments;
- because the schema/privacy and deployment boundaries are red-risk,
  independent review remains required before any release is promoted or
  deployed.

## Active work

- accepted spec: `specs/001-first-slice.md`;
- implementation: complete locally against automated and rendered smoke proof;
- public ui: initial aesthetic pass accepted by the owner;
- remaining first-slice acceptance: owner-visible private gui review;
- blockers: none for local use or review.

## Next useful action

after review and commit, set repository settings → pages → source to **github
actions**, push the deployment workflow, and inspect the reported live url at
the home, integer, and property routes. treat that successful run and direct
browser pass as external deployment proof. private gui refinement remains its
own later visual slice.

## Re-entry notes

- database precedence: `--db`, `--data-dir`, `POLYVALENT_DATA_DIR`, then the
  platform-local application-data directory;
- repository-local development data belongs under ignored `./var/`;
- the tracked public fixture is `web/public/data/`; ordinary previews belong
  under ignored `./artifacts/public/`;
- release ids and output directories are immutable and never overwritten;
- `.env` is neither read nor required;
- pages deployment uploads only the checked `web/dist` output and never invokes
  the publisher;
- recovery procedure: `docs/development.md`;
- no commit, push, deployment, publication, or provider call was performed at
  this checkpoint.
