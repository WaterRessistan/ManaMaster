using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Dibuja una carta de objeto. Solo dibuja, igual que <see cref="VistaCartaMonstruo"/>.
    /// </summary>
    public sealed class VistaCartaObjeto : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private Text nombre;
        [SerializeField] private Text bonusAtaque;
        [SerializeField] private Text bonusVida;
        [SerializeField] private Text bonusCura;

        [Header("Arte")]
        [SerializeField] private Image arte;

        /// <summary>Objeto representado, o null si el hueco esta vacio.</summary>
        public IItemCard Objeto { get; private set; }

        public bool TieneObjeto => Objeto != null;

        /// <summary>Muestra el objeto y enciende el GameObject, o lo apaga si es null.</summary>
        public void Mostrar(IItemCard objeto)
        {
            Objeto = objeto;

            if (objeto == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            Refrescar();
        }

        private void Refrescar()
        {
            if (Objeto == null)
            {
                return;
            }

            Escribir(nombre, Objeto.DisplayName);
            Escribir(bonusAtaque, Objeto.BonusAttack.ToString());
            Escribir(bonusVida, Objeto.BonusMaxHealth.ToString());
            Escribir(bonusCura, Objeto.BonusHealPerTurn.ToString());

            if (arte == null)
            {
                return;
            }

            // El arte es un Sprite y no cabe en IItemCard, igual que pasa con
            // IMonsterCard: el motor de reglas se compila sin UnityEngine.
            Sprite dibujo = (Objeto as CardDefinition)?.Artwork;
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
