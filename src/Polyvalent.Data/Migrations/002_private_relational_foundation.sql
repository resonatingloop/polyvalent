CREATE TABLE direct_relations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    slug TEXT NOT NULL UNIQUE CHECK (
        length(slug) BETWEEN 1 AND 80
        AND slug = lower(slug)
        AND slug NOT GLOB '*[^a-z0-9-]*'
        AND slug NOT LIKE '-%'
        AND slug NOT LIKE '%-'
        AND slug NOT LIKE '%--%'),
    active_revision_id INTEGER REFERENCES direct_relation_revisions(id),
    created_at_utc TEXT NOT NULL
);

CREATE TABLE direct_relation_revisions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    direct_relation_id INTEGER NOT NULL REFERENCES direct_relations(id),
    revision INTEGER NOT NULL CHECK (revision > 0),
    status TEXT NOT NULL CHECK (status IN ('draft', 'accepted', 'withdrawn')),
    directionality TEXT NOT NULL CHECK (directionality IN ('directed', 'symmetric')),
    proposition TEXT NOT NULL,
    warrant TEXT NOT NULL,
    epistemic_register TEXT NOT NULL,
    private_notes TEXT,
    created_at_utc TEXT NOT NULL,
    accepted_at_utc TEXT,
    supersedes_revision_id INTEGER REFERENCES direct_relation_revisions(id),
    UNIQUE (direct_relation_id, revision)
);

CREATE TABLE direct_relation_participants (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    direct_relation_revision_id INTEGER NOT NULL REFERENCES direct_relation_revisions(id),
    integer_id INTEGER NOT NULL REFERENCES integers(id),
    role TEXT NOT NULL,
    ordinal INTEGER NOT NULL CHECK (ordinal >= 0),
    UNIQUE (direct_relation_revision_id, integer_id),
    UNIQUE (direct_relation_revision_id, ordinal)
);

CREATE TABLE direct_relation_revision_citations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    direct_relation_revision_id INTEGER NOT NULL REFERENCES direct_relation_revisions(id),
    source_id INTEGER NOT NULL REFERENCES sources(id),
    source_capture_id INTEGER REFERENCES source_captures(id),
    role TEXT NOT NULL,
    label TEXT NOT NULL,
    locator TEXT
);

CREATE TABLE constellations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    slug TEXT NOT NULL UNIQUE CHECK (
        length(slug) BETWEEN 1 AND 80
        AND slug = lower(slug)
        AND slug NOT GLOB '*[^a-z0-9-]*'
        AND slug NOT LIKE '-%'
        AND slug NOT LIKE '%-'
        AND slug NOT LIKE '%--%'),
    active_revision_id INTEGER REFERENCES constellation_revisions(id),
    created_at_utc TEXT NOT NULL
);

CREATE TABLE constellation_revisions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    constellation_id INTEGER NOT NULL REFERENCES constellations(id),
    revision INTEGER NOT NULL CHECK (revision > 0),
    status TEXT NOT NULL CHECK (status IN ('draft', 'accepted', 'withdrawn')),
    name TEXT NOT NULL,
    account TEXT NOT NULL,
    epistemic_register TEXT NOT NULL,
    private_notes TEXT,
    created_at_utc TEXT NOT NULL,
    accepted_at_utc TEXT,
    supersedes_revision_id INTEGER REFERENCES constellation_revisions(id),
    UNIQUE (constellation_id, revision)
);

CREATE TABLE constellation_members (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    constellation_revision_id INTEGER NOT NULL REFERENCES constellation_revisions(id),
    integer_id INTEGER NOT NULL REFERENCES integers(id),
    role TEXT NOT NULL,
    member_notes TEXT,
    ordinal INTEGER NOT NULL CHECK (ordinal >= 0),
    UNIQUE (constellation_revision_id, integer_id),
    UNIQUE (constellation_revision_id, ordinal)
);

CREATE TABLE constellation_revision_citations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    constellation_revision_id INTEGER NOT NULL REFERENCES constellation_revisions(id),
    source_id INTEGER NOT NULL REFERENCES sources(id),
    source_capture_id INTEGER REFERENCES source_captures(id),
    role TEXT NOT NULL,
    label TEXT NOT NULL,
    locator TEXT
);

CREATE INDEX direct_relation_revisions_lineage_status_idx
ON direct_relation_revisions(direct_relation_id, status, revision);

CREATE INDEX direct_relation_participants_integer_idx
ON direct_relation_participants(integer_id, direct_relation_revision_id);

CREATE INDEX constellation_revisions_lineage_status_idx
ON constellation_revisions(constellation_id, status, revision);

CREATE INDEX constellation_members_integer_idx
ON constellation_members(integer_id, constellation_revision_id);

CREATE TRIGGER direct_relation_lineage_insert_guard
BEFORE INSERT ON direct_relations
WHEN EXISTS (SELECT 1 FROM constellations WHERE slug = NEW.slug)
BEGIN
    SELECT RAISE(ABORT, 'research lineage slugs are globally unique across authored object types');
END;

CREATE TRIGGER constellation_lineage_insert_guard
BEFORE INSERT ON constellations
WHEN EXISTS (SELECT 1 FROM direct_relations WHERE slug = NEW.slug)
BEGIN
    SELECT RAISE(ABORT, 'research lineage slugs are globally unique across authored object types');
END;

CREATE TRIGGER direct_relation_lineage_update_guard
BEFORE UPDATE ON direct_relations
WHEN OLD.id IS NOT NEW.id
    OR OLD.slug IS NOT NEW.slug
    OR OLD.created_at_utc IS NOT NEW.created_at_utc
BEGIN
    SELECT RAISE(ABORT, 'direct-relation lineage identity is immutable');
END;

CREATE TRIGGER direct_relation_active_revision_guard
BEFORE UPDATE OF active_revision_id ON direct_relations
WHEN OLD.active_revision_id IS NOT NEW.active_revision_id
    AND (
        NEW.active_revision_id IS NULL
        OR NOT EXISTS (
            SELECT 1
            FROM direct_relation_revisions revision
            WHERE revision.id = NEW.active_revision_id
              AND revision.direct_relation_id = NEW.id
              AND revision.status = 'accepted')
        OR (
            OLD.active_revision_id IS NOT NULL
            AND NOT EXISTS (
                SELECT 1
                FROM direct_relation_revisions revision
                WHERE revision.id = NEW.active_revision_id
                  AND revision.supersedes_revision_id = OLD.active_revision_id))
    )
BEGIN
    SELECT RAISE(ABORT, 'active direct-relation revision must advance through accepted same-lineage supersession');
END;

CREATE TRIGGER direct_relation_lineage_delete_guard
BEFORE DELETE ON direct_relations
BEGIN
    SELECT RAISE(ABORT, 'direct-relation lineages are never deleted');
END;

CREATE TRIGGER direct_relation_supersession_insert_guard
BEFORE INSERT ON direct_relation_revisions
WHEN NEW.supersedes_revision_id IS NOT NULL
    AND NOT EXISTS (
        SELECT 1
        FROM direct_relation_revisions prior
        JOIN direct_relations lineage ON lineage.id = NEW.direct_relation_id
        WHERE prior.id = NEW.supersedes_revision_id
          AND prior.direct_relation_id = NEW.direct_relation_id
          AND prior.status = 'accepted'
          AND lineage.active_revision_id = prior.id)
BEGIN
    SELECT RAISE(ABORT, 'direct-relation supersession must name the current accepted revision in the same lineage');
END;

CREATE TRIGGER direct_relation_revision_insert_status_guard
BEFORE INSERT ON direct_relation_revisions
WHEN NEW.status <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'direct-relation revisions must enter through draft');
END;

CREATE TRIGGER direct_relation_revision_identity_guard
BEFORE UPDATE ON direct_relation_revisions
WHEN OLD.direct_relation_id IS NOT NEW.direct_relation_id
    OR OLD.revision IS NOT NEW.revision
    OR OLD.created_at_utc IS NOT NEW.created_at_utc
    OR OLD.supersedes_revision_id IS NOT NEW.supersedes_revision_id
BEGIN
    SELECT RAISE(ABORT, 'direct-relation revision identity and lineage are immutable');
END;

CREATE TRIGGER direct_relation_revision_transition_guard
BEFORE UPDATE ON direct_relation_revisions
WHEN OLD.status <> 'draft'
    OR NEW.status NOT IN ('draft', 'accepted', 'withdrawn')
BEGIN
    SELECT RAISE(ABORT, 'direct-relation revision is immutable or transition is invalid');
END;

CREATE TRIGGER direct_relation_revision_freeze_guard
BEFORE UPDATE ON direct_relation_revisions
WHEN OLD.status = 'draft'
    AND NEW.status <> 'draft'
    AND (
        OLD.direct_relation_id IS NOT NEW.direct_relation_id
        OR OLD.revision IS NOT NEW.revision
        OR OLD.directionality IS NOT NEW.directionality
        OR OLD.proposition IS NOT NEW.proposition
        OR OLD.warrant IS NOT NEW.warrant
        OR OLD.epistemic_register IS NOT NEW.epistemic_register
        OR OLD.private_notes IS NOT NEW.private_notes
        OR OLD.created_at_utc IS NOT NEW.created_at_utc
        OR OLD.supersedes_revision_id IS NOT NEW.supersedes_revision_id)
BEGIN
    SELECT RAISE(ABORT, 'direct-relation content cannot change while leaving draft');
END;

CREATE TRIGGER direct_relation_acceptance_guard
BEFORE UPDATE OF status ON direct_relation_revisions
WHEN NEW.status = 'accepted'
    AND (
        length(trim(NEW.proposition)) = 0
        OR length(trim(NEW.warrant)) = 0
        OR length(trim(NEW.epistemic_register)) = 0
        OR NEW.accepted_at_utc IS NULL
        OR (SELECT COUNT(*) FROM direct_relation_participants participant
            WHERE participant.direct_relation_revision_id = NEW.id) <> 2
        OR EXISTS (
            SELECT 1 FROM direct_relation_participants participant
            WHERE participant.direct_relation_revision_id = NEW.id
              AND length(trim(participant.role)) = 0)
        OR (
            (SELECT active_revision_id FROM direct_relations WHERE id = NEW.direct_relation_id) IS NULL
            AND NEW.supersedes_revision_id IS NOT NULL)
        OR (
            (SELECT active_revision_id FROM direct_relations WHERE id = NEW.direct_relation_id) IS NOT NULL
            AND NEW.supersedes_revision_id IS NOT
                (SELECT active_revision_id FROM direct_relations WHERE id = NEW.direct_relation_id))
    )
BEGIN
    SELECT RAISE(ABORT, 'direct-relation acceptance requires complete content, exactly two roles, and current same-lineage supersession');
END;

CREATE TRIGGER direct_relation_activate_after_accept
AFTER UPDATE OF status ON direct_relation_revisions
WHEN OLD.status = 'draft' AND NEW.status = 'accepted'
BEGIN
    UPDATE direct_relations
    SET active_revision_id = NEW.id
    WHERE id = NEW.direct_relation_id;
END;

CREATE TRIGGER direct_relation_revision_delete_guard
BEFORE DELETE ON direct_relation_revisions
BEGIN
    SELECT RAISE(ABORT, 'direct-relation revisions are withdrawn or superseded, never deleted');
END;

CREATE TRIGGER direct_relation_participant_insert_guard
BEFORE INSERT ON direct_relation_participants
WHEN COALESCE((SELECT status FROM direct_relation_revisions WHERE id = NEW.direct_relation_revision_id), '') <> 'draft'
    OR length(trim(NEW.role)) = 0
BEGIN
    SELECT RAISE(ABORT, 'participants may be attached only to a draft direct relation and require a role');
END;

CREATE TRIGGER direct_relation_participant_update_guard
BEFORE UPDATE ON direct_relation_participants
WHEN COALESCE((SELECT status FROM direct_relation_revisions WHERE id = OLD.direct_relation_revision_id), '') <> 'draft'
    OR COALESCE((SELECT status FROM direct_relation_revisions WHERE id = NEW.direct_relation_revision_id), '') <> 'draft'
    OR OLD.direct_relation_revision_id IS NOT NEW.direct_relation_revision_id
    OR length(trim(NEW.role)) = 0
BEGIN
    SELECT RAISE(ABORT, 'accepted direct-relation participants are immutable and cannot be reparented');
END;

CREATE TRIGGER direct_relation_participant_delete_guard
BEFORE DELETE ON direct_relation_participants
WHEN COALESCE((SELECT status FROM direct_relation_revisions WHERE id = OLD.direct_relation_revision_id), '') <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted direct-relation participants are immutable');
END;

CREATE TRIGGER direct_relation_citation_insert_guard
BEFORE INSERT ON direct_relation_revision_citations
WHEN COALESCE((SELECT status FROM direct_relation_revisions WHERE id = NEW.direct_relation_revision_id), '') <> 'draft'
    OR length(trim(NEW.role)) = 0
    OR length(trim(NEW.label)) = 0
    OR (
        NEW.source_capture_id IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM source_captures capture
            WHERE capture.id = NEW.source_capture_id
              AND capture.source_id = NEW.source_id))
BEGIN
    SELECT RAISE(ABORT, 'direct-relation citations require a draft revision and source-matched capture');
END;

CREATE TRIGGER direct_relation_citation_update_guard
BEFORE UPDATE ON direct_relation_revision_citations
WHEN COALESCE((SELECT status FROM direct_relation_revisions WHERE id = OLD.direct_relation_revision_id), '') <> 'draft'
    OR COALESCE((SELECT status FROM direct_relation_revisions WHERE id = NEW.direct_relation_revision_id), '') <> 'draft'
    OR OLD.direct_relation_revision_id IS NOT NEW.direct_relation_revision_id
    OR length(trim(NEW.role)) = 0
    OR length(trim(NEW.label)) = 0
    OR (
        NEW.source_capture_id IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM source_captures capture
            WHERE capture.id = NEW.source_capture_id
              AND capture.source_id = NEW.source_id))
BEGIN
    SELECT RAISE(ABORT, 'accepted direct-relation citations are immutable and cannot be reparented');
END;

CREATE TRIGGER direct_relation_citation_delete_guard
BEFORE DELETE ON direct_relation_revision_citations
WHEN COALESCE((SELECT status FROM direct_relation_revisions WHERE id = OLD.direct_relation_revision_id), '') <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted direct-relation citations are immutable');
END;

CREATE TRIGGER constellation_lineage_update_guard
BEFORE UPDATE ON constellations
WHEN OLD.id IS NOT NEW.id
    OR OLD.slug IS NOT NEW.slug
    OR OLD.created_at_utc IS NOT NEW.created_at_utc
BEGIN
    SELECT RAISE(ABORT, 'constellation lineage identity is immutable');
END;

CREATE TRIGGER constellation_active_revision_guard
BEFORE UPDATE OF active_revision_id ON constellations
WHEN OLD.active_revision_id IS NOT NEW.active_revision_id
    AND (
        NEW.active_revision_id IS NULL
        OR NOT EXISTS (
            SELECT 1
            FROM constellation_revisions revision
            WHERE revision.id = NEW.active_revision_id
              AND revision.constellation_id = NEW.id
              AND revision.status = 'accepted')
        OR (
            OLD.active_revision_id IS NOT NULL
            AND NOT EXISTS (
                SELECT 1
                FROM constellation_revisions revision
                WHERE revision.id = NEW.active_revision_id
                  AND revision.supersedes_revision_id = OLD.active_revision_id))
    )
BEGIN
    SELECT RAISE(ABORT, 'active constellation revision must advance through accepted same-lineage supersession');
END;

CREATE TRIGGER constellation_lineage_delete_guard
BEFORE DELETE ON constellations
BEGIN
    SELECT RAISE(ABORT, 'constellation lineages are never deleted');
END;

CREATE TRIGGER constellation_supersession_insert_guard
BEFORE INSERT ON constellation_revisions
WHEN NEW.supersedes_revision_id IS NOT NULL
    AND NOT EXISTS (
        SELECT 1
        FROM constellation_revisions prior
        JOIN constellations lineage ON lineage.id = NEW.constellation_id
        WHERE prior.id = NEW.supersedes_revision_id
          AND prior.constellation_id = NEW.constellation_id
          AND prior.status = 'accepted'
          AND lineage.active_revision_id = prior.id)
BEGIN
    SELECT RAISE(ABORT, 'constellation supersession must name the current accepted revision in the same lineage');
END;

CREATE TRIGGER constellation_revision_insert_status_guard
BEFORE INSERT ON constellation_revisions
WHEN NEW.status <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'constellation revisions must enter through draft');
END;

CREATE TRIGGER constellation_revision_identity_guard
BEFORE UPDATE ON constellation_revisions
WHEN OLD.constellation_id IS NOT NEW.constellation_id
    OR OLD.revision IS NOT NEW.revision
    OR OLD.created_at_utc IS NOT NEW.created_at_utc
    OR OLD.supersedes_revision_id IS NOT NEW.supersedes_revision_id
BEGIN
    SELECT RAISE(ABORT, 'constellation revision identity and lineage are immutable');
END;

CREATE TRIGGER constellation_revision_transition_guard
BEFORE UPDATE ON constellation_revisions
WHEN OLD.status <> 'draft'
    OR NEW.status NOT IN ('draft', 'accepted', 'withdrawn')
BEGIN
    SELECT RAISE(ABORT, 'constellation revision is immutable or transition is invalid');
END;

CREATE TRIGGER constellation_revision_freeze_guard
BEFORE UPDATE ON constellation_revisions
WHEN OLD.status = 'draft'
    AND NEW.status <> 'draft'
    AND (
        OLD.constellation_id IS NOT NEW.constellation_id
        OR OLD.revision IS NOT NEW.revision
        OR OLD.name IS NOT NEW.name
        OR OLD.account IS NOT NEW.account
        OR OLD.epistemic_register IS NOT NEW.epistemic_register
        OR OLD.private_notes IS NOT NEW.private_notes
        OR OLD.created_at_utc IS NOT NEW.created_at_utc
        OR OLD.supersedes_revision_id IS NOT NEW.supersedes_revision_id)
BEGIN
    SELECT RAISE(ABORT, 'constellation content cannot change while leaving draft');
END;

CREATE TRIGGER constellation_acceptance_guard
BEFORE UPDATE OF status ON constellation_revisions
WHEN NEW.status = 'accepted'
    AND (
        length(trim(NEW.name)) = 0
        OR length(trim(NEW.account)) = 0
        OR length(trim(NEW.epistemic_register)) = 0
        OR NEW.accepted_at_utc IS NULL
        OR (SELECT COUNT(*) FROM constellation_members member
            WHERE member.constellation_revision_id = NEW.id) < 2
        OR EXISTS (
            SELECT 1 FROM constellation_members member
            WHERE member.constellation_revision_id = NEW.id
              AND length(trim(member.role)) = 0)
        OR (
            (SELECT active_revision_id FROM constellations WHERE id = NEW.constellation_id) IS NULL
            AND NEW.supersedes_revision_id IS NOT NULL)
        OR (
            (SELECT active_revision_id FROM constellations WHERE id = NEW.constellation_id) IS NOT NULL
            AND NEW.supersedes_revision_id IS NOT
                (SELECT active_revision_id FROM constellations WHERE id = NEW.constellation_id))
    )
BEGIN
    SELECT RAISE(ABORT, 'constellation acceptance requires complete content, at least two roles, and current same-lineage supersession');
END;

CREATE TRIGGER constellation_activate_after_accept
AFTER UPDATE OF status ON constellation_revisions
WHEN OLD.status = 'draft' AND NEW.status = 'accepted'
BEGIN
    UPDATE constellations
    SET active_revision_id = NEW.id
    WHERE id = NEW.constellation_id;
END;

CREATE TRIGGER constellation_revision_delete_guard
BEFORE DELETE ON constellation_revisions
BEGIN
    SELECT RAISE(ABORT, 'constellation revisions are withdrawn or superseded, never deleted');
END;

CREATE TRIGGER constellation_member_insert_guard
BEFORE INSERT ON constellation_members
WHEN COALESCE((SELECT status FROM constellation_revisions WHERE id = NEW.constellation_revision_id), '') <> 'draft'
    OR length(trim(NEW.role)) = 0
BEGIN
    SELECT RAISE(ABORT, 'members may be attached only to a draft constellation and require a role');
END;

CREATE TRIGGER constellation_member_update_guard
BEFORE UPDATE ON constellation_members
WHEN COALESCE((SELECT status FROM constellation_revisions WHERE id = OLD.constellation_revision_id), '') <> 'draft'
    OR COALESCE((SELECT status FROM constellation_revisions WHERE id = NEW.constellation_revision_id), '') <> 'draft'
    OR OLD.constellation_revision_id IS NOT NEW.constellation_revision_id
    OR length(trim(NEW.role)) = 0
BEGIN
    SELECT RAISE(ABORT, 'accepted constellation members are immutable and cannot be reparented');
END;

CREATE TRIGGER constellation_member_delete_guard
BEFORE DELETE ON constellation_members
WHEN COALESCE((SELECT status FROM constellation_revisions WHERE id = OLD.constellation_revision_id), '') <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted constellation members are immutable');
END;

CREATE TRIGGER constellation_citation_insert_guard
BEFORE INSERT ON constellation_revision_citations
WHEN COALESCE((SELECT status FROM constellation_revisions WHERE id = NEW.constellation_revision_id), '') <> 'draft'
    OR length(trim(NEW.role)) = 0
    OR length(trim(NEW.label)) = 0
    OR (
        NEW.source_capture_id IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM source_captures capture
            WHERE capture.id = NEW.source_capture_id
              AND capture.source_id = NEW.source_id))
BEGIN
    SELECT RAISE(ABORT, 'constellation citations require a draft revision and source-matched capture');
END;

CREATE TRIGGER constellation_citation_update_guard
BEFORE UPDATE ON constellation_revision_citations
WHEN COALESCE((SELECT status FROM constellation_revisions WHERE id = OLD.constellation_revision_id), '') <> 'draft'
    OR COALESCE((SELECT status FROM constellation_revisions WHERE id = NEW.constellation_revision_id), '') <> 'draft'
    OR OLD.constellation_revision_id IS NOT NEW.constellation_revision_id
    OR length(trim(NEW.role)) = 0
    OR length(trim(NEW.label)) = 0
    OR (
        NEW.source_capture_id IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM source_captures capture
            WHERE capture.id = NEW.source_capture_id
              AND capture.source_id = NEW.source_id))
BEGIN
    SELECT RAISE(ABORT, 'accepted constellation citations are immutable and cannot be reparented');
END;

CREATE TRIGGER constellation_citation_delete_guard
BEFORE DELETE ON constellation_revision_citations
WHEN COALESCE((SELECT status FROM constellation_revisions WHERE id = OLD.constellation_revision_id), '') <> 'draft'
BEGIN
    SELECT RAISE(ABORT, 'accepted constellation citations are immutable');
END;
