ARG VERSION=14-3.2
FROM postgis/postgis:$VERSION-alpine

COPY 1-init-user-and-db.sh /docker-entrypoint-initdb.d/
COPY 2-extensions.sh /docker-entrypoint-initdb.d/