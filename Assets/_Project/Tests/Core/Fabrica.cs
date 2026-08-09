using System.Collections.Generic;
using ManaMaster.Core.Board;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Atajos para montar situaciones de partida en los tests.
    /// </summary>
    internal static class Fabrica
    {
        /// <summary>Un monstruo con nombre, para poder seguirlo en el tablero.</summary>
        public static CardInstance Monstruo(
            string nombre,
            int coste = 1,
            int vida = 1,
            int ataque = 0,
            int cura = 0,
            bool melee = true,
            bool rango = false)
            => new(new CartaDePrueba
            {
                CardId = nombre,
                DisplayName = nombre,
                ManaCost = coste,
                MaxHealth = vida,
                Attack = ataque,
                HealPerTurn = cura,
                CanAttackMelee = melee,
                CanAttackRanged = rango
            });

        /// <summary>Mazo con los monstruos dados, en ese orden.</summary>
        public static Deck Mazo(params CardInstance[] monstruos)
            => new(monstruos ?? new CardInstance[0]);

        /// <summary>Mazo de monstruos identicos numerados, para rellenar.</summary>
        public static Deck MazoDe(int cartas, int coste = 1)
        {
            List<CardInstance> monstruos = new(cartas);
            for (int i = 0; i < cartas; i++)
            {
                monstruos.Add(Monstruo($"M{i + 1}", coste));
            }

            return new Deck(monstruos);
        }

        /// <summary>
        /// La arena escrita como en el DESIGN.md: "C A B" o "A B -".
        /// </summary>
        public static string Disposicion(Arena arena)
        {
            string[] carriles = new string[BoardLanes.Count];
            for (int lane = 0; lane < BoardLanes.Count; lane++)
            {
                carriles[lane] = arena[lane]?.Definition.DisplayName ?? "-";
            }

            return string.Join(" ", carriles);
        }
    }
}
