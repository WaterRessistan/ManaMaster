using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Estado de un jugador: mana, mano, mazo y despliegue (DESIGN.md §3 y §7).
    /// </summary>
    [TestFixture]
    public sealed class PlayerStateTests
    {
        [Test]
        public void EmpiezaSinManaYConLaManoRepartida()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));

            Assert.That(jugador.Mana, Is.EqualTo(0));
            Assert.That(jugador.Mano.Count, Is.EqualTo(2));
            Assert.That(jugador.Mazo.Count, Is.EqualTo(8));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
        }

        [Test]
        public void ElManaSeAcumulaYNoTieneTope()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));

            for (int ronda = 0; ronda < 10; ronda++)
            {
                jugador.GanarMana(3);
            }

            Assert.That(jugador.Mana, Is.EqualTo(30));
        }

        [Test]
        public void NoSeGastaManaQueNoSeTiene()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(3);

            Assert.That(jugador.TryGastarMana(4), Is.False);
            Assert.That(jugador.Mana, Is.EqualTo(3));

            Assert.That(jugador.TryGastarMana(3), Is.True);
            Assert.That(jugador.Mana, Is.EqualTo(0));
        }

        [Test]
        public void DesplegarPagaElCosteColocaYReponeLaMano()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10, coste: 2));
            jugador.GanarMana(3);

            ResultadoDespliegue resultado = jugador.TryDesplegar(huecoMano: 0, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.Ok));
            Assert.That(jugador.Mana, Is.EqualTo(1));
            Assert.That(jugador.Arena.Count, Is.EqualTo(1));
            Assert.That(jugador.Mano.Count, Is.EqualTo(2), "la mano se repone al momento");
            Assert.That(jugador.Mazo.Count, Is.EqualTo(7));
        }

        [Test]
        public void ElDespliegueEmpujaComoEnElReglamento()
        {
            // La mano arranca con [A, B] y el mazo guarda [C, D]. Ojo al hueco
            // que se juega: al vaciarlo se repone al momento con la carta
            // siguiente del mazo, no se desplaza la otra carta de la mano.
            PlayerState jugador = new("Ana", Fabrica.Mazo(
                Fabrica.Monstruo("A"),
                Fabrica.Monstruo("B"),
                Fabrica.Monstruo("C"),
                Fabrica.Monstruo("D")));
            jugador.GanarMana(10);

            jugador.TryDesplegar(huecoMano: 0, carril: 0);   // A al carril 1
            Assert.That(Fabrica.Disposicion(jugador.Arena), Is.EqualTo("A - -"));

            jugador.TryDesplegar(huecoMano: 1, carril: 1);   // B detras de A
            Assert.That(Fabrica.Disposicion(jugador.Arena), Is.EqualTo("A B -"));

            // C entra por delante y empuja a los dos hacia atras: el ejemplo
            // literal del DESIGN.md §3.
            jugador.TryDesplegar(huecoMano: 0, carril: 0);

            Assert.That(Fabrica.Disposicion(jugador.Arena), Is.EqualTo("C A B"));
        }

        [Test]
        public void SinManaSuficienteNoSeTocaNada()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10, coste: 5));
            jugador.GanarMana(3);

            ResultadoDespliegue resultado = jugador.TryDesplegar(0, 0);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.ManaInsuficiente));
            Assert.That(jugador.Mana, Is.EqualTo(3));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
            Assert.That(jugador.Mano.Count, Is.EqualTo(2));
        }

        [Test]
        public void NoSePuedeDesplegarDejandoUnHueco()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(10);

            ResultadoDespliegue resultado = jugador.TryDesplegar(0, carril: 2);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.CarrilInvalido));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
            Assert.That(jugador.Mana, Is.EqualTo(10));
        }

        [Test]
        public void ConLaArenaLlenaNoSeDespliegaMas()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(10);
            jugador.TryDesplegar(0, 0);
            jugador.TryDesplegar(0, 1);
            jugador.TryDesplegar(0, 2);

            ResultadoDespliegue resultado = jugador.TryDesplegar(0, 0);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.ArenaLlena));
            Assert.That(jugador.Arena.Count, Is.EqualTo(3));
        }

        [Test]
        public void UnHuecoVacioDeLaManoNoDespliegaNada()
        {
            PlayerState jugador = new("Ana", Fabrica.Mazo(Fabrica.Monstruo("A")));
            jugador.GanarMana(10);

            // Solo habia una carta: el segundo hueco esta vacio.
            Assert.That(jugador.TryDesplegar(1, 0),
                Is.EqualTo(ResultadoDespliegue.HuecoVacio));
            Assert.That(jugador.TryDesplegar(99, 0),
                Is.EqualTo(ResultadoDespliegue.HuecoVacio));
        }

        /// <summary>DESIGN.md §9: cuentan los monstruos de mazo, mano y arena.</summary>
        [Test]
        public void LosMonstruosRestantesSonMazoMasManoMasArena()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(10);

            Assert.That(jugador.MonstruosRestantes, Is.EqualTo(10));

            jugador.TryDesplegar(0, 0);

            Assert.That(jugador.MonstruosRestantes, Is.EqualTo(10),
                "desplegar mueve la carta de sitio, no la elimina");
            Assert.That(jugador.SinMonstruos, Is.False);
        }

        [Test]
        public void SabeSiPuedePagarAlgunaCartaDeLaMano()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10, coste: 3));

            jugador.GanarMana(2);
            Assert.That(jugador.PuedeDesplegarAlguna(), Is.False);

            jugador.GanarMana(1);
            Assert.That(jugador.PuedeDesplegarAlguna(), Is.True);
        }

        [Test]
        public void ConLaArenaLlenaNoPuedeDesplegarAunqueLeSobreMana()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(50);
            jugador.TryDesplegar(0, 0);
            jugador.TryDesplegar(0, 1);
            jugador.TryDesplegar(0, 2);

            Assert.That(jugador.PuedeDesplegarAlguna(), Is.False);
        }
    }
}
