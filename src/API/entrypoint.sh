#!/bin/sh
set -e
chown -R appuser:appgroup /app/LANdalf_Data /app/logs
exec gosu appuser "$@"
