#!/usr/bin/env bash
#
# Tests del dominio, sin abrir Unity. Es el bucle de verificación por defecto:
# tarda segundos y no toca el editor.
#
#   scripts/test.sh                  todos los tests
#   scripts/test.sh --filter Board   solo los que casen con el filtro
#
set -euo pipefail

RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$RAIZ/Tests"

exec dotnet test "$@"
