using System.Collections.Generic;
using System.Linq;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Combat;
using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// El guion que permite reproducir el combate paso a paso cuando el motor
    /// ya lo ha resuelto entero.
    /// </summary>
    [TestFixture]
    public sealed class GuionDeCombateTests
    {
        [Test]
        public void ElPrimerFotogramaEsElEstadoAnteriorAlCombate()
        {
            CardInstance bruto = Fabrica.Monstruo("Bruto", ataque: 2);
            CardInstance defensor = Fabrica.Monstruo("D1", vida: 9);
            PlayerState ana = Fabrica.Jugador("Ana", bruto);
            PlayerState beto = Fabrica.Jugador("Beto", defensor);

            var guion = Reproducir(ana, beto);

            FotogramaCombate inicial = guion[0];

            Assert.That(inicial.Evento, Is.Null);
            Assert.That(inicial.ArenaDefensora, Has.Count.EqualTo(1));
            Assert.That(inicial.Vidas[defensor], Is.EqualTo(9),
                "la vida de antes del golpe, no la de despues");
        }

        [Test]
        public void CadaGolpeDejaSuPropioFotogramaConLaVidaDeEseMomento()
        {
            CardInstance defensor = Fabrica.Monstruo("D1", vida: 9);
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 2));
            PlayerState beto = Fabrica.Jugador("Beto", defensor);

            var guion = Reproducir(ana, beto);

            FotogramaCombate golpe = guion.Last();

            Assert.That(golpe.Evento, Is.TypeOf<AtaqueResuelto>());
            Assert.That(golpe.Vidas[defensor], Is.EqualTo(7));
        }

        /// <summary>
        /// El caso que justifica todo esto: al morir el del carril 1, el de
        /// detras avanza, y hay que poder verlo antes de que ataque el
        /// siguiente (DESIGN.md §6).
        /// </summary>
        [Test]
        public void AlMorirUnoElDeDetrasAvanzaEnElFotogramaSiguiente()
        {
            CardInstance d1 = Fabrica.Monstruo("D1", vida: 2);
            CardInstance d2 = Fabrica.Monstruo("D2", vida: 9);
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 2));
            PlayerState beto = Fabrica.Jugador("Beto", d1, d2);

            var guion = Reproducir(ana, beto);

            FotogramaCombate antesDeMorir =
                guion.Last(f => f.Evento is AtaqueResuelto);
            FotogramaCombate alMorir =
                guion.Last(f => f.Evento is MonstruoDerrotado);

            Assert.That(antesDeMorir.ArenaDefensora.Select(m => m.Definition.DisplayName),
                Is.EqualTo(new[] { "D1", "D2" }), "todavia estan los dos");
            Assert.That(alMorir.ArenaDefensora.Select(m => m.Definition.DisplayName),
                Is.EqualTo(new[] { "D2" }), "D2 ha avanzado al carril principal");
        }

        [Test]
        public void LaCuracionTambienDejaSuFotograma()
        {
            CardInstance curandero = Fabrica.Monstruo("Curandero", vida: 5, cura: 2);
            curandero.ReceiveDamage(3);

            PlayerState ana = Fabrica.Jugador("Ana", curandero);
            PlayerState beto = Fabrica.Jugador("Beto");

            var guion = Reproducir(ana, beto);

            Assert.That(guion[0].Vidas[curandero], Is.EqualTo(2));
            Assert.That(guion.Last().Evento, Is.TypeOf<CuracionAplicada>());
            Assert.That(guion.Last().Vidas[curandero], Is.EqualTo(4));
        }

        [Test]
        public void SinEventosElGuionEsSoloElEstadoInicial()
        {
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Muro", ataque: 0));
            PlayerState beto = Fabrica.Jugador("Beto", Fabrica.Monstruo("D1", vida: 5));

            var guion = Reproducir(ana, beto);

            Assert.That(guion, Has.Count.EqualTo(1));
        }

        [Test]
        public void LosFotogramasNoCompartenEstado()
        {
            CardInstance defensor = Fabrica.Monstruo("D1", vida: 9);
            PlayerState ana = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", ataque: 2));
            PlayerState beto = Fabrica.Jugador("Beto", defensor);

            var guion = Reproducir(ana, beto);

            Assert.That(guion[0].Vidas[defensor], Is.EqualTo(9));
            Assert.That(guion.Last().Vidas[defensor], Is.EqualTo(7),
                "cada fotograma lleva su propia copia");
        }

        /// <summary>
        /// Toma las fotos, resuelve el combate y monta el guion, que es como lo
        /// hara la interfaz.
        /// </summary>
        private static IReadOnlyList<FotogramaCombate> Reproducir(
            PlayerState atacante, PlayerState defensor)
        {
            InstantaneaDeArena fotoAtacante = InstantaneaDeArena.Tomar(atacante.Arena);
            InstantaneaDeArena fotoDefensor = InstantaneaDeArena.Tomar(defensor.Arena);

            var eventos = CombatResolver.Resolver(atacante, defensor);

            return GuionDeCombate.Construir(fotoAtacante, fotoDefensor, eventos);
        }
    }
}
