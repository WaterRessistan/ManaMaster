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

        private void OnEnable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada += Refrescar;
            }

            Refrescar();
        }

        private void OnDisable()
        {
            if (controlador != null)
            {
                controlador.PartidaCambiada -= Refrescar;
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

        private static void Escribir(Text campo, string valor)
        {
            if (campo != null)
            {
                campo.text = valor;
            }
        }
    }
}
