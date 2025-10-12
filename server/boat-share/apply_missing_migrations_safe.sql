-- =====================================================
-- Safe Migration Script for Railway PostgreSQL
-- This script is idempotent - safe to run multiple times
-- =====================================================

BEGIN;

-- Check current state
SELECT 'Current migrations in database:' as info;
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";

-- =====================================================
-- Migration 1: 20251010121449_AddLegacyStatusAndIndexes
-- Only apply if not already in history
-- =====================================================

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory"
        WHERE "MigrationId" = '20251010121449_AddLegacyStatusAndIndexes'
    ) THEN
        RAISE NOTICE 'Applying migration: AddLegacyStatusAndIndexes';

        -- Drop old indexes if they exist
        DROP INDEX IF EXISTS "IX_Reservations_UserId_EndTime";
        DROP INDEX IF EXISTS "IX_Reservations_UserId_ReservationType";

        -- Add new composite indexes
        CREATE INDEX IF NOT EXISTS "IX_Reservations_UserId_Status_StartTime"
        ON "Reservations" ("UserId", "Status", "StartTime");

        CREATE INDEX IF NOT EXISTS "IX_Reservations_UserId_Status_Type_EndTime"
        ON "Reservations" ("UserId", "Status", "ReservationType", "EndTime");

        CREATE INDEX IF NOT EXISTS "IX_Reservations_EndTime_Status"
        ON "Reservations" ("EndTime", "Status");

        CREATE INDEX IF NOT EXISTS "IX_Reservations_BoatId_Status_StartTime"
        ON "Reservations" ("BoatId", "Status", "StartTime");

        -- Record migration
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20251010121449_AddLegacyStatusAndIndexes', '9.0.9');

        RAISE NOTICE 'Migration AddLegacyStatusAndIndexes applied successfully';
    ELSE
        RAISE NOTICE 'Migration AddLegacyStatusAndIndexes already applied, skipping';
    END IF;
END $$;

-- =====================================================
-- Migration 2: 20251011182705_AddQuotaRestoredFlag
-- Only apply if not already in history
-- =====================================================

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory"
        WHERE "MigrationId" = '20251011182705_AddQuotaRestoredFlag'
    ) THEN
        RAISE NOTICE 'Applying migration: AddQuotaRestoredFlag';

        -- Add QuotaRestored column if it doesn't exist
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'Reservations' AND column_name = 'QuotaRestored'
        ) THEN
            ALTER TABLE "Reservations"
            ADD COLUMN "QuotaRestored" boolean NOT NULL DEFAULT false;
        END IF;

        -- Record migration
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20251011182705_AddQuotaRestoredFlag', '9.0.9');

        RAISE NOTICE 'Migration AddQuotaRestoredFlag applied successfully';
    ELSE
        RAISE NOTICE 'Migration AddQuotaRestoredFlag already applied, skipping';
    END IF;
END $$;

COMMIT;

-- Verify final state
SELECT 'Final migrations in database:' as info;
SELECT "MigrationId", "ProductVersion" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
