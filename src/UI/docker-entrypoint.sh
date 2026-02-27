#!/bin/sh
set -e

if [ -z "${NGINX_PORT}" ]; then
  echo "Error: NGINX_PORT is not set" >&2
  exit 1
fi

case "${NGINX_PORT}" in
  *[!0-9]*)
    echo "Error: NGINX_PORT must be a numeric value, got '${NGINX_PORT}'" >&2
    exit 1
    ;;
esac

envsubst '${NGINX_PORT}' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf
exec nginx -g 'daemon off;'
