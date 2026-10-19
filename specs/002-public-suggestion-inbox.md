# spec: public suggestion inbox — the integer tip line

status: accepted

> **continuity role:** accepted transition
> **owner:** repository owner
> **created:** 2026-10-19
> **accepted:** 2026-10-19
> **supersedes:** none

accepted by the repository owner on 2026-10-19. this spec authorizes the
bounded repository implementation described below. a formspree form id remains
required only to activate external delivery in a configured build.

## context

polyvalent already presents a read-only public release, but a visitor who knows
some numerical gossip has no way to pass it to the researcher. accepted
decision `d-005` calls for a lightweight public form whose only destination is
the owner's inbox. contract `submission-001` forbids that correspondence from
mutating the canonical corpus.

the site is static github pages. reliable delivery therefore requires a narrow
external relay; a browser cannot itself send mail. this slice proposes
formspree because its form-id endpoint keeps the recipient address out of the
public bundle, supports static sites and asynchronous submissions, and offers
spam filtering, a `_gotcha` honeypot, and domain restriction without adding a
polyvalent backend.

formspree stores submitted fields in its dashboard as well as forwarding email.
the public form must disclose that third-party transit plainly. the recipient,
retention choices, domain restriction, and abuse controls are provider-side
configuration and therefore require owner-visible proof rather than inference
from this repository.

## desired transition

current:

```text
visitor -> immutable public release -> exploration only
```

proposed:

```text
visitor
  -> bounded suggestion form
  -> formspree form-id endpoint
  -> owner-verified email inbox
  -> human review

human review -/-> automatic corpus, claim, profile, source, or publication write
```

## scope

in:

- one visually integrated `numerical gossip / tip line` plate on the public
  home page, reachable by a clear link from the site footer;
- one required suggestion-kind choice: `property`, `relation`, `profile lore`,
  `correction`, or `other`;
- one optional integer field accepting a canonical signed decimal integer;
- one required plain-text proposal field, limited to `2000` characters;
- one optional source url, limited to `500` characters and accepted only when
  it parses as `http:` or `https:`;
- optional submitter name and reply email fields;
- one optional, unchecked-by-default consent checkbox allowing public credit
  if the suggestion is later used;
- a hidden `_gotcha` honeypot;
- asynchronous delivery through exactly one formspree form-id endpoint;
- honest submitting, delivered, validation-failed, unavailable, and
  provider-failed states announced accessibly without navigating away;
- a plain disclosure that the message is sent through formspree, may be stored
  there according to the provider account's retention settings, reaches the
  owner's email for human review, and does not enter the corpus automatically;
- a build-time `VITE_PUBLIC_SUGGESTION_FORM_ID` whose value is a formspree form
  id, not a secret or recipient address;
- a deployment workflow mapping from the github repository variable
  `PUBLIC_SUGGESTION_FORM_ID` to that build-time variable;
- fail-closed rendering: when the id is absent or malformed, the plate remains
  visible but its fields and submit action are unavailable, with no network
  attempt;
- focused component, validation, endpoint-boundary, build-configuration, and
  accessibility proof, plus one separately authorized live delivery check.

out:

- any api, serverless function, database table, queue, webhook, or moderation
  dashboard owned by polyvalent;
- direct submission to sqlite, a public artifact, github, or the private gui;
- attachments, html, markdown rendering, executable links, rich text, or file
  uploads;
- accounts, submission status, autoresponses, mailing-list use, analytics, or
  visitor persistence;
- a formspree cli project, deploy key, api key, repository secret, or `.env`
  file;
- automatic source retrieval, citation creation, property creation, claim
  acceptance, publication staging, or public credit;
- treating browser validation, a honeypot, or referer-based domain restriction
  as a security boundary;
- committing the recipient email address or a provider credential to the
  repository;
- sending a real message, changing provider configuration, committing,
  pushing, or deploying without separate authorization.

## provider and configuration boundary

the owner creates one form in the formspree dashboard and configures its target
as the requested inbox. only the resulting public form id enters github as the
repository variable `PUBLIC_SUGGESTION_FORM_ID`. the pages workflow exposes it
to vite solely while building `web/dist`.

the web runtime constructs only:

```text
https://formspree.io/f/{validated-form-id}
```

the form id must match a deliberately narrow ascii identifier grammar before
submission. arbitrary urls, protocols, hosts, headers, and credentials are not
configuration. the public-boundary check must fail if another external fetch
target or runtime dependency appears.

recommended provider settings:

- verify the owner-controlled recipient address;
- restrict submissions to `resonatrix.tech` while recognizing that the
  control depends on the browser referer header;
- keep the provider's default spam filtering and recaptcha behavior enabled;
- review the formspree submission-retention setting before activation;
- do not enable integrations, autoresponses, uploads, api access, or webhooks.

local development and pull-request builds do not require a live id. tests use
a fake request boundary and make no external call. a missing id must build
successfully into the explicit unavailable state.

## input and rendering rules

- all submitted values are inert plain text. the public site never renders a
  submitted value after sending it;
- the integer field is checked with the existing canonical-integer parser when
  present;
- whitespace-only proposals are rejected, and values are trimmed before
  delivery;
- optional email validation is browser-side usability only; formspree owns
  provider-side acceptance;
- source urls are sent as text and never fetched, previewed, or followed by
  the submission component;
- credit consent is an explicit boolean distinct from name or reply email;
- the submit button is disabled while one request is in flight to prevent
  accidental duplicates;
- a success message means only that formspree accepted the request. it does not
  claim email arrival, review, acceptance, publication, or credit;
- provider error bodies are not injected into the page. the site maps known
  validation failures to local copy and otherwise shows a bounded generic
  failure state;
- no input is logged, cached, written to storage, added to the url, or retained
  after a confirmed successful submission.

## interface treatment

the plate belongs after `the etiquette of the thing`, so the form arrives as a
third register rather than interrupting the first encounter. its language may
be playful—`the integers have gossip` and `leave a receipt`—while the privacy
and review copy remains literal.

the plate should share the existing etched frame, faint teal depth, cream
display face, and vivid orange/cyan/violet accents. it must preserve visible
labels, keyboard order, focus treatment, narrow-screen legibility, reduced
motion, and an `aria-live` status region. native form controls retain their
semantic roles.

## operational path

activation:

1. owner creates a formspree account/form and verifies the intended recipient;
2. owner applies the recommended provider settings and supplies the public
   form id;
3. implementation is reviewed and the form id is added as github repository
   variable `PUBLIC_SUGGESTION_FORM_ID`;
4. an authorized deployment builds the static site with that variable;
5. owner submits one clearly labeled live canary and confirms both formspree
   acceptance and inbox delivery without exposing the received contents;
6. owner deletes the canary from provider storage if desired.

rollback:

- immediate: remove or blank the github repository variable and redeploy; the
  form becomes visibly unavailable and sends nothing;
- complete: revert the bounded component, styles, tests, boundary allowance,
  workflow mapping, and operational documentation;
- provider: disable or delete the form in formspree separately. repository
  rollback does not prove provider deactivation, and provider deactivation does
  not alter repository history.

## change shape

| dimension | changed? | source of truth | proof |
|---|---:|---|---|
| user-visible behavior | yes | public react form component | component tests, keyboard/viewport browser smoke, owner review |
| persisted state or schema | no | scope boundary | no sqlite, artifact-schema, or visitor-storage change |
| external or provider-visible behavior | yes | formspree dashboard plus form-id endpoint | fake-boundary integration test, provider-settings checklist, separately authorized canary |
| identifiers, secrets or privacy | yes | validated public form id and disclosure copy | malformed-id tests, no-recipient/no-secret scan, built-artifact inspection |
| lifecycle or terminal states | yes | local form state machine | idle/submitting/succeeded/failed/unavailable tests |
| setup, operation or recovery | yes | `docs/development.md` | cold build without id, configured build, rollback rehearsal |
| normative contract or decision | no | `submission-001` and `d-005` already govern | contract trace and documentation check |
| cross-repository dependency | no | repository dependency graph | full build without another checkout |

## acceptance criteria

implementation may be called complete only when:

1. an unconfigured checkout builds and renders a truthful unavailable state
   without making an external request;
2. a configured build can submit only to the validated formspree form-id host
   and path;
3. required, length, canonical-integer, and source-url rules have focused tests;
4. duplicate submission is blocked while a request is in flight;
5. success and failure are announced to assistive technology and never overstate
   delivery or acceptance;
6. the built artifact contains neither the recipient address, credentials,
   private data, nor a general-purpose external endpoint;
7. provider-bound data, retention, human-review, and no-auto-ingestion language
   is visible beside the form;
8. the project-owned full check and documentation check pass;
9. desktop and narrow browser renders are inspected, including keyboard focus
   and provider-unavailable behavior;
10. an independently authorized live canary is accepted by formspree and
    confirmed in the intended inbox;
11. `README.md`, `STATUS.md`, `docs/architecture.md`, and
    `docs/development.md` describe the effective state without implying that
    local tests prove provider configuration or inbox arrival.

## authorization boundary

this accepted spec authorizes the bounded repository implementation and the
formspree integration described above. it does not authorize codex to create an
account, read email, configure the external dashboard, send a live canary,
commit, push, or deploy.
