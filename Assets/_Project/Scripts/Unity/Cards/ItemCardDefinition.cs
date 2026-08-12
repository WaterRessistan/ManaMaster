using ManaMaster.Core.Cards;
using UnityEngine;

namespace ManaMaster.Unity.Cards
{
    /// <summary>
    /// Carta de objeto: no aparece fisicamente en la arena, se aplica sobre una
    /// carta de monstruo para concederle alguna ventaja.
    /// </summary>
    /// <remarks>
    /// Fase 7. Por ahora solo bonus numericos, sumados a las estadisticas del
    /// monstruo que la lleve (DESIGN.md §4); habilidades especiales llegaran
    /// mas adelante sin tener que rehacer esto.
    /// </remarks>
    [CreateAssetMenu(
        menuName = "Mana Master/Carta de objeto",
        fileName = "NuevoObjeto")]
    public sealed class ItemCardDefinition : CardDefinition, IItemCard
    {
        [Header("Efecto")]
        [Tooltip("Descripcion mostrada al jugador.")]
        [SerializeField, TextArea(2, 4)] private string effectDescription;

        [Header("Bonus")]
        [SerializeField] private int bonusAttack;
        [SerializeField] private int bonusMaxHealth;
        [SerializeField] private int bonusHealPerTurn;

        public string EffectDescription => effectDescription;

        public int BonusAttack => bonusAttack;
        public int BonusMaxHealth => bonusMaxHealth;
        public int BonusHealPerTurn => bonusHealPerTurn;
    }
}
