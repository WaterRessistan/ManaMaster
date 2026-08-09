using ManaMaster.Core.Cards;
using UnityEngine;

namespace ManaMaster.Unity.Cards
{
    /// <summary>
    /// Datos inmutables comunes a cualquier carta del juego, como asset de Unity.
    /// </summary>
    /// <remarks>
    /// Una definicion es una plantilla compartida: existe una sola en todo el
    /// juego por cada carta distinta, y nunca cambia durante una partida. El
    /// estado que si cambia (vida actual, objetos equipados) vive en
    /// <see cref="CardInstance"/>.
    ///
    /// Vive en el ensamblado <c>ManaMaster.Unity</c> y no en el Core porque es un
    /// <see cref="ScriptableObject"/>. El motor de reglas la ve solo a traves de
    /// <see cref="ICard"/>, que no sabe nada de Unity.
    /// </remarks>
    public abstract class CardDefinition : ScriptableObject, ICard
    {
        [Header("Identidad")]
        [SerializeField] private string displayName = "Carta sin nombre";
        [SerializeField] private Sprite artwork;
        [SerializeField] private CardRarity rarity = CardRarity.Comun;

        [Header("Coste")]
        [SerializeField, Min(0)] private int manaCost = 1;

        /// <summary>
        /// Identificador estable de la carta. Se usa como clave en la coleccion
        /// del jugador y en los mazos guardados, por lo que renombrar el asset
        /// invalida las partidas guardadas existentes.
        /// </summary>
        public string CardId => name;

        public string DisplayName => displayName;
        public Sprite Artwork => artwork;
        public CardRarity Rarity => rarity;
        public int ManaCost => manaCost;

        /// <summary>
        /// Mana que devuelve sacrificar esta carta desde la arena: la mitad de
        /// su coste, redondeando hacia abajo.
        /// </summary>
        public int SacrificeManaValue => manaCost / 2;
    }
}
