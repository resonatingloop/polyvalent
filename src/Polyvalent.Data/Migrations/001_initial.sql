CREATE TABLE integers (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    canonical_value TEXT NOT NULL UNIQUE,
    CHECK (
        canonical_value = '0'
        OR (
            substr(canonical_value, 1, 1) GLOB '[1-9]'
            AND canonical_value NOT GLOB '*[^0-9]*'
        )
        OR (
            substr(canonical_value, 1, 1) = '-'
            AND substr(canonical_value, 2, 1) GLOB '[1-9]'
            AND substr(canonical_value, 2) NOT GLOB '*[^0-9]*'
        )
    )
);

CREATE TABLE properties (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    slug TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL
);

CREATE TABLE property_versions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    property_id INTEGER NOT NULL REFERENCES properties(id),
    version INTEGER NOT NULL CHECK (version > 0),
    status TEXT NOT NULL CHECK (status IN ('draft', 'accepted', 'rejected')),
    definition TEXT NOT NULL,
    aliases_json TEXT NOT NULL CHECK (json_valid(aliases_json)),
    graph_mode TEXT NOT NULL CHECK (graph_mode IN ('none', 'shared-membership')),
    evidence_contract_json TEXT NOT NULL CHECK (json_valid(evidence_contract_json)),
    validator_key TEXT NOT NULL,
    evaluator_key TEXT,
    created_at_utc TEXT NOT NULL,
    accepted_at_utc TEXT,
    UNIQUE (property_id, version)
);

CREATE TABLE evaluators (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    evaluator_key TEXT NOT NULL,
    version TEXT NOT NULL,
    description TEXT NOT NULL,
    UNIQUE (evaluator_key, version)
);

CREATE TABLE population_runs (
    id TEXT PRIMARY KEY,
    property_version_id INTEGER NOT NULL REFERENCES property_versions(id),
    evaluator_id INTEGER REFERENCES evaluators(id),
    mode TEXT NOT NULL CHECK (mode IN ('deterministic', 'bounded-import')),
    range_min TEXT NOT NULL,
    range_max TEXT NOT NULL,
    inputs_json TEXT NOT NULL CHECK (json_valid(inputs_json)),
    tool_version TEXT NOT NULL,
    status TEXT NOT NULL CHECK (status IN ('prepared', 'running', 'succeeded', 'failed', 'cancelled')),
    prepared_at_utc TEXT NOT NULL,
    started_at_utc TEXT,
    completed_at_utc TEXT,
    error_text TEXT
);

CREATE TABLE comparison_runs (
    id TEXT PRIMARY KEY,
    property_version_id INTEGER NOT NULL REFERENCES property_versions(id),
    population_run_id TEXT NOT NULL REFERENCES population_runs(id),
    source_capture_id INTEGER REFERENCES source_captures(id),
    compared_at_utc TEXT NOT NULL,
    local_count INTEGER NOT NULL CHECK (local_count >= 0),
    source_count INTEGER NOT NULL CHECK (source_count >= 0),
    missing_json TEXT NOT NULL CHECK (json_valid(missing_json)),
    unexpected_json TEXT NOT NULL CHECK (json_valid(unexpected_json)),
    status TEXT NOT NULL CHECK (status IN ('matched', 'mismatched'))
);

CREATE TABLE claims (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    integer_id INTEGER NOT NULL REFERENCES integers(id),
    property_id INTEGER NOT NULL REFERENCES properties(id),
    UNIQUE (integer_id, property_id)
);

CREATE TABLE claim_revisions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    claim_id INTEGER NOT NULL REFERENCES claims(id),
    revision INTEGER NOT NULL CHECK (revision > 0),
    property_version_id INTEGER NOT NULL REFERENCES property_versions(id),
    origin_kind TEXT NOT NULL CHECK (origin_kind IN ('manual', 'computed', 'imported')),
    population_run_id TEXT REFERENCES population_runs(id),
    qualifier_json TEXT NOT NULL CHECK (json_valid(qualifier_json)),
    status TEXT NOT NULL CHECK (status IN ('draft', 'accepted', 'withdrawn', 'disputed')),
    private_notes TEXT,
    created_at_utc TEXT NOT NULL,
    accepted_at_utc TEXT,
    supersedes_revision_id INTEGER REFERENCES claim_revisions(id),
    UNIQUE (claim_id, revision)
);

CREATE TABLE claim_witnesses (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    claim_revision_id INTEGER NOT NULL REFERENCES claim_revisions(id),
    kind TEXT NOT NULL,
    schema_version INTEGER NOT NULL CHECK (schema_version > 0),
    payload_json TEXT NOT NULL CHECK (json_valid(payload_json)),
    display_text TEXT NOT NULL,
    UNIQUE (claim_revision_id)
);

CREATE TABLE coverage_claims (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    property_version_id INTEGER NOT NULL REFERENCES property_versions(id),
    population_run_id TEXT NOT NULL REFERENCES population_runs(id),
    range_min TEXT NOT NULL,
    range_max TEXT NOT NULL,
    basis TEXT NOT NULL CHECK (basis IN ('deterministic', 'source-asserted-bounded')),
    exclusions_json TEXT NOT NULL CHECK (json_valid(exclusions_json)),
    created_at_utc TEXT NOT NULL,
    UNIQUE (property_version_id, range_min, range_max, basis)
);

CREATE TABLE sources (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    slug TEXT NOT NULL UNIQUE,
    title TEXT NOT NULL,
    author TEXT,
    canonical_url TEXT NOT NULL,
    license_metadata TEXT,
    private_notes TEXT,
    created_at_utc TEXT NOT NULL
);

CREATE TABLE source_captures (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    source_id INTEGER NOT NULL REFERENCES sources(id),
    uri TEXT NOT NULL,
    retrieved_at_utc TEXT NOT NULL,
    media_type TEXT NOT NULL,
    sha256 TEXT NOT NULL CHECK (length(sha256) = 64),
    revision_metadata TEXT,
    license_metadata TEXT,
    local_private_path TEXT
);

CREATE TABLE property_version_citations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    property_version_id INTEGER NOT NULL REFERENCES property_versions(id),
    source_id INTEGER NOT NULL REFERENCES sources(id),
    source_capture_id INTEGER REFERENCES source_captures(id),
    role TEXT NOT NULL,
    label TEXT NOT NULL,
    locator TEXT,
    public_safe INTEGER NOT NULL CHECK (public_safe IN (0, 1))
);

CREATE TABLE claim_revision_citations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    claim_revision_id INTEGER NOT NULL REFERENCES claim_revisions(id),
    source_id INTEGER NOT NULL REFERENCES sources(id),
    source_capture_id INTEGER REFERENCES source_captures(id),
    role TEXT NOT NULL,
    label TEXT NOT NULL,
    locator TEXT,
    public_safe INTEGER NOT NULL CHECK (public_safe IN (0, 1))
);

CREATE TABLE profile_entries (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    integer_id INTEGER NOT NULL REFERENCES integers(id),
    slug TEXT NOT NULL,
    UNIQUE (integer_id, slug)
);

CREATE TABLE profile_entry_revisions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    profile_entry_id INTEGER NOT NULL REFERENCES profile_entries(id),
    revision INTEGER NOT NULL CHECK (revision > 0),
    status TEXT NOT NULL CHECK (status IN ('draft', 'accepted', 'withdrawn', 'disputed')),
    title TEXT NOT NULL,
    category TEXT NOT NULL,
    epistemic_register TEXT NOT NULL,
    body TEXT NOT NULL,
    private_notes TEXT,
    created_at_utc TEXT NOT NULL,
    accepted_at_utc TEXT,
    supersedes_revision_id INTEGER REFERENCES profile_entry_revisions(id),
    UNIQUE (profile_entry_id, revision)
);

CREATE TABLE profile_entry_revision_citations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    profile_entry_revision_id INTEGER NOT NULL REFERENCES profile_entry_revisions(id),
    source_id INTEGER NOT NULL REFERENCES sources(id),
    source_capture_id INTEGER REFERENCES source_captures(id),
    role TEXT NOT NULL,
    label TEXT NOT NULL,
    locator TEXT,
    public_safe INTEGER NOT NULL CHECK (public_safe IN (0, 1))
);

CREATE TABLE publication_property_selections (
    property_version_id INTEGER PRIMARY KEY REFERENCES property_versions(id),
    selected_at_utc TEXT NOT NULL
);

CREATE TABLE publication_property_citation_selections (
    property_version_id INTEGER NOT NULL REFERENCES publication_property_selections(property_version_id) ON DELETE CASCADE,
    citation_id INTEGER NOT NULL REFERENCES property_version_citations(id),
    PRIMARY KEY (property_version_id, citation_id)
);

CREATE TABLE publication_claim_selections (
    claim_revision_id INTEGER PRIMARY KEY REFERENCES claim_revisions(id),
    selected_at_utc TEXT NOT NULL
);

CREATE TABLE publication_claim_citation_selections (
    claim_revision_id INTEGER NOT NULL REFERENCES publication_claim_selections(claim_revision_id) ON DELETE CASCADE,
    citation_id INTEGER NOT NULL REFERENCES claim_revision_citations(id),
    PRIMARY KEY (claim_revision_id, citation_id)
);

CREATE TABLE publication_profile_selections (
    profile_entry_revision_id INTEGER PRIMARY KEY REFERENCES profile_entry_revisions(id),
    selected_at_utc TEXT NOT NULL
);

CREATE TABLE publication_profile_citation_selections (
    profile_entry_revision_id INTEGER NOT NULL REFERENCES publication_profile_selections(profile_entry_revision_id) ON DELETE CASCADE,
    citation_id INTEGER NOT NULL REFERENCES profile_entry_revision_citations(id),
    PRIMARY KEY (profile_entry_revision_id, citation_id)
);

CREATE TABLE privacy_sentinels (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    value TEXT NOT NULL UNIQUE CHECK (length(value) >= 8),
    label TEXT NOT NULL,
    created_at_utc TEXT NOT NULL
);

CREATE TABLE public_releases (
    id TEXT PRIMARY KEY,
    schema_version TEXT NOT NULL,
    generated_at_utc TEXT NOT NULL,
    source_schema_version INTEGER NOT NULL,
    data_sha256 TEXT NOT NULL CHECK (length(data_sha256) = 64),
    artifact_path TEXT NOT NULL,
    status TEXT NOT NULL CHECK (status IN ('building', 'sealed'))
);

CREATE TABLE public_release_property_items (
    release_id TEXT NOT NULL REFERENCES public_releases(id),
    property_version_id INTEGER NOT NULL REFERENCES property_versions(id),
    PRIMARY KEY (release_id, property_version_id)
);

CREATE TABLE public_release_claim_items (
    release_id TEXT NOT NULL REFERENCES public_releases(id),
    claim_revision_id INTEGER NOT NULL REFERENCES claim_revisions(id),
    PRIMARY KEY (release_id, claim_revision_id)
);

CREATE TABLE public_release_profile_items (
    release_id TEXT NOT NULL REFERENCES public_releases(id),
    profile_entry_revision_id INTEGER NOT NULL REFERENCES profile_entry_revisions(id),
    PRIMARY KEY (release_id, profile_entry_revision_id)
);

CREATE TABLE public_release_property_citation_items (
    release_id TEXT NOT NULL REFERENCES public_releases(id),
    citation_id INTEGER NOT NULL REFERENCES property_version_citations(id),
    PRIMARY KEY (release_id, citation_id)
);

CREATE TABLE public_release_claim_citation_items (
    release_id TEXT NOT NULL REFERENCES public_releases(id),
    citation_id INTEGER NOT NULL REFERENCES claim_revision_citations(id),
    PRIMARY KEY (release_id, citation_id)
);

CREATE TABLE public_release_profile_citation_items (
    release_id TEXT NOT NULL REFERENCES public_releases(id),
    citation_id INTEGER NOT NULL REFERENCES profile_entry_revision_citations(id),
    PRIMARY KEY (release_id, citation_id)
);

CREATE INDEX claim_revisions_claim_status_idx ON claim_revisions(claim_id, status);
CREATE INDEX coverage_property_idx ON coverage_claims(property_version_id);
CREATE INDEX profile_revisions_entry_status_idx ON profile_entry_revisions(profile_entry_id, status);

CREATE TRIGGER property_version_accepted_update_guard
BEFORE UPDATE ON property_versions
WHEN OLD.status = 'accepted'
BEGIN
    SELECT RAISE(ABORT, 'accepted property versions are immutable');
END;

CREATE TRIGGER property_version_accepted_delete_guard
BEFORE DELETE ON property_versions
WHEN OLD.status = 'accepted'
BEGIN
    SELECT RAISE(ABORT, 'accepted property versions are immutable');
END;

CREATE TRIGGER property_citation_insert_guard
BEFORE INSERT ON property_version_citations
WHEN (SELECT status FROM property_versions WHERE id = NEW.property_version_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'citations may be attached only to draft property versions');
END;

CREATE TRIGGER property_citation_update_guard
BEFORE UPDATE ON property_version_citations
WHEN (SELECT status FROM property_versions WHERE id = OLD.property_version_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted property citations are immutable');
END;

CREATE TRIGGER property_citation_delete_guard
BEFORE DELETE ON property_version_citations
WHEN (SELECT status FROM property_versions WHERE id = OLD.property_version_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted property citations are immutable');
END;

CREATE TRIGGER population_run_transition_guard
BEFORE UPDATE ON population_runs
WHEN NOT (
    (OLD.status = 'prepared' AND NEW.status = 'running')
    OR (OLD.status = 'running' AND NEW.status IN ('succeeded', 'failed', 'cancelled'))
)
BEGIN
    SELECT RAISE(ABORT, 'invalid population run transition');
END;

CREATE TRIGGER population_run_delete_guard
BEFORE DELETE ON population_runs
BEGIN
    SELECT RAISE(ABORT, 'population runs are immutable ledger records');
END;

CREATE TRIGGER comparison_run_update_guard
BEFORE UPDATE ON comparison_runs
BEGIN
    SELECT RAISE(ABORT, 'comparison runs are immutable');
END;

CREATE TRIGGER comparison_run_delete_guard
BEFORE DELETE ON comparison_runs
BEGIN
    SELECT RAISE(ABORT, 'comparison runs are immutable');
END;

CREATE TRIGGER claim_revision_transition_guard
BEFORE UPDATE ON claim_revisions
WHEN OLD.status <> 'draft'
    OR NEW.status NOT IN ('draft', 'accepted', 'withdrawn')
BEGIN
    SELECT RAISE(ABORT, 'claim revision is immutable or transition is invalid');
END;

CREATE TRIGGER claim_revision_freeze_guard
BEFORE UPDATE ON claim_revisions
WHEN OLD.status = 'draft'
    AND NEW.status <> 'draft'
    AND (
        OLD.claim_id IS NOT NEW.claim_id
        OR OLD.revision IS NOT NEW.revision
        OR OLD.property_version_id IS NOT NEW.property_version_id
        OR OLD.origin_kind IS NOT NEW.origin_kind
        OR OLD.population_run_id IS NOT NEW.population_run_id
        OR OLD.qualifier_json IS NOT NEW.qualifier_json
        OR OLD.private_notes IS NOT NEW.private_notes
        OR OLD.created_at_utc IS NOT NEW.created_at_utc
        OR OLD.supersedes_revision_id IS NOT NEW.supersedes_revision_id
    )
BEGIN
    SELECT RAISE(ABORT, 'claim content cannot change while leaving draft');
END;

CREATE TRIGGER claim_revision_delete_guard
BEFORE DELETE ON claim_revisions
BEGIN
    SELECT RAISE(ABORT, 'claim revisions are withdrawn or superseded, never deleted');
END;

CREATE TRIGGER claim_witness_insert_guard
BEFORE INSERT ON claim_witnesses
WHEN (SELECT status FROM claim_revisions WHERE id = NEW.claim_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'witnesses may be attached only to draft revisions');
END;

CREATE TRIGGER claim_witness_update_guard
BEFORE UPDATE ON claim_witnesses
WHEN (SELECT status FROM claim_revisions WHERE id = OLD.claim_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted claim witnesses are immutable');
END;

CREATE TRIGGER claim_witness_delete_guard
BEFORE DELETE ON claim_witnesses
WHEN (SELECT status FROM claim_revisions WHERE id = OLD.claim_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted claim witnesses are immutable');
END;

CREATE TRIGGER claim_citation_insert_guard
BEFORE INSERT ON claim_revision_citations
WHEN (SELECT status FROM claim_revisions WHERE id = NEW.claim_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'citations may be attached only to draft claim revisions');
END;

CREATE TRIGGER claim_citation_update_guard
BEFORE UPDATE ON claim_revision_citations
WHEN (SELECT status FROM claim_revisions WHERE id = OLD.claim_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted claim citations are immutable');
END;

CREATE TRIGGER claim_citation_delete_guard
BEFORE DELETE ON claim_revision_citations
WHEN (SELECT status FROM claim_revisions WHERE id = OLD.claim_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted claim citations are immutable');
END;

CREATE TRIGGER coverage_insert_guard
BEFORE INSERT ON coverage_claims
WHEN (SELECT status FROM population_runs WHERE id = NEW.population_run_id) <> 'succeeded'
BEGIN
    SELECT RAISE(ABORT, 'coverage requires a succeeded population run');
END;

CREATE TRIGGER coverage_update_guard
BEFORE UPDATE ON coverage_claims
BEGIN
    SELECT RAISE(ABORT, 'coverage claims are immutable');
END;

CREATE TRIGGER coverage_delete_guard
BEFORE DELETE ON coverage_claims
BEGIN
    SELECT RAISE(ABORT, 'coverage claims are immutable');
END;

CREATE TRIGGER source_capture_update_guard
BEFORE UPDATE ON source_captures
BEGIN
    SELECT RAISE(ABORT, 'source captures are immutable');
END;

CREATE TRIGGER source_capture_delete_guard
BEFORE DELETE ON source_captures
BEGIN
    SELECT RAISE(ABORT, 'source captures are immutable');
END;

CREATE TRIGGER profile_revision_transition_guard
BEFORE UPDATE ON profile_entry_revisions
WHEN OLD.status <> 'draft'
    OR NEW.status NOT IN ('draft', 'accepted', 'withdrawn')
BEGIN
    SELECT RAISE(ABORT, 'profile revision is immutable or transition is invalid');
END;

CREATE TRIGGER profile_revision_freeze_guard
BEFORE UPDATE ON profile_entry_revisions
WHEN OLD.status = 'draft'
    AND NEW.status <> 'draft'
    AND (
        OLD.profile_entry_id IS NOT NEW.profile_entry_id
        OR OLD.revision IS NOT NEW.revision
        OR OLD.title IS NOT NEW.title
        OR OLD.category IS NOT NEW.category
        OR OLD.epistemic_register IS NOT NEW.epistemic_register
        OR OLD.body IS NOT NEW.body
        OR OLD.private_notes IS NOT NEW.private_notes
        OR OLD.created_at_utc IS NOT NEW.created_at_utc
        OR OLD.supersedes_revision_id IS NOT NEW.supersedes_revision_id
    )
BEGIN
    SELECT RAISE(ABORT, 'profile content cannot change while leaving draft');
END;

CREATE TRIGGER profile_revision_delete_guard
BEFORE DELETE ON profile_entry_revisions
BEGIN
    SELECT RAISE(ABORT, 'profile revisions are withdrawn or superseded, never deleted');
END;

CREATE TRIGGER profile_citation_insert_guard
BEFORE INSERT ON profile_entry_revision_citations
WHEN (SELECT status FROM profile_entry_revisions WHERE id = NEW.profile_entry_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'citations may be attached only to draft profile revisions');
END;

CREATE TRIGGER profile_citation_update_guard
BEFORE UPDATE ON profile_entry_revision_citations
WHEN (SELECT status FROM profile_entry_revisions WHERE id = OLD.profile_entry_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted profile citations are immutable');
END;

CREATE TRIGGER profile_citation_delete_guard
BEFORE DELETE ON profile_entry_revision_citations
WHEN (SELECT status FROM profile_entry_revisions WHERE id = OLD.profile_entry_revision_id) <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted profile citations are immutable');
END;

CREATE TRIGGER claim_selection_guard
BEFORE INSERT ON publication_claim_selections
WHEN (SELECT status FROM claim_revisions WHERE id = NEW.claim_revision_id) <> 'accepted'
    OR EXISTS (
        SELECT 1 FROM claim_revisions newer
        WHERE newer.supersedes_revision_id = NEW.claim_revision_id
          AND newer.status = 'accepted'
    )
BEGIN
    SELECT RAISE(ABORT, 'only active accepted claim revisions may be staged');
END;

CREATE TRIGGER profile_selection_guard
BEFORE INSERT ON publication_profile_selections
WHEN (SELECT status FROM profile_entry_revisions WHERE id = NEW.profile_entry_revision_id) <> 'accepted'
    OR EXISTS (
        SELECT 1 FROM profile_entry_revisions newer
        WHERE newer.supersedes_revision_id = NEW.profile_entry_revision_id
          AND newer.status = 'accepted'
    )
BEGIN
    SELECT RAISE(ABORT, 'only active accepted profile revisions may be staged');
END;

CREATE TRIGGER property_selection_guard
BEFORE INSERT ON publication_property_selections
WHEN (SELECT status FROM property_versions WHERE id = NEW.property_version_id) <> 'accepted'
    OR EXISTS (
        SELECT 1 FROM comparison_runs comparison
        WHERE comparison.property_version_id = NEW.property_version_id
          AND comparison.status = 'mismatched'
    )
BEGIN
    SELECT RAISE(ABORT, 'only accepted property versions without comparison disagreements may be staged');
END;

CREATE TRIGGER claim_citation_selection_guard
BEFORE INSERT ON publication_claim_citation_selections
WHEN NOT EXISTS (
    SELECT 1 FROM claim_revision_citations citation
    WHERE citation.id = NEW.citation_id
      AND citation.claim_revision_id = NEW.claim_revision_id
      AND citation.public_safe = 1
)
BEGIN
    SELECT RAISE(ABORT, 'citation is not public-safe or does not belong to the claim');
END;

CREATE TRIGGER profile_citation_selection_guard
BEFORE INSERT ON publication_profile_citation_selections
WHEN NOT EXISTS (
    SELECT 1 FROM profile_entry_revision_citations citation
    WHERE citation.id = NEW.citation_id
      AND citation.profile_entry_revision_id = NEW.profile_entry_revision_id
      AND citation.public_safe = 1
)
BEGIN
    SELECT RAISE(ABORT, 'citation is not public-safe or does not belong to the profile');
END;

CREATE TRIGGER property_citation_selection_guard
BEFORE INSERT ON publication_property_citation_selections
WHEN NOT EXISTS (
    SELECT 1 FROM property_version_citations citation
    WHERE citation.id = NEW.citation_id
      AND citation.property_version_id = NEW.property_version_id
      AND citation.public_safe = 1
)
BEGIN
    SELECT RAISE(ABORT, 'citation is not public-safe or does not belong to the property');
END;

CREATE TRIGGER public_release_transition_guard
BEFORE UPDATE ON public_releases
WHEN OLD.status <> 'building'
    OR NEW.status <> 'sealed'
    OR OLD.id IS NOT NEW.id
    OR OLD.schema_version IS NOT NEW.schema_version
    OR OLD.generated_at_utc IS NOT NEW.generated_at_utc
    OR OLD.source_schema_version IS NOT NEW.source_schema_version
    OR OLD.data_sha256 IS NOT NEW.data_sha256
    OR OLD.artifact_path IS NOT NEW.artifact_path
BEGIN
    SELECT RAISE(ABORT, 'public releases are immutable after sealing');
END;

CREATE TRIGGER public_release_delete_guard
BEFORE DELETE ON public_releases
BEGIN
    SELECT RAISE(ABORT, 'public releases are immutable');
END;

CREATE TRIGGER public_release_property_insert_guard
BEFORE INSERT ON public_release_property_items
WHEN (SELECT status FROM public_releases WHERE id = NEW.release_id) <> 'building'
BEGIN
    SELECT RAISE(ABORT, 'release items may be added only while building');
END;

CREATE TRIGGER public_release_claim_insert_guard
BEFORE INSERT ON public_release_claim_items
WHEN (SELECT status FROM public_releases WHERE id = NEW.release_id) <> 'building'
BEGIN
    SELECT RAISE(ABORT, 'release items may be added only while building');
END;

CREATE TRIGGER public_release_profile_insert_guard
BEFORE INSERT ON public_release_profile_items
WHEN (SELECT status FROM public_releases WHERE id = NEW.release_id) <> 'building'
BEGIN
    SELECT RAISE(ABORT, 'release items may be added only while building');
END;

CREATE TRIGGER public_release_property_citation_insert_guard
BEFORE INSERT ON public_release_property_citation_items
WHEN (SELECT status FROM public_releases WHERE id = NEW.release_id) <> 'building'
    OR NOT EXISTS (
        SELECT 1
        FROM property_version_citations citation
        JOIN public_release_property_items item
          ON item.property_version_id = citation.property_version_id
         AND item.release_id = NEW.release_id
        WHERE citation.id = NEW.citation_id
          AND citation.public_safe = 1)
BEGIN
    SELECT RAISE(ABORT, 'release citation must be public-safe, belong to a release item, and be added while building');
END;

CREATE TRIGGER public_release_claim_citation_insert_guard
BEFORE INSERT ON public_release_claim_citation_items
WHEN (SELECT status FROM public_releases WHERE id = NEW.release_id) <> 'building'
    OR NOT EXISTS (
        SELECT 1
        FROM claim_revision_citations citation
        JOIN public_release_claim_items item
          ON item.claim_revision_id = citation.claim_revision_id
         AND item.release_id = NEW.release_id
        WHERE citation.id = NEW.citation_id
          AND citation.public_safe = 1)
BEGIN
    SELECT RAISE(ABORT, 'release citation must be public-safe, belong to a release item, and be added while building');
END;

CREATE TRIGGER public_release_profile_citation_insert_guard
BEFORE INSERT ON public_release_profile_citation_items
WHEN (SELECT status FROM public_releases WHERE id = NEW.release_id) <> 'building'
    OR NOT EXISTS (
        SELECT 1
        FROM profile_entry_revision_citations citation
        JOIN public_release_profile_items item
          ON item.profile_entry_revision_id = citation.profile_entry_revision_id
         AND item.release_id = NEW.release_id
        WHERE citation.id = NEW.citation_id
          AND citation.public_safe = 1)
BEGIN
    SELECT RAISE(ABORT, 'release citation must be public-safe, belong to a release item, and be added while building');
END;

CREATE TRIGGER public_release_property_mutation_guard
BEFORE UPDATE ON public_release_property_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_property_delete_guard
BEFORE DELETE ON public_release_property_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_claim_mutation_guard
BEFORE UPDATE ON public_release_claim_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_claim_delete_guard
BEFORE DELETE ON public_release_claim_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_profile_mutation_guard
BEFORE UPDATE ON public_release_profile_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_profile_delete_guard
BEFORE DELETE ON public_release_profile_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_property_citation_mutation_guard
BEFORE UPDATE ON public_release_property_citation_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_property_citation_delete_guard
BEFORE DELETE ON public_release_property_citation_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_claim_citation_mutation_guard
BEFORE UPDATE ON public_release_claim_citation_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_claim_citation_delete_guard
BEFORE DELETE ON public_release_claim_citation_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_profile_citation_mutation_guard
BEFORE UPDATE ON public_release_profile_citation_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;

CREATE TRIGGER public_release_profile_citation_delete_guard
BEFORE DELETE ON public_release_profile_citation_items
BEGIN
    SELECT RAISE(ABORT, 'release items are immutable');
END;
