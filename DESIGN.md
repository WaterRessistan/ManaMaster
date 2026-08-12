# Mana Master — Reglamento y diseño

> Documento de referencia del proyecto. **Es la fuente de verdad**: ante cualquier
> discrepancia con el documento original de Google Docs o con el código, manda este
> fichero. Si una regla cambia, se cambia aquí primero.

Recreación del juego de cartas **Card Monsters** (título original de móvil, ya cerrado).
Motor: **Unity 6000.3.2f1**, URP 2D, Input System nuevo.

---

## 1. Visión general

Juego de cartas por turnos, 1 contra 1 (contra IA en la v1, online más adelante).
Cada jugador lleva una baraja personalizada. **Pierde el jugador que se queda sin
cartas de monstruo** en baraja, mano y arena a la vez.

Partidas cortas, al estilo del original: una batalla en pocos minutos.

---

## 2. Tablero

Tres carriles por bando, con roles distintos. Cada bando ocupa **una sola
fila**, con el principal centrado entre los dos traseros:

```
              RIVAL
   [3]        [1]        [2]
 traseros   principal   traseros
 (rango)     (melee)     (rango)
 ───────────────────────────────
   [2]        [1]        [3]
 traseros   principal   traseros
 (rango)     (melee)     (rango)
              TÚ
```

- **Máximo 3 monstruos** desplegados por jugador.
- En código los carriles se indexan **0, 1, 2**; al jugador se le muestran como
  **"Carril 1, 2 y 3"**. Ver `BoardLanes`.

### Invariante: la arena nunca tiene huecos

El carril 1 siempre está ocupado si hay al menos un monstruo. Cualquier hueco se
cierra automáticamente desplazando las cartas hacia delante.

---

## 3. Colocación de monstruos

Requiere: **maná suficiente** + **al menos un carril libre**.

**Inserción con empuje.** El jugador elige la posición de inserción, y todo lo que
haya de esa posición hacia atrás retrocede un carril:

```
Estado:   [1] A   [2] B   [3] —

Inserta C en 1  →  [1] C   [2] A   [3] B
Inserta C en 2  →  [1] A   [2] C   [3] B
Inserta C en 3  →  [1] A   [2] B   [3] C
```

- Posiciones de inserción válidas: de `1` a `(ocupadas + 1)`. **Nunca se dejan huecos**:
  no se puede ocupar el carril 3 si el 1 o el 2 están libres.
- Una vez en la arena, **las cartas no se reordenan a voluntad**. La única forma de
  mover un monstruo hacia atrás es insertar otro por delante.

**En la interfaz, el carril sobre el que sueltas la carta es la posición de
inserción.** Soltar sobre el carril 2 mete la carta en el 2 y empuja hacia atrás
lo que hubiera. Durante el arrastre se resaltan las posiciones válidas, que son
las de `1` a `(ocupadas + 1)`.

> Esta es la salida al problema del monstruo de rango atrapado en el carril 1: se
> juega un melee delante y el rango pasa al carril 2, desde donde ya puede atacar.

---

## 4. Cartas

### Atributos de un monstruo

| Atributo | Descripción |
|---|---|
| **Vida** | Al llegar a 0 queda fuera de combate |
| **Ataque** | Daño que inflige a un monstruo rival |
| **Cura** | Vida que restaura a **cada** aliado en arena, incluido él mismo. 0 = no cura |
| **Maná** | Coste de despliegue |
| **Melee** | Puede atacar desde el carril principal |
| **Rango** | Puede atacar desde los carriles traseros |

**Melee y Rango son dos capacidades independientes**, no un booleano único:

| Tipo | Melee | Rango | Comportamiento |
|---|---|---|---|
| Melee puro | ✅ | ❌ | Solo ataca desde el carril 1 |
| Rango puro | ❌ | ✅ | Solo ataca desde los carriles 2 y 3 |
| **Mixta** (cartas especiales de rareza alta) | ✅ | ✅ | Ataca desde cualquier carril, según su posición |

Un monstruo situado en un carril desde el que no puede atacar **sigue siendo un
objetivo válido**: simplemente no ataca.

### Cartas de objeto

No aparecen físicamente en la arena; se aplican sobre un monstruo para darle una
ventaja (hoy, un bonus numérico a ataque, vida máxima o cura por turno; más
adelante también habilidades especiales, como el ejemplo original de "permitir
atacar a distancia desde el carril 1").

**Reglas de equipamiento:**

- **Un monstruo lleva como máximo un objeto.** No se puede sustituir ni quitar
  una vez puesto: la única forma de perderlo es que el monstruo muera (por
  combate o por sacrificio), y en ese caso el objeto se pierde con él.
- Se equipa desde la mano de objetos, **gratis** (sin coste de maná): el coste
  económico ya se pagó en diamantes al comprarlo en la Tienda.
- Se compran con diamantes 💎 en la Tienda, se coleccionan y forman parte del
  mazo de 10+10 (máximo 2 copias por carta, igual que los monstruos).
- **Estado: Fase 7, implementada.** El Rival (IA) no equipa objetos todavía
  —no forma parte de esta tanda—, y los sobres de la Tienda solo dan
  monstruos: los objetos se venden sueltos.

**Pociones.** Un objeto puede marcarse como poción (hoy, los que solo dan
bonus de vida máxima). Una poción **no ocupa el hueco de objeto** del
monstruo: se aplica al momento y no cuenta para "máximo un objeto", así que
un monstruo puede llevar una poción y además un objeto normal, en cualquier
orden, y ni una ni otro se bloquean entre sí. Tampoco tiene tope de usos: se
puede aplicar más de una poción al mismo monstruo, y cada una suma.

---

## 5. Estructura del turno

```
1. +3 de maná
2. FASE PRINCIPAL  (el jugador actúa libremente)
     · desplegar monstruos
     · usar objetos
     · sacrificar monstruos propios
3. → botón FINALIZAR TURNO
4. FASE DE COMBATE  (automática)
     a. CURACIÓN — todos los monstruos curanderos sanan a sus aliados
     b. ATAQUES  — carril 1, luego 2, luego 3
5. Turno del rival, idéntico
```

Cada cambio de turno es un **cambio de ronda**: turno y ronda son el mismo número,
y la interfaz muestra **solo el contador de Ronda**. El jugador inicial se elige al azar.
Un monstruo desplegado **ataca en la fase de combate de ese mismo turno** (no hay
enfermedad de invocación).

---

## 6. Combate

### Objetivos

| Atacante | Objetivo |
|---|---|
| Carril 1 (melee) | Carril 1 del rival |
| Carriles 2 y 3 (rango) | Carriles **traseros** del rival, **cruzados**: mi 2 → su 3, mi 3 → su 2 |

Reglas de sustitución para los atacantes a distancia:

1. Si el rival solo tiene **un** carril trasero ocupado → **ambos** rangos atacan a ese.
2. Si el rival **no tiene ningún carril trasero** ocupado → atacan a su **carril 1**.
3. Si el rival **no tiene nada** en la arena → el ataque no hace nada.

### Resolución secuencial

Los ataques se resuelven **en orden: carril 1, después 2, después 3**.

Cuando un ataque mata a un monstruo, **la compactación ocurre en ese momento**, con
una breve pausa, antes de que ataque el siguiente carril. El atacante siguiente
recalcula su objetivo sobre el tablero **ya compactado**.

> Consecuencia táctica: el orden de tus carriles importa mucho, y un monstruo
> potente puede encadenar bajas cuyo hueco va ocupando la carta de detrás.

### Curación

- Se aplica **antes** de todos los ataques.
- El curandero cura a **cada** aliado en arena, **incluido él mismo**.
- **No puede superar la vida máxima** del monstruo (lo garantiza el motor:
  `CardInstance.ReceiveHealing` recorta la curación a lo que falte para
  llegar al máximo, por muchas veces que se aplique).
- Solo mientras el curandero esté en la arena.

---

## 7. Maná y sacrificio

- **+3 de maná** al empezar cada turno.
- **El maná no gastado se acumula de un turno al siguiente, y no hay tope.** Los
  incrementos progresivos y un posible máximo son materia de la fase de balanceo.
- **Sacrificio voluntario:** un monstruo propio en la arena puede retirarse a
  voluntad para recuperar maná. Devuelve **la mitad de su coste, redondeando hacia
  abajo** (`coste / 2`). El monstruo sale de la partida definitivamente y su hueco
  se compacta.

> ⚠️ Con la fórmula actual, un monstruo de coste 1 devuelve 0 de maná. Es un valor
> configurable, pendiente de la fase de balanceo.

---

## 8. Mazo y mano

- **Mazo: 10 cartas de monstruo + 10 cartas de objeto.**
- **Máximo 2 copias** de cada carta por mazo.
- **Dos manos separadas**, una de monstruos y otra de objetos.
- Cada mano muestra **2 cartas**. Al jugar una, se reemplaza al instante por otra
  aleatoria del mazo.
- Cuando en el mazo solo queda 1 carta, la mano muestra 1. No hay fase de robo
  explícita: el reemplazo es inmediato.

---

## 9. Condición de derrota

Un jugador pierde cuando:

- **Se queda sin cartas de monstruo** en baraja, mano y arena a la vez
  (las 10 han sido derrotadas), **o**
- **No tiene monstruos en la arena y no le llega el maná** para desplegar ninguno.

No existe vida de héroe: toda la presión del juego viene de perder monstruos.

### Empate: válvula de seguridad, no un desenlace de diseño

Las dos condiciones de arriba son las únicas formas **previstas** de terminar
una partida. No hay tablas por diseño.

Existe, aun así, un tope técnico de **300 rondas** que fuerza el fin de la
partida si se llega hasta ahí, para cubrir una situación rara pero posible:
con las dos arenas llenas nadie puede desplegar, y si la curación iguala al
daño que se hacen, ningún monstruo muere y la partida no acabaría nunca.
Simulando 2.000 partidas con el roster anterior (varios curanderos)
aparecía en un 0,45 % de los casos; con un solo curandero en el roster
(§13) esa coincidencia exacta es todavía más improbable. En la práctica un
jugador no debería llegar nunca a verlo.

> El límite anterior era 60, medido para que las partidas normales (que
> duran entre 11 y 36 rondas) tuvieran de sobra margen. Se subió a 300 al
> dejar de tratarse como un desenlace normal: ahora solo importa que sea
> lo bastante alto para no colgar la partida en el caso extremo, no que sea
> ajustado.

> 📌 El documento original decía *"ganará el jugador que se quede sin cartas"*. Es
> una errata: **pierde**.

---

## 10. Meta-juego

### Moneda: Diamantes 💎

Se ganan jugando partidas, con bonus por victoria. Se gastan en la tienda, tanto en
cartas sueltas como en sobres.

### Valores provisionales

Pendientes de la fase de balanceo. Vivirán en un ScriptableObject de configuración.

| Concepto | Valor |
|---|---|
| Victoria / Derrota | 50 💎 / 15 💎 |
| Empate (a cada jugador) | 30 💎 |
| Sobre (3 cartas, ≥1 Rara garantizada) | 100 💎 |
| Carta suelta: Común / Rara / Épica / Legendaria | 50 / 150 / 500 / 1500 💎 |
| Probabilidad de sobre | Común 70% · Rara 25% · Épica 4,5% · Legendaria 0,5% |
| Cuenta nueva | 500 💎 + mazo inicial |

### Pantallas

1. **Inicio** — jugar, tienda, mazos, opciones
2. **Tienda** — sobres y cartas sueltas
3. **Deckbuild** — construcción del mazo 10+10
4. **Duelo** — la partida

---

## 11. Hoja de ruta

Los números de fase son nombres estables: el resto del documento los cita, así que
no se renumeran. El **orden de ejecución** es otra cosa y va aparte.

| Fase | Contenido | Orden | Estado |
|---|---|---|---|
| **1** | Saneamiento, git, ScriptableObjects, eliminar estado global | 1.º | ✅ Hecha |
| **0** | Andamiaje: Core sin Unity, tests, scripts de verificación | 2.º | ✅ Hecha |
| **2** | Motor de reglas del duelo (dominio puro + tests) | 3.º | ✅ Hecha |
| **3** | Duelo jugable contra IA | 4.º | ✅ Hecha |
| **5** | Las 4 pantallas y el flujo entre ellas | 5.º | ✅ Hecha |
| **4** | Persistencia local, colección, diamantes | 6.º | ✅ Hecha |
| **6** | Contenido, balanceo, arte, audio | 7.º | 🔨 Siguiente |
| **7** | Objetos y equipamiento | — | ✅ Hecha (adelantada) |
| **8** | Adaptación a móvil | — | Fuera de la v1 |
| **9** | Online y backend | — | Fuera de la v1 |

**La v1 son las fases 0 a 6.** La 5 se adelanta a la 4 porque la persistencia
guarda mazos y colección, y esas pantallas son las que definen qué hay que
guardar: hacerla antes obliga a rehacer el formato de guardado. La 7 estaba
marcada "fuera de la v1", pero se adelantó a petición explícita del usuario
antes de cerrar la 6; el resto de fases fuera de la v1 (8 y 9) siguen sin
tocar.

---

## 12. Decisiones técnicas

- **Datos de carta en ScriptableObjects** (`CardDefinition` y derivadas). Las
  definiciones son plantillas inmutables compartidas; el estado de partida vive en
  `CardInstance`. Sin esta separación, dañar un monstruo modificaría el asset y
  afectaría a todas sus copias.
- **El motor de reglas no depende de Unity.** Vive en `ManaMaster.Core`, sin
  MonoBehaviours, de modo que se puede testear sin abrir el editor y, más adelante,
  ejecutar en el servidor para el modo online. Esto no es una buena intención: el
  ensamblado declara `noEngineReferences: true`, así que **el compilador rechaza
  cualquier `using UnityEngine` que se cuele**. Los ScriptableObject de definición
  de carta viven aparte, en `ManaMaster.Unity`, y el Core los ve solo a través de
  la interfaz `IMonsterCard`.
- **Los mismos tests corren en dos sitios.** Un único conjunto NUnit se compila
  desde Unity (EditMode) y desde `dotnet test`, que no necesita el editor y tarda
  segundos. Es el bucle de verificación por defecto.
- **Las escenas se construyen con scripts de editor versionados**, no cableando a
  mano en el Inspector: así la jerarquía se revisa en el diff y se puede regenerar.
- **Solo español en la v1**, sin sistema de localización. Los textos van
  directamente en la interfaz.
- **Nada en `Resources/`.** Carga todo en memoria al arrancar; mal para móvil.
- **Diseño pensado para móvil desde el principio** aunque la v1 sea de PC: anclas
  responsive, input por puntero abstracto, sin dependencia del hover.
- **`CardId` es el nombre del asset.** Se usa como clave en la colección y en los
  mazos guardados: renombrar un asset invalida las partidas guardadas.

---

## 13. Puntos abiertos

- Precio y contenido exactos de los sobres (valores provisionales en §10).
- Reparto de rarezas del set de cartas actual. Hoy hay 9 monstruos (4 comunes,
  4 raras, 0 épicas, 1 legendaria) y 5 cartas de objeto (2 comunes, 2 raras,
  1 épica): el mazo de 10+10 del §8 ya se puede construir (con alguna carta
  repetida hasta el máximo de 2 copias), pero con poca variedad — ampliar el
  roster sigue siendo trabajo de la Fase 6.
- Arte definitivo de la carta: la plantilla actual es provisional.

> Resuelto y movido al §3: el drag & drop **sí** elige posición de inserción,
> soltando sobre el carril donde quieres que entre la carta.
