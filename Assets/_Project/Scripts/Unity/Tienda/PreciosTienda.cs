using System;
using ManaMaster.Core.Cards;

namespace ManaMaster.Unity.Tienda
{
    /// <summary>
    /// Precios de la tienda (DESIGN.md §10).
    /// </summary>
    /// <remarks>
    /// Son los valores provisionales del documento, pendientes de la fase de
    /// balanceo (Fase 6), donde pasaran a vivir en un ScriptableObject de
    /// configuracion. Aqui, en Unity y no en el Core, porque es economia de
    /// meta-juego y no una regla de combate.
    /// </remarks>
    public static class PreciosTienda
    {
        /// <summary>Sobre de 3 cartas, con al menos una Rara garantizada.</summary>
        public const int Sobre = 100;

        /// <summary>Precio de una carta suelta segun su rareza.</summary>
        public static int DeCartaSuelta(CardRarity rareza) => rareza switch
        {
            CardRarity.Comun => 50,
            CardRarity.Rara => 150,
            CardRarity.Epica => 500,
            CardRarity.Legendaria => 1500,
            _ => throw new ArgumentOutOfRangeException(nameof(rareza), rareza, null)
        };
    }
}
