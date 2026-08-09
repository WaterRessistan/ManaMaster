using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control del turno actual y del maná que se concede al empezar cada turno.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). El estado de turno era <c>static</c>, por lo que se
/// compartía entre partidas y no se reiniciaba al recargar la escena. Aquí pasa
/// a ser estado de instancia.
///
/// En la Fase 2 este componente desaparece: la máquina de estados de turno
/// (Inicio → Cristales → Principal → Combate → Fin) vivirá en el dominio, fuera
/// de MonoBehaviour, y este script quedará reducido a una vista.
/// </remarks>
public class SistemaTurnos : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField, Min(0)] private int manaPorTurno = 3;

    [Header("Jugadores")]
    [SerializeField] private Jugador jugador1;
    [SerializeField] private Jugador jugador2;

    [Header("UI")]
    [SerializeField] private Text textoTurno;
    [SerializeField] private Text contadorTurno;

    /// <summary>Turno del jugador 1 (humano). Antes era una variable estática.</summary>
    public bool TurnoJugador1 { get; private set; } = true;

    /// <summary>
    /// Ronda en curso, empezando en 1. Cada cambio de turno es un cambio de
    /// ronda (DESIGN.md §5), así que este es el único contador que se muestra.
    /// </summary>
    public int RondaActual { get; private set; } = 1;

    /// <summary>Se emite después de cada cambio de turno.</summary>
    public event Action TurnoCambiado;

    /// <summary>Jugador al que le toca actuar ahora mismo.</summary>
    public Jugador JugadorActivo => TurnoJugador1 ? jugador1 : jugador2;

    private void Awake()
    {
        // DESIGN.md §5: el jugador inicial se elige al azar. Va en Awake y no en
        // Start porque Start ya reparte el maná del primer turno, y para eso hay
        // que saber de quién es.
        // UnityEngine.Random explícito: este fichero importa System y UnityEngine,
        // y "Random" a secas sería ambiguo.
        TurnoJugador1 = UnityEngine.Random.value < 0.5f;
    }

    private void Start()
    {
        // El primer turno también concede maná.
        ConcederManaDelTurno();
        ActualizarUI();
    }

    /// <summary>Conectado al botón "Terminar turno" de la escena.</summary>
    public void terminarTurno()
    {
        TurnoJugador1 = !TurnoJugador1;
        RondaActual++;

        ConcederManaDelTurno();
        ActualizarUI();

        TurnoCambiado?.Invoke();
    }

    private void ConcederManaDelTurno()
    {
        // Antes solo el jugador 1 recibía maná, y además el maná era estático,
        // así que ambos jugadores compartían la misma bolsa.
        Jugador activo = JugadorActivo;
        if (activo != null)
        {
            activo.AnadirMana(manaPorTurno);
        }
    }

    private void ActualizarUI()
    {
        if (textoTurno != null)
        {
            textoTurno.text = TurnoJugador1
                ? "Turno del Jugador 1"
                : "Turno del Jugador 2";
        }

        if (contadorTurno != null)
        {
            contadorTurno.text = $"Ronda: {RondaActual}";
        }
    }
}
