using ManaMaster.Core.Board;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;

namespace ManaMaster.Core.Agents
{
    /// <summary>
    /// IA de la v1: gasta el mana en lo mas caro que pueda pagar y coloca con
    /// cabeza.
    /// </summary>
    /// <remarks>
    /// <para>
    /// No mira las cartas del rival ni planifica turnos futuros. Es a proposito:
    /// da un rival que se comporta de forma razonable y predecible, que es lo
    /// que hace falta para probar el juego, y sirve de rival de referencia para
    /// las simulaciones de balanceo de la Fase 6.
    /// </para>
    /// <para>
    /// No sacrifica nunca. Sacrificar acerca a la derrota del §9 y una heuristica
    /// simple lo usaria mal; queda para una IA mejor.
    /// </para>
    /// <para>
    /// Es determinista: con el mismo estado decide lo mismo. Asi una partida con
    /// la misma semilla se repite entera.
    /// </para>
    /// </remarks>
    public sealed class AgenteHeuristico : IMatchAgent
    {
        public AccionTurno DecidirAccion(MatchState partida)
        {
            if (partida == null || partida.Terminada)
            {
                return AccionTurno.TerminarTurno();
            }

            PlayerState yo = partida.Activo;

            if (yo.Arena.IsFull)
            {
                return AccionTurno.TerminarTurno();
            }

            int hueco = ElegirCarta(yo);
            if (hueco < 0)
            {
                return AccionTurno.TerminarTurno();
            }

            return AccionTurno.Desplegar(hueco, ElegirCarril(yo.Mano[hueco], yo.Arena));
        }

        /// <summary>
        /// La carta mas cara que pueda pagar, usando el ataque para desempatar.
        /// </summary>
        /// <remarks>
        /// El coste es el mejor indicador de potencia que hay sin un sistema de
        /// puntuacion, y gastar el mana en lo mas caro evita quedarselo sin usar.
        /// </remarks>
        private static int ElegirCarta(PlayerState jugador)
        {
            int mejorHueco = -1;
            int mejorCoste = -1;
            int mejorAtaque = -1;

            for (int hueco = 0; hueco < Hand.Capacity; hueco++)
            {
                CardInstance carta = jugador.Mano[hueco];
                if (carta == null || carta.Definition.ManaCost > jugador.Mana)
                {
                    continue;
                }

                int coste = carta.Definition.ManaCost;
                if (coste < mejorCoste
                    || (coste == mejorCoste && carta.Attack <= mejorAtaque))
                {
                    continue;
                }

                mejorHueco = hueco;
                mejorCoste = coste;
                mejorAtaque = carta.Attack;
            }

            return mejorHueco;
        }

        /// <summary>
        /// Donde meter la carta.
        /// </summary>
        /// <remarks>
        /// Dos reglas, en este orden:
        ///
        /// 1. Si el que ocupa el carril principal no puede atacar desde ahi (un
        ///    monstruo de rango atrapado delante), meterle por delante uno que si
        ///    pueda lo arregla: el nuevo ataca y el atrapado pasa a un trasero,
        ///    desde donde ya dispara. Es exactamente para lo que existe la
        ///    insercion con empuje (DESIGN.md §3).
        /// 2. En cualquier otro caso, al final. Colocar por delante empujaria
        ///    hacia atras al que ya esta atacando desde el frente.
        /// </remarks>
        private static int ElegirCarril(CardInstance carta, Arena arena)
        {
            if (arena.IsEmpty)
            {
                return BoardLanes.Principal;
            }

            CardInstance enElFrente = arena[BoardLanes.Principal];

            if (carta.CanAttackFrom(BoardLanes.Principal)
                && !enElFrente.CanAttackFrom(BoardLanes.Principal))
            {
                return BoardLanes.Principal;
            }

            return arena.Count;
        }
    }
}
