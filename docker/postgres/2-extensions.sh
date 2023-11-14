#!/bin/sh
set -e

# Only a superuser can create extensions, so we need to do this during initialization.
# Unfortunately that means that database migrations can't add extensions.

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "aidemo" <<-EOSQL
    CREATE EXTENSION IF NOT EXISTS "citext";
    CREATE EXTENSION IF NOT EXISTS "postgis";
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
EOSQL