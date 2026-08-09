#!/usr/bin/env bash
#
# Compila el proyecto dentro de Unity y ejecuta los tests, en batchmode y sin
# abrir el editor.
#
#   scripts/unity-check.sh            EditMode y PlayMode
#   scripts/unity-check.sh EditMode   solo EditMode (mas rapido)
#   scripts/unity-check.sh PlayMode   solo PlayMode
#
# Complementa a scripts/test.sh: aquel verifica el dominio en segundos, este
# verifica lo que solo Unity puede verificar (que los MonoBehaviour compilan,
# que los ensamblados estan bien referenciados, que los assets importan) y, en
# PlayMode, que la escena de duelo arranca y se juega de verdad.
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

MODOS=("EditMode" "PlayMode")
if [[ $# -ge 1 ]]; then
    MODOS=("$1")
fi

CODIGO=0

for MODO in "${MODOS[@]}"; do
    minusculas="$(echo "$MODO" | tr '[:upper:]' '[:lower:]')"
    LOG="$RAIZ/Tests/unity-${minusculas}.log"
    RESULTADOS="$RAIZ/Tests/unity-${minusculas}-results.xml"
    rm -f "$LOG" "$RESULTADOS"

    echo "Ejecutando Unity en batchmode ($MODO)..."

    set +e
    "$UNITY" \
        -batchmode \
        -nographics \
        -projectPath "$(wslpath -w "$RAIZ")" \
        -runTests \
        -testPlatform "$MODO" \
        -testResults "$(wslpath -w "$RESULTADOS")" \
        -logFile "$(wslpath -w "$LOG")"
    ESTE=$?
    set -e

    # Codigos de -runTests: 0 todo pasa, 2 hay tests fallando, 3 la ejecucion
    # fallo (normalmente un error de compilacion).
    if [[ "$ESTE" -eq 0 ]]; then
        RESUMEN="$(grep -o 'total="[0-9]*" passed="[0-9]*" failed="[0-9]*"' \
            "$RESULTADOS" 2>/dev/null | head -1 || true)"
        echo "  OK $MODO ${RESUMEN}"
        continue
    fi

    CODIGO="$ESTE"

    case "$ESTE" in
        2) echo "  FALLO $MODO: hay tests en rojo. Detalle en $RESULTADOS" >&2 ;;
        3) echo "  FALLO $MODO: Unity no pudo ejecutarlos (error de compilacion?)." >&2 ;;
        *) echo "  FALLO $MODO: Unity termino con codigo $ESTE." >&2 ;;
    esac

    if [[ -f "$LOG" ]]; then
        grep -E "error CS|Compilation failed|Assembly.*error" "$LOG" | head -20 >&2 || true
        echo "  log completo en $LOG" >&2
    fi
done

exit "$CODIGO"
