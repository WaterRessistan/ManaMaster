using ManaMaster.Core.Board;
using UnityEngine;

/// <summary>
/// Los tres carriles de la arena de un jugador.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). Se mantienen los tres campos sueltos porque la escena
/// ya los tiene cableados, pero se exponen además como una colección indexable,
/// que es como los tratará el motor de reglas.
///
/// La compactación de carriles (al vaciarse un carril, los de detrás avanzan) se
/// implementa en la Fase 2 dentro del dominio, no aquí.
/// </remarks>
public class Tablero : MonoBehaviour
{
    [Header("Carriles (1 = principal, 2 y 3 = traseros)")]
    // Nombres conservados: la escena ya referencia estos campos.
    [SerializeField] private BoardSlot boardSlotPrincipal;
    [SerializeField] private BoardSlot boardSlotSecundario;
    [SerializeField] private BoardSlot boardSlotTerciario;

    private BoardSlot[] _carriles;

    /// <summary>
    /// Jugador dueño de esta arena. Lo registra <see cref="Jugador"/> al
    /// despertar; sin esto los carriles no sabrían de quién son y aceptarían
    /// cartas del rival.
    /// </summary>
    public Jugador Propietario { get; private set; }

    public void RegistrarPropietario(Jugador jugador) => Propietario = jugador;

    /// <summary>Carriles ordenados: índice 0 = principal, 1 y 2 = traseros.</summary>
    public BoardSlot[] Carriles => _carriles ??= new[]
    {
        boardSlotPrincipal,
        boardSlotSecundario,
        boardSlotTerciario
    };

    public BoardSlot GetCarril(int laneIndex)
        => BoardLanes.IsValid(laneIndex) ? Carriles[laneIndex] : null;

    /// <summary>Número de carriles ocupados.</summary>
    public int CartasEnJuego
    {
        get
        {
            int contador = 0;
            foreach (BoardSlot carril in Carriles)
            {
                if (carril != null && carril.IsOccupied())
                {
                    contador++;
                }
            }

            return contador;
        }
    }

    public bool EstaVacio => CartasEnJuego == 0;
    public bool EstaLleno => CartasEnJuego >= BoardLanes.Count;

    /// <summary>
    /// Primer carril libre respetando el llenado en orden, o -1 si está lleno.
    /// </summary>
    public int PrimerCarrilLibre()
    {
        for (int i = 0; i < Carriles.Length; i++)
        {
            if (Carriles[i] == null || !Carriles[i].IsOccupied())
            {
                return i;
            }
        }

        return -1;
    }

    private void Awake()
    {
        for (int i = 0; i < Carriles.Length; i++)
        {
            if (Carriles[i] == null)
            {
                Debug.LogError(
                    $"[Tablero] {BoardLanes.ToDisplayName(i)} sin asignar.", this);
                continue;
            }

            Carriles[i].Configurar(this, i);
        }
    }
}
