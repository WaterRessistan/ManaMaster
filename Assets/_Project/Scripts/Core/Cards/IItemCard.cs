namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Datos de un objeto, vistos por el motor de reglas.
    /// </summary>
    /// <remarks>
    /// Fase 7: por ahora solo bonus numericos, sumados a las estadisticas del
    /// monstruo que lo lleve (ver <see cref="CardInstance.EquippedItem"/>).
    /// Es el punto de extension para las habilidades especiales que vendran
    /// mas adelante (p. ej. atacar desde cualquier carril): anadirlas aqui no
    /// debe tocar <c>CombatResolver</c>, igual que pasa con los bonus.
    /// </remarks>
    public interface IItemCard : ICard
    {
        int BonusAttack { get; }

        int BonusMaxHealth { get; }

        int BonusHealPerTurn { get; }
    }
}
