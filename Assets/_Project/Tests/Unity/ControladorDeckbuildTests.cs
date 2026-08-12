using System.IO;
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
        private string _rutaTemporal;

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

                foreach (ItemCardDefinition objeto in _catalogo.Items)
                {
                    if (objeto != null)
                    {
                        Object.DestroyImmediate(objeto);
                    }
                }

                Object.DestroyImmediate(_catalogo);
            }

            if (_sesion != null)
            {
                Object.DestroyImmediate(_sesion);
            }

            if (_rutaTemporal != null && File.Exists(_rutaTemporal))
            {
                File.Delete(_rutaTemporal);
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
        public void PuedeGuardarExigeMonstruosYObjetosCompletos()
        {
            ControladorDeckbuild controlador = Controlador(10);
            for (int i = 0; i < 5; i++)
            {
                controlador.Anadir($"Monstruo{i}");
                controlador.Anadir($"Monstruo{i}");
            }

            Assert.That(controlador.Total, Is.EqualTo(10));
            Assert.That(controlador.PuedeGuardar, Is.False,
                "los 10 monstruos no bastan sin los 10 objetos");

            for (int i = 0; i < 5; i++)
            {
                controlador.AnadirObjeto($"Objeto{i}");
                controlador.AnadirObjeto($"Objeto{i}");
            }

            Assert.That(controlador.TotalObjetos, Is.EqualTo(10));
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

        // Nota: no hay un test EditMode que llame a Guardar() con las dos
        // mitades completas — eso dispara SceneManager.LoadScene de verdad,
        // que solo es valido en PlayMode. Ese camino completo (incluida la
        // sesion con ambos mazos fijados) lo cubre DeckbuildAlDueloTests.

        [Test]
        public void AnadirUnObjetoValidoAumentaElTotalObjetosYAvisa()
        {
            ControladorDeckbuild controlador = Controlador(10);
            int avisos = 0;
            controlador.SeleccionCambiada += () => avisos++;

            bool anadido = controlador.AnadirObjeto("Objeto0");

            Assert.That(anadido, Is.True);
            Assert.That(controlador.TotalObjetos, Is.EqualTo(1));
            Assert.That(controlador.CopiasObjeto("Objeto0"), Is.EqualTo(1));
            Assert.That(avisos, Is.EqualTo(1));
        }

        [Test]
        public void QuitarUnObjetoElegidoLoQuitaYAvisa()
        {
            ControladorDeckbuild controlador = Controlador(10);
            controlador.AnadirObjeto("Objeto0");
            int avisos = 0;
            controlador.SeleccionCambiada += () => avisos++;

            bool quitado = controlador.QuitarObjeto("Objeto0");

            Assert.That(quitado, Is.True);
            Assert.That(controlador.CopiasObjeto("Objeto0"), Is.EqualTo(0));
            Assert.That(avisos, Is.EqualTo(1));
        }

        [Test]
        public void NoSePuedeAnadirUnObjetoQueNoSePosee()
        {
            ControladorDeckbuild controlador = Controlador(10, copiasPoseidasPorCarta: 0);

            bool anadido = controlador.AnadirObjeto("Objeto0");

            Assert.That(anadido, Is.False);
            Assert.That(controlador.TotalObjetos, Is.EqualTo(0));
        }

        [Test]
        public void NoSePuedeAnadirUnaCartaQueNoSePosee()
        {
            ControladorDeckbuild controlador = Controlador(10, copiasPoseidasPorCarta: 0);

            bool anadida = controlador.Anadir("Monstruo0");

            Assert.That(anadida, Is.False);
            Assert.That(controlador.Total, Is.EqualTo(0));
        }

        [Test]
        public void NoSePuedeAnadirMasCopiasDeLasQueSePoseenAunqueElReglamentoPermitaDos()
        {
            ControladorDeckbuild controlador = Controlador(10, copiasPoseidasPorCarta: 1);

            bool primera = controlador.Anadir("Monstruo0");
            bool segunda = controlador.Anadir("Monstruo0");

            Assert.That(primera, Is.True);
            Assert.That(segunda, Is.False, "solo se posee 1 copia");
            Assert.That(controlador.Copias("Monstruo0"), Is.EqualTo(1));
        }

        private ControladorDeckbuild Controlador(
            int monstruosEnCatalogo, int copiasPoseidasPorCarta = 2)
        {
            _catalogo = CatalogoDePrueba(monstruosEnCatalogo);
            _sesion = ScriptableObject.CreateInstance<SesionDeJuego>();
            _rutaTemporal = Path.Combine(Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json");
            _sesion.UsarRutaDeGuardadoParaTests(_rutaTemporal);

            for (int i = 0; i < monstruosEnCatalogo; i++)
            {
                _sesion.AnadirAColeccion($"Monstruo{i}", copiasPoseidasPorCarta);
                _sesion.AnadirAColeccion($"Objeto{i}", copiasPoseidasPorCarta);
            }

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
        /// Catalogo de mentira con N monstruos ("Monstruo0"..) y N objetos
        /// ("Objeto0"..) distintos (el CardId es el nombre del asset).
        /// </summary>
        private static CardCatalog CatalogoDePrueba(int monstruos)
        {
            CardCatalog catalogo = ScriptableObject.CreateInstance<CardCatalog>();
            catalogo.name = "CatalogoDePrueba";

            SerializedObject serializado = new(catalogo);
            SerializedProperty listaMonstruos = serializado.FindProperty("monsters");
            listaMonstruos.arraySize = monstruos;

            for (int i = 0; i < monstruos; i++)
            {
                MonsterCardDefinition definicion =
                    ScriptableObject.CreateInstance<MonsterCardDefinition>();
                definicion.name = $"Monstruo{i}";

                listaMonstruos.GetArrayElementAtIndex(i).objectReferenceValue = definicion;
            }

            SerializedProperty listaObjetos = serializado.FindProperty("items");
            listaObjetos.arraySize = monstruos;

            for (int i = 0; i < monstruos; i++)
            {
                ItemCardDefinition definicion =
                    ScriptableObject.CreateInstance<ItemCardDefinition>();
                definicion.name = $"Objeto{i}";

                listaObjetos.GetArrayElementAtIndex(i).objectReferenceValue = definicion;
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();

            return catalogo;
        }
    }
}
