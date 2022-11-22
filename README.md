# Postgres Sharding
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

## How to

### Init database
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

### Create the table on the shard server
```
CREATE TABLE books (
	id bigint not null,
	category_id  int not null
		CONSTRAINT category_id_check CHECK ( category_id = 1 ),
	author character varying not null,
	title character varying not null,
	year int not null
);
```

Then create an index.
```
CREATE INDEX books_category_id_idx ON books USING btree(category_id);
```

### Add configuration and mapping on the main server
Add foreign data wrapper
```
CREATE EXTENSION postgres_fdw;
CREATE SERVER books_1_server 
FOREIGN DATA WRAPPER postgres_fdw 
OPTIONS( host 'postgre-shard-1', port '5432', dbname 'ShardingDb' );
```

And mapping
```
CREATE USER MAPPING FOR postgre
SERVER books_1_server
OPTIONS (user 'postgre', password 'password');
```

### Create foreign table on the main server
```
CREATE FOREIGN TABLE books_1 (
	id bigint not null,
	category_id  int not null,
	author character varying not null,
	title character varying not null,
	year int not null )
SERVER books_1_server
OPTIONS (schema_name 'public', table_name 'books');
```

### Set another shards as many as needed

### Set rules for the table on the main server
To stop manage main table
```
CREATE RULE books_insert AS ON INSERT TO books DO INSTEAD NOTHING;
CREATE RULE books_update AS ON UPDATE TO books DO INSTEAD NOTHING;
CREATE RULE books_delete AS ON DELETE TO books DO INSTEAD NOTHING;
```

And to manage correct shards
```
CREATE RULE books_insert_to_1 AS ON INSERT TO books
WHERE ( category_id = 1 )
DO INSTEAD INSERT INTO books_1 VALUES (NEW.*);
```

### Insert values to test sharding works
```
insert into books (id, category_id, author, title, year)
values (1, 1, 'Donald Trump', 'How to make people love you', '2018');

insert into books (id, category_id, author, title, year)
values (1, 2, 'Nick Fury', 'Guid to create best squard', '2022');
```

### Create view to select books as no sharding at all
```
CREATE VIEW v_books AS
	SELECT * FROM books_1
		UNION ALL
	SELECT * FROM books_2;
```

# Performance Tests
The goal is to compare writting and reading speed between sharded and regular database.

We will create an application which will write down 1M records in seceral threads and measure the time.
Before this we will create table with wide range sharding values, like 6 tables between 3 servers with the sharding step like 100k.

Target table
```
CREATE TABLE simple_data (
	id bigint not null primary key,
	raw_data character varying not null,
	random_value int not null
);
```

First shard
```
CREATE TABLE simple_data (
	id bigint not null primary key,
	raw_data character varying not null,
	random_value int not null
		CONSTRAINT random_value_check CHECK (random_value > 100000 and random_value < 200000)
);

CREATE INDEX simple_data_random_value_idx ON simple_data USING btree(random_value);
```

And the second shard
```
CREATE TABLE simple_data (
	id bigint not null primary key,
	raw_data character varying not null,
	random_value int not null
		CONSTRAINT random_value_check CHECK (random_value > 200000 and random_value < 300000)
);

CREATE INDEX simple_data_random_value_idx ON simple_data USING btree(random_value);
```

Finish with the creating view
```
CREATE VIEW v_simple_data AS
	SELECT * FROM simple_data
		UNION ALL
	SELECT * FROM simple_data_1
		UNION ALL
	SELECT * FROM simple_data_2;
```

## Results

### Writting
3 shardes, range is 100k per shard, writting in 4 threads
```
Total time: 2086582 ms, 2086.582 sec, 34.8 min
Total raws: 1000405
Main DB rows: 333418
Shard 1 rows: 333848
Shard 2 rows: 333139
```

No shards
```
Total time: 1525869 ms, 1525.869 sec, 25.43115 min
Total raws: 1000000
```

### Reading
1000 read for by random values
```
With shards
Avg read time: 57.534ms

No shards
Avg read time: 98.871ms
```