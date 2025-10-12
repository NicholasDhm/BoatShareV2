-- PostgreSQL Migration Script
-- Adds QuotaRestored column to Reservations table

BEGIN;

-- Add QuotaRestored column to Reservations table
ALTER TABLE "Reservations"
ADD COLUMN "QuotaRestored" boolean NOT NULL DEFAULT false;

-- Record this migration in EF Core's history table
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251011182705_AddQuotaRestoredFlag', '9.0.9');

COMMIT;
