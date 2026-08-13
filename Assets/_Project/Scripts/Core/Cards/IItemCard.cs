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

        /// <summary>
        /// Si es una pocion: se aplica al momento sin ocupar el hueco de
        /// objeto del monstruo (DESIGN.md §4), asi que no compite con
        /// <see cref="CardInstance.EquippedItem"/> ni se ve bloqueada por el.
        /// </summary>
        bool EsPocion { get; }
    }
}
