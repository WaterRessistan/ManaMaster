using ManaMaster.Core.Agents;
using ManaMaster.Core.Board;
using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>Como decide la IA de la v1.</summary>
    [TestFixture]
    public sealed class AgenteHeuristicoTests
    {
        private readonly AgenteHeuristico _agente = new();

        [Test]
        public void GastaElManaEnLaCartaMasCaraQuePuedePagar()
        {
            PlayerState ana = new("Ana", Fabrica.Mazo(
                Fabrica.Monstruo("Barata", coste: 1),
                Fabrica.Monstruo("Cara", coste: 3)));

            AccionTurno accion = _agente.DecidirAccion(PartidaCon(ana));

            Assert.That(accion.Tipo, Is.EqualTo(TipoAccion.Desplegar));
            Assert.That(accion.HuecoMano, Is.EqualTo(1), "la de coste 3");
        }

        [Test]
        public void EntreDosCartasDelMismoCosteEligeLaQuePegaMas()
        {
            PlayerState ana = new("Ana", Fabrica.Mazo(
                Fabrica.Monstruo("Floja", coste: 2, ataque: 1),
                Fabrica.Monstruo("Fuerte", coste: 2, ataque: 3)));

            AccionTurno accion = _agente.DecidirAccion(PartidaCon(ana));

            Assert.That(accion.HuecoMano, Is.EqualTo(1));
        }

        [Test]
        public void NoDespliegaLoQueNoPuedePagar()
        {
            // Con algo en la arena para que no salte la derrota por ahogo del §9.
            PlayerState ana = new("Ana", Fabrica.Mazo(
                Fabrica.Monstruo("Carisima", coste: 9)));
            ana.Arena.Insert(0, Fabrica.Monstruo("Veterano"));

            AccionTurno accion = _agente.DecidirAccion(PartidaCon(ana));

            Assert.That(accion.Tipo, Is.EqualTo(TipoAccion.TerminarTurno));
        }

        [Test]
        public void ConLaArenaLlenaTerminaElTurno()
        {
            PlayerState ana = new("Ana", Fabrica.MazoDe(10));
            ana.Arena.Insert(0, Fabrica.Monstruo("A"));
            ana.Arena.Insert(1, Fabrica.Monstruo("B"));
            ana.Arena.Insert(2, Fabrica.Monstruo("C"));

            Assert.That(_agente.DecidirAccion(PartidaCon(ana)).Tipo,
                Is.EqualTo(TipoAccion.TerminarTurno));
        }

        [Test]
        public void ConLaArenaVaciaColocaEnElCarrilPrincipal()
        {
            PlayerState ana = new("Ana", Fabrica.MazoDe(10));

            Assert.That(_agente.DecidirAccion(PartidaCon(ana)).Carril,
                Is.EqualTo(BoardLanes.Principal));
        }

        /// <summary>
        /// Colocar por delante empujaria hacia atras al que ya esta atacando
        /// desde el frente, asi que por defecto se anade al final.
        /// </summary>
        [Test]
        public void PorDefectoColocaDetrasParaNoEstorbarAlDelFrente()
        {
            PlayerState ana = new("Ana", Fabrica.MazoDe(10));
            ana.Arena.Insert(0, Fabrica.Monstruo("Bruto", ataque: 3));

            Assert.That(_agente.DecidirAccion(PartidaCon(ana)).Carril,
                Is.EqualTo(1));
        }

        /// <summary>
        /// El caso para el que existe la insercion con empuje (DESIGN.md §3): un
        /// monstruo de rango atrapado en el carril principal no ataca, y meterle
        /// por delante un cuerpo a cuerpo lo libera.
        /// </summary>
        [Test]
        public void MeteUnCuerpoACuerpoPorDelanteParaLiberarAlRangoAtrapado()
        {
            PlayerState ana = new("Ana", Fabrica.Mazo(
                Fabrica.Monstruo("Bruto", coste: 1, ataque: 3)));
            ana.Arena.Insert(0,
                Fabrica.Monstruo("Arquero", ataque: 3, melee: false, rango: true));

            AccionTurno accion = _agente.DecidirAccion(PartidaCon(ana));

            Assert.That(accion.Tipo, Is.EqualTo(TipoAccion.Desplegar));
            Assert.That(accion.Carril, Is.EqualTo(BoardLanes.Principal));
        }

        [Test]
        public void NoSacrificaNunca()
        {
            PlayerState ana = new("Ana", Fabrica.MazoDe(10, coste: 9));
            ana.Arena.Insert(0, Fabrica.Monstruo("Caro", coste: 8));

            Assert.That(_agente.DecidirAccion(PartidaCon(ana)).Tipo,
                Is.Not.EqualTo(TipoAccion.Sacrificar));
        }

        [Test]
        public void ConLaPartidaTerminadaNoPideNada()
        {
            Assert.That(_agente.DecidirAccion(null).Tipo,
                Is.EqualTo(TipoAccion.TerminarTurno));
        }

        /// <summary>Ana es el jugador 1 y AzarFijo(0) le da a ella el turno.</summary>
        private static MatchState PartidaCon(PlayerState ana)
            => new(ana, new PlayerState("Beto", Fabrica.MazoDe(10)), new AzarFijo(0));
    }
}
