using System.Linq;
using ManaMaster.Core.Combat;
using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// La partida: turnos, mana por ronda y condiciones de derrota
    /// (DESIGN.md §5, §7 y §9).
    /// </summary>
    [TestFixture]
    public sealed class MatchStateTests
    {
        [Test]
        public void ElJugadorInicialSeSortea()
        {
            Assert.That(Partida(new AzarFijo(0)).Activo.Nombre, Is.EqualTo("Ana"));
            Assert.That(Partida(new AzarFijo(1)).Activo.Nombre, Is.EqualTo("Beto"));
        }

        [Test]
        public void ElPrimerTurnoYaConcedeMana()
        {
            MatchState partida = Partida(new AzarFijo(0));

            Assert.That(partida.Activo.Mana, Is.EqualTo(MatchState.ManaPorTurno));
            Assert.That(partida.Rival.Mana, Is.EqualTo(0));
            Assert.That(partida.Ronda, Is.EqualTo(1));
        }

        [Test]
        public void TerminarElTurnoCambiaDeJugadorSubeLaRondaYDaMana()
        {
            MatchState partida = Partida(new AzarFijo(0));
            PlayerState primero = partida.Activo;

            partida.TerminarTurno();

            Assert.That(partida.Activo, Is.Not.SameAs(primero));
            Assert.That(partida.Ronda, Is.EqualTo(2));
            Assert.That(partida.Activo.Mana, Is.EqualTo(MatchState.ManaPorTurno));
        }

        [Test]
        public void ElManaNoGastadoSeAcumulaEntreRondas()
        {
            MatchState partida = Partida(new AzarFijo(0));
            PlayerState primero = partida.Activo;

            partida.TerminarTurno();   // pasa a Beto
            partida.TerminarTurno();   // vuelve a Ana

            Assert.That(partida.Activo, Is.SameAs(primero));
            Assert.That(primero.Mana, Is.EqualTo(2 * MatchState.ManaPorTurno));
        }

        [Test]
        public void AlTerminarElTurnoSeResuelveElCombate()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 2));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("D1", vida: 9));
            MatchState partida = new(ana, beto, new AzarFijo(0));

            var eventos = partida.TerminarTurno();

            Assert.That(eventos.OfType<AtaqueResuelto>().Single().Dano, Is.EqualTo(2));
            Assert.That(beto.Arena[0].CurrentHealth, Is.EqualTo(7));
        }

        // ------------------------------------------------------------------
        // Derrota
        // ------------------------------------------------------------------

        /// <summary>
        /// Primera clausula del §9: sin monstruos en baraja, mano ni arena.
        /// </summary>
        [Test]
        public void PierdeQuienSeQuedaSinMonstruosEnNingunSitio()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 5));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("D1", vida: 1));
            MatchState partida = new(ana, beto, new AzarFijo(0));

            Assert.That(partida.Terminada, Is.False);

            partida.TerminarTurno();

            Assert.That(partida.Resultado,
                Is.EqualTo(ResultadoPartida.VictoriaJugador1));
            Assert.That(partida.Ganador, Is.SameAs(ana));
        }

        [Test]
        public void UnaPartidaTerminadaNoCambiaDeTurno()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 5));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("D1", vida: 1));
            MatchState partida = new(ana, beto, new AzarFijo(0));

            partida.TerminarTurno();
            int rondaAlAcabar = partida.Ronda;

            Assert.That(partida.TerminarTurno(), Is.Empty);
            Assert.That(partida.Ronda, Is.EqualTo(rondaAlAcabar));
            Assert.That(partida.Activo, Is.SameAs(ana));
        }

        [Test]
        public void UnaPartidaTerminadaNoAceptaJugadas()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 5));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("D1", vida: 1));
            MatchState partida = new(ana, beto, new AzarFijo(0));
            partida.TerminarTurno();

            Assert.That(partida.Desplegar(0, 0),
                Is.EqualTo(ResultadoDespliegue.HuecoVacio));
            Assert.That(partida.Sacrificar(0), Is.EqualTo(-1));
            Assert.That(partida.Equipar(0, 0), Is.EqualTo(ResultadoEquipar.HuecoVacio));
        }

        // ------------------------------------------------------------------
        // Objetos (DESIGN.md §4)
        // ------------------------------------------------------------------

        [Test]
        public void EquiparDelegaEnElJugadorActivo()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto"));
            PlayerState beto = new("Beto", Fabrica.MazoDe(10));
            MatchState partida = new(ana, beto, new AzarFijo(0));
            partida.Activo.IniciarObjetos(Fabrica.MazoDeObjetos(Fabrica.Objeto("Espada")));

            ResultadoEquipar resultado = partida.Equipar(huecoManoObjeto: 0, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.Ok));
            Assert.That(partida.Activo.Arena[0].EquippedItem?.CardId, Is.EqualTo("Espada"));
        }

        /// <summary>
        /// Confirma que CombatResolver no necesita saber nada de objetos: lee
        /// el ataque y la vida a traves de CardInstance, que ya incluyen el
        /// bonus. Sin el escudo, este ataque (3) mataria a D1 (vida 2).
        /// </summary>
        [Test]
        public void ElBonusDeVidaDelObjetoSeAplicaDeVerdadEnCombate()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 3));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("D1", vida: 2));
            beto.Arena[0].TryEquip(Fabrica.Objeto("Escudo", bonusVida: 3));
            MatchState partida = new(ana, beto, new AzarFijo(0));

            partida.TerminarTurno();

            Assert.That(beto.Arena.IsEmpty, Is.False, "el escudo deberia haberlo salvado");
            Assert.That(beto.Arena[0].CurrentHealth, Is.EqualTo(2));
        }

        /// <summary>
        /// El objeto no vive en ningun sitio aparte del monstruo: si el
        /// monstruo muere en combate, el objeto se pierde con el sin que
        /// haga falta ningun codigo extra (DESIGN.md §4).
        /// </summary>
        [Test]
        public void SiElMonstruoMuereEnCombateElObjetoSePierdeConEl()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 10));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("D1", vida: 2));
            beto.Arena[0].TryEquip(Fabrica.Objeto("Escudo", bonusVida: 3));
            MatchState partida = new(ana, beto, new AzarFijo(0));

            Assert.That(beto.Arena[0].EquippedItem, Is.Not.Null, "el escudo deberia estar puesto");

            partida.TerminarTurno();

            Assert.That(beto.Arena.IsEmpty, Is.True,
                "el ataque (10) supera incluso la vida con escudo (5)");
        }

        /// <summary>
        /// Segunda clausula del §9, implementada al pie de la letra: sin nada en
        /// la arena y sin mana para desplegar, se pierde. Como el mana se
        /// acumula, en la practica esto castiga tener la mano cara en la ronda 1;
        /// esta anotado en MatchState como pendiente de revisar.
        /// </summary>
        [Test]
        public void PierdeQuienNoTieneArenaNiManaParaDesplegar()
        {
            PlayerState ana = new("Ana", Fabrica.MazoDe(10, coste: 5));
            PlayerState beto = new("Beto", Fabrica.MazoDe(10, coste: 1));

            MatchState partida = new(ana, beto, new AzarFijo(0));

            Assert.That(partida.Resultado,
                Is.EqualTo(ResultadoPartida.VictoriaJugador2));
        }

        [Test]
        public void ConManaSuficienteNoSePierdePorAhogo()
        {
            PlayerState ana = new("Ana", Fabrica.MazoDe(10, coste: 3));
            PlayerState beto = new("Beto", Fabrica.MazoDe(10, coste: 1));

            MatchState partida = new(ana, beto, new AzarFijo(0));

            Assert.That(partida.Terminada, Is.False);
        }

        [Test]
        public void SinFuenteDeAzarNoSePuedeEmpezar()
        {
            PlayerState ana = new("Ana", Fabrica.MazoDe(4));
            PlayerState beto = new("Beto", Fabrica.MazoDe(4));

            Assert.That(() => new MatchState(ana, beto, null),
                Throws.ArgumentNullException);
            Assert.That(() => new MatchState(null, beto, new AzarFijo(0)),
                Throws.ArgumentNullException);
        }

        private static MatchState Partida(AzarFijo azar)
            => new(
                new PlayerState("Ana", Fabrica.MazoDe(10)),
                new PlayerState("Beto", Fabrica.MazoDe(10)),
                azar);
    }
}
