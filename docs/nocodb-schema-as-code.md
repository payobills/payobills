# NocoDB Schema as Code

## Problem

NocoDB has no native schema versioning. Anyone cloning the repo must manually recreate tables and columns through the UI before services will work. This is error-prone and undocumented.

## Goal

- Export NocoDB schema from a live instance into a committed JSON file
- Bootstrap a fresh NocoDB instance from that JSON
- Handle linked fields (relations) correctly by treating table **title** (display name) as the canonical identifier, not the instance-specific table ID

---

## Canonical Schema Format

The schema is stored in `devenv/nocodb/schema.json`. Table titles are the source of truth — IDs are never committed.

```json
{
  "tables": [
    {
      "title": "Transactions",
      "table_name": "nc_transactions",
      "columns": [
        { "title": "Id", "column_name": "id", "uidt": "SingleLineText" },
        { "title": "Amount", "column_name": "amount", "uidt": "Decimal" },
        { "title": "Trip", "column_name": "trip_id", "uidt": "Links",
          "colOptions": { "related_table_title": "Trips", "type": "mm" } }
      ]
    },
    {
      "title": "Trips",
      "table_name": "nc_trips",
      "columns": [
        { "title": "Id", "column_name": "id", "uidt": "SingleLineText" },
        { "title": "StartDate", "column_name": "start_date", "uidt": "DateTime" }
      ]
    }
  ]
}
```

Key difference from the raw NocoDB API response: `related_table_id` is replaced with `related_table_title` during export, and resolved back to the new instance's ID during bootstrap.

---

## Export Script (`devenv/nocodb/export.sh`)

Pulls schema from a live NocoDB instance and writes `schema.json`.

**What it does:**

1. `GET /api/v2/meta/bases/{base_id}/tables` — list all tables with their columns
2. For each table, iterate columns — if `uidt == "Links"`, resolve `colOptions.related_table_id` to the table title using the same response and replace the ID with `related_table_title`
3. Strip all instance-specific IDs from the output (`id`, `table_id`, `fk_model_id`, etc.) — keep only `title`, `table_name`, `uidt`, `colOptions` (with title substitution)
4. Write to `devenv/nocodb/schema.json`

**Inputs (env vars):**
- `NOCODB_URL` — e.g. `http://homelab:8080`
- `NOCODB_TOKEN` — API token
- `NOCODB_BASE_ID` — the base to export from

---

## Bootstrap Script (`devenv/nocodb/bootstrap.py`)

Reads `schema.json` and recreates the schema on a fresh NocoDB instance.

### Step 1 — Build dependency graph

Parse all tables. For each column where `uidt == "Links"`, record an edge:

```
table_title → related_table_title
```

This forms a DAG. Tables with no outgoing Link edges have no dependencies.

### Step 2 — Topological sort

Sort tables so that a table is always created before any table that links to it. Since Links in NocoDB are bidirectional (both sides get a column), only one side needs to be created first — pick the side that is referenced (the "one" side in a one-to-many, or either in many-to-many).

Cycle detection: if a cycle exists in the Links graph, raise an error and report which tables are involved. NocoDB does not support circular references.

### Step 3 — Create tables (without Links columns)

For each table in sorted order:

1. `POST /api/v2/meta/bases/{base_id}/tables` with `title`, `table_name`, and all non-Link columns
2. Record the returned table ID: `name_to_id["Trips"] = "md_newxyz"`

Strip any `colOptions` from non-Link columns — NocoDB auto-generates those.

### Step 4 — Add Links columns

After all tables exist, iterate tables again and for each Links column:

1. Resolve `related_table_title` → `related_table_id` using `name_to_id`
2. `POST /api/v2/meta/tables/{table_id}/columns` with the Links column payload, substituting the real ID

Links columns are added last because both the source and target table must exist before NocoDB will accept the relation.

### Step 5 — Verify

After all columns are created, `GET` each table and compare column titles against the schema. Report any missing columns as warnings.

**Inputs (env vars):**
- `NOCODB_URL`
- `NOCODB_TOKEN`
- `NOCODB_BASE_ID` — the target base on the fresh instance

---

## File Layout

```
devenv/
  nocodb/
    schema.json        # committed canonical schema (no IDs)
    export.sh          # dump schema from a live instance
    bootstrap.py       # recreate schema on a fresh instance
    README.md          # how to run both scripts
```

---

## Workflow

### First-time setup (new developer)

```bash
# 1. Start NocoDB locally (docker compose)
docker compose -f devenv/docker-compose.yaml up -d nocodb

# 2. Create a base in NocoDB UI, grab the base ID

# 3. Bootstrap schema
NOCODB_URL=http://localhost:8080 \
NOCODB_TOKEN=<token> \
NOCODB_BASE_ID=<new-base-id> \
python devenv/nocodb/bootstrap.py
```

### Updating schema (after making changes in homelab NocoDB)

```bash
NOCODB_URL=http://homelab:8080 \
NOCODB_TOKEN=<token> \
NOCODB_BASE_ID=<homelab-base-id> \
bash devenv/nocodb/export.sh

git add devenv/nocodb/schema.json
git commit -m "chore(nocodb): update schema"
```

---

## Decisions

**Why table title, not table_name?** — `table_name` is the underlying DB identifier (e.g. `nc_transactions_abc123`) and also varies per instance. `title` is the stable display name set by the user and consistent across instances.

**Why not commit the NocoDB SQLite file?** — It contains instance state (user accounts, API tokens, row data). The JSON schema is portable and contains only structure.

**Why a Python script, not shell?** — The DAG/topological sort logic is non-trivial. Python's `graphlib.TopologicalSorter` (stdlib, Python 3.9+) handles it cleanly with no extra dependencies.

---

## Open Questions

1. **`plz` integration** — The workflow section uses raw shell commands. Should the export and bootstrap steps be wrapped as `plz run` targets under `devenv/nocodb/BUILD`, consistent with how sonar is invoked?

2. **SingleSelect / MultiSelect options** — For columns of type `SingleSelect` or `MultiSelect`, the export should also capture the defined options (the dropdown choices). Does the NocoDB meta API include these in the column response, and should they be recorded and replayed during bootstrap?

3. **Schema JSON format** — If NocoDB's meta API documentation defines a canonical JSON shape for tables/columns (e.g. in their Swagger/OpenAPI spec), the canonical schema file should mirror that format rather than a custom one, to reduce translation overhead in both scripts. What does the actual API response look like, and can we use it as-is?
