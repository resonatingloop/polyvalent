# polyvalent project brief

> **continuity role:** durable intent
> **status:** current
> **last verified:** 2026-10-19
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
- give each integer sourced, revisioned profile entries for mathematical,
  scientific, historical, or cultural lore that does not honestly form a
  graph edge;
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
- a **property version** names a reusable way an integer may qualify and the
  typed evidence contract by which that qualification can be accepted;
- a **claim revision** records that a particular integer satisfies a property,
  together with its qualifier, witness, provenance, review state, and lineage;
- a **source** identifies an intellectual or published work, a **capture** fixes
  a retrieved instance, and a **citation** records how particular evidence uses
  it;
- an **evaluation or import run** records the inputs, versions, outcome, and
  claims produced by a reproducible population action;
- a **coverage claim** records which integer range was completely evaluated by
  a particular computation or bounded import;
- a **profile entry** is sourced, revisioned contextual lore about one integer
  and does not create a relation by default;
- a **constellation** is an authored, potentially many-member relation whose
  meaning is carried by its account, member roles, and provenance rather than
  by an exhaustive predicate;
- a **public projection** is the explicitly selected subset of claims and safe
  evidence available to the web artifact.

sqlite is the accepted canonical local store. exact implemented tables and
migrations belong to the accepted first-slice spec and the data project rather
than this durable brief.

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

the accepted first slice is implemented locally: a .net domain and sqlite
ledger, avalonia research rolodex, explicit static publisher, deterministic
reference corpus, and react public explorer now form one tested vertical path.
[`specs/001-first-slice.md`](specs/001-first-slice.md) remains active until its
final owner-visible browser and native desktop acceptance is recorded.

the public visual pass is owner-accepted and a bounded github pages delivery
workflow now carries only the reviewed static web artifact. live deployment is
a separate external transition. suggestion delivery, session-held integers,
lenses, and private constellations remain later slices rather than hidden
extensions of this one.

## future apertures

- session-held integers that can recognize and announce a newly encountered
  relation during one visitor's exploration;
- optional thematic lenses, such as an occult lens, that surface selected
  public significance without fabricating edges among every marked integer;
- private constellations and direct authored integer relations once their
  authoring and publication boundaries receive a dedicated slice.

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
