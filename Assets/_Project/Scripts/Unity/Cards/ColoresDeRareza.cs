using ManaMaster.Core.Cards;
using UnityEngine;

namespace ManaMaster.Unity.Cards
{
    /// <summary>
    /// Paleta de color por rareza, compartida por el marco de las cartas y
    /// el resumen de apertura de sobre.
    /// </summary>
    public static class ColoresDeRareza
    {
        private static readonly Color Comun = new(0.62f, 0.64f, 0.68f, 1f);
        private static readonly Color Rara = new(0.4f, 0.65f, 1f, 1f);
        private static readonly Color Epica = new(0.7f, 0.4f, 0.95f, 1f);
        private static readonly Color Legendaria = new(1f, 0.8f, 0.2f, 1f);

        public static Color De(CardRarity rareza) => rareza switch
        {
            CardRarity.Comun => Comun,
            CardRarity.Rara => Rara,
            CardRarity.Epica => Epica,
            CardRarity.Legendaria => Legendaria,
            _ => Comun,
        };
    }
}
