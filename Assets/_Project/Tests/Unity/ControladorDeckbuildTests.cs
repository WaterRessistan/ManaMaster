using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Deckbuild;
using ManaMaster.Unity.Sesion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Unity.Tests
{
    /// <summary>La seleccion de deckbuild, sin escena.</summary>
    [TestFixture]
    public sealed class ControladorDeckbuildTests
    {
        private GameObject _objeto;
        private CardCatalog _catalogo;
        private SesionDeJuego _sesion;

        [TearDown]
        public void Limpiar()
        {
            if (_objeto != null)
            {
                Object.DestroyImmediate(_objeto);
            }

            if (_catalogo != null)
            {
                foreach (MonsterCardDefinition monstruo in _catalogo.Monsters)
                {
                    if (monstruo != null)
                    {
                        Object.DestroyImmediate(monstruo);
                    }
                }

                Object.DestroyImmediate(_catalogo);
            }

            if (_sesion != null)
            {
                Object.DestroyImmediate(_sesion);
            }
        }

        [Test]
        public void EmpiezaVacio()
        {
            ControladorDeckbuild controlador = Controlador(10);

            Assert.That(controlador.Total, Is.EqualTo(0));
            Assert.That(controlador.PuedeGuardar, Is.False);
            Assert.That(controlador.Copias("Monstruo0"), Is.EqualTo(0));
        }

        [Test]
        public void AnadirUnaCartaValidaAumentaElTotalYAvisa()
        {
            ControladorDeckbuild controlador = Controlador(10);
            int avisos = 0;
            controlador.SeleccionCambiada += () => avisos++;

            bool anadida = controlador.Anadir("Monstruo0");

            Assert.That(anadida, Is.True);
            Assert.That(controlador.Total, Is.EqualTo(1));
            Assert.That(controlador.Copias("Monstruo0"), Is.EqualTo(1));
            Assert.That(avisos, Is.EqualTo(1));
        }

        [Test]
        public void NoAnadeMasDeDosCopiasDeLaMismaCarta()
        {
            ControladorDeckbuild controlador = Controlador(10);
            controlador.Anadir("Monstruo0");
            controlador.Anadir("Monstruo0");

            bool tercera = controlador.Anadir("Monstruo0");

            Assert.That(tercera, Is.False);
            Assert.That(controlador.Copias("Monstruo0"), Is.EqualTo(2));
        }

        [Test]
        public void NoAnadeMasDeDiezCartasEnTotal()
        {
            ControladorDeckbuild controlador = Controlador(10);
            for (int i = 0; i < 5; i++)
            {
                controlador.Anadir($"Monstruo{i}");
                controlador.Anadir($"Monstruo{i}");
            }

            Assert.That(controlador.Total, Is.EqualTo(10));

            bool undecima = controlador.Anadir("Monstruo5");

            Assert.That(undecima, Is.False);
            Assert.That(controlador.Total, Is.EqualTo(10));
        }

        [Test]
        public void AnadirUnaCartaQueNoExisteEnElCatalogoDevuelveFalse()
        {
            ControladorDeckbuild controlador = Controlador(10);

            bool anadida = controlador.Anadir("NoExiste");

            Assert.That(anadida, Is.False);
            Assert.That(controlador.Total, Is.EqualTo(0));
        }

        [Test]
        public void QuitarUnaCartaElegidaLaQuitaYAvisa()
        {
            ControladorDeckbuild controlador = Controlador(10);
            controlador.Anadir("Monstruo0");
            int avisos = 0;
            controlador.SeleccionCambiada += () => avisos++;

            bool quitada = controlador.Quitar("Monstruo0");

            Assert.That(quitada, Is.True);
            Assert.That(controlador.Copias("Monstruo0"), Is.EqualTo(0));
            Assert.That(avisos, Is.EqualTo(1));
        }

        [Test]
        public void QuitarUnaCartaNoElegidaDevuelveFalseYNoAvisa()
        {
            ControladorDeckbuild controlador = Controlador(10);
            int avisos = 0;
            controlador.SeleccionCambiada += () => avisos++;

            bool quitada = controlador.Quitar("Monstruo0");

            Assert.That(quitada, Is.False);
            Assert.That(avisos, Is.EqualTo(0));
        }

        [Test]
        public void PuedeGuardarSoloConLasDiezCartas()
        {
            ControladorDeckbuild controlador = Controlador(10);
            for (int i = 0; i < 4; i++)
            {
                controlador.Anadir($"Monstruo{i}");
                controlador.Anadir($"Monstruo{i}");
            }

            Assert.That(controlador.PuedeGuardar, Is.False);

            controlador.Anadir("Monstruo4");
            controlador.Anadir("Monstruo5");

            Assert.That(controlador.Total, Is.EqualTo(10));
            Assert.That(controlador.PuedeGuardar, Is.True);
        }

        [Test]
        public void GuardarSinElMazoCompletoNoTocaLaSesion()
        {
            ControladorDeckbuild controlador = Controlador(10);
            controlador.Anadir("Monstruo0");

            controlador.Guardar();

            Assert.That(_sesion.TieneMazoElegido, Is.False);
        }

        private ControladorDeckbuild Controlador(int monstruosEnCatalogo)
        {
            _catalogo = CatalogoDePrueba(monstruosEnCatalogo);
            _sesion = ScriptableObject.CreateInstance<SesionDeJuego>();

            _objeto = new GameObject("ControladorDeckbuild");
            ControladorDeckbuild controlador =
                _objeto.AddComponent<ControladorDeckbuild>();

            SerializedObject serializado = new(controlador);
            serializado.FindProperty("catalogo").objectReferenceValue = _catalogo;
            serializado.FindProperty("sesion").objectReferenceValue = _sesion;
            serializado.ApplyModifiedPropertiesWithoutUndo();

            return controlador;
        }

        /// <summary>
        /// Catalogo de mentira con N monstruos distintos, nombrados
        /// "Monstruo0".."MonstruoN-1" (el CardId es el nombre del asset).
        /// </summary>
        private static CardCatalog CatalogoDePrueba(int monstruos)
        {
            CardCatalog catalogo = ScriptableObject.CreateInstance<CardCatalog>();
            catalogo.name = "CatalogoDePrueba";

            SerializedObject serializado = new(catalogo);
            SerializedProperty lista = serializado.FindProperty("monsters");
            lista.arraySize = monstruos;

            for (int i = 0; i < monstruos; i++)
            {
                MonsterCardDefinition definicion =
                    ScriptableObject.CreateInstance<MonsterCardDefinition>();
                definicion.name = $"Monstruo{i}";

                lista.GetArrayElementAtIndex(i).objectReferenceValue = definicion;
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();

            return catalogo;
        }
    }
}
