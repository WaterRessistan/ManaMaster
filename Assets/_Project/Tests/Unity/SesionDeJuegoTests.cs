using ManaMaster.Unity.Sesion;
using NUnit.Framework;
using UnityEngine;

namespace ManaMaster.Unity.Tests
{
    /// <summary>
    /// El asset que lleva el mazo elegido de Deckbuild a Duelo.
    /// </summary>
    [TestFixture]
    public sealed class SesionDeJuegoTests
    {
        private SesionDeJuego _sesion;

        [SetUp]
        public void Preparar()
        {
            _sesion = ScriptableObject.CreateInstance<SesionDeJuego>();
        }

        [TearDown]
        public void Limpiar()
        {
            Object.DestroyImmediate(_sesion);
        }

        [Test]
        public void EmpiezaSinMazoElegido()
        {
            Assert.That(_sesion.TieneMazoElegido, Is.False);
            Assert.That(_sesion.MazoHumano, Is.Empty);
        }

        [Test]
        public void FijarMazoHumanoGuardaLosCardIdsEnOrden()
        {
            _sesion.FijarMazoHumano(new[] { "Golem", "Golem", "Zombie" });

            Assert.That(_sesion.TieneMazoElegido, Is.True);
            Assert.That(_sesion.MazoHumano,
                Is.EqualTo(new[] { "Golem", "Golem", "Zombie" }));
        }

        [Test]
        public void FijarMazoHumanoDosVecesSustituyeElAnterior()
        {
            _sesion.FijarMazoHumano(new[] { "Golem" });
            _sesion.FijarMazoHumano(new[] { "Zombie", "Azul" });

            Assert.That(_sesion.MazoHumano, Is.EqualTo(new[] { "Zombie", "Azul" }));
        }

        [Test]
        public void LimpiarMazoHumanoVuelveAlRepartoAleatorio()
        {
            _sesion.FijarMazoHumano(new[] { "Golem" });

            _sesion.LimpiarMazoHumano();

            Assert.That(_sesion.TieneMazoElegido, Is.False);
        }
    }
}
