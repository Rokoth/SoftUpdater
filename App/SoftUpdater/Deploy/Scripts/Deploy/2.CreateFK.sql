--client
alter table client 
add constraint fk_client_user_id 
	foreign key(userid) 
		references "user"(id) 
		on delete no action on update no action;


--release
alter table release 
add constraint fk_release_client_id 
	foreign key(client_id) 
		references client(id) 
		on delete no action on update no action;

--release_architect
alter table release_architect 
add constraint fk_release_architect_release_id 
	foreign key(release_id) 
		references release(id) 
		on delete no action on update no action;

--load_history
alter table load_history 
add constraint fk_load_history_client_id 
	foreign key(client_id) 
		references client(id) 
		on delete no action on update no action;

alter table load_history 
add constraint fk_load_history_release_id 
	foreign key(release_id) 
		references release(id) 
		on delete no action on update no action;

alter table load_history 
add constraint fk_load_history_release_architect_id 
	foreign key(architect_id) 
		references release_architect(id) 
		on delete no action on update no action;



