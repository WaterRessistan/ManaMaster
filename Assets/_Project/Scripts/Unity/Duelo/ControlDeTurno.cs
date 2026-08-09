using System.Collections;
using System.Collections.Generic;
using ManaMaster.Core.Combat;
using UnityEngine;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Encadena los turnos: cierra el del jugador, anima su combate, deja jugar
    /// al rival y anima el suyo.
    /// </summary>
    /// <remarks>
    /// Es el unico sitio con ritmo: el motor no sabe nada de esperas y las
    /// vistas solo dibujan lo que se les da. Aqui se decide cuando pasa cada
    /// cosa.
    /// </remarks>
    public sealed class ControlDeTurno : MonoBehaviour
    {
        [SerializeField] private MatchController controlador;
        [SerializeField] private ReproductorDeCombate reproductor;

        [Tooltip("Espera antes de que el rival empiece a jugar sus cartas.")]
        [SerializeField, Min(0f)] private float pausaAntesDelRival = 0.6f;

        /// <summary>
        /// Se marca antes de arrancar la corrutina y no despues: una corrutina
        /// empieza a ejecutarse dentro de la propia llamada a StartCoroutine, y
        /// para entonces ya tiene que estar cerrada la puerta.
        /// </summary>
        private bool _ocupado;

        private void OnEnable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada += ComprobarSiLeTocaAlRival;
            }
        }

        private void OnDisable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada -= ComprobarSiLeTocaAlRival;
            }
        }

        /// <summary>Conectado al boton "Terminar turno".</summary>
        public void TerminarTurno()
        {
            if (_ocupado || controlador == null || !controlador.EsTurnoDelHumano)
            {
                return;
            }

            _ocupado = true;
            StartCoroutine(Secuencia(cerrarElTurnoDelHumano: true));
        }

        /// <summary>
        /// Por si al rival le toca empezar: el jugador inicial se sortea
        /// (DESIGN.md §5).
        /// </summary>
        private void ComprobarSiLeTocaAlRival()
        {
            if (_ocupado || !LaPartidaSigue() || controlador.EsTurnoDelHumano)
            {
                return;
            }

            _ocupado = true;
            StartCoroutine(Secuencia(cerrarElTurnoDelHumano: false));
        }

        private IEnumerator Secuencia(bool cerrarElTurnoDelHumano)
        {
            if (cerrarElTurnoDelHumano)
            {
                yield return ResolverCombateDelActivo();
            }

            // Puede encadenar varios turnos del rival si el jugador se queda sin
            // nada que hacer, aunque con dos jugadores solo dara una vuelta.
            while (LaPartidaSigue() && !controlador.EsTurnoDelHumano)
            {
                yield return new WaitForSeconds(pausaAntesDelRival);

                controlador.JugarJugadasDelRival();

                yield return ResolverCombateDelActivo();
            }

            _ocupado = false;
        }

        /// <summary>
        /// Fotografia las dos arenas, resuelve el combate y lo reproduce.
        /// </summary>
        /// <remarks>
        /// Las fotos se toman ANTES de resolver porque despues ya no hay forma
        /// de saber quien estaba vivo ni con cuanta vida. Y los avisos a las
        /// vistas se suspenden mientras dura la animacion: si no, saltarian al
        /// estado final y el golpe a golpe se veria como un rebobinado.
        /// </remarks>
        private IEnumerator ResolverCombateDelActivo()
        {
            if (!LaPartidaSigue())
            {
                yield break;
            }

            bool atacaElHumano = controlador.EsTurnoDelHumano;

            InstantaneaDeArena fotoAtacante =
                InstantaneaDeArena.Tomar(controlador.Partida.Activo.Arena);
            InstantaneaDeArena fotoDefensora =
                InstantaneaDeArena.Tomar(controlador.Partida.Rival.Arena);

            controlador.SuspenderAvisos();

            IReadOnlyList<EventoCombate> eventos = controlador.TerminarTurno();

            if (reproductor != null)
            {
                yield return reproductor.Reproducir(
                    fotoAtacante, fotoDefensora, eventos, atacaElHumano);
            }

            controlador.ReanudarAvisos();
        }

        private bool LaPartidaSigue()
            => controlador != null
               && controlador.HayPartida
               && !controlador.Partida.Terminada;
    }
}
