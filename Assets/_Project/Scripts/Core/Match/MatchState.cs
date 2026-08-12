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
        /// Salida de emergencia para el caso raro pero posible de que las dos
        /// arenas esten llenas y la curacion iguale al dano: entonces no muere
        /// nadie y sin este tope la partida no acabaria nunca.
        ///
        /// El valor esta medido, no elegido a ojo: simulando 2.000 partidas, las
        /// que se deciden duran entre 11 y 36 rondas, con mediana 18. Hay que
        /// volver a medirlo en la fase de balanceo con las cartas definitivas.
        /// </remarks>
        public const int MaxRondas = 60;

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
        /// OJO: implementada al pie de la letra del §9, y tal cual escrita es
        /// dudosa. Como el mana se acumula sin tope (§7), un jugador al que hoy
        /// no le llega si tendra suficiente dentro de un turno o dos, asi que
        /// esta regla no distingue "sin salida" de "temporalmente corto": con la
        /// mano llena de cartas caras se pierde en la ronda 1. Pendiente de
        /// decidir si debe exigir ademas que la mano este vacia.
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
