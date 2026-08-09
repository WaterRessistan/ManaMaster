using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>La mano de dos cartas y su reposicion inmediata (DESIGN.md §8).</summary>
    [TestFixture]
    public sealed class HandTests
    {
        [Test]
        public void LaManoMuestraDosCartas()
        {
            Assert.That(Hand.Capacity, Is.EqualTo(2));
        }

        [Test]
        public void SeLlenaConLasDosPrimerasCartasDelMazo()
        {
            Deck mazo = Fabrica.Mazo(
                Fabrica.Monstruo("A"),
                Fabrica.Monstruo("B"),
                Fabrica.Monstruo("C"));
            Hand mano = new();

            Assert.That(mano.Refill(mazo), Is.EqualTo(2));
            Assert.That(mano[0].Definition.DisplayName, Is.EqualTo("A"));
            Assert.That(mano[1].Definition.DisplayName, Is.EqualTo("B"));
            Assert.That(mazo.Count, Is.EqualTo(1));
        }

        /// <summary>DESIGN.md §8: si en el mazo solo queda 1 carta, la mano muestra 1.</summary>
        [Test]
        public void ConUnaSolaCartaEnElMazoLaManoMuestraUna()
        {
            Deck mazo = Fabrica.Mazo(Fabrica.Monstruo("A"));
            Hand mano = new();

            mano.Refill(mazo);

            Assert.That(mano.Count, Is.EqualTo(1));
            Assert.That(mano[0], Is.Not.Null);
            Assert.That(mano[1], Is.Null);
        }

        [Test]
        public void ConElMazoVacioLaManoSeQuedaVacia()
        {
            Hand mano = new();

            Assert.That(mano.Refill(Fabrica.Mazo()), Is.EqualTo(0));
            Assert.That(mano.IsEmpty, Is.True);
        }

        /// <summary>
        /// El hueco que se juega se repone en su sitio, no se desplaza la otra
        /// carta: la que no has jugado no debe moverse de la pantalla.
        /// </summary>
        [Test]
        public void ElHuecoJugadoSeReponeEnSuMismaPosicion()
        {
            Deck mazo = Fabrica.Mazo(
                Fabrica.Monstruo("A"),
                Fabrica.Monstruo("B"),
                Fabrica.Monstruo("C"));
            Hand mano = new();
            mano.Refill(mazo);

            CardInstance jugada = mano.Take(0);
            mano.Refill(mazo);

            Assert.That(jugada.Definition.DisplayName, Is.EqualTo("A"));
            Assert.That(mano[0].Definition.DisplayName, Is.EqualTo("C"));
            Assert.That(mano[1].Definition.DisplayName, Is.EqualTo("B"));
        }

        [Test]
        public void SacarDeUnHuecoVacioDevuelveNull()
        {
            Hand mano = new();

            Assert.That(mano.Take(0), Is.Null);
        }

        [Test]
        public void LosHuecosQueNoExistenSeIgnoran()
        {
            Deck mazo = Fabrica.MazoDe(4);
            Hand mano = new();
            mano.Refill(mazo);

            Assert.That(mano[-1], Is.Null);
            Assert.That(mano[Hand.Capacity], Is.Null);
            Assert.That(mano.Take(9), Is.Null);
            Assert.That(mano.Count, Is.EqualTo(2));
        }
    }
}
