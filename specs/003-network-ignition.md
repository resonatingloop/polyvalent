# spec: network ignition — seven evidence-bearing circles

status: draft

> **continuity role:** proposed transition
> **owner:** repository owner
> **created:** 2026-10-20
> **supersedes:** none
> **related decisions:** `d-003`, `d-004`, `d-012`, `d-014`, `d-017`

this draft does not authorize implementation. owner acceptance would authorize
only the bounded repository change described below; it would not authorize a
commit, push, deployment, or mutation of an existing private ledger.

## context

the reference corpus currently publishes ten properties and `728` accepted
positive claims over integers `1–2000`. its public network is already derived
from shared published membership rather than stored pairwise edges, but much of
the mathematical social field is still carried by broad foundational classes.

the next useful corpus slice should make the graph more particular without
weakening its epistemic rules. seven small or strongly overlapping classes give
`1729`, `220`, `2`, `43`, and their neighborhoods markedly richer biographies
while remaining reproducible, source-linked, and completely evaluated inside
the existing range.

the proposed classes are:

- carmichael numbers;
- fermat pseudoprimes to base `2`;
- amicable numbers;
- members of sylvester's sequence;
- primary pseudoperfect numbers;
- heegner numbers;
- integers expressible as a sum of two positive cubes in more than one way,
  presented as two-way taxicab numbers.

this is an ontology and corpus expansion, not a new relation system. properties
remain reusable predicates, profile entries remain edge-silent biographies,
and authored ensembles remain deferred constellations.

## desired transition

current:

```text
seed-v5
  -> 10 property definitions
  -> 728 accepted positive claims
  -> reference-v5 with 529 materialized integer profiles
```

proposed:

```text
seven source-linked property definitions
  -> six complete deterministic evaluations over 1–2000
  -> one source-asserted bounded import over 1–2000
  -> typed claim witnesses and explicit coverage
  -> source comparisons or import provenance
  -> explicit staging
  -> seed-v6
  -> immutable reference-v6
     17 properties / 761 claims / 541 integer profiles / 20 profile entries
```

the twelve newly materialized public integers are `42`, `220`, `284`, `341`,
`645`, `1105`, `1184`, `1210`, `1387`, `1806`, `1807`, and `1905`. every other
new claim enriches an integer already present in `reference-v5`.

## scope

in:

- seven immutable property definitions, aliases, graph modes, evidence
  contracts, validator keys, and evaluator or importer bindings;
- complete membership for the canonical integer range `1–2000`;
- six deterministic evaluators and one bounded heegner import;
- one bounded attributed oeis extract and immutable source capture per
  property;
- comparison runs for every deterministic population against its corresponding
  bounded extract;
- checked qualifiers and witnesses for every positive claim;
- explicit coverage claims using the existing deterministic or
  source-asserted-bounded bases;
- publication staging through the existing allowlist and a regenerated
  deterministic public reference artifact;
- generalized exact-partner display and salience for amicable numbers without
  adding stored pairwise edges;
- public number pages, property dossiers, complete relation lists, and bounded
  graph rendering driven by the existing artifact schema;
- focused mathematical, evidence-contract, population, comparison,
  publication, relation, graph, artifact, privacy, and rendered-browser proof;
- documentation and third-party attribution reconciliation.

out:

- a schema migration or a new relation table;
- automatic mutation or upgrade of an existing `seed-v5` private ledger;
- arbitrary property creation in the desktop;
- scalar-valued facts such as the number of groups of a given order;
- collatz trajectories, recamán visitation, lychrel candidates, or other
  horizon-dependent computations;
- square-triangular as a redundant stored property;
- giuga, weird, vampire, factorion, fermat-prime, wieferich-prime, lucky, ulam,
  happy, self, highly-composite, or refactorable membership;
- new profile lore, direct historical relations, public constellations,
  session-held integers, or thematic lenses;
- live oeis fetching, scheduled synchronization, or a general evaluator plugin
  system;
- private gui redesign;
- commit, push, deployment, or modification of provider or github settings.

## governing contracts and decisions

- `epistemic-001`, `coverage-001`, `provenance-001`, `evidence-001`,
  `history-001`, `relation-001`, `publication-002`, and `publication-003` remain
  unchanged;
- `d-003` keeps these reusable predicates separate from authored
  constellations;
- `d-004` requires a valid complete coverage warrant before an absent claim may
  appear as covered-negative;
- `d-012` requires each accepted claim to satisfy the exact property version's
  typed evidence contract;
- `d-014` keeps true shared membership graphable while bounding only its visual
  presentation and permits checked exact-partner qualifiers to affect salience;
- `d-017` establishes the pattern of local deterministic membership, exact
  witnesses, property-level oeis provenance, fresh-ledger seeding, and no
  automatic rewrite of older ledgers.

no normative contract changes are proposed.

## property vocabulary and population contracts

| slug | display name | population mode | expected positive members in `1–2000` | graph mode | oeis source |
|---|---|---|---|---|---|
| `carmichael-number` | carmichael number | deterministic | `561`, `1105`, `1729` | shared membership | [a002997](https://oeis.org/A002997) |
| `base-2-fermat-pseudoprime` | fermat pseudoprime to base 2 | deterministic | `341`, `561`, `645`, `1105`, `1387`, `1729`, `1905` | shared membership | [a001567](https://oeis.org/A001567) |
| `amicable-number` | amicable number | deterministic | `220`, `284`, `1184`, `1210` | shared membership | [a063990](https://oeis.org/A063990) |
| `sylvester-sequence-member` | sylvester sequence member | deterministic | `2`, `3`, `7`, `43`, `1807` | shared membership | [a000058](https://oeis.org/A000058) |
| `primary-pseudoperfect-number` | primary pseudoperfect number | deterministic | `2`, `6`, `42`, `1806` | shared membership | [a054377](https://oeis.org/A054377) |
| `heegner-number` | heegner number | bounded import | `1`, `2`, `3`, `7`, `11`, `19`, `43`, `67`, `163` | shared membership | [a003173](https://oeis.org/A003173) |
| `two-way-taxicab-number` | two-way taxicab number | deterministic | `1729` | shared membership | [a001235](https://oeis.org/A001235) |

the seeded aliases are `sarrus number` and `poulet number` for the base-2
pseudoprime property, `amicable-pair member` for amicable number, and
`taxi-cab number` for the explicitly defined two-way taxicab property. the
other four definitions begin without aliases. `hardy-ramanujan number` belongs
to `1729` specifically and is not a property alias.

the two-way taxicab definition is deliberately narrower than the overloaded
phrase `taxicab number`: membership means expressibility as a sum of two
positive cubes in at least two distinct unordered ways. `1729` is the sole
member in range because
`1^3 + 12^3 = 9^3 + 10^3 = 1729`; `4104` is the next source term.

the heegner population is imported rather than presented as locally proved.
the capture records the source's complete nine-member class-number-one list,
and the local ledger claims only source-asserted bounded coverage over
`1–2000`. no evaluator is allowed to masquerade a hard-coded list as a proof
of the stark-heegner theorem.

## evidence contracts

every property version requires a version-`1` witness. empty, malformed,
mislabelled, or mathematically inconsistent witnesses are rejected before
acceptance.

### carmichael number

- validator and evaluator use korselt's criterion: the integer is composite
  and squarefree, and `p - 1` divides `n - 1` for every prime divisor `p`;
- witness kind: `korselt_certificate`;
- payload records the complete distinct prime factorization and the exact
  quotient for each `(n - 1) / (p - 1)` divisibility check;
- display text presents the factorization and divisibility certificate;
- no qualifier fields are allowed.

### fermat pseudoprime to base 2

- validator and evaluator require a composite integer `n` satisfying
  `2^(n - 1) ≡ 1 (mod n)`;
- witness kind: `modular_congruence`;
- payload fixes base `2`, exponent `n - 1`, modulus `n`, remainder `1`, and a
  checked nontrivial factorization proving compositeness;
- display text presents the factorization and congruence;
- no qualifier fields are allowed.

### amicable number

- evaluator computes the exact sum of positive proper divisors;
- qualifier contract permits exactly one canonical integer-text field,
  `partner`;
- witness kind: `amicable_pair`;
- payload records the partner and both proper-divisor sums;
- validator requires a positive distinct partner, `s(n) = partner`,
  `s(partner) = n`, and exact agreement between witness and qualifier;
- display text presents both aliquot equalities.

amicable membership remains one shared property. it does not create four
stored pairwise edges. public relation ordering generalizes the existing
twin-prime rule so that the exact `partner` named by the center claim sorts
ahead of other integers that merely share amicable membership.

### sylvester sequence member

- evaluator begins with `a(0) = 2` and applies
  `a(n + 1) = a(n)^2 - a(n) + 1` until it exceeds the range;
- witness kind: `recurrence_term`;
- payload records the zero-based sequence index, preceding term when present,
  result, and recurrence expression;
- validator regenerates through the claimed index and checks every field;
- display text presents the exact indexed recurrence identity;
- no qualifier fields are allowed.

### primary pseudoperfect number

- evaluator factors `n > 1` and checks exactly
  `1/n + sum(1/p) = 1` over the distinct prime divisors `p` of `n`;
- witness kind: `reciprocal_identity`;
- payload records the complete prime-divisor set and exact rational terms;
- validator recomputes the factorization and identity using integer or exact
  rational arithmetic, never floating point;
- display text presents the complete reciprocal identity;
- no qualifier fields are allowed.

### heegner number

- population mode is `bounded-import`, with no deterministic evaluator key;
- witness kind: `source_sequence_term`;
- payload records accession `a003173`, one-based source index, and integer
  value;
- validator checks payload shape, accession, canonical value equality, and the
  exact value/index pair against one immutable source-derived nine-term
  registry; the bounded importer consumes the captured list, and focused proof
  requires the registry and capture to agree exactly;
- each claim requires a public-safe citation to the captured source;
- coverage kind is `source-asserted-bounded`, not deterministic;
- no qualifier fields are allowed.

### two-way taxicab number

- evaluator enumerates unordered pairs of positive cubes whose sum lies in the
  requested range and keeps values with at least two distinct pairs;
- witness kind: `cube_sum_representations`;
- payload records every distinct normalized pair found for the value;
- validator requires positive bases, pair uniqueness, at least two pairs, and
  exact equality of every cube sum to the claimed integer;
- display text presents the complete equality;
- no qualifier fields are allowed.

## sources, captures, and comparison behavior

seven new source identities and seven immutable bounded captures enter the
ledger. tracked json extracts live under `data/source-extracts/schema-v1/`,
carry retrieval time, range metadata, accession, ordered values, and the first
source term above `2000` where the sequence continues. attribution and source
terms are recorded in `THIRD_PARTY_NOTICES.md`.

the six deterministic populations are compared with their corresponding
captures. a mismatch is preserved as a terminal comparison record and withholds
coverage, property staging, claim staging, and publication for that property;
no side silently wins. the heegner capture instead supplies the bounded import
itself and therefore follows the existing source-asserted import path.

an oeis page identifies and contextualizes the property. deterministic
membership remains locally computed, and a citation alone never establishes
coverage.

## relation and presentation behavior

the public artifact schema does not change. relation derivation continues to
intersect graphable published claims.

observable consequences include:

- `1729` retains its existing ramanujan taxi profile and gains carmichael,
  base-2 pseudoprime, and two-way taxicab claims;
- `1729`, `561`, and `1105` relate through both carmichael and base-2
  pseudoprime membership;
- `1729` additionally relates to `341`, `645`, `1387`, and `1905` through the
  base-2 pseudoprime property;
- `220` relates to all three other amicable members, but exact partner `284`
  receives salience before ambient members `1184` and `1210`; the reciprocal
  ordering holds for every amicable member;
- `2` gains sylvester, primary pseudoperfect, and heegner membership atop its
  existing affiliations;
- `43` gains sylvester and heegner membership atop prime and twin-prime;
- `163` gains heegner membership without merging or converting its existing
  sourced profile lore;
- `1806` and `1807` do not relate merely because their source sequences have a
  notable adjacent construction;
- a property with one in-range member, such as two-way taxicab, renders a valid
  dossier and claim while producing no invented mutual.

qualifier rendering adds `partner {n}` for amicable claims. trace output shows
that qualifier and both endpoint witnesses through the existing accessible
readout. graph limits, complete-list behavior, keyboard semantics, and
reduced-motion behavior remain unchanged.

## persistence and release behavior

- no sql migration is added;
- a fresh `seed-v6` ledger contains `2000` integers, `17` property versions,
  `761` accepted positive claims, complete warranted coverage, the existing
  twenty profile entries, and the new source/capture/comparison records;
- `seed-v6` is idempotent only on an unchanged matching ledger;
- an older or divergent ledger is refused rather than rewritten;
- migration or reconciliation of a real authored ledger remains a separate
  explicit revision workflow;
- `reference-v6` is generated through the existing publisher and replaces the
  tracked deterministic fixture only after byte comparison and review;
- publication generation remains local, atomic, immutable, and distinct from
  commit or deployment.

## likely files and components

- `src/Polyvalent.Domain/KnowledgeModels.cs` for stable property slugs;
- `src/Polyvalent.Domain/InitialPropertyCatalog.cs` for definitions, contracts,
  qualifiers, and validator routing;
- `src/Polyvalent.Domain/PropertyEvaluators.cs` for deterministic mathematics;
- `src/Polyvalent.Data/CorpusSeeder.cs` for seed versioning, bounded import,
  captures, comparisons, citations, population, coverage, and staging;
- `data/source-extracts/schema-v1/*.json` and `THIRD_PARTY_NOTICES.md` for bounded public source
  material and attribution;
- domain, data, publication, and desktop tests under `tests/`;
- `web/src/relations.ts`, graph-trace logic, and focused web tests for partner
  labels, salience, complete reasons, and the regenerated artifact;
- `web/public/data/manifest.json` and `web/public/data/data.json` for the
  reviewed `reference-v6` fixture;
- `README.md`, `STATUS.md`, `docs/architecture.md`, `docs/development.md`, and
  `DECISIONS.md` for the implemented checkpoint.

## implementation by invariant

1. establish vocabulary and evidence: add the seven definitions, exact witness
   schemas, validator routes, and rejection tests before producing claims.
2. establish reproducible mathematics: implement the six deterministic
   evaluators with exact arithmetic, expected-member tests, and boundary cases.
3. establish source accountability: add seven bounded extracts and captures,
   run six comparisons, import the heegner list with source-bound witnesses,
   and prove mismatch withholding.
4. establish immutable corpus state: produce `seed-v6`, verify exact counts and
   idempotence, refuse prior/divergent ledgers, and stage only warranted exact
   revisions and citations.
5. establish honest social behavior: generalize exact-partner salience, prove
   complete shared reasons and lists, and prove that adjacency, lore, and
   singleton membership manufacture no edge.
6. establish the public projection: generate `reference-v6`, verify its digest,
   schema, exact counts, source links, witness text, and privacy exclusions,
   then exercise representative pages and graph traces at desktop and narrow
   widths.
7. establish continuity: reconcile attribution, corpus inventories,
   architecture, operation, recovery, decisions, and status, then run the full
   project and documentation checks.

## change shape

| dimension | changed? | source of truth | proof |
|---|---:|---|---|
| user-visible behavior | yes | public release plus relation and graph renderers | relation tests, graph tests, desktop/narrow browser smoke, owner review |
| persisted state or schema | yes, state only | `seed-v6`, existing ledger schema | exact database counts, idempotence, prior-ledger refusal, migration inventory |
| external or provider-visible behavior | no | public runtime boundary | no new runtime fetch, endpoint, provider, or deployment step |
| identifiers, secrets or privacy | yes, public identifiers only | property slugs, source ids, public artifact | slug/source assertions, sentinel scan, built-boundary scan |
| lifecycle or terminal states | no | existing run, revision, staging, and release state machines | transition and immutability regression suite |
| setup, operation or recovery | yes | `README.md` and `docs/development.md` | fresh bootstrap, deterministic fixture reproduction, old-ledger refusal path |
| normative contract or decision | no | existing contracts and `d-003`, `d-004`, `d-012`, `d-014`, `d-017` | contract trace and documentation review |
| cross-repository dependency | no | solution and web dependency boundaries | cold full check without another checkout |

## acceptance criteria

implementation may be called complete only when:

1. every new property has one accepted version, exact stable slug, graph mode,
   validator key, population binding, oeis citation, and immutable capture;
2. the exact seven member sets above are reproduced for `1–2000`;
3. all `33` new claims carry contract-valid witnesses, and amicable claims
   carry exact reciprocal `partner` qualifiers;
4. malformed factors, false congruences, wrong sequence indices, inexact
   reciprocal identities, false partners, duplicate cube pairs, and source
   value/index mismatches are rejected by focused tests;
5. all six deterministic source comparisons match; an injected mismatch for a
   representative new property is preserved and prevents coverage and staging;
6. heegner membership and coverage remain visibly source-asserted rather than
   deterministic;
7. `seed-v6` produces exactly `761` accepted claims and remains idempotent on a
   matching fresh ledger while refusing to rewrite `seed-v5` or divergent
   state;
8. `reference-v6` contains exactly `17` properties, `761` claims, `541`
   materialized integer profiles, and `20` profile entries with a matching
   manifest digest;
9. `1729` exposes three new mathematical claims, its existing profile, and the
   complete double-reason relations to `561` and `1105`;
10. `220` ranks `284` before ambient amicable members, labels the exact partner,
    and retains a complete textual list independent of the graph;
11. no relationship is returned between `1806` and `1807`, and the two-way
    taxicab singleton creates no self-edge or invented mutual;
12. profile entries remain edge-silent, existing citations and witnesses remain
    byte-semantically intact, and private sentinels remain absent from the
    complete public artifact and built web output;
13. representative `/integer/1729`, `/integer/220`, `/integer/2`,
    `/integer/163`, and new property pages are inspected at desktop and narrow
    widths, including pointer and keyboard trace behavior;
14. the project-owned full check and documentation continuity check pass;
15. documentation distinguishes implemented local proof, owner-visible browser
    review, committed artifact, deployed site, and any unverified external
    state.

## validation expected after implementation

focused proof:

```text
dotnet test tests/Polyvalent.Domain.Tests/Polyvalent.Domain.Tests.csproj -c Release
dotnet test tests/Polyvalent.Data.Tests/Polyvalent.Data.Tests.csproj -c Release
dotnet test tests/Polyvalent.Publication.Tests/Polyvalent.Publication.Tests.csproj -c Release
npm --prefix web test
```

deterministic fixture proof:

```text
dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj -c Release -- bootstrap --db /tmp/polyvalent-reference-v6.sqlite3 --output /tmp/polyvalent-reference-v6 --release-id reference-v6 --generated-at 2026-10-20T00:00:00Z
diff -ru web/public/data /tmp/polyvalent-reference-v6
```

complete proof:

```text
./scripts/check.sh
python3 ~/.codex/skills/manage-project-docs/scripts/check_docset.py . --require PROJECT_BRIEF.md --require CONTRACTS.md --require DECISIONS.md --require SCHEMA.md --review-claims
git diff --check
direct review of the complete diff, bounded source extracts, and generated public artifact
```

rendered browser inspection is additional proof; it is not inferred from the
automated suite. deployment remains separately authorized and verified.

## rollback

- before release generation: revert the bounded domain, seed, source, test,
  web, and documentation diff;
- after local generation: remove the untracked preview and fresh `seed-v6`
  ledger, or restore the previously reviewed tracked `reference-v5` files;
- after commit but before deployment: revert the cohort commit and rebuild the
  prior tracked artifact;
- deployment rollback, if later authorized, selects a previously reviewed
  immutable release through the existing pages workflow;
- no rollback path edits an existing private ledger or deletes authored data.

## risks and unknowns

- amicable partner salience currently has only the twin-prime precedent; the
  implementation must generalize exact-partner behavior without turning every
  populated string qualifier into a relation;
- carmichael and primary-pseudoperfect witnesses require complete, exact
  factorization inside the supported range; partial factors must never pass;
- heegner membership has a theorem-sized warrant that this slice does not
  re-prove. its source-asserted import semantics must remain visible;
- `taxicab number` is overloaded in the literature. public copy and identifiers
  must retain the narrower two-way definition used by a001235;
- adding seven comparison captures increases hard-coded seeder surface. a
  general source synchronization or plugin system remains intentionally out of
  scope;
- richer affiliations can reorder bounded graph selections around highly
  connected integers; complete textual relations and rendered salience require
  direct regression review;
- the existing operational checkpoint contains deployment claims that may need
  separate current-state reconciliation; this slice must not infer live state
  from repository history.

## open questions

- none inside the proposed slice. accepting this spec accepts the seven exact
  properties, names, member sets, evidence contracts, source strategy,
  `seed-v6`/`reference-v6` transition, and listed deferrals.

## authorization boundary

if the repository owner marks this spec accepted, it authorizes the bounded
local implementation and verification above. it does not authorize codex to
commit, push, deploy, contact a provider, overwrite an existing ledger, or
promote the generated release outside the reviewed repository transition.
