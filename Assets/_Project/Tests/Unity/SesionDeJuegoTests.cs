using System.IO;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Sesion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Unity.Tests
{
    /// <summary>
    /// El asset que lleva los datos del jugador entre escenas y los persiste
    /// en disco (Fase 4).
    /// </summary>
    /// <remarks>
    /// Cada test redirige el guardado a un fichero temporal propio con
    /// <see cref="SesionDeJuego.UsarRutaDeGuardadoParaTests"/>: la partida
    /// real usa <c>Application.persistentDataPath</c>, y estos tests no deben
    /// tocar el guardado del desarrollador.
    /// </remarks>
    [TestFixture]
    public sealed class SesionDeJuegoTests
    {
        private SesionDeJuego _sesion;
        private string _rutaTemporal;

        [SetUp]
        public void Preparar()
        {
            _sesion = ScriptableObject.CreateInstance<SesionDeJuego>();
            _rutaTemporal = Path.Combine(Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json");
            _sesion.UsarRutaDeGuardadoParaTests(_rutaTemporal);
        }

        [TearDown]
        public void Limpiar()
        {
            Object.DestroyImmediate(_sesion);

            if (File.Exists(_rutaTemporal))
            {
                File.Delete(_rutaTemporal);
            }
        }

        [Test]
        public void SinCatalogoLaCuentaNuevaTieneQuinientosDiamantesYNadaMas()
        {
            Assert.That(_sesion.Diamantes, Is.EqualTo(500));
            Assert.That(_sesion.TieneMazoElegido, Is.False);
            Assert.That(_sesion.MazoHumano, Is.Empty);
            Assert.That(_sesion.CopiasEnColeccion("Golem"), Is.EqualTo(0));
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

        [Test]
        public void GanarDiamantesAcumulaSobreElSaldo()
        {
            _sesion.GanarDiamantes(50);
            _sesion.GanarDiamantes(15);

            Assert.That(_sesion.Diamantes, Is.EqualTo(565));
        }

        [Test]
        public void GanarDiamantesConCantidadNoPositivaNoHaceNada()
        {
            _sesion.GanarDiamantes(0);
            _sesion.GanarDiamantes(-10);

            Assert.That(_sesion.Diamantes, Is.EqualTo(500));
        }

        [Test]
        public void TryGastarDiamantesDescuentaSiHaySaldo()
        {
            bool gastado = _sesion.TryGastarDiamantes(100);

            Assert.That(gastado, Is.True);
            Assert.That(_sesion.Diamantes, Is.EqualTo(400));
        }

        [Test]
        public void TryGastarDiamantesFallaSinSaldoYNoTocaNada()
        {
            bool gastado = _sesion.TryGastarDiamantes(501);

            Assert.That(gastado, Is.False);
            Assert.That(_sesion.Diamantes, Is.EqualTo(500));
        }

        [Test]
        public void AnadirAColeccionAcumulaCopias()
        {
            _sesion.AnadirAColeccion("Golem");
            _sesion.AnadirAColeccion("Golem");
            _sesion.AnadirAColeccion("Zombie", 3);

            Assert.That(_sesion.CopiasEnColeccion("Golem"), Is.EqualTo(2));
            Assert.That(_sesion.CopiasEnColeccion("Zombie"), Is.EqualTo(3));
            Assert.That(_sesion.CopiasEnColeccion("Azul"), Is.EqualTo(0));
        }

        [Test]
        public void LosCambiosSobrevivenAUnaInstanciaNueva()
        {
            _sesion.GanarDiamantes(200);
            _sesion.AnadirAColeccion("Golem", 2);
            _sesion.FijarMazoHumano(new[] { "Golem", "Golem" });

            SesionDeJuego otra = ScriptableObject.CreateInstance<SesionDeJuego>();
            otra.UsarRutaDeGuardadoParaTests(_rutaTemporal);

            try
            {
                Assert.That(otra.Diamantes, Is.EqualTo(700));
                Assert.That(otra.CopiasEnColeccion("Golem"), Is.EqualTo(2));
                Assert.That(otra.MazoHumano, Is.EqualTo(new[] { "Golem", "Golem" }));
            }
            finally
            {
                Object.DestroyImmediate(otra);
            }
        }

        [Test]
        public void CambiadaSeDisparaAlMutar()
        {
            int avisos = 0;
            _sesion.Cambiada += () => avisos++;

            _sesion.GanarDiamantes(10);
            _sesion.AnadirAColeccion("Golem");
            _sesion.FijarMazoHumano(new[] { "Golem" });

            Assert.That(avisos, Is.EqualTo(3));
        }

        [Test]
        public void ConDiezMonstruosDistintosLaCuentaNuevaDaUnaCopiaDeCadaUnoYUnMazoListo()
        {
            string[] nombres = NombresDePrueba(ConstructorDeMazos.CartasPorMazo);
            CardCatalog catalogo = CatalogoDePrueba(nombres);

            EjecutarConCuentaNueva(catalogo, conCatalogo =>
            {
                Assert.That(conCatalogo.Diamantes, Is.EqualTo(500));
                foreach (string nombre in nombres)
                {
                    Assert.That(conCatalogo.CopiasEnColeccion(nombre), Is.EqualTo(1));
                }

                Assert.That(conCatalogo.TieneMazoElegido, Is.True);
                Assert.That(conCatalogo.MazoHumano, Is.EquivalentTo(nombres));
            });
        }

        /// <summary>
        /// Con menos monstruos distintos que un mazo, la cuenta nueva reparte
        /// copias extra por rondas (hasta el maximo de 2) hasta completar un
        /// mazo jugable de <see cref="ConstructorDeMazos.CartasPorMazo"/>
        /// cartas — el caso real del catalogo del proyecto, que hoy tiene
        /// menos de 10 monstruos distintos.
        /// </summary>
        [Test]
        public void ConMenosDeDiezMonstruosDistintosElMazoInicialSeCompletaConCopiasExtra()
        {
            string[] nombres = NombresDePrueba(6);
            CardCatalog catalogo = CatalogoDePrueba(nombres);

            EjecutarConCuentaNueva(catalogo, conCatalogo =>
            {
                Assert.That(conCatalogo.TieneMazoElegido, Is.True);
                Assert.That(conCatalogo.MazoHumano.Count, Is.EqualTo(ConstructorDeMazos.CartasPorMazo));

                int totalEnColeccion = 0;
                foreach (string nombre in nombres)
                {
                    int copias = conCatalogo.CopiasEnColeccion(nombre);
                    Assert.That(copias, Is.LessThanOrEqualTo(ConstructorDeMazos.MaxCopiasPorCarta),
                        $"'{nombre}' supera el maximo de copias");
                    totalEnColeccion += copias;
                }

                Assert.That(totalEnColeccion, Is.EqualTo(ConstructorDeMazos.CartasPorMazo));
            });
        }

        private static string[] NombresDePrueba(int cuantos)
        {
            string[] nombres = new string[cuantos];
            for (int i = 0; i < cuantos; i++)
            {
                nombres[i] = $"Monstruo{i}";
            }

            return nombres;
        }

        private static void EjecutarConCuentaNueva(
            CardCatalog catalogo, System.Action<SesionDeJuego> asercion)
        {
            SesionDeJuego conCatalogo = ScriptableObject.CreateInstance<SesionDeJuego>();
            conCatalogo.UsarRutaDeGuardadoParaTests(
                Path.Combine(Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json"));

            SerializedObject serializado = new(conCatalogo);
            serializado.FindProperty("catalogo").objectReferenceValue = catalogo;
            serializado.ApplyModifiedPropertiesWithoutUndo();

            try
            {
                asercion(conCatalogo);
            }
            finally
            {
                Object.DestroyImmediate(conCatalogo);
                foreach (MonsterCardDefinition monstruo in catalogo.Monsters)
                {
                    Object.DestroyImmediate(monstruo);
                }

                Object.DestroyImmediate(catalogo);
            }
        }

        private static CardCatalog CatalogoDePrueba(params string[] nombres)
        {
            CardCatalog catalogo = ScriptableObject.CreateInstance<CardCatalog>();
            catalogo.name = "CatalogoDePrueba";

            SerializedObject serializado = new(catalogo);
            SerializedProperty lista = serializado.FindProperty("monsters");
            lista.arraySize = nombres.Length;

            for (int i = 0; i < nombres.Length; i++)
            {
                MonsterCardDefinition definicion =
                    ScriptableObject.CreateInstance<MonsterCardDefinition>();
                definicion.name = nombres[i];

                lista.GetArrayElementAtIndex(i).objectReferenceValue = definicion;
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();

            return catalogo;
        }
    }
}
