BEGIN TRANSACTION;
ALTER TABLE "Reservations" ADD "QuotaRestored" INTEGER NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251011182705_AddQuotaRestoredFlag', '9.0.9');

COMMIT;

