using UnityEngine;

/// <summary>
/// Comprobación de la condición de derrota.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). El cuerpo de este componente estaba enteramente
/// comentado, pero <c>Update()</c> seguía llamándolo en cada frame. También
/// declaraba una lista de <c>Eventos</c>, una clase vacía sin ningún uso, y una
/// bandera <c>static</c> de fin de partida que no se reiniciaba entre partidas.
///
/// Aquí queda solo la consulta, sin sondeo por frame. En la Fase 2 la condición
/// de derrota la evalúa la máquina de estados al terminar cada turno, que es
/// cuando puede cambiar.
/// </remarks>
public class LogicaPartida : MonoBehaviour
{
    [SerializeField] private Jugador jugador1;
    [SerializeField] private Jugador jugador2;

    /// <summary>
    /// Un jugador pierde cuando se queda sin monstruos: ni en la baraja, ni en
    /// la mano, ni en la arena.
    /// </summary>
    /// <remarks>
    /// APROXIMADO en la Fase 1: la mano todavía no es una entidad del modelo, así
    /// que solo se comprueban baraja y arena. Al modelarse la mano en la Fase 2
    /// hay que incluirla aquí.
    /// </remarks>
    public bool HaPerdido(Jugador jugador)
    {
        if (jugador == null)
        {
            return false;
        }

        bool sinBaraja = jugador.Baraja == null || jugador.Baraja.EstaVacia;
        bool sinArena = jugador.CartasEnJuego == 0;

        return sinBaraja && sinArena;
    }

    /// <summary>Jugador derrotado, o null si la partida sigue.</summary>
    public Jugador ComprobarDerrota()
    {
        if (HaPerdido(jugador1))
        {
            return jugador1;
        }

        return HaPerdido(jugador2) ? jugador2 : null;
    }
}
