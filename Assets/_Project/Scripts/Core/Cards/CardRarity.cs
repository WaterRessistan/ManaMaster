namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Rareza de una carta. Determina su precio en la tienda y su probabilidad
    /// de aparecer en un sobre.
    /// </summary>
    /// <remarks>
    /// Los valores numericos son explicitos y no deben reordenarse: se guardan
    /// serializados en los assets y en la partida guardada del jugador.
    /// </remarks>
    public enum CardRarity
    {
        Comun = 0,
        Rara = 1,
        Epica = 2,
        Legendaria = 3
    }
}
