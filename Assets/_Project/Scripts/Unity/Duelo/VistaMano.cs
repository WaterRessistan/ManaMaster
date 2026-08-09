using ManaMaster.Core.Match;
using UnityEngine;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Las dos cartas visibles de la mano de un jugador.
    /// </summary>
    /// <remarks>
    /// Los huecos son posiciones fijas y se redibujan enteros cada vez que el
    /// motor avisa. Al jugar una carta el motor repone ese hueco al momento
    /// (DESIGN.md §8), asi que aqui no hay nada que reponer a mano: era
    /// justamente lo que se olvidaba el prototipo, que dejaba la mano vacia
    /// tras dos jugadas.
    ///
    /// La mano del rival no se muestra.
    /// </remarks>
    public sealed class VistaMano : MonoBehaviour
    {
        [SerializeField] private MatchController controlador;

        [Tooltip("Marcar si es la mano del rival: se dibuja siempre oculta.")]
        [SerializeField] private bool esDelRival;

        [Tooltip("Un hueco por carta visible, en orden.")]
        [SerializeField] private CartaDeMano[] huecos = new CartaDeMano[Hand.Capacity];

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

            PlayerState jugador = esDelRival ? controlador.Rival : controlador.Humano;

            for (int hueco = 0; hueco < huecos.Length; hueco++)
            {
                CartaDeMano carta = huecos[hueco];
                if (carta == null || carta.Vista == null)
                {
                    continue;
                }

                // Del rival no se ve la mano: se sabe cuantas cartas tiene, no
                // cuales son.
                carta.Vista.Mostrar(esDelRival ? null : jugador.Mano[hueco]);
            }
        }
    }
}
