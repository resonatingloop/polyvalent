# polyvalent agent guide

this file governs ai coding agents working in this repository. read it before
acting.

## required reading

1. `README.md` for the operational front door and authority map.
2. `STATUS.md` for the last verified checkpoint.
3. `CONTRACTS.md` before changing domain behavior, persisted state, publication,
   submissions, or privacy boundaries.
4. the active accepted spec under `specs/` when one exists.
5. `PROJECT_BRIEF.md` for generative or scope-changing work.
6. `DECISIONS.md` when a choice may already have accepted rationale.

if documents disagree, identify which document owns the disputed fact. do not
infer authority from the filename alone. surface unresolved conflicts before
editing.

## authorization

- discussion, brainstorming, critique, grounding, planning, resume, and audit
  requests are read-only unless the user explicitly requests changes;
- a direct bounded change request authorizes that change;
- new-project, substantial-feature, ambiguous, or cross-cutting implementation
  requires an explicitly accepted spec;
- agents may draft specs, but only the owner may mark them accepted;
- never change a normative contract merely to make implementation agree;
- never commit, push, deploy, publish, change repository visibility, or call a
  paid or live external service without explicit authorization.

## project boundaries

- this repository owns both the private avalonia gui and the public web ui, plus
  the domain and publication seams they intentionally share;
- private research data is canonical but must never be reachable by the public
  runtime or included in a public artifact;
- public output is allowlisted. never implement publication as "everything not
  marked private";
- private properties, notes, sources, and constellations remain private unless
  an explicit publication action selects a safe claim and its public evidence;
- absence of a property attestation means unknown unless a recorded complete
  coverage claim establishes that the integer was evaluated;
- public submissions are untrusted suggestions delivered for human review.
  they never mutate the canonical corpus automatically;
- other resonance repositories are visual and architectural references, not
  runtime dependencies, unless an accepted spec establishes an interface;
- use fictional fixtures only. do not commit private research data, personal
  identifiers, source exports, credentials, or `.env` contents;
- dependencies, schemas, migrations, public interfaces, and deployment paths
  require an accepted slice or explicit bounded authorization.

## current validation

the repository has no native build or test suite yet. run:

```text
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --review-claims
git diff --check
```

replace this section with project-owned commands when the first implementation
slice establishes them.

## working protocol

before changing files:

1. read the governing documents for the task;
2. inspect version-control status and preserve unrelated work;
3. state the authorized slice, risk floor, rollback shape, and likely proof;
4. distinguish confirmed decisions from proposals and unresolved questions.

during implementation:

1. work by invariant rather than accumulating unrelated changes;
2. keep mathematical and epistemic truth outside presentation components;
3. keep private storage, publication projection, and public rendering as
   separately testable representations;
4. add proof at the layer that owns the claim;
5. surface contract conflicts, undeclared scope, and uncertain facts promptly.

before completion:

1. revisit the accepted spec's change-shape matrix when applicable;
2. reconcile code, persisted state, public projection, visible behavior, tests,
   and affected documents;
3. update `STATUS.md` at a meaningful new checkpoint;
4. record consequential decisions without inventing retrospective rationale;
5. run focused proof, complete project validation, the documentation checker,
   and direct diff review;
6. report owner-visible, external, privacy, and deployment proof separately
   from local automated proof.

## stop and ask

- a normative contract must change;
- a framework, database, schema, dependency, or deployment choice lacks an
  accepted spec or explicit bounded authorization;
- private material may cross into tracked source or a public artifact;
- persisted data, migrations, public submission handling, or public interfaces
  expand beyond the accepted slice;
- the task has multiple materially different interpretations;
- verification requires destructive, privileged, external, or costly action;
- the requested change exceeds the authorized slice.
