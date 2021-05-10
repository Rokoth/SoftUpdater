--user
create unique index uidx_user_login 
	on "user"("login") where not is_deleted;

create index idx_user_name
    on "user"("name");

--client
create unique index uidx_client_login 
	on client("login") where not is_deleted;

create index idx_client_name
    on client("name");

create index idx_client_login_password
    on client("login", "password");

create index idx_client_user_id
    on client(userid);

create index idx_client_is_deleted
    on client(is_deleted);


--release_architect
create unique index uidx_release_architect_name 
	on release_architect(release_id, "name") where not is_deleted;

create unique index uidx_release_architect_path 
	on release_architect(release_id, "path") where not is_deleted and "path" is not null;

create index idx_release_architect_release_id
    on release_architect(release_id);

create index idx_release_architect_is_deleted
    on release_architect(is_deleted);

    
--release
create unique index uidx_release_client_id_version 
	on release(client_id, "version") where not is_deleted;

create unique index uidx_release_client_id_number 
	on release(client_id, number) where not is_deleted;

create unique index uidx_release_client_id_path 
	on release(client_id, "path") where not is_deleted and "path" is not null;

create index idx_release_client_id
    on release(client_id);

create index idx_release_is_deleted
    on release(is_deleted);


--load_history
create index idx_load_history_client_id
    on load_history(client_id);

create index idx_load_history_release_id
    on load_history(release_id);

create index idx_load_history_architect_id
    on load_history(architect_id);

create index idx_load_history_load_date
    on load_history(load_date);

create index idx_load_history_success
    on load_history(success);

create index idx_load_history_is_deleted
    on load_history(is_deleted);
