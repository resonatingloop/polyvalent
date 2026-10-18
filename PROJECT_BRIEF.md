# polyvalent project brief

> **continuity role:** durable intent
> **status:** current
> **last verified:** 2026-10-17
> **owns:** purpose, users, scope, non-goals, constraints, and success
> **update when:** project direction changes, not when implementation merely advances

## purpose

polyvalent is a research instrument for encountering integers through their
many mathematical, historical, and privately interpreted relations. it should
support serious local research without forcing that research process into the
shape of the public experience.

the repository contains two surfaces over one canonical corpus:

- a private avalonia gui for authoring, sourcing, annotation, curation, and
  investigation;
- a public web ui where a visitor enters an integer, meets its public profile,
  and follows explained relations to other integers.

## intended users and consumers

- the owner, conducting private number research and curating publication;
- public visitors exploring interesting integers without an account or prior
  knowledge of the owner's private research context;
- occasional contributors proposing a property or relation for human review;
- future agents maintaining the corpus, evaluators, publication boundary, and
  two user interfaces.

## goals

- begin with a contained corpus of integers `1` through `2000`;
- populate a bounded initial property set reproducibly rather than through
  incomplete manual tagging;
- let a property state what a number is and open a path to other numbers that
  share it;
- preserve the witness, qualifier, source, coverage, and epistemic register of
  a claim rather than flattening them into a boolean tag;
- support private authored constellations among several integers without
  presenting them as exhaustive mathematical classes;
- make the private gui a dense rolodex and research record with collapsible
  connections, not a graph or social profile;
- make the public web ui a legible, playful social network for integers with
  profile cards, explained edges, list alternatives, and accessible graph
  exploration;
- accept lightweight public suggestions by email for manual analysis;
- keep public output intentionally derived from the private corpus.

## non-goals

- one shared interaction design stretched across desktop and web;
- a graph-based private research gui in the first slice;
- public accounts, followers, messaging, crowdsourced editing, or contributor
  dashboards;
- automatic submission-to-database ingestion;
- treating an absent property row as proof that a number lacks the property;
- treating every interpretive, theological, or personal relation as a public
  mathematical fact;
- exhaustive coverage of all integers or every interesting number property in
  the first slice;
- runtime dependence on another resonance repository or a private corpus.

## conceptual model

- an **integer** is the stable object around which records and views gather;
- a **property definition** names a reusable way an integer may qualify;
- an **attestation** records that a particular integer satisfies a property,
  together with qualifiers, witnesses, sources, and publication state;
- a **coverage claim** records which integer range was completely evaluated by
  a particular computation or bounded import;
- a **constellation** is an authored, potentially many-member relation whose
  meaning is carried by its account, member roles, and provenance rather than
  by an exhaustive predicate;
- a **public projection** is the explicitly selected subset of claims and safe
  evidence available to the web artifact.

the physical schema is intentionally not chosen here.

## experience and visual lineage

the private gui inherits the resonance plate family through avalonia-native
materials: matte layered plates, compact evidence-bearing controls, serif
display type, mono operational type, restrained ember/iris accents, and a
research object occupying the center of the field.

the public web ui shares family resemblance with resonatrix and upward:
cartographic darkness, unequal visual gravity, precise luminous traces, and
motion used for discovery. it remains its own instrument rather than copying
their components or depending on their repositories.

## repository boundary

this repository owns both applications, the shared domain vocabulary, the
canonical private research boundary, the public projection mechanism, and
their project-owned tests and documentation.

other resonance applications are references or separate instruments. no
private corpus, personal research database, credentials, or source export may
enter tracked fixtures or public artifacts.

## current phase

the project is pre-implementation. the next phase must settle framework,
database ownership, public artifact generation, and the initial property pack
in a draft first-slice spec. only the owner may accept that spec.

## success

- a cold checkout can build and exercise both surfaces through documented
  commands;
- the private gui can inspect and curate an integer record, its evidence, and
  its private connections without exposing them publicly;
- every initial property makes its completeness or incompleteness legible;
- the public site can begin at an integer, render its public properties, and
  explain why it is related to another integer;
- a private-only sentinel fixture is mechanically proven absent from the
  complete public artifact;
- a public suggestion reaches the configured inbox without writing canonical
  data;
- desktop and public visual acceptance is established through rendered,
  owner-visible proof in addition to automated checks.

## constraints

- public publication is explicit and fail-closed;
- private, sourced, computed, interpretive, and personal registers remain
  distinguishable;
- deterministic mathematical logic remains independent of either renderer;
- the two interfaces may share domain truth but not a forced component system;
- the public experience remains usable without graph interaction alone;
- dependencies, providers, deployment, and database migrations require an
  accepted bounded transition;
- private data stays local unless the owner deliberately publishes a safe
  projection.
