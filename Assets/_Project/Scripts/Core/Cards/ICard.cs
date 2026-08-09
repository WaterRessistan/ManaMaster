namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Lo que el motor de reglas necesita saber de cualquier carta.
    /// </summary>
    /// <remarks>
    /// El motor no conoce los ScriptableObject: los datos de carta le llegan a
    /// traves de esta interfaz. Asi <c>ManaMaster.Core</c> se compila sin
    /// UnityEngine y sus tests corren sin abrir el editor (DESIGN.md §12).
    ///
    /// La implementacion de produccion es <c>CardDefinition</c>, en el ensamblado
    /// <c>ManaMaster.Unity</c>; en los tests se implementa con objetos normales.
    /// </remarks>
    public interface ICard
    {
        /// <summary>
        /// Identificador estable. Es la clave de la coleccion del jugador y de
        /// los mazos guardados, por lo que cambiarlo invalida las partidas
        /// guardadas existentes.
        /// </summary>
        string CardId { get; }

        string DisplayName { get; }

        CardRarity Rarity { get; }

        int ManaCost { get; }

        /// <summary>
        /// Mana que devuelve sacrificar esta carta desde la arena: la mitad de
        /// su coste, redondeando hacia abajo (DESIGN.md §7).
        /// </summary>
        int SacrificeManaValue { get; }
    }
}
