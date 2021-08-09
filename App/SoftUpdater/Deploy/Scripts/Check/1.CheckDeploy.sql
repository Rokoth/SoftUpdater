--Copyright 2021 Dmitriy Rokoth
--Licensed under the Apache License, Version 2.0
--
--ref1

select exists (
	select 1 from information_schema.tables 
		where table_schema = 'public' 
			and table_name = 'deploy_history'
);