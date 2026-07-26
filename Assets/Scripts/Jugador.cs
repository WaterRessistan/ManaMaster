using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Estado de un jugador durante la partida: su maná, su baraja y su tablero.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). El maná era <c>static</c>, de modo que los dos
/// jugadores de la escena compartían literalmente la misma variable y ninguno
/// podía tener una cantidad distinta. Ahora es estado de instancia.
///
/// En la Fase 2 pasará a ser un <c>PlayerState</c> del dominio y este componente
/// quedará como vista.
/// </remarks>
public class Jugador : MonoBehaviour
{
    [Header("Referencias de partida")]
    // Los nombres de estos campos se conservan tal cual porque la escena ya los
    // tiene cableados: renombrarlos rompería las referencias del Inspector.
    [SerializeField] private PlayerDeck barajaMonstruosJugador;
    [SerializeField] private Tablero tablero;

    [Header("UI")]
    [SerializeField] private Text contadorMana;

    /// <summary>Maná disponible. Antes era una variable estática compartida.</summary>
    public int Mana { get; private set; }

    public PlayerDeck Baraja => barajaMonstruosJugador;
    public Tablero Tablero => tablero;

    /// <summary>Se emite cada vez que cambia el maná, con el valor nuevo.</summary>
    public event Action<int> ManaCambiado;

    private void Awake()
    {
        // Sin esto los carriles no sabrían a qué jugador pertenecen.
        if (tablero != null)
        {
            tablero.RegistrarPropietario(this);
        }
    }

    private void Start()
    {
        ActualizarMana();
    }

    public void AnadirMana(int cantidad)
    {
        if (cantidad <= 0)
        {
            return;
        }

        Mana += cantidad;
        ActualizarMana();
    }

    /// <summary>
    /// Descuenta el coste si hay maná suficiente. Devuelve false y no modifica
    /// nada en caso contrario.
    /// </summary>
    public bool TryGastarMana(int coste)
    {
        if (coste < 0 || coste > Mana)
        {
            return false;
        }

        Mana -= coste;
        ActualizarMana();
        return true;
    }

    /// <summary>Maná recuperado al sacrificar voluntariamente un monstruo.</summary>
    public void RecuperarManaPorSacrificio(int cantidad) => AnadirMana(cantidad);

    /// <summary>Número de monstruos que este jugador tiene desplegados.</summary>
    public int CartasEnJuego => tablero != null ? tablero.CartasEnJuego : 0;

    private void ActualizarMana()
    {
        if (contadorMana != null)
        {
            contadorMana.text = $"Maná: {Mana}";
        }

        ManaCambiado?.Invoke(Mana);
    }
}
