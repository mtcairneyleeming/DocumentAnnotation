# shellcheck shell=bash
# Sourced by update.sh and deploy.sh: puts dotnet and npm on the PATH (user-level installs).
if ! command -v dotnet >/dev/null 2>&1 && [ -x "$HOME/.dotnet/dotnet" ]; then
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$PATH"
fi
if ! command -v npm >/dev/null 2>&1 && [ -s "$HOME/.nvm/nvm.sh" ]; then
  # shellcheck disable=SC1091
  . "$HOME/.nvm/nvm.sh" >/dev/null
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
for tool in dotnet npm git; do
  command -v "$tool" >/dev/null 2>&1 || { echo "Missing $tool: see ops/README.md (Locally)" >&2; exit 1; }
done
