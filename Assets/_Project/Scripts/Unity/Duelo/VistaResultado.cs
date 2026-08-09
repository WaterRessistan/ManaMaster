using ManaMaster.Core.Match;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Panel de fin de partida: victoria, derrota o empate (DESIGN.md §9).
    /// </summary>
    /// <remarks>
    /// No reparte diamantes: la economia es de la Fase 4. Aqui solo se dice
    /// como ha acabado.
    /// </remarks>
    public sealed class VistaResultado : MonoBehaviour
    {
        [SerializeField] private MatchController controlador;

        [Tooltip("Raiz del panel: se enciende al terminar la partida.")]
        [SerializeField] private GameObject panel;

        [SerializeField] private Text titulo;
        [SerializeField] private Text detalle;
        [SerializeField] private Button revancha;

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
                return;
            }

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

        private static void Escribir(Text campo, string valor)
        {
            if (campo != null)
            {
                campo.text = valor;
            }
        }
    }
}
