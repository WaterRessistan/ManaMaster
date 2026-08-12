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

        /// <summary>Cartas de objeto de un mazo (DESIGN.md §8: el mismo 10 que los monstruos).</summary>
        public const int CartasPorMazoDeObjetos = 10;

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

        /// <summary>
        /// Mazo a partir de una seleccion concreta de cartas, hecha en la
        /// pantalla de deckbuild.
        /// </summary>
        /// <remarks>
        /// Valida el reglamento (DESIGN.md §8) por su cuenta aunque la interfaz
        /// de deckbuild ya deba impedir estos casos: es la ultima linea de
        /// defensa antes de montar la partida. No baraja: la seleccion llega en
        /// el orden en que el jugador la fue eligiendo, y barajarla es cosa de
        /// quien monta la partida.
        /// </remarks>
        public static Deck DesdeSeleccion(CardCatalog catalogo, IReadOnlyList<string> cardIds)
        {
            if (catalogo == null)
            {
                throw new ArgumentNullException(nameof(catalogo));
            }

            if (cardIds == null)
            {
                throw new ArgumentNullException(nameof(cardIds));
            }

            if (cardIds.Count != CartasPorMazo)
            {
                throw new ArgumentException(
                    $"La seleccion tiene {cardIds.Count} cartas; un mazo son " +
                    $"exactamente {CartasPorMazo} (DESIGN.md §8).",
                    nameof(cardIds));
            }

            List<CardInstance> mazo = new(cardIds.Count);
            Dictionary<string, int> copias = new();

            foreach (string cardId in cardIds)
            {
                MonsterCardDefinition definicion = catalogo.FindMonster(cardId);
                if (definicion == null)
                {
                    throw new ArgumentException(
                        $"'{cardId}' no existe en el catalogo '{catalogo.name}'.",
                        nameof(cardIds));
                }

                int vistas = copias.TryGetValue(cardId, out int n) ? n + 1 : 1;
                copias[cardId] = vistas;

                if (vistas > MaxCopiasPorCarta)
                {
                    throw new ArgumentException(
                        $"'{cardId}' aparece {vistas} veces; el maximo son " +
                        $"{MaxCopiasPorCarta} copias (DESIGN.md §8).",
                        nameof(cardIds));
                }

                mazo.Add(new CardInstance(definicion));
            }

            return new Deck(mazo);
        }

        /// <summary>
        /// Mazo de objetos aleatorio respetando el maximo de copias por
        /// carta. Mismo patron que <see cref="Aleatorio"/>, pero sin envolver
        /// cada copia en una instancia propia: un objeto no tiene estado
        /// mutable, asi que dos copias son la misma referencia repetida.
        /// </summary>
        public static ItemDeck ObjetosAleatorio(
            CardCatalog catalogo, IRandom azar, int cartas = CartasPorMazoDeObjetos)
        {
            if (catalogo == null)
            {
                throw new ArgumentNullException(nameof(catalogo));
            }

            if (azar == null)
            {
                throw new ArgumentNullException(nameof(azar));
            }

            List<IItemCard> reserva = new();
            foreach (ItemCardDefinition definicion in catalogo.Items)
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
                    "objeto, asi que no se puede montar un mazo.",
                    nameof(catalogo));
            }

            Barajar(reserva, azar);

            int aRepartir = Math.Min(cartas, reserva.Count);
            return new ItemDeck(reserva.GetRange(0, aRepartir));
        }

        /// <summary>
        /// Mazo de objetos a partir de una seleccion concreta hecha en
        /// deckbuild. Mismo patron que <see cref="DesdeSeleccion"/>.
        /// </summary>
        public static ItemDeck ObjetosDesdeSeleccion(
            CardCatalog catalogo, IReadOnlyList<string> cardIds)
        {
            if (catalogo == null)
            {
                throw new ArgumentNullException(nameof(catalogo));
            }

            if (cardIds == null)
            {
                throw new ArgumentNullException(nameof(cardIds));
            }

            if (cardIds.Count != CartasPorMazoDeObjetos)
            {
                throw new ArgumentException(
                    $"La seleccion tiene {cardIds.Count} cartas; un mazo de " +
                    $"objetos son exactamente {CartasPorMazoDeObjetos} (DESIGN.md §8).",
                    nameof(cardIds));
            }

            List<IItemCard> mazo = new(cardIds.Count);
            Dictionary<string, int> copias = new();

            foreach (string cardId in cardIds)
            {
                ItemCardDefinition definicion = catalogo.FindItem(cardId);
                if (definicion == null)
                {
                    throw new ArgumentException(
                        $"'{cardId}' no existe en el catalogo '{catalogo.name}'.",
                        nameof(cardIds));
                }

                int vistas = copias.TryGetValue(cardId, out int n) ? n + 1 : 1;
                copias[cardId] = vistas;

                if (vistas > MaxCopiasPorCarta)
                {
                    throw new ArgumentException(
                        $"'{cardId}' aparece {vistas} veces; el maximo son " +
                        $"{MaxCopiasPorCarta} copias (DESIGN.md §8).",
                        nameof(cardIds));
                }

                mazo.Add(definicion);
            }

            return new ItemDeck(mazo);
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
