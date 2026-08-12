using ManaMaster.Core.Match;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Panel de fin de partida: victoria, derrota o empate (DESIGN.md §9).
    /// </summary>
    /// <remarks>
    /// Reparte diamantes al humano por el resultado (DESIGN.md §10), una sola
    /// vez por partida: <see cref="_premioRepartido"/> se apaga de nuevo en
    /// cuanto <see cref="MatchState.Terminada"/> vuelve a ser falso, es decir,
    /// al empezar la revancha. La IA no tiene economia.
    /// </remarks>
    public sealed class VistaResultado : MonoBehaviour
    {
        /// <summary>Diamantes por resultado (DESIGN.md §10).</summary>
        private const int DiamantesPorVictoria = 50;
        private const int DiamantesPorDerrota = 15;
        private const int DiamantesPorEmpate = 30;

        [SerializeField] private MatchController controlador;

        [Tooltip("Raiz del panel: se enciende al terminar la partida.")]
        [SerializeField] private GameObject panel;

        [SerializeField] private Text titulo;
        [SerializeField] private Text detalle;
        [SerializeField] private Button revancha;

        private bool _premioRepartido;

        private void OnEnable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada += Refrescar;
            }

            if (revancha != null)
            {
                revancha.onClick.AddListener(Revancha);
            }

            Refrescar();
        }

        private void OnDisable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada -= Refrescar;
            }

            if (revancha != null)
            {
                revancha.onClick.RemoveListener(Revancha);
            }
        }

        /// <summary>Conectado al boton de revancha.</summary>
        public void Revancha() => controlador?.Comenzar();

        private void Refrescar()
        {
            if (panel == null || controlador == null || !controlador.HayPartida)
            {
                return;
            }

            MatchState partida = controlador.Partida;

            panel.SetActive(partida.Terminada);

            if (!partida.Terminada)
            {
                _premioRepartido = false;
                return;
            }

            RepartirDiamantesSiHaceFalta(partida);

            switch (partida.Resultado)
            {
                case ResultadoPartida.Empate:
                    Escribir(titulo, "Empate");
                    Escribir(detalle,
                        $"Se han cumplido las {MatchState.MaxRondas} rondas sin " +
                        "que nadie se quedara sin monstruos.");
                    break;

                default:
                    bool ganaElJugador =
                        ReferenceEquals(partida.Ganador, controlador.Humano);

                    Escribir(titulo, ganaElJugador ? "Victoria" : "Derrota");
                    Escribir(detalle, ganaElJugador
                        ? $"{controlador.Rival.Nombre} se ha quedado sin monstruos."
                        : $"Te has quedado sin monstruos en la ronda {partida.Ronda}.");
                    break;
            }
        }

        private void RepartirDiamantesSiHaceFalta(MatchState partida)
        {
            if (_premioRepartido)
            {
                return;
            }

            _premioRepartido = true;

            if (controlador.Sesion == null)
            {
                return;
            }

            int diamantes = partida.Resultado == ResultadoPartida.Empate
                ? DiamantesPorEmpate
                : ReferenceEquals(partida.Ganador, controlador.Humano)
                    ? DiamantesPorVictoria
                    : DiamantesPorDerrota;

            controlador.Sesion.GanarDiamantes(diamantes);
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
