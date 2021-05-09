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