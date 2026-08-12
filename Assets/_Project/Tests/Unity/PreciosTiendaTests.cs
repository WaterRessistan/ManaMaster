using ManaMaster.Core.Cards;
using ManaMaster.Unity.Tienda;
using NUnit.Framework;

namespace ManaMaster.Unity.Tests
{
    /// <summary>Los precios provisionales de DESIGN.md §10.</summary>
    [TestFixture]
    public sealed class PreciosTiendaTests
    {
        [TestCase(CardRarity.Comun, 50)]
        [TestCase(CardRarity.Rara, 150)]
        [TestCase(CardRarity.Epica, 500)]
        [TestCase(CardRarity.Legendaria, 1500)]
        public void CadaRarezaTieneElPrecioDelDocumento(CardRarity rareza, int esperado)
        {
            Assert.That(PreciosTienda.DeCartaSuelta(rareza), Is.EqualTo(esperado));
        }

        [Test]
        public void ElSobreCuestaCienDiamantes()
        {
            Assert.That(PreciosTienda.Sobre, Is.EqualTo(100));
        }
    }
}
