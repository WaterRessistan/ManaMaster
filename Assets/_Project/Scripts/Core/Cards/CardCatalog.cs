using System.Collections.Generic;
using UnityEngine;

namespace ManaMaster.Core.Cards
{
    /// <summary>
    /// Listado de todas las cartas que existen en el juego.
    /// </summary>
    /// <remarks>
    /// Sustituye al antiguo <c>CardDatabase</c>, que construia una lista
    /// estatica en <c>Awake()</c>: al ser estatica se duplicaba cada vez que se
    /// recargaba la escena, y al cargar los sprites por nombre desde
    /// <c>Resources</c> las rutas rotas fallaban en silencio.
    ///
    /// Aqui las referencias las resuelve el editor, asi que una carta sin arte
    /// se detecta al importar y no en tiempo de ejecucion.
    /// </remarks>
    [CreateAssetMenu(
        menuName = "Mana Master/Catalogo de cartas",
        fileName = "CardCatalog")]
    public sealed class CardCatalog : ScriptableObject
    {
        [SerializeField] private List<MonsterCardDefinition> monsters = new();
        [SerializeField] private List<ItemCardDefinition> items = new();

        public IReadOnlyList<MonsterCardDefinition> Monsters => monsters;
        public IReadOnlyList<ItemCardDefinition> Items => items;

        public MonsterCardDefinition FindMonster(string cardId)
        {
            foreach (MonsterCardDefinition monster in monsters)
            {
                if (monster != null && monster.CardId == cardId)
                {
                    return monster;
                }
            }

            return null;
        }

        public ItemCardDefinition FindItem(string cardId)
        {
            foreach (ItemCardDefinition item in items)
            {
                if (item != null && item.CardId == cardId)
                {
                    return item;
                }
            }

            return null;
        }

        private void OnValidate()
        {
            WarnOnNullEntries(monsters, nameof(monsters));
            WarnOnNullEntries(items, nameof(items));
        }

        private void WarnOnNullEntries<T>(List<T> entries, string listName)
            where T : CardDefinition
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] == null)
                {
                    Debug.LogWarning(
                        $"[{name}] la lista '{listName}' tiene un hueco vacio en " +
                        $"la posicion {i}.", this);
                }
            }
        }
    }
}
