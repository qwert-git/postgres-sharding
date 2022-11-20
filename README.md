### Postgres Sharding
This is pet project to try horizontal sharding on PostgreSQL.

## Infrastructure
The infrastructure seting up with docker compose. Run command
``` docker compose up -d ```

When deploying this setup, the pgAdmin web interface will be available at port 5050 (e.g. http://localhost:5050).

After logging in with your credentials of the .env file, you can add your database to pgAdmin.

Right-click "Servers" in the top-left corner and select "Create" -> "Server..."
Name your connection
Change to the "Connection" tab and add the connection details:
* Hostname: "postgres-main" (this would normally be your IP address of the postgres database - however, docker can resolve this container ip by its name)
* Port: "5432"
* Maintenance Database: $POSTGRES_DB (see .env)
* Username: $POSTGRES_USER (see .env)
* Password: $POSTGRES_PW (see .env)

It will create three containers: 
postgre-main - main server. Make read/write requests to this server;
posrgre-shard-1 - the first shard server;
postgre-shard-2 - the second shard server;

# Init database
The empty database ShardingDb created with the docker after running.

For test purposes, we will create empty table first and then we will set up sharding for it. 
To create test table, use this script:
```
CREATE TABLE books (
	id bigint not null primary key,
	category_id int not null,
	author character varying not null,
	title character varying not null,
	year int not null 
);
```
