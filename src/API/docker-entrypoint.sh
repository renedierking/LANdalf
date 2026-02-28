#!/bin/sh
set -e

# Ensure volume mount-point directories exist and are writable by appuser.
# Docker named volumes default to root:root ownership, so we chown them here
# before dropping privileges.
mkdir -p /app/LANdalf_Data /app/logs
chown -R appuser:appgroup /app/LANdalf_Data /app/logs

exec gosu appuser "$@"
