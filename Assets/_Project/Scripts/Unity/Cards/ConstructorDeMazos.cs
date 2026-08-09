using System;
using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;
using ManaMaster.Core.Util;

namespace ManaMaster.Unity.Cards
{
    /// <summary>
    /// Construye mazos de partida a partir del catalogo de cartas.
    /// </summary>
    /// <remarks>
    /// Es el puente entre los assets y el motor: convierte las
    /// <see cref="MonsterCardDefinition"/> del catalogo en
    /// <see cref="CardInstance"/> independientes, cada una con su propia vida,
    /// de modo que danar un monstruo en partida nunca toque el asset.
    ///
    /// FASE 5: el mazo lo definira el jugador desde la pantalla de deckbuild.
    /// Hasta entonces se genera uno aleatorio para poder jugar.
    /// </remarks>
    public static class ConstructorDeMazos
    {
        /// <summary>Copias maximas de una misma carta en un mazo (DESIGN.md §8).</summary>
        public const int MaxCopiasPorCarta = 2;

        /// <summary>Cartas de monstruo de un mazo (DESIGN.md §8).</summary>
        public const int CartasPorMazo = 10;

        /// <summary>
        /// Mazo aleatorio respetando el maximo de copias por carta.
        /// </summary>
        /// <remarks>
        /// El limite se cumple por construccion: la reserva de la que se reparte
        /// solo contiene <see cref="MaxCopiasPorCarta"/> de cada carta, asi que
        /// barajarla y cortar no puede sacar una tercera copia.
        /// </remarks>
        public static Deck Aleatorio(
            CardCatalog catalogo, IRandom azar, int cartas = CartasPorMazo)
        {
            if (catalogo == null)
            {
                throw new ArgumentNullException(nameof(catalogo));
            }

            if (azar == null)
            {
                throw new ArgumentNullException(nameof(azar));
            }

            List<MonsterCardDefinition> reserva = new();
            foreach (MonsterCardDefinition definicion in catalogo.Monsters)
            {
                if (definicion == null)
                {
                    continue;
                }

                for (int copia = 0; copia < MaxCopiasPorCarta; copia++)
                {
                    reserva.Add(definicion);
                }
            }

            if (reserva.Count == 0)
            {
                throw new ArgumentException(
                    $"El catalogo '{catalogo.name}' no tiene ninguna carta de " +
                    "monstruo, asi que no se puede montar un mazo.",
                    nameof(catalogo));
            }

            Barajar(reserva, azar);

            int aRepartir = Math.Min(cartas, reserva.Count);
            List<CardInstance> mazo = new(aRepartir);
            for (int i = 0; i < aRepartir; i++)
            {
                mazo.Add(new CardInstance(reserva[i]));
            }

            return new Deck(mazo);
        }

        private static void Barajar<T>(IList<T> lista, IRandom azar)
        {
            for (int i = lista.Count - 1; i > 0; i--)
            {
                int j = azar.Next(i + 1);
                (lista[i], lista[j]) = (lista[j], lista[i]);
            }
        }
    }
}
