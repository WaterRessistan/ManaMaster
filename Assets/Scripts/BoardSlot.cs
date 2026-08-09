using ManaMaster.Core.Board;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Un carril de la arena. Acepta que se suelte una carta sobre él.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). Antes cualquier carril aceptaba cualquier carta: no
/// sabía a qué jugador pertenecía, así que se podían soltar cartas en el tablero
/// del rival, y descontaba el maná del primer <c>Jugador</c> que encontrase en la
/// escena con <c>FindObjectOfType</c>.
///
/// Ahora conoce su tablero y su índice de carril, valida el propietario y exige
/// que los carriles se llenen en orden.
///
/// FASE 2: la inserción con empuje (colocar delante de otra carta y desplazarla
/// hacia atrás) y la compactación al morir un monstruo se resolverán en el
/// dominio; este componente pasará a ser solo la zona de soltado.
/// </remarks>
[RequireComponent(typeof(RectTransform))]
public class BoardSlot : MonoBehaviour, IDropHandler
{
    private Tablero _tablero;
    private int _laneIndex = -1;
    private SistemaTurnos _turnos;

    /// <summary>Índice de carril: 0 = principal, 1 y 2 = traseros.</summary>
    public int LaneIndex => _laneIndex;

    public Tablero Tablero => _tablero;

    /// <summary>Llamado por <see cref="Tablero"/> al inicializarse.</summary>
    public void Configurar(Tablero tablero, int laneIndex)
    {
        _tablero = tablero;
        _laneIndex = laneIndex;
    }

    public bool IsOccupied() => transform.childCount > 0;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject arrastrada = eventData.pointerDrag;
        if (arrastrada == null)
        {
            return;
        }

        if (!PuedeAceptarCarta(arrastrada, out DisplayCard carta, out Jugador propietario))
        {
            return;
        }

        if (!propietario.TryGastarMana(carta.CosteMana))
        {
            Debug.Log($"[BoardSlot] Maná insuficiente: la carta cuesta " +
                      $"{carta.CosteMana} y hay {propietario.Mana}.");
            return;
        }

        DesplegarCopiaEnCarril(carta);

        // Antes se movía al carril el propio objeto de la mano, así que el hueco
        // desaparecía para siempre: tras jugar dos cartas te quedabas sin mano el
        // resto de la partida. Ahora al carril va una copia, y el original vuelve
        // a la mano para que la baraja lo reponga.
        if (arrastrada.TryGetComponent(out DraggableCard arrastrable))
        {
            arrastrable.MarcarComoJugada(propietario.Baraja);
        }
    }

    private bool PuedeAceptarCarta(
        GameObject arrastrada, out DisplayCard carta, out Jugador propietario)
    {
        carta = null;
        propietario = null;

        if (_tablero == null || !BoardLanes.IsValid(_laneIndex))
        {
            Debug.LogError("[BoardSlot] Carril sin configurar por su Tablero.", this);
            return false;
        }

        if (IsOccupied())
        {
            return false;
        }

        carta = arrastrada.GetComponent<DisplayCard>();
        if (carta == null || !carta.TieneCarta)
        {
            Debug.LogWarning("[BoardSlot] El objeto arrastrado no es una carta válida.");
            return false;
        }

        propietario = _tablero.Propietario;
        if (propietario == null)
        {
            Debug.LogError("[BoardSlot] El tablero no tiene propietario asignado.", this);
            return false;
        }

        // Antes faltaba esta comprobación: se podían soltar cartas en el
        // tablero del rival.
        if (Turnos != null && Turnos.JugadorActivo != propietario)
        {
            Debug.Log("[BoardSlot] No es tu turno, o este no es tu tablero.");
            return false;
        }

        // Los carriles se llenan en orden: no se puede ocupar el 3 sin que el
        // 1 y el 2 lo estén.
        int primerLibre = _tablero.PrimerCarrilLibre();
        if (primerLibre < 0)
        {
            Debug.Log("[BoardSlot] La arena ya tiene los 3 monstruos desplegados.");
            return false;
        }

        if (_laneIndex != primerLibre)
        {
            Debug.Log($"[BoardSlot] Hay que llenar los carriles en orden: " +
                      $"toca el {BoardLanes.ToDisplayName(primerLibre)}.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Crea en este carril la carta desplegada, a partir de la que se arrastró
    /// desde la mano.
    /// </summary>
    private void DesplegarCopiaEnCarril(DisplayCard origen)
    {
        GameObject copia = Instantiate(origen.gameObject, transform);
        copia.name = origen.gameObject.name;

        // Una carta ya desplegada no se vuelve a arrastrar.
        if (copia.TryGetComponent(out DraggableCard arrastrable))
        {
            Destroy(arrastrable);
        }

        // Instantiate copia los campos serializados, pero no el estado de
        // ejecución: hay que reasignar la instancia de partida para que la copia
        // comparta la vida actual del monstruo y no una carta vacía.
        if (copia.TryGetComponent(out DisplayCard vista))
        {
            vista.Mostrar(origen.Carta);
        }

        // El original está a media transparencia y sin raycasts porque se está
        // arrastrando; la copia no debe heredar ese estado.
        if (copia.TryGetComponent(out CanvasGroup grupo))
        {
            grupo.alpha = 1f;
            grupo.blocksRaycasts = true;
        }

        if (copia.transform is RectTransform rectCopia)
        {
            rectCopia.position = ((RectTransform)transform).position;
        }
    }

    /// <summary>
    /// TRANSITORIO: búsqueda perezosa del sistema de turnos. En la Fase 2 el
    /// MatchController inyectará esta dependencia y desaparecerá el Find.
    /// </summary>
    private SistemaTurnos Turnos
        => _turnos != null ? _turnos : _turnos = FindFirstObjectByType<SistemaTurnos>();
}
