using System;
using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Util;
using ManaMaster.Unity.Cards;

namespace ManaMaster.Unity.Tienda
{
    /// <summary>
    /// Abre un sobre de la tienda: 3 cartas al azar por rareza, con al menos
    /// una Rara o superior garantizada (DESIGN.md §10).
    /// </summary>
    /// <remarks>
    /// Usa <see cref="IRandom"/> y no <c>UnityEngine.Random</c>, igual que
    /// <see cref="ConstructorDeMazos"/>, para que el resultado sea
    /// determinista y testeable con una semilla fija.
    /// </remarks>
    public static class GeneradorDeSobres
    {
        public const int CartasPorSobre = 3;

        /// <summary>Probabilidades de DESIGN.md §10, en el orden de la tabla.</summary>
        private static readonly (CardRarity rareza, float peso)[] Pesos =
        {
            (CardRarity.Comun, 0.70f),
            (CardRarity.Rara, 0.25f),
            (CardRarity.Epica, 0.045f),
            (CardRarity.Legendaria, 0.005f),
        };

        /// <summary>CardId de las 3 cartas del sobre.</summary>
        public static List<string> Abrir(CardCatalog catalogo, IRandom azar)
        {
            if (catalogo == null)
            {
                throw new ArgumentNullException(nameof(catalogo));
            }

            if (azar == null)
            {
                throw new ArgumentNullException(nameof(azar));
            }

            Dictionary<CardRarity, List<MonsterCardDefinition>> porRareza = AgruparPorRareza(catalogo);
            if (porRareza.Count == 0)
            {
                throw new ArgumentException(
                    $"El catalogo '{catalogo.name}' no tiene ninguna carta de " +
                    "monstruo, asi que no se puede abrir un sobre.",
                    nameof(catalogo));
            }

            MonsterCardDefinition[] elegidas = new MonsterCardDefinition[CartasPorSobre];
            for (int i = 0; i < CartasPorSobre; i++)
            {
                elegidas[i] = CartaAlAzar(porRareza, azar);
            }

            if (!HayRaraOSuperior(elegidas))
            {
                MonsterCardDefinition garantizada = CartaDeRarezaMinima(porRareza, CardRarity.Rara, azar);
                if (garantizada != null)
                {
                    elegidas[^1] = garantizada;
                }
            }

            List<string> cartas = new(CartasPorSobre);
            foreach (MonsterCardDefinition carta in elegidas)
            {
                cartas.Add(carta.CardId);
            }

            return cartas;
        }

        private static Dictionary<CardRarity, List<MonsterCardDefinition>> AgruparPorRareza(
            CardCatalog catalogo)
        {
            Dictionary<CardRarity, List<MonsterCardDefinition>> grupos = new();

            foreach (MonsterCardDefinition monstruo in catalogo.Monsters)
            {
                if (monstruo == null)
                {
                    continue;
                }

                if (!grupos.TryGetValue(monstruo.Rarity, out List<MonsterCardDefinition> lista))
                {
                    lista = new List<MonsterCardDefinition>();
                    grupos[monstruo.Rarity] = lista;
                }

                lista.Add(monstruo);
            }

            return grupos;
        }

        private static MonsterCardDefinition CartaAlAzar(
            Dictionary<CardRarity, List<MonsterCardDefinition>> porRareza, IRandom azar)
        {
            CardRarity rareza = RarezaAlAzar(porRareza, azar);
            List<MonsterCardDefinition> lista = porRareza[rareza];
            return lista[azar.Next(lista.Count)];
        }

        /// <summary>
        /// Elige una rareza con los pesos de DESIGN.md §10, redistribuidos
        /// entre las rarezas que de verdad tienen cartas en el catalogo (hoy
        /// no hay legendarias: su peso pasa a las demas).
        /// </summary>
        private static CardRarity RarezaAlAzar(
            Dictionary<CardRarity, List<MonsterCardDefinition>> porRareza, IRandom azar)
        {
            const int precision = 1_000_000;

            float pesoTotal = 0f;
            foreach ((CardRarity rareza, float peso) in Pesos)
            {
                if (porRareza.ContainsKey(rareza))
                {
                    pesoTotal += peso;
                }
            }

            float tirada = azar.Next(precision) / (float)precision * pesoTotal;

            float acumulado = 0f;
            foreach ((CardRarity rareza, float peso) in Pesos)
            {
                if (!porRareza.ContainsKey(rareza))
                {
                    continue;
                }

                acumulado += peso;
                if (tirada < acumulado)
                {
                    return rareza;
                }
            }

            // Redondeo de punto flotante: cae en la ultima rareza presente.
            for (int i = Pesos.Length - 1; i >= 0; i--)
            {
                if (porRareza.ContainsKey(Pesos[i].rareza))
                {
                    return Pesos[i].rareza;
                }
            }

            throw new InvalidOperationException("porRareza no deberia estar vacio aqui");
        }

        private static bool HayRaraOSuperior(IEnumerable<MonsterCardDefinition> cartas)
        {
            foreach (MonsterCardDefinition carta in cartas)
            {
                if (carta.Rarity >= CardRarity.Rara)
                {
                    return true;
                }
            }

            return false;
        }

        private static MonsterCardDefinition CartaDeRarezaMinima(
            Dictionary<CardRarity, List<MonsterCardDefinition>> porRareza,
            CardRarity minima, IRandom azar)
        {
            List<MonsterCardDefinition> candidatas = new();
            foreach (KeyValuePair<CardRarity, List<MonsterCardDefinition>> grupo in porRareza)
            {
                if (grupo.Key >= minima)
                {
                    candidatas.AddRange(grupo.Value);
                }
            }

            return candidatas.Count > 0 ? candidatas[azar.Next(candidatas.Count)] : null;
        }
    }
}
