# polyvalent

> **continuity role:** operational front door
> **status:** implemented locally; owner visual acceptance pending
> **last verified:** 2026-10-18
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

the public runtime has no database, backend, administrative route, or write
path. private changes do nothing publicly until the owner runs the publisher;
generation still does not deploy anything.

## quickest successful use

prerequisites:

- the .net sdk selected by [`global.json`](global.json);
- a node version accepted by [`web/package.json`](web/package.json);
- npm and git.

from the repository root, run the complete local proof:

```text
./scripts/check.sh
```

the script verifies the private git boundary; restores, builds, format-checks,
and tests the .net solution; installs the locked web dependencies; verifies the
committed public artifact and runtime boundary; builds the site; and checks the
diff for whitespace errors.

to open the public reference release:

```text
npm --prefix web run dev
```

then visit `http://localhost:5173`, `/integer/153`, `/integer/343`, or
`/integer/137`.

to create a private development ledger and open the rolodex:

```text
dotnet run --project src/Polyvalent.Publisher -c Release -- seed --data-dir ./var/local
dotnet run --project src/Polyvalent.Desktop -c Release -- --data-dir ./var/local
```

the seed is idempotent. it creates integers `1` through `2000`, five versioned
properties, `524` accepted positive claims, complete recorded coverage where
warranted, attributed sources and captures, and one sourced profile entry for
`137`.

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
```

[`docs/architecture.md`](docs/architecture.md) owns the dependency direction,
data model, lifecycle, and publication firewall. [`docs/development.md`](docs/development.md)
owns exact setup, seed, launch, preview, migration, backup, and restore
procedures.

## first corpus

| property | positive members in `1–2000` | coverage warrant | public graph |
|---|---:|---|---|
| prime | 303 | deterministic evaluation | no |
| triangular number | 62 | deterministic evaluation | yes |
| decimal palindrome | 118 | deterministic evaluation | no |
| decimal narcissistic number | 14 | deterministic evaluation, compared with oeis a005188 | yes |
| decimal friedman number | 27 | source-asserted bounded import with locally validated expressions | yes |

relations are derived by intersecting published membership claims; there is no
pairwise-edge table. the reference release therefore explains
`153 → decimal friedman number → 343`, including `3 × 51` for `153` and the
orderly/nice witness `(3 + 4) ^ 3` for `343`.

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

## documentation authority

| document | role | status | owns |
|---|---|---|---|
| `README.md` | operational | current | front door and shortest successful use |
| `STATUS.md` | operational | current | verified checkpoint and next useful action |
| `docs/development.md` | operational | current | setup, commands, local data, and recovery |
| `docs/architecture.md` | explanatory | current | implemented boundaries and representation map |
| `AGENTS.md` | behavioral | current | agent authorization and completion protocol |
| `PROJECT_BRIEF.md` | durable intent | current | purpose, audience, scope, and non-goals |
| `CONTRACTS.md` | normative | current | epistemic, privacy, and publication invariants |
| `DECISIONS.md` | historical | append-only | accepted choices and their consequences |
| `specs/001-first-slice.md` | planning | accepted | active first-slice scope and acceptance criteria |

when documents disagree, identify which document owns the disputed fact rather
than applying filename precedence.

## intentionally absent

- deployment, hosting, analytics, and production monitoring;
- the public suggestion form and email delivery;
- session-held integers and encounter pings;
- public thematic lenses;
- private constellations and arbitrary new property definitions;
- application-level database encryption.

headless browser and x11 renders exist as implementation evidence, but final
owner acceptance on the actual browser and native desktop remains a distinct
manual checkpoint.
