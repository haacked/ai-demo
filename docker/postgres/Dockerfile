FROM ankane/pgvector

COPY 1-init-user-and-db.sh /docker-entrypoint-initdb.d/
COPY 2-extensions.sh /docker-entrypoint-initdb.d/

RUN apt-get update && apt-get install -y postgresql-15-postgis-3

