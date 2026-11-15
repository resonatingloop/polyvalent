# polyvalent

> **continuity role:** operational front door
> **status:** reference-v6 tracked on main; reference-v5 remains the last verified live release
> **last verified:** 2026-10-27
> **owns:** the shortest successful use and the map to deeper documentation
> **update when:** setup, primary use, maturity, or document authority changes

polyvalent is a social network(ing graph) for integers with a deliberately
unsocial private back room. one canonical sqlite provenance ledger feeds two
different surfaces:

- a private avalonia rolodex for evidence, sources, revisions, profile entries,
  and explicit publication staging;
- a public react site that reads one immutable json release and renders integer
  profiles, property dossiers, complete explained relation lists, and a bounded
  bipartite graph.

the public runtime has no database, polyvalent backend, administrative route,
or corpus-write path. private changes do nothing publicly until the owner runs
the publisher; generation still does not deploy anything. github pages delivery
is a separate, checked repository action over the committed public release. an
optional formspree tip line is the sole outbound correspondence path and never
touches publication or canonical state.

## quickest successful use

prerequisites:

- the .net sdk selected by [`global.json`](global.json);
- a node version accepted by [`web/package.json`](web/package.json);
- npm and git.

from the repository root, run the complete local proof:

```text
./scripts/check.sh
```

the script verifies the private git and pages-workflow boundaries; restores,
builds, format-checks, and tests the .net solution; installs the locked web
dependencies; verifies the committed public artifact and runtime boundary;
builds the site; and checks the diff for whitespace errors.

to open the public reference release:

```text
npm --prefix web run dev
```

then visit `http://localhost:5173`, `/integer/1729`, `/integer/220`, or
`/integer/2`. the network-ignition properties are visible at
`/property/carmichael-number`, `/property/amicable-number`, and the other
source-linked dossiers. the home-page tip line deliberately renders dormant
without a configured formspree form id.

to create a fresh versioned reference ledger and open the rolodex only after
seeding succeeds:

```text
dotnet run --project src/Polyvalent.Publisher -c Release -- seed --data-dir ./var/reference/seed-v6 &&
dotnet run --project src/Polyvalent.Desktop -c Release -- --data-dir ./var/reference/seed-v6
```

the current `seed-v6` corpus is idempotent on an unchanged matching ledger. it
creates integers `1` through `2000`, seventeen versioned properties, `761`
accepted positive claims, complete recorded coverage where warranted,
attributed sources and captures, and twenty sourced profile entries across
eighteen integers. it refuses to rewrite an older or divergent ledger; use a
fresh data directory or an explicit revision workflow when existing authored
research must be preserved.

to orient around a surviving owner ledger without creating, migrating, or
repairing it, inspect first:

```text
dotnet run --project src/Polyvalent.Publisher -c Release -- inspect --data-dir ./var/local
```

`inspect` reports `missing`, `empty`, `current-reference`, or
`non-current-or-authored` using bounded structural evidence only. it never
prints private rows. opening an existing ledger remains a separate deliberate
action:

```text
dotnet run --project src/Polyvalent.Desktop -c Release -- --data-dir ./var/local
```

ordinary desktop startup retains its existing initialize-and-migrate behavior;
its persistent ledger plate identifies the selected cabinet without making an
older or authored cabinet read-only.

to generate a new local preview from exactly what is staged:

```text
dotnet run --project src/Polyvalent.Publisher -c Release -- publish --data-dir ./var/local --output ./artifacts/public/local-v1 --release-id local-v1
```

the output directory and release id must both be new. the command makes no
network request and performs no deployment.

## architecture in one glance

```text
private sqlite ledger
   ├── avalonia desktop: read, draft, cite, accept, stage
   └── explicit publisher: select, validate, sanitize, seal
                                      │
                                      ▼
                           manifest.json + data.json
                                      │
                                      ▼
                           static react/vite site
                              │               └── suggestion -> formspree -> inbox
                              ▼
                         checked github pages artifact
```

[`docs/architecture.md`](docs/architecture.md) owns the dependency direction,
data model, lifecycle, and publication firewall. [`docs/development.md`](docs/development.md)
owns exact setup, seed, launch, preview, deployment, migration, backup, and
restore procedures.

## first corpus

| property | positive members in `1–2000` | coverage warrant | public graph |
|---|---:|---|---|
| prime | 303 | deterministic evaluation | yes, bounded display |
| triangular number | 62 | deterministic evaluation | yes |
| perfect square | 44 | deterministic evaluation | yes |
| perfect cube | 12 | deterministic evaluation | yes |
| fibonacci number | 16 | deterministic evaluation | yes |
| power of two | 11 | deterministic evaluation | yes |
| twin-prime member | 121 | deterministic evaluation | yes |
| decimal palindrome | 118 | deterministic evaluation | no |
| decimal narcissistic number | 14 | deterministic evaluation, compared with oeis a005188 | yes |
| decimal friedman number | 27 | source-asserted bounded import with locally validated expressions | yes |
| carmichael number | 3 | deterministic evaluation, compared with oeis a002997 | yes |
| fermat pseudoprime to base 2 | 7 | deterministic evaluation, compared with oeis a001567 | yes |
| amicable number | 4 | deterministic evaluation, compared with oeis a063990 | yes |
| sylvester sequence member | 5 | deterministic evaluation, compared with oeis a000058 | yes |
| primary pseudoperfect number | 4 | deterministic evaluation, compared with oeis a054377 | yes |
| heegner number | 9 | source-asserted bounded import from oeis a003173 | yes |
| two-way taxicab number | 1 | deterministic evaluation, compared with oeis a001235 | yes |

relations are derived by intersecting published membership claims; there is no
pairwise-edge table. the reference release therefore explains
`153 → decimal friedman number → 343`, including `3 × 51` for `153` and the
orderly/nice witness `(3 + 4) ^ 3` for `343`.
it also gives `137` a prime and twin-prime social field, ranks its exact partner
`139` first, shows `343` as the perfect cube `7^3`, and places `1` through
`1024` in the source-linked power-of-two lineage with exact `2^n` witnesses.
the network ignition gives `1729` overlapping carmichael, base-2 pseudoprime,
and two-way taxicab memberships; gives `220` an exact amicable partner `284`
without storing a pairwise edge; and keeps `1806` and `1807` unrelated despite
their conspicuous adjacency in different source sequences.

the graph is an interactive trace instrument rather than a decorative diagram.
pointer contact or keyboard focus isolates the exact published route, conducts
a visible signal through its property station, and opens a compact readout of
the shared reasons, qualifiers, and endpoint witnesses. narrow screens receive
a denser geometry with larger nodes instead of a miniaturized desktop map;
reduced-motion preference suppresses the ambient and traced motion.

profile lore remains a separate register from mathematical affiliation. the
reference artifact gives `153` a carefully separated vesica-piscis and john
21:11 vignette, `93` a sourced thelemic greeting, `146` its minor-planet
identity, `404` its http status, `808` its rhythm machine, and `86` its disputed
slang history. the expanded cabinet adds rabbinic, kabbalistic, patristic,
thelemic, astronomical, literary, mathematical, manuscript, museum, and
video-game biographies from `36` through `1729`; `137` and `153` now each carry
two separately cited profiles. none of these entries creates a cultural
relation or pairwise edge.

## configuration and data

database location precedence is:

1. `--db <path>`;
2. `--data-dir <directory>`;
3. `POLYVALENT_DATA_DIR`;
4. the platform-local application-data directory under `polyvalent`.

the application never loads `.env`. repository-local private state belongs
under ignored `./var/` or `./captures/`; common database filenames and
sidecars, migration backups, preview releases, node modules, and compiled
output are ignored as defense in depth. the reviewed deterministic artifact
under `web/public/data/` is the only tracked release.

the public suggestion endpoint is configured only through the non-secret
build-time form id `VITE_PUBLIC_SUGGESTION_FORM_ID`; github pages maps it from
the repository variable `PUBLIC_SUGGESTION_FORM_ID`. absent or malformed values
leave the plate visibly dormant and make no request.

## public deployment

the live manifest was rechecked on 2026-10-27 and still identifies reference-v5:
[`https://resonatrix.tech/polyvalent/`](https://resonatrix.tech/polyvalent/).
the reviewed local artifact is reference-v6; generation did not deploy it.

[`deploy-pages.yml`](.github/workflows/deploy-pages.yml) deploys the public site
to github pages after relevant pushes to `main`, and also supports explicit
manual dispatch. it checks the private git boundary and public release seam,
builds for the pages-provided base path, scans the built boundary, and uploads
only `web/dist`. the public form id is configured as a repository variable, but
the currently live site remains unchanged until a reviewed commit and later
authorized deployment.

the repository owner must select **github actions** once under settings → pages
before the first run can deploy. no private database, untracked publisher
preview, repository secret, or `.env` file participates in the workflow. see
[`docs/development.md`](docs/development.md#github-pages-deployment) for the
operator path and rollback boundary.

## documentation authority

| document | role | status | owns |
|---|---|---|---|
| `README.md` | operational | current | front door and shortest successful use |
| `STATUS.md` | operational | current | verified checkpoint and next useful action |
| `docs/development.md` | operational | current | setup, commands, local data, and recovery |
| `docs/architecture.md` | explanatory | current | implemented boundaries and representation map |
| `SCHEMA.md` | explanatory | current | private sqlite tables, relationships, guards, and version axes |
| `AGENTS.md` | behavioral | current | agent authorization and completion protocol |
| `PROJECT_BRIEF.md` | durable intent | current | purpose, audience, scope, and non-goals |
| `CONTRACTS.md` | normative | current | epistemic, privacy, and publication invariants |
| `DECISIONS.md` | historical | append-only | accepted choices and their consequences |
| `specs/001-first-slice.md` | planning | accepted | active first-slice scope and acceptance criteria |
| `specs/002-public-suggestion-inbox.md` | planning | accepted | public suggestion-to-inbox transition and activation boundary |
| `specs/003-network-ignition.md` | planning | accepted | active seven-property `seed-v6` and `reference-v6` transition |
| `specs/004-ledger-reentry.md` | planning | implemented; owner review pending | non-mutating orientation, exact recipe identity, and durable ledger re-entry |

when documents disagree, identify which document owns the disputed fact rather
than applying filename precedence.

## intentionally absent

- analytics and production monitoring;
- session-held integers and encounter pings;
- public thematic lenses;
- private constellations and arbitrary new property definitions;
- application-level database encryption.

headless browser and x11 renders exist as implementation evidence, but final
owner acceptance on the actual browser and native desktop remains a distinct
manual checkpoint.
