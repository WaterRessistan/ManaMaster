using UnityEngine;

namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Carta de monstruo: la unica que se despliega fisicamente en la arena,
    /// ataca y recibe dano.
    /// </summary>
    [CreateAssetMenu(
        menuName = "Mana Master/Carta de monstruo",
        fileName = "NuevoMonstruo")]
    public sealed class MonsterCardDefinition : CardDefinition
    {
        [Header("Combate")]
        [SerializeField, Min(1)] private int maxHealth = 1;
        [SerializeField, Min(0)] private int attack;

        [Tooltip("Puntos de vida que restaura a CADA aliado en arena (incluido " +
                 "el propio monstruo) al inicio de la fase de combate. 0 = no cura.")]
        [SerializeField, Min(0)] private int healPerTurn;

        [Header("Modo de ataque")]
        [Tooltip("Puede atacar desde el carril principal (carril 1).")]
        [SerializeField] private bool canAttackMelee = true;

        [Tooltip("Puede atacar desde los carriles traseros (carriles 2 y 3).")]
        [SerializeField] private bool canAttackRanged;

        public int MaxHealth => maxHealth;
        public int Attack => attack;
        public int HealPerTurn => healPerTurn;

        public bool CanAttackMelee => canAttackMelee;
        public bool CanAttackRanged => canAttackRanged;

        /// <summary>Este monstruo cura a sus aliados mientras este en la arena.</summary>
        public bool IsHealer => healPerTurn > 0;

        private void OnValidate()
        {
            // Una carta sin ningun modo de ataque nunca podria atacar desde
            // ningun carril: casi siempre es un error de configuracion del asset.
            if (!canAttackMelee && !canAttackRanged && attack > 0)
            {
                Debug.LogWarning(
                    $"[{name}] tiene {attack} de ataque pero no puede atacar ni " +
                    "cuerpo a cuerpo ni a distancia: nunca llegara a atacar.",
                    this);
            }
        }
    }
}
