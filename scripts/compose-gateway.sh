#!/usr/bin/env bash
# Compõe o schema do gateway GraphQL (Fusion) a partir dos subgraphs.
#
#   ./scripts/compose-gateway.sh
#
# 1. Exporta o schema de cada subgraph (schema.graphqls; o schema-settings.json é mantido, com o endereço
#    por ambiente). Não precisa do banco nem da API no ar.
# 2. Gera um .far por ambiente no Gateway.Web.Api, com o endereço do schema-settings.json:
#    gateway.local.far (dotnet run) e gateway.compose.far (docker compose).
#
# Rode depois de mudar o schema de um subgraph e versione os arquivos gerados; o Gateway.Tests falha se o
# schema versionado não for o que o subgraph exporta.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GATEWAY="$ROOT/src/backend/Gateway/Gateway.Web.Api"
# projeto do subgraph:nome do schema (o "name" do schema-settings.json).
SUBGRAPHS=(
  "src/backend/Catalog/Catalog.Web.Api:catalog"
  "src/backend/Customers/Customers.Web.Api:customers"
  "src/backend/Store/Orders.Web.Api:orders"
)

cd "$ROOT"
dotnet tool restore > /dev/null

schemas=()
for subgraph in "${SUBGRAPHS[@]}"; do
  project="$ROOT/${subgraph%%:*}"
  name="${subgraph##*:}"
  echo "Exportando o schema $name..."
  (cd "$project" && dotnet run -- schema export --schema-name "$name" --output schema.graphqls > /dev/null)
  schemas+=(-f "$project/schema.graphqls")
done

for env in local compose; do
  rm -f "$GATEWAY/gateway.$env.far"
  dotnet nitro fusion compose "${schemas[@]}" --archive "$GATEWAY/gateway.$env.far" --env "$env" > /dev/null
  echo "Gerado gateway.$env.far"
done
