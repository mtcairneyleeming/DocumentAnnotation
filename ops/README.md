# Building and deploying

## Locally

Needs the .NET 10 SDK, Node (with npm) and git. If they are not on the PATH, `ops/common.sh` picks up
`~/.dotnet` and nvm.

```sh
cd server
npm ci                # frontend dependencies (package-lock.json)
dotnet build          # also runs `npm run build-once` (webpack) into server/wwwroot
dotnet run            # Development: needs Postgres, see server/appsettings.Development.json
```

`appsettings.Development.json` only holds placeholders (database `annotations_dev`, user `annotations`,
password `dev-password`). Swagger and sensitive-data logging are on in Development only.

## Updating dependencies

```sh
ops/update.sh
```

Moves NuGet packages to the newest version within the same major version (10.x for the Microsoft and
Npgsql packages) and npm packages within their ranges, builds, then prints `--vulnerable`, `--outdated`,
`npm audit`, `npm outdated` and `git diff --stat`. It never deploys. Majors (Bootstrap 5, jQuery 4,
webpack-cli 7, TypeScript 7, Konva 10, EF Core 11, ...) are deliberate manual jobs. `packages.lock.json`
and `server/package-lock.json` are committed.

## Deploying

```sh
ops/deploy.sh             # publish, copy, back up the database, switch, restart, health check
ops/deploy.sh --status    # current release, all releases, latest backups
ops/deploy.sh --rollback  # switch to the previous release (the database is NOT rolled back)
```

A deploy: clean `dotnet publish -c Release` into a temp dir; rsync to
`hetzner:/srv/docann/releases/<UTC YYYYmmdd-HHMMSS>-<sha>[-dirty]/` (hard links to unchanged files of the
current release); write `REVISION`, link `Data`, set `root:www-data` and mode 750/640;
`pg_dump -Fc annotations` (as `postgres`) into `/root/backups/docann/`; switch `current`
atomically; restart `dotnet-docann`; `curl http://127.0.0.1:3041/` on the server for up to 30 s. If that
fails it switches back and restarts by itself and exits non-zero. It then keeps the newest 3 releases.
Uncommitted changes are deployed with a warning and `-dirty` in the id. Database dumps are not pruned;
delete old ones by hand.

Before doing anything it checks that the server has the setup below and says what is missing. Every
server command is appended to `ops/ssh-log.md` (git ignores it). Needs `rsync` on both machines.

## One-time server setup

The app reads its secrets from environment variables and its data from `Data/`, which each release
links to `/srv/docann/shared/Data`. Run as root on the server. Do it right before the first deploy:
once the unit points at `/srv/docann/current` the app will not start until a release exists.

```sh
# 1. directories
install -d -m 755 -o root -g root /srv/docann /srv/docann/releases /srv/docann/shared

# 2. persistent data (the app only reads Data/ProcessedTexts, it writes nothing under its directory)
cp -a /server/DocumentAnnotation/Data /srv/docann/shared/Data
chown -R root:www-data /srv/docann/shared
chmod -R u=rwX,g=rX,o= /srv/docann/shared
ls /srv/docann/shared/Data/ProcessedTexts | head      # must list the *.json.gz files

# 3. secrets (not in the release). Copy the values from the old appsettings.Production.json.
install -d -m 750 -o root -g root /etc/docann
install -m 600 -o root -g root /dev/null /etc/docann/env
nano /etc/docann/env
```

`/etc/docann/env` must contain exactly these two lines (no `export`, no spaces around `=`; if a value
contains spaces or `#`, wrap it in double quotes):

```
DBConnectionString=Server=localhost;Port=5432;UserName=annotations;Password=<the password>;Database=annotations
AppConfig__InvitationCode=<the invitation code>
```

`AppConfig:AdminEmails` is not secret and ships in the release as `appsettings.Production.json`
(`mtcairneyleeming@gmail.com`). Check it matches the old production file; edit
`server/appsettings.Production.json` in the repo if not. To override it from the env file instead, add
`AppConfig__AdminEmails__0=someone@example.com` (environment variables win over the json files).

```sh
# 4. point the unit at the new layout (a drop-in; leaves the existing unit file alone)
install -d /etc/systemd/system/dotnet-docann.service.d
cat > /etc/systemd/system/dotnet-docann.service.d/docann.conf <<'EOF'
[Service]
WorkingDirectory=/srv/docann/current
EnvironmentFile=/etc/docann/env
ExecStart=
ExecStart=/usr/bin/dotnet /srv/docann/current/DocumentAnnotation.dll --urls http://127.0.0.1:3041
EOF
systemctl daemon-reload      # does not restart the running app
systemctl cat dotnet-docann  # check User=www-data and Environment=ASPNETCORE_ENVIRONMENT=Production are still there
```

Then, on your machine, `ops/deploy.sh`. The first deploy has no previous release, so no hard links and no
automatic fall-back; the old `/server/DocumentAnnotation` is still there if you need to point the unit
back at it. Afterwards check `journalctl -u dotnet-docann -n 50` (there should be no SQL and no
"Sensitive data logging" lines, and `/swagger` should return 404), then archive and remove
`/server/DocumentAnnotation`.

Nothing else is needed: the port stays 3041 and Caddy does not change. The unit's `User=www-data` only
needs read access to the release. (ASP.NET's cookie encryption keys are stored under www-data's home,
not the release; that is unchanged.)

## Rolling back

`ops/deploy.sh --rollback` switches to the next older release and restarts. A rollback does not undo
database changes: restore with
`runuser -u postgres -- pg_restore -c -d annotations /root/backups/docann/annotations-<release>.dump`
only if a release changed data you need back.
