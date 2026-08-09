using System;
using System.Collections.Generic;
using ManaMaster.Core.Agents;
using ManaMaster.Core.Combat;
using ManaMaster.Core.Match;
using ManaMaster.Core.Util;
using ManaMaster.Unity.Cards;
using UnityEngine;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Dueno de la partida dentro de la escena.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Es el unico punto por el que la escena toca el motor. Las reglas no estan
    /// aqui: aqui solo se monta la partida, se le pasan las acciones del jugador
    /// y se avisa a las vistas de que hay que redibujar. Si algo de este fichero
    /// empieza a parecer una regla del juego, va al Core.
    /// </para>
    /// <para>
    /// Sustituye al trio <c>SistemaTurnos</c> + <c>Jugador</c> + <c>LogicaPartida</c>,
    /// que llevaban el estado en variables estaticas repartidas.
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class MatchController : MonoBehaviour
    {
        [Header("Contenido")]
        [Tooltip("Assets/_Project/Content/Cards/CardCatalog.asset")]
        [SerializeField] private CardCatalog catalogo;

        [SerializeField, Min(1)]
        private int cartasPorMazo = ConstructorDeMazos.CartasPorMazo;

        [Header("Jugadores")]
        [SerializeField] private string nombreHumano = "Tu";
        [SerializeField] private string nombreRival = "Rival";

        [Header("Depuracion")]
        [Tooltip("Semilla de la partida. 0 = distinta cada vez. Fijarla repite " +
                 "la partida entera: mismo reparto, mismo jugador inicial y " +
                 "mismas decisiones de la IA.")]
        [SerializeField] private int semilla;

        private IMatchAgent _agenteRival;

        /// <summary>
        /// Avisos en pausa mientras se reproduce el combate. Sin esto las vistas
        /// saltarian al estado final en cuanto se resuelve, y el golpe a golpe
        /// que viene despues seria un rebobinado a la vista del jugador.
        /// </summary>
        private int _avisosSuspendidos;

        /// <summary>Partida en curso, o null si todavia no ha empezado.</summary>
        public MatchState Partida { get; private set; }

        public PlayerState Humano { get; private set; }

        public PlayerState Rival { get; private set; }

        /// <summary>Semilla con la que se monto la partida, para poder repetirla.</summary>
        public int SemillaEnUso { get; private set; }

        public bool HayPartida => Partida != null;

        /// <summary>
        /// La partida esta en mitad de algo (tipicamente la animacion del
        /// combate) y no debe aceptar jugadas.
        /// </summary>
        public bool Ocupado => _avisosSuspendidos > 0;

        public bool EsTurnoDelHumano
            => HayPartida && !Partida.Terminada
               && ReferenceEquals(Partida.Activo, Humano);

        /// <summary>Algo del estado ha cambiado y las vistas deben redibujar.</summary>
        public event Action PartidaCambiada;

        /// <summary>La partida ha acabado, con o sin ganador.</summary>
        public event Action PartidaTerminada;

        private void Start()
        {
            if (!HayPartida)
            {
                Comenzar();
            }
        }

        /// <summary>Monta una partida nueva y avisa a las vistas.</summary>
        public void Comenzar()
        {
            if (catalogo == null)
            {
                Debug.LogError(
                    "[MatchController] No hay CardCatalog asignado: arrastra " +
                    "Assets/_Project/Content/Cards/CardCatalog.asset al Inspector.",
                    this);
                return;
            }

            SemillaEnUso = semilla != 0 ? semilla : Environment.TickCount;
            IRandom azar = new SystemRandom(SemillaEnUso);

            Humano = new PlayerState(
                nombreHumano, ConstructorDeMazos.Aleatorio(catalogo, azar, cartasPorMazo));
            Rival = new PlayerState(
                nombreRival, ConstructorDeMazos.Aleatorio(catalogo, azar, cartasPorMazo));

            _agenteRival = new AgenteHeuristico();

            // El humano es siempre el jugador 1; quien empieza lo sortea la
            // propia partida (DESIGN.md §5).
            Partida = new MatchState(Humano, Rival, azar);

            Avisar();
        }

        /// <summary>Despliega una carta de la mano del jugador activo.</summary>
        public ResultadoDespliegue Desplegar(int huecoMano, int carril)
        {
            if (!HayPartida)
            {
                return ResultadoDespliegue.HuecoVacio;
            }

            ResultadoDespliegue resultado = Partida.Desplegar(huecoMano, carril);
            if (resultado == ResultadoDespliegue.Ok)
            {
                Avisar();
            }

            return resultado;
        }

        /// <summary>Sacrifica un monstruo propio (DESIGN.md §7).</summary>
        public int Sacrificar(int carril)
        {
            if (!HayPartida)
            {
                return -1;
            }

            int manaRecuperado = Partida.Sacrificar(carril);
            if (manaRecuperado >= 0)
            {
                Avisar();
            }

            return manaRecuperado;
        }

        /// <summary>
        /// Cierra la fase principal del jugador activo: resuelve su combate y
        /// pasa el turno.
        /// </summary>
        /// <returns>
        /// Lo que ha pasado en el combate, para que la vista lo reproduzca.
        /// </returns>
        public IReadOnlyList<EventoCombate> TerminarTurno()
        {
            if (!HayPartida || Partida.Terminada)
            {
                return Array.Empty<EventoCombate>();
            }

            IReadOnlyList<EventoCombate> eventos = Partida.TerminarTurno();

            Avisar();

            return eventos;
        }

        /// <summary>
        /// Juega el turno entero del rival: todas sus jugadas y su combate.
        /// </summary>
        /// <remarks>
        /// Devuelve solo los eventos de su combate porque es lo unico que el §6
        /// pide animar. Los despliegues del rival los recoge la vista al
        /// redibujar.
        /// </remarks>
        public IReadOnlyList<EventoCombate> JugarTurnoDelRival()
        {
            JugarJugadasDelRival();

            return TerminarTurno();
        }

        /// <summary>
        /// Solo la fase principal del rival: despliega y sacrifica, pero no
        /// cierra el turno.
        /// </summary>
        /// <remarks>
        /// Va separado de <see cref="TerminarTurno"/> para que quien anima el
        /// combate pueda fotografiar las arenas justo antes de resolverlo.
        /// </remarks>
        public void JugarJugadasDelRival()
        {
            if (!HayPartida || Partida.Terminada || EsTurnoDelHumano)
            {
                return;
            }

            // Tope de seguridad: una IA con un fallo no debe colgar el juego.
            for (int jugadas = 0; jugadas < 32; jugadas++)
            {
                AccionTurno accion = _agenteRival.DecidirAccion(Partida);

                if (accion.Tipo == TipoAccion.Desplegar
                    && Partida.Desplegar(accion.HuecoMano, accion.Carril)
                       == ResultadoDespliegue.Ok)
                {
                    continue;
                }

                if (accion.Tipo == TipoAccion.Sacrificar
                    && Partida.Sacrificar(accion.Carril) >= 0)
                {
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// Deja de avisar a las vistas hasta que se llame a
        /// <see cref="ReanudarAvisos"/>. Se puede anidar.
        /// </summary>
        public void SuspenderAvisos() => _avisosSuspendidos++;

        /// <summary>Vuelve a avisar a las vistas y las pone al dia.</summary>
        public void ReanudarAvisos()
        {
            _avisosSuspendidos = Mathf.Max(0, _avisosSuspendidos - 1);

            if (_avisosSuspendidos == 0)
            {
                Avisar();
            }
        }

        private void Avisar()
        {
            if (_avisosSuspendidos > 0)
            {
                return;
            }

            PartidaCambiada?.Invoke();

            if (Partida != null && Partida.Terminada)
            {
                PartidaTerminada?.Invoke();
            }
        }
    }
}
