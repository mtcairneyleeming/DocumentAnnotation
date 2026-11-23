#!/usr/bin/env bash
# Bumps the dependencies, builds, and shows what changed. It never deploys: review the diff, test,
# commit if you like, then run ops/deploy.sh.
#   NuGet: the newest stable version within the same major version (10.x for the Microsoft and Npgsql
#          packages, 1.x for Markdig, 4.x for Serilog, ...). Nothing crosses to the next major.
#   npm:   `npm update`, so only within the ranges in server/package.json.
set -euo pipefail
cd "$(dirname "$0")/.."
. ops/common.sh

if [ -n "$(git status --porcelain)" ]; then
  echo "WARNING: uncommitted changes in the working tree; the dependency changes will mix with them in git diff."
fi

echo "== NuGet: newest versions within each package's major version"
python3 -I - server/server.csproj <<'PY'
import json, re, sys, urllib.request

path = sys.argv[1]
raw = open(path, encoding="utf-8-sig").read()
bom = open(path, "rb").read(3) == b"\xef\xbb\xbf"

def newest_in_major(package, current):
    url = f"https://api.nuget.org/v3-flatcontainer/{package.lower()}/index.json"
    with urllib.request.urlopen(url, timeout=30) as r:
        versions = json.load(r)["versions"]
    major = int(current.split(".")[0])
    key = lambda v: tuple(int(x) for x in v.split("."))
    stable = [v for v in versions if "-" not in v and int(v.split(".")[0]) == major]
    return max(stable, key=key) if stable else current

def bump(match):
    package, version = match.group(2), match.group(4)
    new = newest_in_major(package, version)
    if new != version:
        print(f"  {package}: {version} -> {new}")
    return f"{match.group(1)}{package}{match.group(3)}{new}{match.group(5)}"

pattern = re.compile(r'(<PackageReference\s+Include=")([^"]+)("\s+Version=")([^"]+)(")')
out = pattern.sub(bump, raw)
if out != raw:
    open(path, "w", encoding="utf-8-sig" if bom else "utf-8").write(out)
else:
    print("  no changes")
PY
dotnet restore server/server.csproj --force-evaluate >/dev/null

echo "== npm: updating within the ranges in package.json"
(cd server && npm update --no-fund)

echo "== Building (also builds the frontend)"
dotnet build server/server.csproj -c Release --no-restore

echo
echo "== dotnet list package --vulnerable --include-transitive"
dotnet list server/server.csproj package --vulnerable --include-transitive || true
echo
echo "== dotnet list package --outdated (anything not within the same major is deliberately left)"
dotnet list server/server.csproj package --outdated || true
echo
echo "== npm audit"
(cd server && npm audit) || true
echo
echo "== npm outdated"
(cd server && npm outdated) || true
echo
echo "== git diff --stat"
git diff --stat
echo
echo "Done. Review the diff above, try the app locally, then run ops/deploy.sh. Nothing was deployed."
