# Mana Master

Juego de cartas por turnos en Unity 6000.3.2f1 (URP 2D, Input System nuevo).
Recreación de *Card Monsters*.

**`DESIGN.md` es la fuente de verdad.** Ante cualquier discrepancia entre el
código y el documento, manda el documento. Si una regla del juego cambia, se
cambia primero ahí y después en el código.

---

## Comandos

```bash
scripts/test.sh          # tests del dominio con dotnet, sin abrir Unity (segundos)
scripts/unity-check.sh   # compila en Unity y corre los EditMode en batchmode (minutos)
```

`scripts/test.sh` es el bucle por defecto. `unity-check.sh` **requiere que el
editor esté cerrado** (Unity bloquea el proyecto) y es lo que detecta lo que
dotnet no ve: MonoBehaviours que no compilan, referencias de ensamblado rotas,
assets que no importan.

Antes de dar por cerrado cualquier trabajo, los dos en verde.

---

## Ensamblados

| Ensamblado | Dónde | Qué va aquí |
|---|---|---|
| `ManaMaster.Core` | `Assets/_Project/Scripts/Core/` | Reglas del juego. **C# puro.** |
| `ManaMaster.Unity` | `Assets/_Project/Scripts/Unity/` | ScriptableObjects y adaptadores |
| `ManaMaster.Core.Tests` | `Assets/_Project/Tests/Core/` | Tests NUnit del dominio |
| `Assembly-CSharp` | `Assets/Scripts/` | Código heredado, en retirada |

**`ManaMaster.Core` declara `noEngineReferences: true`.** No es una convención,
es el compilador: un `using UnityEngine` ahí dentro no compila. Si necesitas un
tipo de Unity en el Core, la respuesta correcta es una interfaz en el Core y la
implementación en `ManaMaster.Unity` — como `IMonsterCard` y
`MonsterCardDefinition`. Nunca añadir la referencia al motor.

Esto es lo que permite que los tests corran sin editor y que las reglas puedan
ejecutarse en servidor cuando llegue el online (DESIGN.md §12).

### Los tests se compilan dos veces

Los ficheros de `Assets/_Project/Tests/Core/` los compila Unity (EditMode) y
también `Tests/ManaMaster.Core.Tests.csproj` vía `dotnet test`. **Un solo
fuente.** Por eso usamos NUnit 3 y sintaxis `Assert.That(...)`: es lo que
entienden los dos.

---

## Convenciones

- **Todo en español**: comentarios, documentación XML, mensajes de log,
  nombres de tests. El código de la v1 no lleva sistema de localización.
- **Los comentarios explican el porqué, no el qué.** Si un comentario describe
  lo que la línea siguiente ya dice, sobra.
- **No renombrar campos `[SerializeField]` a la ligera**: la escena y los
  prefabs los referencian por nombre y el renombrado rompe el cableado en
  silencio. Si hay que hacerlo, se migra la escena en el mismo commit.
- **Los `.meta` viajan con su fichero.** Mover un `.cs` sin su `.meta` le
  cambia el GUID y rompe todas las referencias de assets y escenas a ese script.
- **Nada nuevo en `Assets/Resources/`** (DESIGN.md §12). Lo que queda ahí es
  heredado y se saca en la Fase 6.
- **Sin estado `static` mutable.** Fue el problema central de la Fase 1: no
  se reinicia entre partidas ni entre recargas de escena.
- **Las escenas se construyen con scripts de editor versionados**, no cableando
  a mano en el Inspector.

---

## Dónde estamos

Fases y orden de ejecución, en `DESIGN.md` §11. Resumen:

- **Hecho**: Fase 1 (saneamiento) y Fase 0 (andamiaje: Core puro, tests, scripts).
- **Siguiente**: Fase 2, el motor de reglas del duelo. Hoy **no existe combate**:
  nadie ataca, nadie cura y nadie gana.
- El código de `Assets/Scripts/` es transitorio y muere en la Fase 3.

Cada fase se cierra con los dos comandos de verificación en verde y un commit
propio.
