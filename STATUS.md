# polyvalent status

> **continuity role:** operational checkpoint
> **status:** current
> **last verified:** 2026-10-27
> **owns:** verified current state, active work, known gaps, and next useful action
> **update when:** a meaningful stopping point changes any of those facts

## Verified checkpoint

the accepted network-ignition slice is implemented locally on top of the
accepted first slice, public-delivery follow-up, bounded corpus enrichments,
and public suggestion inbox:

- the canonical local corpus is `seed-v6`, spanning integers `1–2000`;
- the prepared tracked public artifact is `reference-v6` with `17` properties,
  `761` accepted positive claims, `541` materialized public integers, and `20`
  profile entries;
- its sha-256 release digest is
  `3f339aa9d6c8701eaefbffe0acd691ba037ba7b61ef8f3ce17f80f3860462e81`;
- seven oeis-grounded properties were added: carmichael number, fermat
  pseudoprime to base 2, amicable number, sylvester sequence member, primary
  pseudoperfect number, heegner number, and two-way taxicab number;
- six new populations are derived by exact local evaluators over `1–2000` and
  compared with bounded source extracts before coverage or staging is allowed;
- heegner membership is a finite source-asserted import whose one-based source
  positions and citations are validated before insertion;
- the private avalonia gui, private sqlite ledger, publisher, and public web ui
  continue to share domain truth without sharing private runtime custody;
- public output remains an explicit staged projection, and public suggestions
  remain untrusted proposals that cannot mutate the canonical corpus.

the tracked public artifact was reproduced by two independent fresh bootstrap
runs. both produced `2000` ledger integers and byte-for-byte identical release
bytes. neither run used networking, an existing ledger, or deployment.

the project-owned complete local check passed on 2026-10-27:

```text
./scripts/check.sh
```

evidence:

- private git boundary: `30` representative private paths ignored, `3`
  intentional public fixtures trackable, and no tracked file hidden by an
  ignore rule;
- pages workflow boundary: pinned actions, checked `web/dist`-only build, and
  isolated deployment job;
- solution restore and release build: `0` warnings and `0` errors;
- .net tests: `83` passed across domain (`52`), data (`20`), publication
  (`10`), and desktop view-model (`1`) suites;
- .net formatting verification: succeeded;
- web type checking: succeeded;
- web tests: `38` passed across artifact, relation, graph trace, routing,
  suggestion, citation, coverage, and release behavior;
- public artifact checker: `17` properties, `541` integers, exact digest match;
- public source boundary: `22` source files, `2` runtime dependencies, `2`
  expected local release fetches, and `1` bounded provider adapter;
- production web build: succeeded;
- built public boundary: `6` inspectable files and no recipient address,
  credential, or private runtime token;
- `git diff --check`: succeeded inside the complete check.

focused proof also established:

- each deterministic evaluator accepts the intended bounded population and
  emits an exact, contract-valid witness;
- malformed factors, congruences, partner claims, recurrence positions,
  reciprocal identities, source positions, and duplicate cube pairs fail
  closed;
- source disagreement is preserved as a comparison and withholds both coverage
  and publication staging;
- the generated artifact contains all seven source citations, exact amicable
  partner qualifiers, the two taxicab cube decompositions for `1729`, and
  source-asserted heegner coverage;
- the profile-only list no longer contains `1729`, because it now has three
  evidence-bearing affiliations.

headless chromium renders were inspected at `1440px` and `390px` for
`/integer/1729`, `/integer/220`, `/integer/2`, `/integer/163`, and
`/property/amicable-number`. the new evidence plates, partner labels, graph
stations, coverage panel, and narrow layouts were legible without visible
horizontal overflow. browser-control automation was unavailable in this
session, so tactile pointer and keyboard behavior remains owner-visible proof;
component tests cover focus and trace semantics but are not represented as a
substitute for that review.

## Working now

- `1729` is simultaneously a base-2 fermat pseudoprime, a carmichael number,
  and a two-way taxicab number, with exact modular, korselt, and cube-sum
  witnesses;
- `220` and `284`, and `1184` and `1210`, carry reciprocal amicable partner
  qualifiers; exact partners rank ahead of ambient shared-membership relations;
- `2`, `6`, `42`, and `1806` carry exact primary-pseudoperfect reciprocal
  identities, while `2`, `3`, `7`, `43`, and `1807` carry exact sylvester
  recurrence positions;
- the nine finite heegner values carry exact oeis term positions and
  source-asserted coverage rather than fabricated evaluator proof;
- property relations remain bipartite and evidence-led: matching values do not
  manufacture a relation between `1806` and `1807`, and singleton taxicab
  membership does not invent a mutual;
- complete relation lists remain available even when graph salience bounds the
  visible field;
- absence remains covered-negative only inside succeeded recorded coverage and
  is otherwise unknown;
- incomplete drafts, mismatched comparisons, unsafe citations, private notes,
  and unstaged revisions cannot enter release bytes;
- manual claims and profiles remain editable only as drafts and immutable after
  acceptance unless an explicit superseding revision is opened;
- the public router, assets, and release fetches honor the deployment base path;
- the suggestion inbox remains a bounded, fail-closed external correspondence
  seam and never becomes a corpus-write path.

## Incomplete or broken

- the live manifest at `https://resonatrix.tech/polyvalent/data/manifest.json`
  was rechecked on 2026-10-27 and still identifies `reference-v5` with digest
  `2c0b485abc1dbb025296743c525d1948080f31e84ed71438251c1866affae441`;
- `reference-v6` has not been committed, pushed, deployed, or published;
- no provider request or suggestion canary was made;
- provider settings, recipient verification, actual delivery, and inbox receipt
  remain owner-controlled external proof;
- no analytics or production monitoring exists;
- actual owner-data privacy, a real wayland/native desktop session, and
  filesystem power-loss behavior remain unverified in their distinct
  environments;
- because the private-to-public and deployment boundaries are red-risk,
  independent review remains required before promotion or deployment.

## Active work

- accepted spec: `specs/001-first-slice.md`;
- accepted spec: `specs/002-public-suggestion-inbox.md`;
- accepted spec: `specs/003-network-ignition.md` governs `seed-v6` and
  `reference-v6`;
- implementation: complete locally against evaluator, persistence,
  publication, web, artifact, boundary, and render proof;
- live site: still `reference-v5`;
- blockers: none for local use or review.

## Next useful action

perform an independent release-boundary review and owner browser pass. if both
are accepted, separately authorize commit, push, and deployment of
`reference-v6`. any real suggestion-provider canary remains a distinct later
authorization.

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
