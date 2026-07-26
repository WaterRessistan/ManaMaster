using System;
using ManaMaster.Core.Board;

namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Un monstruo concreto dentro de una partida, con su estado mutable.
    /// </summary>
    /// <remarks>
    /// Separar la instancia de la definicion es lo que permite que dos copias
    /// de la misma carta tengan vidas distintas, y evita que danar un monstruo
    /// en partida modifique la plantilla compartida del catalogo.
    /// </remarks>
    public sealed class CardInstance
    {
        public MonsterCardDefinition Definition { get; }

        /// <summary>Vida restante. Al llegar a 0 el monstruo queda fuera de combate.</summary>
        public int CurrentHealth { get; private set; }

        public CardInstance(MonsterCardDefinition definition)
        {
            Definition = definition
                ?? throw new ArgumentNullException(nameof(definition));
            CurrentHealth = definition.MaxHealth;
        }

        public int MaxHealth => Definition.MaxHealth;
        public int Attack => Definition.Attack;
        public int HealPerTurn => Definition.HealPerTurn;
        public int SacrificeManaValue => Definition.SacrificeManaValue;

        public bool IsAlive => CurrentHealth > 0;
        public bool IsHealer => Definition.IsHealer;
        public bool IsDamaged => CurrentHealth < MaxHealth;

        /// <summary>
        /// Indica si este monstruo puede atacar situado en el carril indicado.
        /// </summary>
        /// <remarks>
        /// Punto de extension para la Fase 7: los objetos que permiten atacar a
        /// distancia desde el carril principal (o al reves) se resolveran aqui,
        /// sin tocar el resolvedor de combate.
        /// </remarks>
        public bool CanAttackFrom(int laneIndex)
        {
            if (!BoardLanes.IsValid(laneIndex))
            {
                return false;
            }

            return BoardLanes.IsFront(laneIndex)
                ? Definition.CanAttackMelee
                : Definition.CanAttackRanged;
        }

        /// <summary>Aplica dano y devuelve el dano realmente infligido.</summary>
        public int ReceiveDamage(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int applied = Math.Min(amount, CurrentHealth);
            CurrentHealth -= applied;
            return applied;
        }

        /// <summary>
        /// Restaura vida sin superar el maximo y devuelve la curacion efectiva.
        /// </summary>
        public int ReceiveHealing(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return 0;
            }

            int applied = Math.Min(amount, MaxHealth - CurrentHealth);
            CurrentHealth += applied;
            return applied;
        }

        public override string ToString()
            => $"{Definition.DisplayName} ({CurrentHealth}/{MaxHealth})";
    }
}
