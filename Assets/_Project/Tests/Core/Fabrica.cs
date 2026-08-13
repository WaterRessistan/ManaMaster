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

        /// <summary>Un objeto con nombre y bonus, para poder seguirlo en los tests.</summary>
        public static IItemCard Objeto(
            string nombre, int bonusAtaque = 0, int bonusVida = 0, int bonusCura = 0,
            bool pocion = false)
            => new ObjetoDePrueba
            {
                CardId = nombre,
                DisplayName = nombre,
                BonusAttack = bonusAtaque,
                BonusMaxHealth = bonusVida,
                BonusHealPerTurn = bonusCura,
                EsPocion = pocion
            };

        /// <summary>Mazo con los monstruos dados, en ese orden.</summary>
        public static Deck Mazo(params CardInstance[] monstruos)
            => new(monstruos ?? new CardInstance[0]);

        /// <summary>Mazo de objetos con los dados, en ese orden.</summary>
        public static ItemDeck MazoDeObjetos(params IItemCard[] objetos)
            => new(objetos ?? new IItemCard[0]);

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
        /// Jugador con los monstruos ya desplegados en los carriles dados, en
        /// orden. Su mazo va vacio: para los tests de combate solo importa la
        /// arena.
        /// </summary>
        public static PlayerState Jugador(string nombre, params CardInstance[] enArena)
        {
            PlayerState jugador = new(nombre, Mazo());

            for (int carril = 0; carril < enArena.Length; carril++)
            {
                jugador.Arena.Insert(carril, enArena[carril]);
            }

            return jugador;
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
