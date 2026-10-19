# polyvalent status

> **continuity role:** operational checkpoint
> **status:** current
> **last verified:** 2026-10-19
> **owns:** verified current state, active work, known gaps, and next useful action
> **update when:** a meaningful stopping point changes any of those facts

## Verified checkpoint

the accepted first slice, public-delivery follow-up, and bounded corpus
enrichment are implemented locally as one vertical instrument:

- a .net 10 solution with canonical integer semantics, nine property
  evaluators/contracts, a bounded non-executing friedman parser, application
  services, direct sqlite persistence, avalonia desktop, and publisher;
- a migrated provenance ledger with immutable accepted evidence, source and
  capture distinctions, comparisons, coverage, revision lineage, explicit
  staging, privacy sentinels, and immutable release records;
- a deterministic `1–2000` seed-v2 containing `717` accepted positive claims and
  one sourced profile entry;
- a prepared reference-v2 artifact in the tracked release path with `9`
  properties, `520` materialized integer profiles, and sha-256
  `e7e7a7611f86f77b84b9d9be6e7952e499601c72e8ca27513c7cafc9ac1a4569`;
- a static react/vite public ui with search, number profiles, property dossiers,
  complete explained relation lists, a bounded accessible bipartite graph, and
  a native in-page continuation seam from the home hero into the public
  dispatch;
- a least-privilege github pages workflow that verifies tracked public inputs,
  builds for the configured pages base path, and uploads only the static web
  output;
- a live reference-v1 deployment at `https://resonatrix.tech/polyvalent/`;
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
- .net tests: `68` passed across domain (`41`), data (`18`), publication (`8`),
  and desktop view-model (`1`) suites;
- .net formatting verification: succeeded;
- web tests: `19` passed across relation, canonical-integer, coverage, salience,
  release-digest, and deployment-base routing behavior;
- public artifact: digest matched; `9` properties and `520` integers;
- public boundary: `16` source files, `2` runtime dependencies, and only the `2`
  expected local release fetches;
- root and `/polyvalent/` production web builds: succeeded; the project-path
  build emitted matching `index.html` and `404.html` routing shells;
- `git diff --check`: succeeded as the final step of the script.

focused integration proof exercised two independent fresh `bootstrap` commands.
both produced `2000` integers, `717` claims, one profile entry, and byte-for-byte
identical sealed release bytes without networking or deployment.

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
- prime, perfect-square, perfect-cube, fibonacci, and twin-prime membership are
  graphable; the graph remains bounded while the relation list stays complete;
- `137` carries independent sourced profile context plus prime and twin-prime
  affiliations, with its exact partner `139` ranked before ambient mutuals;
- `343` carries a checked `7^3 = 343` perfect-cube witness in addition to its
  palindrome and friedman affiliations;
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
- the home-page hero uses a viewport-aware floor, a keyboard-focusable native
  `#dispatch` link, and the next plate's visible upper seam to disclose that the
  opening composition continues below the fold without scroll interception;
- relevant pushes to `main` or an explicit manual dispatch can build and deploy
  only `web/dist` through separate github pages build and deploy jobs.

## Incomplete or broken

- reference-v2 is only the tracked local artifact; the live site still reports
  reference-v1 until a later reviewed commit and deployment;
- the in-app browser bridge rejected its own bundled runtime during the new
  local visual smoke, so interactive rendering of the enriched `137` profile
  and the home-page continuation seam remain owner-visible checks; type,
  relation, artifact, and production-build proofs passed;
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
- implementation: corpus enrichment complete locally against automated proof;
- public ui: initial aesthetic pass accepted by the owner; home-page scroll
  disclosure is implemented locally and awaits rendered acceptance;
- live site: reference-v1; reference-v2 is not committed, pushed, or deployed;
- remaining visual acceptance: home-page continuation seam, enriched `137`
  public profile, and the deferred private gui slice;
- blockers: none for local use or review.

## Next useful action

open the local reference-v2 home page at desktop and narrow widths, confirm that
the continuation seam discloses the public dispatch without crowding the search,
then inspect `/integer/137` and confirm that `139` is the first exact twin-prime
acquaintance. review and commit after those visual checks. a later authorized
push will deploy that exact tracked artifact through the existing pages
workflow. private gui refinement remains its own visual slice.

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
