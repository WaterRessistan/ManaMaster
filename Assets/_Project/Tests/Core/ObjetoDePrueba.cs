using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Carta de objeto de mentira, para los tests del dominio.
    /// </summary>
    /// <remarks>Mismo papel que <see cref="CartaDePrueba"/>, pero para <see cref="IItemCard"/>.</remarks>
    internal sealed class ObjetoDePrueba : IItemCard
    {
        public string CardId { get; set; } = "objeto-de-prueba";
        public string DisplayName { get; set; } = "Objeto de prueba";
        public CardRarity Rarity { get; set; } = CardRarity.Comun;
        public int ManaCost { get; set; }

        public int BonusAttack { get; set; }
        public int BonusMaxHealth { get; set; }
        public int BonusHealPerTurn { get; set; }

        public int SacrificeManaValue => 0;
    }
}
