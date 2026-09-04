INSERT INTO "Employees" ("Id", "EmployeeCode", "FullName", "IsAdmin", "Status") VALUES (gen_random_uuid(), 'ADMIN001', 'System Administrator', true, 0) ON CONFLICT ("EmployeeCode") DO NOTHING;
