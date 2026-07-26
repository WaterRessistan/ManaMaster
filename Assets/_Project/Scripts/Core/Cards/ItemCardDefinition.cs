using UnityEngine;

namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Carta de objeto: no aparece fisicamente en la arena, se aplica sobre una
    /// carta de monstruo para concederle alguna ventaja.
    /// </summary>
    /// <remarks>
    /// FASE 7. En la v1 los objetos existen como datos (se compran, se coleccionan
    /// y forman parte del mazo de 10+10) pero NO tienen efecto en combate y la
    /// mano de objetos permanece oculta. Los efectos concretos se modelaran aqui
    /// cuando el juego base este cerrado.
    /// </remarks>
    [CreateAssetMenu(
        menuName = "Mana Master/Carta de objeto",
        fileName = "NuevoObjeto")]
    public sealed class ItemCardDefinition : CardDefinition
    {
        [Header("Efecto")]
        [Tooltip("Descripcion mostrada al jugador. Sin efecto mecanico hasta la Fase 7.")]
        [SerializeField, TextArea(2, 4)] private string effectDescription;

        public string EffectDescription => effectDescription;
    }
}
