#!/bin/sh
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER aidemouser WITH PASSWORD 'mtUvGABu';
    CREATE DATABASE aidemo;
    GRANT ALL PRIVILEGES ON DATABASE aidemo TO aidemo;
EOSQL