using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>El sacrificio voluntario de un monstruo propio (DESIGN.md §7).</summary>
    [TestFixture]
    public sealed class SacrificioTests
    {
        [Test]
        public void DevuelveLaMitadDelCosteYRetiraElMonstruo()
        {
            PlayerState jugador = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Caro", coste: 4));

            int recuperado = jugador.TrySacrificar(0);

            Assert.That(recuperado, Is.EqualTo(2));
            Assert.That(jugador.Mana, Is.EqualTo(2));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
        }

        /// <summary>
        /// El §7 avisa de esto: con la formula actual una carta de coste 1 no
        /// devuelve nada. Queda pendiente de la fase de balanceo.
        /// </summary>
        [Test]
        public void UnMonstruoDeCosteUnoNoDevuelveMana()
        {
            PlayerState jugador = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Barato", coste: 1));

            Assert.That(jugador.TrySacrificar(0), Is.EqualTo(0));
            Assert.That(jugador.Mana, Is.EqualTo(0));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
        }

        [Test]
        public void ElHuecoSeCierraAlSacrificar()
        {
            PlayerState jugador = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("A"),
                Fabrica.Monstruo("B"),
                Fabrica.Monstruo("C"));

            jugador.TrySacrificar(0);

            Assert.That(Fabrica.Disposicion(jugador.Arena), Is.EqualTo("B C -"));
        }

        [Test]
        public void SacrificarUnCarrilVacioNoHaceNada()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("A"));

            Assert.That(jugador.TrySacrificar(2), Is.EqualTo(-1));
            Assert.That(jugador.TrySacrificar(99), Is.EqualTo(-1));
            Assert.That(jugador.Arena.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// El monstruo sale de la partida definitivamente, asi que sacrificar
        /// acerca a la derrota del §9.
        /// </summary>
        [Test]
        public void ElMonstruoSacrificadoNoVuelveALaPartida()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("A"));

            Assert.That(jugador.MonstruosRestantes, Is.EqualTo(1));

            jugador.TrySacrificar(0);

            Assert.That(jugador.MonstruosRestantes, Is.EqualTo(0));
            Assert.That(jugador.SinMonstruos, Is.True);
        }

        [Test]
        public void SoloSacrificaElJugadorActivo()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("A", coste: 4));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("B", coste: 4));
            MatchState partida = new(ana, beto, new AzarFijo(0));

            partida.Sacrificar(0);

            Assert.That(ana.Arena.IsEmpty, Is.True, "sacrifica el jugador activo");
            Assert.That(beto.Arena.Count, Is.EqualTo(1), "el rival no se toca");
        }
    }
}
