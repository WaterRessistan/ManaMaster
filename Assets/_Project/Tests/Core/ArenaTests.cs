using ManaMaster.Core.Board;
using ManaMaster.Core.Cards;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// La arena: insercion con empuje y la invariante de que nunca hay huecos
    /// (DESIGN.md §2 y §3).
    /// </summary>
    [TestFixture]
    public sealed class ArenaTests
    {
        [Test]
        public void NaceVacia()
        {
            Arena arena = new();

            Assert.That(arena.Count, Is.EqualTo(0));
            Assert.That(arena.IsEmpty, Is.True);
            Assert.That(arena.IsFull, Is.False);
            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("- - -"));
        }

        [Test]
        public void EnUnaArenaVaciaSoloSePuedeColocarEnElCarrilPrincipal()
        {
            Arena arena = new();

            Assert.That(arena.CanInsertAt(0), Is.True);
            Assert.That(arena.CanInsertAt(1), Is.False);
            Assert.That(arena.CanInsertAt(2), Is.False);
        }

        // ------------------------------------------------------------------
        // Los tres ejemplos del DESIGN.md §3, tal cual estan escritos:
        //
        //   Estado:   [1] A   [2] B   [3] —
        //   Inserta C en 1  →  [1] C   [2] A   [3] B
        //   Inserta C en 2  →  [1] A   [2] C   [3] B
        //   Inserta C en 3  →  [1] A   [2] B   [3] C
        // ------------------------------------------------------------------

        [Test]
        public void InsertarDelanteEmpujaTodoHaciaAtras()
        {
            Arena arena = ArenaConAyB();

            arena.Insert(0, Fabrica.Monstruo("C"));

            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("C A B"));
        }

        [Test]
        public void InsertarEnMedioEmpujaSoloLoQueHayDetras()
        {
            Arena arena = ArenaConAyB();

            arena.Insert(1, Fabrica.Monstruo("C"));

            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("A C B"));
        }

        [Test]
        public void InsertarDetrasNoMueveNada()
        {
            Arena arena = ArenaConAyB();

            arena.Insert(2, Fabrica.Monstruo("C"));

            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("A B C"));
        }

        // ------------------------------------------------------------------

        [Test]
        public void NoSePuedeDejarUnHuecoPorDelante()
        {
            Arena arena = new();
            arena.Insert(0, Fabrica.Monstruo("A"));

            // Con un monstruo desplegado, el carril 3 (indice 2) dejaria libre el 2.
            Assert.That(arena.CanInsertAt(2), Is.False);
            Assert.That(() => arena.Insert(2, Fabrica.Monstruo("B")),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void NoSePuedeInsertarEnUnaArenaLlena()
        {
            Arena arena = ArenaLlena();

            Assert.That(arena.IsFull, Is.True);
            Assert.That(arena.CanInsertAt(0), Is.False);
            Assert.That(() => arena.Insert(0, Fabrica.Monstruo("D")),
                Throws.TypeOf<System.InvalidOperationException>());
        }

        [Test]
        public void RetirarDelCarrilPrincipalAdelantaALosDeDetras()
        {
            Arena arena = ArenaLlena();

            CardInstance retirado = arena.RemoveAt(0);

            Assert.That(retirado.Definition.DisplayName, Is.EqualTo("A"));
            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("B C -"));
        }

        [Test]
        public void RetirarDeEnMedioCierraElHueco()
        {
            Arena arena = ArenaLlena();

            arena.RemoveAt(1);

            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("A C -"));
        }

        [Test]
        public void RetirarUnCarrilVacioNoHaceNada()
        {
            Arena arena = ArenaConAyB();

            Assert.That(arena.RemoveAt(2), Is.Null);
            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("A B -"));
        }

        [Test]
        public void LosMuertosSalenDeLaArenaYSeCierraElHueco()
        {
            Arena arena = new();
            CardInstance a = Fabrica.Monstruo("A", vida: 3);
            CardInstance b = Fabrica.Monstruo("B", vida: 3);
            CardInstance c = Fabrica.Monstruo("C", vida: 3);
            arena.Insert(0, a);
            arena.Insert(1, b);
            arena.Insert(2, c);

            a.ReceiveDamage(3);

            var caidos = arena.RemoveDead();

            Assert.That(caidos, Has.Count.EqualTo(1));
            Assert.That(caidos[0], Is.SameAs(a));
            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("B C -"));
        }

        [Test]
        public void SinMuertosLaArenaNoSeToca()
        {
            Arena arena = ArenaConAyB();

            Assert.That(arena.RemoveDead(), Is.Empty);
            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("A B -"));
        }

        [Test]
        public void VariosMuertosALaVezSeRetiranEnUnaSolaPasada()
        {
            Arena arena = new();
            CardInstance a = Fabrica.Monstruo("A", vida: 1);
            CardInstance b = Fabrica.Monstruo("B", vida: 1);
            CardInstance c = Fabrica.Monstruo("C", vida: 5);
            arena.Insert(0, a);
            arena.Insert(1, b);
            arena.Insert(2, c);

            a.ReceiveDamage(1);
            b.ReceiveDamage(1);

            Assert.That(arena.RemoveDead(), Has.Count.EqualTo(2));
            Assert.That(Fabrica.Disposicion(arena), Is.EqualTo("C - -"));
        }

        [Test]
        public void SabeEnQueCarrilEstaCadaMonstruo()
        {
            Arena arena = new();
            CardInstance a = Fabrica.Monstruo("A");
            CardInstance b = Fabrica.Monstruo("B");
            arena.Insert(0, a);
            arena.Insert(0, b);

            Assert.That(arena.IndexOf(b), Is.EqualTo(0));
            Assert.That(arena.IndexOf(a), Is.EqualTo(1));
            Assert.That(arena.IndexOf(Fabrica.Monstruo("Z")), Is.EqualTo(-1));
            Assert.That(arena.Contains(a), Is.True);
        }

        [Test]
        public void LosDesplegadosSalenEnOrdenDeCarrilYSinHuecos()
        {
            Arena arena = ArenaConAyB();

            var desplegados = arena.Desplegados;

            Assert.That(desplegados, Has.Count.EqualTo(2));
            Assert.That(desplegados[0].Definition.DisplayName, Is.EqualTo("A"));
            Assert.That(desplegados[1].Definition.DisplayName, Is.EqualTo("B"));
        }

        private static Arena ArenaConAyB()
        {
            Arena arena = new();
            arena.Insert(0, Fabrica.Monstruo("A"));
            arena.Insert(1, Fabrica.Monstruo("B"));
            return arena;
        }

        private static Arena ArenaLlena()
        {
            Arena arena = ArenaConAyB();
            arena.Insert(2, Fabrica.Monstruo("C"));
            return arena;
        }
    }
}
