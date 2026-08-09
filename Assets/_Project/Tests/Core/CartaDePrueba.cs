using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Carta de monstruo de mentira, para los tests del dominio.
    /// </summary>
    /// <remarks>
    /// Existe para no depender de assets de Unity: gracias a <see cref="IMonsterCard"/>
    /// los tests construyen cartas con los valores que necesitan, sin cargar
    /// ningun ScriptableObject y sin abrir el editor.
    /// </remarks>
    internal sealed class CartaDePrueba : IMonsterCard
    {
        public string CardId { get; set; } = "carta-de-prueba";
        public string DisplayName { get; set; } = "Carta de prueba";
        public CardRarity Rarity { get; set; } = CardRarity.Comun;
        public int ManaCost { get; set; } = 1;

        public int MaxHealth { get; set; } = 1;
        public int Attack { get; set; }
        public int HealPerTurn { get; set; }

        public bool CanAttackMelee { get; set; } = true;
        public bool CanAttackRanged { get; set; }

        public bool IsHealer => HealPerTurn > 0;

        /// <summary>La mitad del coste, redondeando hacia abajo (DESIGN.md §7).</summary>
        public int SacrificeManaValue => ManaCost / 2;
    }
}
