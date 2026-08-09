namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Datos de combate de una carta de monstruo, vistos por el motor de reglas.
    /// </summary>
    /// <remarks>
    /// Son valores de plantilla y no cambian durante la partida: el estado que si
    /// cambia (la vida actual) vive en <see cref="CardInstance"/>.
    /// </remarks>
    public interface IMonsterCard : ICard
    {
        int MaxHealth { get; }

        int Attack { get; }

        /// <summary>
        /// Vida que restaura a CADA aliado en arena, incluido el mismo, al
        /// principio de la fase de combate. 0 = no cura (DESIGN.md §6).
        /// </summary>
        int HealPerTurn { get; }

        /// <summary>Puede atacar desde el carril principal.</summary>
        bool CanAttackMelee { get; }

        /// <summary>Puede atacar desde los carriles traseros.</summary>
        bool CanAttackRanged { get; }

        /// <summary>Cura a sus aliados mientras este en la arena.</summary>
        bool IsHealer { get; }
    }
}
