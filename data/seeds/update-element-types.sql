BEGIN;

-- Create temporary table to hold records loaded from CSV
CREATE TEMPORARY TABLE element_types_update (
  id integer PRIMARY KEY,
  service_id integer,
  name text NOT NULL,
  subjective_code text,
  framework_subjective_code text,
  type element_type_type NOT NULL,
  cost_type element_cost_type NOT NULL,
  non_personal_budget boolean NOT NULL,
  billing element_billing_type NOT NULL,
  position integer NOT NULL,
  is_archived boolean NOT NULL,
  is_s117 boolean NOT NULL
);

-- Import element type updates
\copy element_types_update(id, service_id, name, subjective_code, framework_subjective_code, type, cost_type, billing, non_personal_budget, position, is_archived, is_s117) FROM 'element_types.csv' CSV HEADER;

-- Delete any element types
DELETE FROM element_types WHERE id NOT IN (
  SELECT id FROM element_types_update
);

-- Update element types
INSERT INTO element_types (
  id, service_id, name, subjective_code, framework_subjective_code,
  type, cost_type, billing, non_personal_budget, position, is_archived, is_s117
)
SELECT
  id, service_id, name, subjective_code, framework_subjective_code,
  type, cost_type, billing, non_personal_budget, position, is_archived, is_s117
FROM
  element_types_update
ON CONFLICT (id) DO UPDATE SET
  service_id = EXCLUDED.service_id,
  name = EXCLUDED.name,
  subjective_code = EXCLUDED.subjective_code,
  framework_subjective_code = EXCLUDED.framework_subjective_code,
  type = EXCLUDED.type,
  cost_type = EXCLUDED.cost_type,
  non_personal_budget = EXCLUDED.non_personal_budget,
  billing = EXCLUDED.billing,
  position = EXCLUDED.position,
  is_archived = EXCLUDED.is_archived,
  is_s117 = EXCLUDED.is_s117;

-- We created the table as temporary so PostgreSQL will clean
-- it up anyway but we'll drop it here for the sake of completeness
DROP TABLE element_types_update;

COMMIT;
