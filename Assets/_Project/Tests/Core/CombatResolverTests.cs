using System.Collections.Generic;
using System.Linq;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Combat;
using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>La fase de combate completa (DESIGN.md §5 y §6).</summary>
    [TestFixture]
    public sealed class CombatResolverTests
    {
        // ------------------------------------------------------------------
        // Curacion
        // ------------------------------------------------------------------

        /// <summary>
        /// DESIGN.md §6: el curandero cura a CADA aliado en arena, incluido el
        /// mismo, y nunca por encima de la vida maxima.
        /// </summary>
        [Test]
        public void ElCuranderoCuraATodosLosAliadosIncluidoElMismo()
        {
            CardInstance curandero = Fabrica.Monstruo("Curandero", vida: 5, cura: 2);
            CardInstance companero = Fabrica.Monstruo("Companero", vida: 5);
            curandero.ReceiveDamage(3);
            companero.ReceiveDamage(4);

            PlayerState atacante = Fabrica.Jugador("Ana", curandero, companero);
            PlayerState defensor = Fabrica.Jugador("Beto");

            CombatResolver.Resolver(atacante, defensor);

            Assert.That(curandero.CurrentHealth, Is.EqualTo(4));
            Assert.That(companero.CurrentHealth, Is.EqualTo(3));
        }

        [Test]
        public void LaCuracionNoPasaDeLaVidaMaxima()
        {
            CardInstance curandero = Fabrica.Monstruo("Curandero", vida: 5, cura: 9);
            curandero.ReceiveDamage(1);

            PlayerState atacante = Fabrica.Jugador("Ana", curandero);

            var eventos = CombatResolver.Resolver(atacante, Fabrica.Jugador("Beto"));

            Assert.That(curandero.CurrentHealth, Is.EqualTo(5));
            Assert.That(eventos.OfType<CuracionAplicada>().Single().Cantidad,
                Is.EqualTo(1));
        }

        [Test]
        public void CurarAQuienEstaIntactoNoGeneraEvento()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Curandero", vida: 5, cura: 2));

            var eventos = CombatResolver.Resolver(atacante, Fabrica.Jugador("Beto"));

            Assert.That(eventos.OfType<CuracionAplicada>(), Is.Empty);
        }

        /// <summary>DESIGN.md §5: la curacion va antes que todos los ataques.</summary>
        [Test]
        public void SeCuraAntesDeAtacar()
        {
            CardInstance curandero =
                Fabrica.Monstruo("Curandero", vida: 5, cura: 2, ataque: 1);
            curandero.ReceiveDamage(3);

            PlayerState atacante = Fabrica.Jugador("Ana", curandero);
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 9));

            var eventos = CombatResolver.Resolver(atacante, defensor);

            Assert.That(eventos.First(), Is.TypeOf<CuracionAplicada>());
            Assert.That(eventos.Any(e => e is AtaqueResuelto), Is.True);
            Assert.That(
                eventos.ToList().FindIndex(e => e is CuracionAplicada),
                Is.LessThan(eventos.ToList().FindIndex(e => e is AtaqueResuelto)));
        }

        // ------------------------------------------------------------------
        // Quien puede atacar
        // ------------------------------------------------------------------

        [Test]
        public void UnRangoPuroEnElCarrilPrincipalNoAtaca()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Arquero", ataque: 3, melee: false, rango: true));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 9));

            var eventos = CombatResolver.Resolver(atacante, defensor);

            Assert.That(eventos, Is.Empty);
            Assert.That(defensor.Arena[0].CurrentHealth, Is.EqualTo(9));
        }

        [Test]
        public void UnMeleePuroEnUnCarrilTraseroNoAtaca()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Escudo", ataque: 0),
                Fabrica.Monstruo("Bruto", ataque: 4));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 9));

            var eventos = CombatResolver.Resolver(atacante, defensor);

            Assert.That(eventos, Is.Empty);
            Assert.That(defensor.Arena[0].CurrentHealth, Is.EqualTo(9));
        }

        [Test]
        public void UnMonstruoSinAtaqueNoGolpea()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Muro", vida: 9, ataque: 0));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 9));

            Assert.That(CombatResolver.Resolver(atacante, defensor), Is.Empty);
        }

        [Test]
        public void AtacarSinNadaEnfrenteSeAnotaPeroNoHaceNada()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Bruto", ataque: 4));

            var eventos = CombatResolver.Resolver(atacante, Fabrica.Jugador("Beto"));

            Assert.That(eventos.Single(), Is.TypeOf<AtaqueSinObjetivo>());
        }

        // ------------------------------------------------------------------
        // Orden, muerte y compactacion
        // ------------------------------------------------------------------

        [Test]
        public void ElAtaqueCruzadoGolpeaAlTraseroContrario()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Escudo", ataque: 0),
                Fabrica.Monstruo("Arquero", ataque: 1, melee: false, rango: true));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 9),
                Fabrica.Monstruo("D2", vida: 9),
                Fabrica.Monstruo("D3", vida: 9));

            var eventos = CombatResolver.Resolver(atacante, defensor);
            AtaqueResuelto ataque = eventos.OfType<AtaqueResuelto>().Single();

            // Mi carril 2 (indice 1) golpea a su carril 3 (indice 2).
            Assert.That(ataque.CarrilObjetivo, Is.EqualTo(2));
            Assert.That(ataque.Objetivo.Definition.DisplayName, Is.EqualTo("D3"));
        }

        [Test]
        public void AlMorirUnMonstruoLosDeDetrasAvanzan()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Bruto", ataque: 2));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 2),
                Fabrica.Monstruo("D2", vida: 9));

            var eventos = CombatResolver.Resolver(atacante, defensor);

            Assert.That(Fabrica.Disposicion(defensor.Arena), Is.EqualTo("D2 - -"));
            Assert.That(eventos.OfType<MonstruoDerrotado>().Single()
                .Monstruo.Definition.DisplayName, Is.EqualTo("D1"));
            Assert.That(eventos.Any(e => e is ArenaCompactada), Is.True);
        }

        [Test]
        public void MatarEnElUltimoCarrilNoCompactaNada()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("Bruto", ataque: 5));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 5));

            var eventos = CombatResolver.Resolver(atacante, defensor);

            Assert.That(defensor.Arena.IsEmpty, Is.True);
            Assert.That(eventos.OfType<ArenaCompactada>(), Is.Empty);
        }

        /// <summary>
        /// El caso que el DESIGN.md §6 pone como consecuencia tactica: cada
        /// muerte compacta la arena rival y el atacante siguiente recalcula su
        /// objetivo sobre el tablero ya movido, encadenando bajas.
        /// </summary>
        [Test]
        public void CadaAtacanteApuntaSobreElTableroYaCompactado()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("A1", ataque: 5),
                Fabrica.Monstruo("A2", ataque: 5, melee: false, rango: true),
                Fabrica.Monstruo("A3", ataque: 5, melee: false, rango: true));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 1),
                Fabrica.Monstruo("D2", vida: 1),
                Fabrica.Monstruo("D3", vida: 1));

            var eventos = CombatResolver.Resolver(atacante, defensor);

            List<string> caidos = eventos.OfType<MonstruoDerrotado>()
                .Select(e => e.Monstruo.Definition.DisplayName)
                .ToList();

            // A1 mata a D1 por delante; al compactar, D3 pasa al unico trasero y
            // se lo lleva A2; A3 ya solo encuentra a D2 en el carril principal.
            Assert.That(caidos, Is.EqualTo(new[] { "D1", "D3", "D2" }));
            Assert.That(defensor.Arena.IsEmpty, Is.True);
        }

        [Test]
        public void LosAtaquesSalenEnOrdenDeCarril()
        {
            PlayerState atacante = Fabrica.Jugador("Ana",
                Fabrica.Monstruo("A1", ataque: 1),
                Fabrica.Monstruo("A2", ataque: 1, melee: false, rango: true),
                Fabrica.Monstruo("A3", ataque: 1, melee: false, rango: true));
            PlayerState defensor = Fabrica.Jugador("Beto",
                Fabrica.Monstruo("D1", vida: 9),
                Fabrica.Monstruo("D2", vida: 9),
                Fabrica.Monstruo("D3", vida: 9));

            var atacantes = CombatResolver.Resolver(atacante, defensor)
                .OfType<AtaqueResuelto>()
                .Select(e => e.Atacante.Definition.DisplayName)
                .ToList();

            Assert.That(atacantes, Is.EqualTo(new[] { "A1", "A2", "A3" }));
        }

        [Test]
        public void SinJugadoresNoSePuedeResolver()
        {
            PlayerState jugador = Fabrica.Jugador("Ana");

            Assert.That(() => CombatResolver.Resolver(null, jugador),
                Throws.ArgumentNullException);
            Assert.That(() => CombatResolver.Resolver(jugador, null),
                Throws.ArgumentNullException);
        }
    }
}
