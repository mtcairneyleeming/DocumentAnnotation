#!/usr/bin/env bash
# Publishes a clean Release build, copies it to a new release directory on the server, backs up the
# database, switches /srv/docann/current to it and restarts the app. If the health check fails it
# switches back by itself. Works with uncommitted changes (the release id gets "-dirty").
#
#   ops/deploy.sh             deploy
#   ops/deploy.sh --rollback  switch to the previous release (the database is NOT rolled back)
#   ops/deploy.sh --status    show the current release and all releases
#
# Server layout and one-time setup: ops/README.md. Every server command goes through ops/logged.sh,
# which appends it and its output to ops/ssh-log.md.
# shellcheck disable=SC2016  # the remote scripts are deliberately single-quoted
set -euo pipefail
cd "$(dirname "$0")/.."

host=${DOCANN_HOST:-hetzner}   # overrides are for testing only
base=${DOCANN_BASE:-/srv/docann}
service=dotnet-docann
port=3041
keep=3
ssh_opts=(-o ConnectTimeout=20 -o ServerAliveInterval=15 -o ServerAliveCountMax=4)

# Shell functions that run on the server (switching, restarting, health check).
read -r -d '' remote_lib <<'LIB' || true
set -euo pipefail
switch_to() {   # atomic: new symlink next to the old one, then rename over it
  ln -sfn "$1" "$BASE/.current.new"
  mv -T "$BASE/.current.new" "$BASE/current"
}
healthy() {
  for _ in $(seq 1 30); do
    if systemctl is-active --quiet "$SERVICE" && curl -fsS -o /dev/null --max-time 5 "http://127.0.0.1:$PORT/"; then
      return 0
    fi
    sleep 1
  done
  return 1
}
restart_and_check() {
  systemctl restart "$SERVICE"
  healthy
}
# Switch to $1 (a release directory); if it is not healthy go back to $2 (may be empty).
switch_with_fallback() {
  new=$1; old=${2:-}
  switch_to "$new"
  if restart_and_check; then
    echo "OK: $(basename "$new") is live and answering on port $PORT"
    return 0
  fi
  echo "HEALTH CHECK FAILED for $(basename "$new"). Last log lines:" >&2
  journalctl -u "$SERVICE" -n 25 --no-pager >&2 || true
  if [ -n "$old" ] && [ -d "$old" ]; then
    echo "Switching back to $(basename "$old")" >&2
    switch_to "$old"
    if restart_and_check; then echo "Rolled back: $(basename "$old") is live again" >&2
    else echo "ROLLBACK ALSO FAILED: look at: journalctl -u $SERVICE" >&2; fi
  else
    echo "No previous release to go back to" >&2
  fi
  return 1
}
LIB

# Runs a script on the server through ops/logged.sh. $1 = why, $2 = script body.
remote() {
  local script
  script="BASE=$base SERVICE=$service PORT=$port KEEP=$keep
$remote_lib
$2"
  ops/logged.sh "$1" ssh "${ssh_opts[@]}" "$host" "bash -c $(printf '%q' "$script")"
}

case "${1:-}" in
--status)
  remote "Status: current release and release list" '
if [ -e "$BASE/current" ]; then echo "current -> $(readlink -f "$BASE/current")"; else echo "current: (none)"; fi
echo "service: $(systemctl is-active "$SERVICE" || true)"
echo
echo "REVISION of current:"; cat "$BASE/current/REVISION" 2>/dev/null || echo "  (none)"
echo
echo "releases (newest first):"
for r in $(ls -1 "$BASE/releases" 2>/dev/null | sort -r); do
  mark=" "; [ "$(readlink -f "$BASE/current" 2>/dev/null)" = "$BASE/releases/$r" ] && mark="*"
  echo "  $mark $r"
done
echo
echo "database backups:"; ls -1t /root/backups/docann 2>/dev/null | head -5 | sed "s/^/  /"
'
  exit 0
  ;;
--rollback)
  remote "Rollback: switch to the previous release" '
cur=$(readlink -f "$BASE/current")
curname=$(basename "$cur")
prev=$(ls -1 "$BASE/releases" | sort -r | awk -v c="$curname" "f{print; exit} \$0==c{f=1}")
[ -n "$prev" ] || { echo "No release older than $curname to roll back to" >&2; exit 1; }
echo "Rolling back $curname -> $prev (the database is not touched)"
switch_with_fallback "$BASE/releases/$prev" "$cur"
'
  exit 0
  ;;
"") ;;
*)
  echo "Usage: ops/deploy.sh [--rollback | --status]" >&2
  exit 2
  ;;
esac

. ops/common.sh

# ---- release id
sha=$(git rev-parse --short HEAD)
fullsha=$(git rev-parse HEAD)
dirty=false
if [ -n "$(git status --porcelain)" ]; then
  dirty=true
  echo "WARNING: uncommitted changes in the working tree. Deploying them anyway; the release is marked -dirty."
fi
id="$(date -u +%Y%m%d-%H%M%S)-$sha"
[ "$dirty" = true ] && id="$id-dirty"
echo "Release: $id"

# ---- build (clean output directory)
tmp=$(mktemp -d)
trap 'rm -rf "$tmp"' EXIT
[ -d server/node_modules ] || (cd server && npm ci --no-fund)
dotnet publish server/server.csproj -c Release -p:UseAppHost=false -o "$tmp/out"
[ -f "$tmp/out/DocumentAnnotation.dll" ] || { echo "Publish produced no DocumentAnnotation.dll" >&2; exit 1; }
# Data comes from /srv/docann/shared/Data on the server (symlinked into the release).
rm -rf "$tmp/out/Data"

# ---- check the server is set up for the new layout (see ops/README.md)
pre=$(remote "Deploy: check the server layout" '
install -d -m 755 -o root -g root "$BASE" "$BASE/releases"
fail=0
if [ -z "$(ls -A "$BASE/shared/Data/ProcessedTexts" 2>/dev/null)" ]; then
  echo "ERROR: $BASE/shared/Data/ProcessedTexts is missing or empty (README step: move Data)" >&2; fail=1; fi
if [ ! -r /etc/docann/env ]; then
  echo "ERROR: /etc/docann/env is missing (README step: secrets)" >&2; fail=1; fi
if ! systemctl show -p ExecStart --value "$SERVICE" | grep -q "$BASE/current/DocumentAnnotation.dll"; then
  echo "ERROR: unit $SERVICE does not run $BASE/current/DocumentAnnotation.dll (README step: unit)" >&2; fail=1; fi
if ! systemctl show -p EnvironmentFiles --value "$SERVICE" | grep -q /etc/docann/env; then
  echo "ERROR: unit $SERVICE has no EnvironmentFile=/etc/docann/env (README step: unit)" >&2; fail=1; fi
[ "$fail" = 0 ] || exit 3
[ -d "$BASE/current" ] && echo "PREV=$(readlink -f "$BASE/current")"
true
') || { printf '%s\n' "$pre"; exit 1; }
printf '%s\n' "$pre" | grep -v '^PREV=' || true
prev=$(printf '%s\n' "$pre" | sed -n 's/^PREV=//p' | tail -1)
echo "Previous release: ${prev:-(none)}"

# ---- copy: hard links to unchanged files of the current release
link_dest=()
[ -n "$prev" ] && link_dest=(--link-dest="$prev/")
ops/logged.sh "Deploy: rsync release $id" \
  rsync -az --delete --chown=root:www-data --chmod=D750,F640 "${link_dest[@]}" \
  -e "ssh ${ssh_opts[*]}" "$tmp/out/" "$host:$base/releases/$id/"

# ---- REVISION, Data link, permissions, database backup, switch, health check, prune
remote "Deploy: finish release $id, back up the database, switch and restart" "
REL=$base/releases/$id
ID=$id
cat > \"\$REL/REVISION\" <<REV
release=$id
sha=$fullsha
dirty=$dirty
date=\$(date -u +%Y-%m-%dT%H:%M:%SZ)
built_on=$(hostname)
REV
ln -sfn \"\$BASE/shared/Data\" \"\$REL/Data\"
chown -h root:www-data \"\$REL/REVISION\" \"\$REL/Data\"
chmod 640 \"\$REL/REVISION\"
chmod 750 \"\$REL\"
chown root:www-data \"\$REL\"
test -f \"\$REL/DocumentAnnotation.dll\"

install -d -m 700 /root/backups/docann
dump=/root/backups/docann/annotations-\$ID.dump
runuser -u postgres -- pg_dump -Fc annotations > \"\$dump\"
test -s \"\$dump\"
chmod 600 \"\$dump\"
echo \"Database backup: \$dump (\$(du -h \"\$dump\" | cut -f1))\"

old=; [ -d \"\$BASE/current\" ] && old=\$(readlink -f \"\$BASE/current\")
if ! switch_with_fallback \"\$REL\" \"\$old\"; then
  exit 1
fi

# keep the newest \$KEEP releases (and always the live one)
live=\$(basename \"\$(readlink -f \"\$BASE/current\")\")
for r in \$(ls -1 \"\$BASE/releases\" | sort -r | tail -n +\$((KEEP + 1))); do
  [ \"\$r\" = \"\$live\" ] && continue
  echo \"Removing old release \$r\"
  rm -rf \"\${BASE:?}/releases/\$r\"
done
"
echo "Deployed $id."
