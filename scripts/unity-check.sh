#!/usr/bin/env bash
#
# Compila el proyecto dentro de Unity y ejecuta los tests EditMode, en batchmode
# y sin abrir el editor.
#
# Complementa a scripts/test.sh: aquel verifica el dominio en segundos, este
# verifica lo que solo Unity puede verificar (que los MonoBehaviour compilan,
# que los ensamblados están bien referenciados, que los assets importan).
#
# IMPORTANTE: el editor tiene que estar CERRADO. Unity bloquea el proyecto con
# Temp/UnityLockfile y una segunda instancia falla o corrompe la Library.
#
set -euo pipefail

RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY="${UNITY_EXE:-/mnt/d/Aplicaciones/UnityHub/6000.3.2f1/Editor/Unity.exe}"

if [[ ! -x "$UNITY" ]]; then
    echo "No encuentro el editor de Unity en:" >&2
    echo "  $UNITY" >&2
    echo "Define UNITY_EXE con la ruta correcta." >&2
    exit 1
fi

if [[ -f "$RAIZ/Temp/UnityLockfile" ]]; then
    echo "El proyecto parece abierto en el editor (existe Temp/UnityLockfile)." >&2
    echo "Cierra Unity y vuelve a lanzar este script." >&2
    exit 1
fi

LOG="$RAIZ/Tests/unity-check.log"
RESULTADOS="$RAIZ/Tests/unity-editmode-results.xml"
rm -f "$LOG" "$RESULTADOS"

echo "Ejecutando Unity en batchmode (esto tarda: tiene que importar assets)..."

set +e
"$UNITY" \
    -batchmode \
    -nographics \
    -projectPath "$(wslpath -w "$RAIZ")" \
    -runTests \
    -testPlatform EditMode \
    -testResults "$(wslpath -w "$RESULTADOS")" \
    -logFile "$(wslpath -w "$LOG")"
CODIGO=$?
set -e

# Códigos de -runTests: 0 todo pasa, 2 hay tests fallando, 3 la ejecución falló.
case "$CODIGO" in
    0) echo "OK: compila y los tests EditMode pasan." ;;
    2) echo "FALLO: hay tests EditMode en rojo. Detalle en $RESULTADOS" >&2 ;;
    3) echo "FALLO: Unity no pudo ejecutar los tests (¿error de compilación?)." >&2 ;;
    *) echo "FALLO: Unity terminó con código $CODIGO." >&2 ;;
esac

if [[ "$CODIGO" -ne 0 && -f "$LOG" ]]; then
    echo "--- errores del log ---" >&2
    grep -E "error CS|Compilation failed|Assembly.*error" "$LOG" | head -30 >&2 || true
    echo "--- log completo en $LOG ---" >&2
fi

exit "$CODIGO"
