using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Dibuja una carta de monstruo. Solo dibuja.
    /// </summary>
    /// <remarks>
    /// Sustituye a <c>DisplayCard</c>, que era vista y modelo a la vez:
    /// duplicaba los datos de la carta y ademas se sorteaba a si misma cual
    /// mostrar al arrancar. Aqui no se decide nada: se le da una
    /// <see cref="CardInstance"/> y la pinta.
    /// </remarks>
    public sealed class VistaCartaMonstruo : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private Text nombre;
        [SerializeField] private Text ataque;
        [SerializeField] private Text mana;
        [SerializeField] private Text cura;
        [SerializeField] private Text vida;

        [Header("Arte")]
        [SerializeField] private Image arte;

        /// <summary>Carta representada, o null si el hueco esta vacio.</summary>
        public CardInstance Carta { get; private set; }

        public bool TieneCarta => Carta != null;

        /// <summary>Muestra la carta y enciende el objeto.</summary>
        public void Mostrar(CardInstance carta)
        {
            Carta = carta;

            if (carta == null)
            {
                Ocultar();
                return;
            }

            gameObject.SetActive(true);
            Refrescar();
        }

        /// <summary>Vacia el hueco y apaga el objeto.</summary>
        public void Ocultar()
        {
            Carta = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Vuelve a leer el estado de la carta. Hay que llamarlo cuando la vida
        /// cambia: la vista no se entera sola.
        /// </summary>
        public void Refrescar()
        {
            if (Carta == null)
            {
                return;
            }

            IMonsterCard definicion = Carta.Definition;

            Escribir(nombre, definicion.DisplayName);
            Escribir(ataque, definicion.Attack.ToString());
            Escribir(mana, definicion.ManaCost.ToString());
            Escribir(cura, definicion.HealPerTurn.ToString());
            Escribir(vida, Carta.CurrentHealth.ToString());

            if (arte == null)
            {
                return;
            }

            // El arte es un Sprite y por eso no cabe en IMonsterCard: el motor de
            // reglas se compila sin UnityEngine. Se lee de la definicion concreta.
            Sprite dibujo = (definicion as CardDefinition)?.Artwork;
            arte.sprite = dibujo;
            arte.enabled = dibujo != null;
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
