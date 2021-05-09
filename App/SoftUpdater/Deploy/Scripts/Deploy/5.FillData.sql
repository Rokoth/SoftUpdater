INSERT INTO "user"(
	id, name, description, login, password, version_date, is_deleted)
	VALUES (uuid_generate_v4(), 'admin', 'admin', 'admin', sha512('admin'), now(), false);