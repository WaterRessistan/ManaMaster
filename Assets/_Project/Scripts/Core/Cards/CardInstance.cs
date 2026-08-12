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
        public IMonsterCard Definition { get; }

        /// <summary>Vida restante. Al llegar a 0 el monstruo queda fuera de combate.</summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Objeto equipado, o null si no lleva ninguno. Como mucho uno
        /// (DESIGN.md §4): no hay ningun metodo para quitarlo, solo
        /// <see cref="TryEquip"/>. Si el monstruo muere, esta instancia deja
        /// de estar referenciada por la arena y el objeto se pierde con ella
        /// sin necesidad de codigo aparte.
        /// </summary>
        public IItemCard EquippedItem { get; private set; }

        public CardInstance(IMonsterCard definition)
        {
            Definition = definition
                ?? throw new ArgumentNullException(nameof(definition));
            CurrentHealth = definition.MaxHealth;
        }

        public int MaxHealth => Definition.MaxHealth + (EquippedItem?.BonusMaxHealth ?? 0);
        public int Attack => Definition.Attack + (EquippedItem?.BonusAttack ?? 0);
        public int HealPerTurn => Definition.HealPerTurn + (EquippedItem?.BonusHealPerTurn ?? 0);
        public int SacrificeManaValue => Definition.SacrificeManaValue;

        public bool IsAlive => CurrentHealth > 0;
        public bool IsHealer => Definition.IsHealer;
        public bool IsDamaged => CurrentHealth < MaxHealth;

        /// <summary>
        /// Indica si este monstruo puede atacar situado en el carril indicado.
        /// </summary>
        /// <remarks>
        /// Punto de extension para futuras habilidades de objeto (p. ej.
        /// atacar a distancia desde el carril principal): hoy los objetos
        /// solo dan bonus numericos (ver <see cref="EquippedItem"/>), pero
        /// cuando haya habilidades se resolveran aqui, sin tocar el
        /// resolvedor de combate.
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

        /// <summary>
        /// Equipa un objeto si todavia no lleva ninguno (DESIGN.md §4: como
        /// mucho uno, y no se puede quitar ni sustituir). Si da vida maxima
        /// extra, esa vida se nota ya mismo, no solo en la proxima curacion.
        /// </summary>
        public bool TryEquip(IItemCard item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (EquippedItem != null)
            {
                return false;
            }

            EquippedItem = item;
            CurrentHealth += item.BonusMaxHealth;
            return true;
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
