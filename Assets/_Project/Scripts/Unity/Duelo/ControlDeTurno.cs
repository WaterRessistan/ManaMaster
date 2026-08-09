using UnityEngine;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Encadena los turnos: cierra el del jugador y hace que el rival juegue el
    /// suyo.
    /// </summary>
    /// <remarks>
    /// FASE 3c: de momento el turno del rival se resuelve entero en un solo
    /// fotograma. Aqui es donde entrara el reproductor de eventos, que ira
    /// soltando el combate con pausas para que se vea la compactacion que pide
    /// el §6.
    /// </remarks>
    public sealed class ControlDeTurno : MonoBehaviour
    {
        [SerializeField] private MatchController controlador;

        /// <summary>
        /// Evita reentrar mientras el rival juega: sus jugadas van avisando de
        /// que la partida ha cambiado, y esos avisos volverian aqui.
        /// </summary>
        private bool _resolviendo;

        private void OnEnable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada += ComprobarTurnoDelRival;
            }
        }

        private void OnDisable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada -= ComprobarTurnoDelRival;
            }
        }

        /// <summary>Conectado al boton "Terminar turno".</summary>
        public void TerminarTurno()
        {
            if (controlador == null || _resolviendo || !controlador.EsTurnoDelHumano)
            {
                return;
            }

            controlador.TerminarTurno();
        }

        private void ComprobarTurnoDelRival()
        {
            if (_resolviendo
                || controlador == null
                || !controlador.HayPartida
                || controlador.Partida.Terminada
                || controlador.EsTurnoDelHumano)
            {
                return;
            }

            _resolviendo = true;
            try
            {
                controlador.JugarTurnoDelRival();
            }
            finally
            {
                _resolviendo = false;
            }
        }
    }
}
