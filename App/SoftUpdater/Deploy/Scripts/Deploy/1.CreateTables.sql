create extension if not exists "uuid-ossp";

create table if not exists settings(	 
	  id            int           not null primary key
	, param_name    varchar(100)  not null
	, param_value   varchar(1000) not null	
);

create table if not exists "user"(
      id            uuid          not null default uuid_generate_v4() primary key
	, "name"        varchar(100)  not null
	, "description" varchar(1000) null
	, "login"       varchar(100)  not null
	, "password"    bytea         not null
	, version_date  timestamptz   not null default now()
	, is_deleted    boolean       not null
);

create table if not exists client(
	  id            uuid          not null primary key
	, "name"        varchar(100)  not null
	, "description" varchar(1000) null
	, "login"       varchar(100)  not null
	, "password"    bytea         not null
	, base_path     varchar(1000) not null
	, userid        uuid          not null
	, version_date  timestamptz   not null
	, is_deleted    boolean       not null default false	
);

create table if not exists "h_user"(
      h_id          bigserial     not null primary key        
    , id            uuid          null
	, "name"        varchar(100)  null
	, "description" varchar(1000) null
	, "login"       varchar(100)  null
	, "password"    bytea         null
	, version_date  timestamptz   null
	, is_deleted    boolean       null
	, change_date   timestamptz   not null default now()
	, "user_id"     varchar       null
);

create table if not exists h_client(
      h_id          bigserial     not null primary key
	, id            uuid          null
	, "name"        varchar(100)  null
	, "description" varchar(1000) null
	, "login"       varchar(100)  null
	, "password"    bytea         null
	, base_path     varchar(1000) null
	, userid        uuid          null
	, version_date  timestamptz   null
	, is_deleted    boolean       null
	, change_date   timestamptz   not null default now()
	, "user_id"     varchar       null
);

create table if not exists release(
	  id            uuid          not null primary key
	, client_id     uuid          not null
	, "version"     varchar       not null
	, "path"        varchar       null
	, number        int           not null
	, version_date  timestamptz   not null
	, is_deleted    boolean       not null default false	
);

create table if not exists h_release(
      h_id          bigserial     not null primary key
	, id            uuid          null
	, client_id     uuid          null
	, "version"     varchar       null
	, "path"        varchar       null
	, number        int           null
	, version_date  timestamptz   null
	, is_deleted    boolean       null
	, change_date   timestamptz   not null default now()
	, "user_id"     varchar       null
);

create table if not exists release_architect(
	  id            uuid          not null primary key
	, release_id    uuid          not null
	, "name"        varchar       not null
	, "path"        varchar       not null	
	, "file_name"   varchar       not null	
	, version_date  timestamptz   not null
	, is_deleted    boolean       not null default false	
);

create table if not exists h_release_architect(
      h_id          bigserial     not null primary key
	, id            uuid          null
	, release_id    uuid          null
	, "name"        varchar       null
	, "path"        varchar       null	
	, "file_name"   varchar       null
	, version_date  timestamptz   null
	, is_deleted    boolean       null
	, change_date   timestamptz   not null default now()
	, "user_id"     varchar       null
);

create table if not exists load_history(
	  id            uuid          not null primary key
	, client_id     uuid          not null
	, release_id    uuid          not null
	, architect_id  uuid          not null
	, load_date     timestamptz   not null default now()
	, success       boolean       not null
	, version_date  timestamptz   not null
	, is_deleted    boolean       not null default false	
);