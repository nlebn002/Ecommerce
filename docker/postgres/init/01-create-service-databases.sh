#!/bin/sh
set -eu

create_db() {
  db_name="$1"

  if psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -tAc "SELECT 1 FROM pg_database WHERE datname='${db_name}'" | grep -q 1; then
    echo "Database '${db_name}' already exists"
    return
  fi

  echo "Creating database '${db_name}'"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -c "CREATE DATABASE \"${db_name}\" OWNER \"${POSTGRES_USER}\";"
}

create_db "basketdb"
create_db "orderdb"
create_db "logisticsdb"
