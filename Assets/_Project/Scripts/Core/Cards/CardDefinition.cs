using UnityEngine;

namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Datos inmutables comunes a cualquier carta del juego.
    /// </summary>
    /// <remarks>
    /// Una definicion es una plantilla compartida: existe una sola en todo el
    /// juego por cada carta distinta, y nunca cambia durante una partida. El
    /// estado que si cambia (vida actual, objetos equipados) vive en
    /// <see cref="CardInstance"/>.
    /// </remarks>
    public abstract class CardDefinition : ScriptableObject
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
