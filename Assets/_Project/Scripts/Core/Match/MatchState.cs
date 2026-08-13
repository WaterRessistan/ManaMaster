using System;
using System.Collections.Generic;
using ManaMaster.Core.Combat;
using ManaMaster.Core.Util;

namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Una partida entera: los dos jugadores, de quien es el turno y quien gana.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El turno del DESIGN.md §5 es: se concede mana, el jugador actua libremente
    /// y al pulsar "finalizar turno" se resuelve el combate y pasa el turno. No
    /// hace falta un enum de fases porque la fase principal es simplemente el
    /// tiempo entre dos llamadas a <see cref="TerminarTurno"/>, y el combate se
    /// resuelve entero dentro de esa llamada.
    /// </para>
    /// <para>
    /// Sustituye a <c>SistemaTurnos</c>, cuyo estado era <c>static</c> y por
    /// tanto se arrastraba de una partida a la siguiente. Aqui no hay nada
    /// estatico: dos MatchState son dos partidas independientes.
    /// </para>
    /// </remarks>
    public sealed class MatchState
    {
        /// <summary>Mana que se concede al empezar cada turno (DESIGN.md §7).</summary>
        public const int ManaPorTurno = 3;

        /// <summary>
        /// Rondas tras las cuales la partida acaba en tablas (DESIGN.md §9).
        /// </summary>
        /// <remarks>
        /// No es un desenlace de diseno: las unicas dos formas de perder
        /// pretendidas son quedarse sin monstruos y quedarse ahogado. Esto es
        /// solo una valvula de seguridad tecnica para el caso extremo, raro
        /// pero posible, de que las dos arenas esten llenas y la curacion
        /// iguale al dano turno tras turno: entonces no muere nadie y sin
        /// este tope la partida no acabaria nunca. El valor es
        /// deliberadamente muy alto (partidas normales se deciden en unas
        /// pocas decenas de rondas) para que el jugador no lo vea nunca en la
        /// practica.
        /// </remarks>
        public const int MaxRondas = 300;

        /// <summary>
        /// Monta la partida y sortea quien empieza (DESIGN.md §5).
        /// </summary>
        public MatchState(PlayerState jugador1, PlayerState jugador2, IRandom azar)
        {
            Jugador1 = jugador1 ?? throw new ArgumentNullException(nameof(jugador1));
            Jugador2 = jugador2 ?? throw new ArgumentNullException(nameof(jugador2));

            if (azar == null)
            {
                throw new ArgumentNullException(nameof(azar));
            }

            Activo = azar.Next(2) == 0 ? Jugador1 : Jugador2;

            ComenzarTurno();
        }

        public PlayerState Jugador1 { get; }

        public PlayerState Jugador2 { get; }

        /// <summary>Jugador al que le toca actuar.</summary>
        public PlayerState Activo { get; private set; }

        /// <summary>El otro.</summary>
        public PlayerState Rival
            => ReferenceEquals(Activo, Jugador1) ? Jugador2 : Jugador1;

        /// <summary>
        /// Ronda en curso, empezando en 1. Cada cambio de turno es un cambio de
        /// ronda (DESIGN.md §5).
        /// </summary>
        public int Ronda { get; private set; } = 1;

        public ResultadoPartida Resultado { get; private set; }

        public bool Terminada => Resultado != ResultadoPartida.EnCurso;

        /// <summary>Ganador, o null si la partida sigue o ha quedado en tablas.</summary>
        public PlayerState Ganador => Resultado switch
        {
            ResultadoPartida.VictoriaJugador1 => Jugador1,
            ResultadoPartida.VictoriaJugador2 => Jugador2,
            _ => null
        };

        /// <summary>Despliega una carta de la mano del jugador activo.</summary>
        public ResultadoDespliegue Desplegar(int huecoMano, int carril)
            => Terminada
                ? ResultadoDespliegue.HuecoVacio
                : Activo.TryDesplegar(huecoMano, carril);

        /// <summary>Sacrifica un monstruo del jugador activo (DESIGN.md §7).</summary>
        public int Sacrificar(int carril)
            => Terminada ? -1 : Activo.TrySacrificar(carril);

        /// <summary>
        /// Equipa un objeto de la mano del jugador activo sobre un monstruo
        /// propio (DESIGN.md §4).
        /// </summary>
        public ResultadoEquipar Equipar(int huecoManoObjeto, int carril)
            => Terminada
                ? ResultadoEquipar.HuecoVacio
                : Activo.TryEquipar(huecoManoObjeto, carril);

        /// <summary>
        /// Cierra la fase principal: resuelve el combate del jugador activo y
        /// pasa el turno. Devuelve lo que ha pasado en el combate para que la
        /// vista lo reproduzca.
        /// </summary>
        public IReadOnlyList<EventoCombate> TerminarTurno()
        {
            if (Terminada)
            {
                return Array.Empty<EventoCombate>();
            }

            IReadOnlyList<EventoCombate> eventos =
                CombatResolver.Resolver(Activo, Rival);

            // El combate es lo unico que mata monstruos, asi que es aqui donde
            // puede aparecer un jugador sin nada.
            ComprobarDerrotaPorFaltaDeMonstruos();

            if (Terminada)
            {
                return eventos;
            }

            if (Ronda >= MaxRondas)
            {
                Resultado = ResultadoPartida.Empate;
                return eventos;
            }

            Activo = Rival;
            Ronda++;

            ComenzarTurno();

            return eventos;
        }

        private void ComenzarTurno()
        {
            Activo.GanarMana(ManaPorTurno);

            ComprobarDerrotaPorFaltaDeMonstruos();

            if (!Terminada)
            {
                ComprobarDerrotaPorAhogo();
            }
        }

        /// <summary>
        /// Primera clausula del §9: se queda sin cartas de monstruo en baraja,
        /// mano y arena a la vez.
        /// </summary>
        private void ComprobarDerrotaPorFaltaDeMonstruos()
        {
            if (Jugador1.SinMonstruos)
            {
                Resultado = ResultadoPartida.VictoriaJugador2;
            }
            else if (Jugador2.SinMonstruos)
            {
                Resultado = ResultadoPartida.VictoriaJugador1;
            }
        }

        /// <summary>
        /// Segunda clausula del §9: no tiene monstruos en la arena y no le llega
        /// el mana para desplegar ninguno.
        /// </summary>
        /// <remarks>
        /// Implementada al pie de la letra del §9 a proposito: aunque el mana se
        /// acumule sin tope (§7) y un jugador corto de mana hoy pueda tenerlo
        /// manana, la regla no distingue "sin salida" de "temporalmente corto" —
        /// es la interpretacion confirmada, no un descuido.
        /// </remarks>
        private void ComprobarDerrotaPorAhogo()
        {
            if (!Activo.Arena.IsEmpty || Activo.PuedeDesplegarAlguna())
            {
                return;
            }

            Resultado = ReferenceEquals(Activo, Jugador1)
                ? ResultadoPartida.VictoriaJugador2
                : ResultadoPartida.VictoriaJugador1;
        }
    }
}
