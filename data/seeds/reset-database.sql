-- Connect to the database using the psql client:
--
--   $ psql postgresql://localhost/brokerage_api
--
-- Use the correct url for your database and then run this file:
--
--   brokerage_api=# \i reset-database.sql
--
-- The API can then be started using `dotnet run`

-- Disable output for the current database check
\o /dev/null

-- Get the current database name and store it locally in the client
SELECT current_database() = 'brokerage_api_db_production' AS is_production;
\gset

-- Re-enable output for the rest of the script
\o

-- Bail if the current database matches the production database name
\if :is_production
  \warn 'You can''t run this script on the production database'
  \quit
\endif

-- All timestamps are recorded as UTC
SET TimeZone='UTC';

-- Wrap in a transaction so that if it fails the database is still in a consistent state
BEGIN;

TRUNCATE TABLE
  audit_events,
  element_types,
  elements,
  referral_amendment,
  referral_elements,
  referrals,
  service_users
RESTART IDENTITY;

\COPY element_types(id, service_id, name, subjective_code, framework_subjective_code, type, cost_type, billing, cost_operation, payment_operation, non_personal_budget, position, is_archived, is_s117) FROM 'element_types.csv' CSV HEADER;
\COPY referrals(id, workflow_id, workflow_type, form_name, social_care_id, resident_name, primary_support_reason, direct_payments, urgent_since, note, status, assigned_broker_email, assigned_approver_email, comment, started_at, created_at, updated_at) FROM 'referrals.csv' CSV HEADER;
\COPY elements(id, social_care_id, element_type_id, non_personal_budget, provider_id, details, internal_status, parent_element_id, start_date, end_date, monday, tuesday, wednesday, thursday, friday, saturday, sunday, quantity, cost, cost_centre, is_suspension, suspended_element_id, comment, created_by, created_at, updated_at) FROM 'elements.csv' CSV HEADER;
\COPY referral_elements(referral_id, element_id, pending_cancellation, pending_comment, pending_end_date) FROM 'referral_elements.csv' CSV HEADER;
\COPY service_users(social_care_id, service_user_name, date_of_birth, created_at, updated_at) FROM 'service_users.csv' CSV HEADER;

SELECT setval('referrals_id_seq', (SELECT COALESCE(MAX(id), 1) FROM referrals));
SELECT setval('elements_id_seq', (SELECT COALESCE(MAX(id), 1) FROM elements));


COMMIT;
