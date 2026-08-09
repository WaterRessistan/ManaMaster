using System.Collections.Generic;
using System.Linq;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>El mazo de monstruos y su barajado (DESIGN.md §8).</summary>
    [TestFixture]
    public sealed class DeckTests
    {
        [Test]
        public void SeRobaPorArriba()
        {
            Deck mazo = Fabrica.Mazo(
                Fabrica.Monstruo("A"),
                Fabrica.Monstruo("B"));

            Assert.That(mazo.Draw().Definition.DisplayName, Is.EqualTo("A"));
            Assert.That(mazo.Draw().Definition.DisplayName, Is.EqualTo("B"));
            Assert.That(mazo.IsEmpty, Is.True);
        }

        [Test]
        public void RobarDeUnMazoVacioDevuelveNull()
        {
            Deck mazo = Fabrica.Mazo();

            Assert.That(mazo.Draw(), Is.Null);
            Assert.That(mazo.Count, Is.EqualTo(0));
        }

        [Test]
        public void UnMazoConHuecosNoSePuedeConstruir()
        {
            Assert.That(() => new Deck(new CardInstance[] { null }),
                Throws.ArgumentException);
        }

        [Test]
        public void BarajarConservaLasMismasCartas()
        {
            Deck mazo = Fabrica.MazoDe(10);
            List<CardInstance> antes = mazo.Cartas.ToList();

            mazo.Shuffle(new AzarFijo(3, 1, 4, 1, 5, 9, 2, 6));

            Assert.That(mazo.Count, Is.EqualTo(10));
            Assert.That(mazo.Cartas, Is.EquivalentTo(antes));
        }

        /// <summary>
        /// Con la misma fuente de azar sale el mismo orden: es lo que permite
        /// repetir una partida entera a partir de su semilla.
        /// </summary>
        [Test]
        public void ConElMismoAzarSaleElMismoOrden()
        {
            Deck primero = Fabrica.MazoDe(10);
            Deck segundo = Fabrica.MazoDe(10);

            primero.Shuffle(new AzarFijo(3, 1, 4, 1, 5, 9, 2, 6));
            segundo.Shuffle(new AzarFijo(3, 1, 4, 1, 5, 9, 2, 6));

            var nombresPrimero = primero.Cartas.Select(c => c.Definition.CardId);
            var nombresSegundo = segundo.Cartas.Select(c => c.Definition.CardId);

            Assert.That(nombresPrimero, Is.EqualTo(nombresSegundo));
        }

        [Test]
        public void BarajarLlegaARemoverElOrden()
        {
            Deck mazo = Fabrica.MazoDe(10);

            mazo.Shuffle(new AzarFijo(0));

            // Con Next() siempre 0, Fisher-Yates deja la primera carta al final.
            Assert.That(mazo.Cartas[9].Definition.CardId, Is.EqualTo("M1"));
        }

        [Test]
        public void BarajarSinFuenteDeAzarEsUnError()
        {
            Deck mazo = Fabrica.MazoDe(4);

            Assert.That(() => mazo.Shuffle(null), Throws.ArgumentNullException);
        }
    }
}
