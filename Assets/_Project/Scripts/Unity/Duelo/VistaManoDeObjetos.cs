using ManaMaster.Core.Match;
using UnityEngine;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Las dos cartas visibles de la mano de objetos del humano.
    /// </summary>
    /// <remarks>
    /// Mismo patron que <see cref="VistaMano"/>. El Rival no tiene mazo de
    /// objetos (ver <see cref="MatchController.Comenzar"/>), asi que esta
    /// vista solo se usa para el humano.
    /// </remarks>
    public sealed class VistaManoDeObjetos : MonoBehaviour
    {
        [SerializeField] private MatchController controlador;

        [Tooltip("Un hueco por objeto visible, en orden.")]
        [SerializeField] private CartaDeObjeto[] huecos = new CartaDeObjeto[ItemHand.Capacity];

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

            for (int hueco = 0; hueco < huecos.Length; hueco++)
            {
                CartaDeObjeto carta = huecos[hueco];
                if (carta == null || carta.Vista == null)
                {
                    continue;
                }

                carta.Vista.Mostrar(controlador.Humano.ManoDeObjetos[hueco]);
            }
        }
    }
}
