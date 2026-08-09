using ManaMaster.Core.Board;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Reglas de los tres carriles de la arena (DESIGN.md §2 y §6).
    /// </summary>
    [TestFixture]
    public sealed class BoardLanesTests
    {
        [Test]
        public void HayTresCarriles()
        {
            Assert.That(BoardLanes.Count, Is.EqualTo(3));
        }

        [Test]
        public void SoloLosIndicesDeCeroADosSonValidos()
        {
            Assert.That(BoardLanes.IsValid(-1), Is.False);
            Assert.That(BoardLanes.IsValid(0), Is.True);
            Assert.That(BoardLanes.IsValid(2), Is.True);
            Assert.That(BoardLanes.IsValid(3), Is.False);
        }

        [Test]
        public void ElCarrilPrincipalEsElFrontalYLosOtrosDosSonTraseros()
        {
            Assert.That(BoardLanes.IsFront(BoardLanes.Principal), Is.True);
            Assert.That(BoardLanes.IsRear(BoardLanes.Principal), Is.False);

            Assert.That(BoardLanes.IsRear(1), Is.True);
            Assert.That(BoardLanes.IsRear(2), Is.True);
            Assert.That(BoardLanes.IsFront(1), Is.False);
            Assert.That(BoardLanes.IsFront(2), Is.False);
        }

        /// <summary>
        /// DESIGN.md §6: los ataques a distancia se cruzan, mi carril 2 apunta al
        /// 3 del rival y mi 3 al 2.
        /// </summary>
        [Test]
        public void LosCarrilesTraserosSeCruzan()
        {
            Assert.That(BoardLanes.MirrorRear(1), Is.EqualTo(2));
            Assert.That(BoardLanes.MirrorRear(2), Is.EqualTo(1));
        }

        [Test]
        public void ElCarrilPrincipalNoTieneEspejo()
        {
            Assert.That(BoardLanes.MirrorRear(BoardLanes.Principal),
                Is.EqualTo(BoardLanes.Principal));
        }

        /// <summary>
        /// DESIGN.md §2: en codigo los carriles van de 0 a 2, pero al jugador se
        /// le muestran como 1, 2 y 3.
        /// </summary>
        [Test]
        public void ElNombreVisibleEmpiezaEnUno()
        {
            Assert.That(BoardLanes.ToDisplayName(0), Is.EqualTo("Carril 1"));
            Assert.That(BoardLanes.ToDisplayName(2), Is.EqualTo("Carril 3"));
        }
    }
}
