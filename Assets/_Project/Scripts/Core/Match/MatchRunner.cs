using System;

namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Juega una partida entera de agente contra agente, sin interfaz.
    /// </summary>
    /// <remarks>
    /// Sirve para dos cosas: comprobar en los tests que el motor completo no se
    /// atasca ni se contradice, y simular miles de duelos para el balanceo de la
    /// Fase 6. En el juego real no se usa: alli el humano manda sus acciones
    /// desde la interfaz y solo el rival es un agente.
    /// </remarks>
    public static class MatchRunner
    {
        /// <summary>
        /// Tope de acciones antes de dar la partida por atascada.
        /// </summary>
        /// <remarks>
        /// Una partida solo termina si alguien pierde, y los monstruos solo
        /// mueren en combate. Dos arenas llenas de monstruos sin ataque no se
        /// matarian nunca, asi que hace falta un tope para que una simulacion no
        /// se quede colgada.
        /// </remarks>
        public const int MaxAcciones = 5000;

        /// <summary>
        /// Juega hasta que haya ganador o se agote el tope de acciones.
        /// </summary>
        /// <returns>
        /// El resultado. Puede ser <see cref="ResultadoPartida.EnCurso"/> si se
        /// llego al tope sin que nadie perdiera.
        /// </returns>
        public static ResultadoPartida Jugar(
            MatchState partida, IMatchAgent agenteJugador1, IMatchAgent agenteJugador2)
        {
            if (partida == null)
            {
                throw new ArgumentNullException(nameof(partida));
            }

            if (agenteJugador1 == null)
            {
                throw new ArgumentNullException(nameof(agenteJugador1));
            }

            if (agenteJugador2 == null)
            {
                throw new ArgumentNullException(nameof(agenteJugador2));
            }

            int acciones = 0;

            while (!partida.Terminada && acciones < MaxAcciones)
            {
                acciones++;

                IMatchAgent agente =
                    ReferenceEquals(partida.Activo, partida.Jugador1)
                        ? agenteJugador1
                        : agenteJugador2;

                Aplicar(partida, agente.DecidirAccion(partida));
            }

            return partida.Resultado;
        }

        /// <summary>
        /// Ejecuta la accion. Si el agente pide algo imposible se le termina el
        /// turno, para que una IA con un fallo no deje la partida girando en
        /// bucle.
        /// </summary>
        private static void Aplicar(MatchState partida, AccionTurno accion)
        {
            switch (accion.Tipo)
            {
                case TipoAccion.Desplegar:
                    if (partida.Desplegar(accion.HuecoMano, accion.Carril)
                        != ResultadoDespliegue.Ok)
                    {
                        partida.TerminarTurno();
                    }

                    break;

                case TipoAccion.Sacrificar:
                    if (partida.Sacrificar(accion.Carril) < 0)
                    {
                        partida.TerminarTurno();
                    }

                    break;

                default:
                    partida.TerminarTurno();
                    break;
            }
        }
    }
}
