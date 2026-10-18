# polyvalent

> **continuity role:** operational front door
> **status:** pre-implementation
> **last verified:** 2026-10-17
> **owns:** the shortest successful use and the map to deeper documentation
> **update when:** setup, primary use, maturity, or document authority changes

polyvalent is a research instrument for investigating interesting integers and
the relations among them. the repository is intended to contain two deliberately
different surfaces over one canonical research corpus: a private avalonia gui
for research and curation, and a public web instrument for exploratory profiles
and relation graphs.

## current state

the repository has no executable application yet. its continuity documents
record the accepted product shape, epistemic distinctions, privacy boundary,
and unresolved decisions that must be settled before implementation begins.

see [`STATUS.md`](STATUS.md) for the current checkpoint and next useful action.

## quickest successful use

prerequisites:

- python 3;
- the local `manage-project-docs` codex skill.

from the repository root:

```text
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --review-claims
```

expected evidence of success:

```text
documentation check completes with 0 errors and 0 warnings; any listed strong claims remain subject to human review.
```

there is not yet an application build or launch command.

## intended shape

- the private gui is a serious research surface: an integer rolodex, editable
  evidence, and collapsible connections rather than a social graph;
- the public web ui is a playful social network for integers: search for a
  number, inspect its public properties, and follow explained relations to
  other integers;
- the first bounded corpus targets integers `1` through `2000` with a contained,
  reproducibly populated property set;
- public suggestions arrive for human review and never write directly to the
  canonical corpus.

these are durable product intentions, not an implementation spec. framework,
database, schema, graph library, deployment, and submission-provider choices
remain unresolved.

## configuration and data

- configuration: none implemented;
- secrets: none required; `.env` is ignored and must not be read or committed;
- persistent data: none implemented;
- external integrations: none implemented;
- public delivery: not configured.

## documentation authority

| document | role | status | owns |
|---|---|---|---|
| `README.md` | operational | current | front door and shortest successful use |
| `STATUS.md` | operational | current | verified checkpoint and next useful action |
| `AGENTS.md` | behavioral | current | agent authorization and completion protocol |
| `PROJECT_BRIEF.md` | durable intent | current | purpose, audience, scope, and non-goals |
| `CONTRACTS.md` | normative | current | epistemic, privacy, and publication invariants |
| `DECISIONS.md` | historical | append-only | accepted choices and their consequences |
| `specs/*.md` | planning | absent | proposed or accepted implementation slices |

when documents disagree, identify which document owns the disputed fact rather
than applying a filename-based precedence rule.

## known limitations

- no framework, database, executable entry point, test suite, or deployment
  path has been selected;
- the initial property pack and its population methods are not yet specified;
- the public/private export format and submission delivery mechanism are not
  yet specified;
- no live, rendered, persistence, privacy, or delivery behavior exists to test.
