using System.Collections;
using ManaMaster.Core.Match;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>Mana de los dos jugadores, ronda y de quien es el turno.</summary>
    /// <remarks>
    /// El §5 dice que cada cambio de turno es un cambio de ronda, asi que turno
    /// y ronda son el mismo numero y solo se muestra el contador de Ronda.
    /// </remarks>
    public sealed class VistaMarcador : MonoBehaviour
    {
        [SerializeField] private MatchController controlador;

        [Header("Textos")]
        [SerializeField] private Text manaHumano;
        [SerializeField] private Text manaRival;
        [SerializeField] private Text ronda;
        [SerializeField] private Text turno;

        [Header("Botones")]
        [Tooltip("Se apaga cuando no es el turno del jugador.")]
        [SerializeField] private Button terminarTurno;

        // Dorado cuando es tu turno, rojo apagado cuando es el del rival: asi
        // se ve de un vistazo sin tener que leer el texto.
        private static readonly Color ColorTuTurno = new(1f, 0.82f, 0.2f, 1f);
        private static readonly Color ColorTurnoRival = new(0.78f, 0.28f, 0.28f, 1f);
        private static readonly Color ColorNeutro = Color.white;

        private const float VelocidadDePulso = 3f;
        private const float AmplitudDePulso = 0.08f;

        private Coroutine _pulso;

        private void OnEnable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada += Refrescar;
            }

            Refrescar();

            _pulso ??= StartCoroutine(PulsarTextoDeTurno());
        }

        private void OnDisable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada -= Refrescar;
            }

            if (_pulso != null)
            {
                StopCoroutine(_pulso);
                _pulso = null;
            }
        }

        public void Refrescar()
        {
            if (controlador == null || !controlador.HayPartida)
            {
                return;
            }

            MatchState partida = controlador.Partida;

            Escribir(manaHumano, $"Mana: {controlador.Humano.Mana}");
            Escribir(manaRival, $"Mana: {controlador.Rival.Mana}");
            Escribir(ronda, $"Ronda: {partida.Ronda}");
            Escribir(turno, TextoDeTurno(partida));

            if (turno != null)
            {
                turno.color = ColorDeTurno(partida);
            }

            if (terminarTurno != null)
            {
                terminarTurno.interactable = controlador.EsTurnoDelHumano;
            }
        }

        private string TextoDeTurno(MatchState partida) => partida.Resultado switch
        {
            ResultadoPartida.EnCurso => controlador.EsTurnoDelHumano
                ? "Tu turno"
                : "Turno del rival",
            ResultadoPartida.Empate => "Empate",
            _ => ReferenceEquals(partida.Ganador, controlador.Humano)
                ? "Has ganado"
                : "Has perdido"
        };

        private Color ColorDeTurno(MatchState partida)
            => partida.Resultado != ResultadoPartida.EnCurso
                ? ColorNeutro
                : controlador.EsTurnoDelHumano ? ColorTuTurno : ColorTurnoRival;

        /// <summary>
        /// Pulso continuo de escala mientras la partida esta en curso, para
        /// que el turno se note sin tener que leer el texto. Se para solo
        /// (vuelve a escala 1) cuando la partida termina.
        /// </summary>
        private IEnumerator PulsarTextoDeTurno()
        {
            while (true)
            {
                bool enCurso = controlador != null && controlador.HayPartida
                    && controlador.Partida.Resultado == ResultadoPartida.EnCurso;

                if (turno != null)
                {
                    float escala = enCurso
                        ? 1f + AmplitudDePulso * Mathf.Sin(Time.unscaledTime * VelocidadDePulso)
                        : 1f;
                    turno.rectTransform.localScale = new Vector3(escala, escala, 1f);
                }

                yield return null;
            }
        }

        private static void Escribir(Text campo, string valor)
        {
            if (campo != null)
            {
                campo.text = valor;
            }
        }
    }
}
