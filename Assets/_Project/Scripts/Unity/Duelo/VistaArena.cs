using ManaMaster.Core.Board;
using ManaMaster.Core.Match;
using UnityEngine;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Los tres carriles de un jugador, redibujados desde el estado de la partida.
    /// </summary>
    /// <remarks>
    /// La vista no mueve cartas ni cierra huecos: lee la arena del motor y la
    /// copia. La compactacion del §6 ya la ha hecho el dominio, aqui solo se ve
    /// el resultado.
    /// </remarks>
    public sealed class VistaArena : MonoBehaviour
    {
        [SerializeField] private MatchController controlador;

        [Tooltip("Marcar si esta arena es la del rival.")]
        [SerializeField] private bool esDelRival;

        [Tooltip("Los tres carriles, en orden: 0 principal, 1 y 2 traseros.")]
        [SerializeField] private CarrilDeInsercion[] carriles = new CarrilDeInsercion[BoardLanes.Count];

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

        /// <summary>Jugador al que pertenece esta arena.</summary>
        public PlayerState Propietario
            => controlador == null
                ? null
                : esDelRival ? controlador.Rival : controlador.Humano;

        public void Refrescar()
        {
            PlayerState jugador = Propietario;
            if (jugador == null)
            {
                return;
            }

            for (int carril = 0; carril < carriles.Length; carril++)
            {
                CarrilDeInsercion zona = carriles[carril];
                if (zona == null || zona.Vista == null)
                {
                    continue;
                }

                zona.Vista.Mostrar(jugador.Arena[carril]);
            }
        }

        /// <summary>
        /// Enciende la marca en los carriles donde la carta que se esta
        /// arrastrando puede entrar: de 0 a (ocupados), sin dejar huecos
        /// (DESIGN.md §3).
        /// </summary>
        public void ResaltarPosicionesValidas(bool encendido)
        {
            PlayerState jugador = Propietario;

            for (int carril = 0; carril < carriles.Length; carril++)
            {
                CarrilDeInsercion zona = carriles[carril];
                if (zona == null)
                {
                    continue;
                }

                bool valido = encendido
                              && jugador != null
                              && jugador.Arena.CanInsertAt(carril);

                zona.Resaltar(valido);
            }
        }
    }
}
