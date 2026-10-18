#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

expected_ignored=(
  ".env"
  ".env.local"
  "nested/.env.production"
  "secrets.env"
  "secrets.env.local"
  ".envrc"
  "nested/.envrc.local"
  ".direnv/allow"
  "web/.direnv/allow"
  ".aws/credentials"
  ".codex/auth.json"
  ".agents/private-note.md"
  "var/local/polyvalent.sqlite3"
  "captures/source.html"
  "artifacts/public/local-v1/data.json"
  "polyvalent.sqlite3"
  "polyvalent.sqlite3-wal"
  "polyvalent.sqlite3-shm"
  "polyvalent.sqlite3-journal"
  "polyvalent.sqlite3.backup-20261018"
  "research.sqlite"
  "research.sqlite-wal"
  "research.sqlite-shm"
  "research.sqlite-journal"
  "research.sqlite.backup-20261018"
  "research.db"
  "research.db-wal"
  "research.db-shm"
  "research.db-journal"
  "research.db.backup-20261018"
)

for path in "${expected_ignored[@]}"; do
  if ! match="$(git check-ignore --verbose --no-index -- "$path")"; then
    printf 'private git boundary failed: expected ignored path %s\n' "$path" >&2
    exit 1
  fi

  source_file="${match%%:*}"
  if [[ "$source_file" != ".gitignore" ]]; then
    printf 'private git boundary failed: %s is ignored by %s, not the repository .gitignore\n' \
      "$path" \
      "$source_file" >&2
    exit 1
  fi
done

expected_trackable=(
  "data/seed/v1/friedman.json"
  "web/public/data/data.json"
  "web/public/data/manifest.json"
)

for path in "${expected_trackable[@]}"; do
  if match="$(git check-ignore --verbose --no-index -- "$path")"; then
    printf 'private git boundary failed: expected trackable path %s, matched %s\n' \
      "$path" \
      "${match%%$'\t'*}" >&2
    exit 1
  fi
done

tracked_ignored="$(git ls-files --cached --ignored --exclude-standard)"
if [[ -n "$tracked_ignored" ]]; then
  printf 'private git boundary failed: tracked files now match ignore rules:\n%s\n' "$tracked_ignored" >&2
  exit 1
fi

printf 'private git boundary ok: %s private paths ignored, %s public fixtures trackable\n' \
  "${#expected_ignored[@]}" \
  "${#expected_trackable[@]}"
