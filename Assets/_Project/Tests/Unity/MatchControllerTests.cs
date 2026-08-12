using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManaMaster.Core.Agents;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using ManaMaster.Unity.Sesion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ManaMaster.Unity.Tests
{
    /// <summary>
    /// El motor enchufado a Unity, usando el catalogo de cartas de verdad.
    /// </summary>
    /// <remarks>
    /// Los tests del dominio prueban las reglas con cartas de mentira. Estos
    /// prueban lo otro: que los assets reales se convierten en una partida
    /// jugable y que el camino de la escena no se atasca.
    /// </remarks>
    [TestFixture]
    public sealed class MatchControllerTests
    {
        private const string RutaCatalogo =
            "Assets/_Project/Content/Cards/CardCatalog.asset";

        private GameObject _objeto;
        private SesionDeJuego _sesion;
        private string _rutaTemporal;

        [TearDown]
        public void Limpiar()
        {
            if (_objeto != null)
            {
                Object.DestroyImmediate(_objeto);
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
        public void ElCatalogoDelProyectoExisteYTieneMonstruos()
        {
            CardCatalog catalogo =
                AssetDatabase.LoadAssetAtPath<CardCatalog>(RutaCatalogo);

            Assert.That(catalogo, Is.Not.Null, $"no existe {RutaCatalogo}");
            Assert.That(catalogo.Monsters.Count, Is.GreaterThan(0));
        }

        /// <summary>
        /// El humano tambien recibe un mazo de objetos de verdad (DESIGN.md
        /// §4); el Rival no, porque AgenteHeuristico no sabe equiparlos.
        /// </summary>
        [Test]
        public void ComenzarMontaTambienElMazoDeObjetosDelHumano()
        {
            MatchController controlador = Controlador(semilla: 1);

            controlador.Comenzar();

            Assert.That(controlador.Humano.ManoDeObjetos.Count, Is.EqualTo(2));
            Assert.That(controlador.Rival.ManoDeObjetos.IsEmpty, Is.True);
        }

        [Test]
        public void MontaUnaPartidaConLosDosMazosRepartidos()
        {
            MatchController controlador = Controlador(semilla: 1);

            controlador.Comenzar();

            Assert.That(controlador.HayPartida, Is.True);
            Assert.That(controlador.Humano.MonstruosRestantes,
                Is.EqualTo(ConstructorDeMazos.CartasPorMazo));
            Assert.That(controlador.Rival.MonstruosRestantes,
                Is.EqualTo(ConstructorDeMazos.CartasPorMazo));
            Assert.That(controlador.Humano.Mano.Count, Is.EqualTo(2));
        }

        [Test]
        public void ConLaMismaSemillaSeMontaLaMismaPartida()
        {
            MatchController primero = Controlador(semilla: 99);
            primero.Comenzar();
            string manoPrimera = ManoDe(primero);

            Object.DestroyImmediate(_objeto);

            MatchController segundo = Controlador(semilla: 99);
            segundo.Comenzar();

            Assert.That(ManoDe(segundo), Is.EqualTo(manoPrimera));
        }

        /// <summary>
        /// Una partida entera por el camino de la escena: el rival lo mueve el
        /// controlador y el humano se simula con el mismo agente.
        /// </summary>
        [Test]
        public void UnaPartidaCompletaLlegaASuFinPorElCaminoDeLaEscena()
        {
            MatchController controlador = Controlador(semilla: 5);
            controlador.Comenzar();

            AgenteHeuristico manosDelHumano = new();

            for (int paso = 0; paso < 5000 && !controlador.Partida.Terminada; paso++)
            {
                if (!controlador.EsTurnoDelHumano)
                {
                    controlador.JugarTurnoDelRival();
                    continue;
                }

                AccionTurno accion = manosDelHumano.DecidirAccion(controlador.Partida);

                if (accion.Tipo != TipoAccion.Desplegar
                    || controlador.Desplegar(accion.HuecoMano, accion.Carril)
                       != ResultadoDespliegue.Ok)
                {
                    controlador.TerminarTurno();
                }
            }

            Assert.That(controlador.Partida.Terminada, Is.True,
                "la partida no llego a terminar");
        }

        /// <summary>
        /// Si la sesion trae un mazo elegido en deckbuild, el humano juega con
        /// exactamente esas cartas y no con un reparto aleatorio.
        /// </summary>
        [Test]
        public void ConMazoElegidoEnSesionElHumanoJuegaConEseMazo()
        {
            CardCatalog catalogo = AssetDatabase.LoadAssetAtPath<CardCatalog>(RutaCatalogo);
            string[] seleccion = SeleccionDeDiezCartas(catalogo);

            _sesion = NuevaSesionDeTest();
            _sesion.FijarMazoHumano(seleccion);

            MatchController controlador = Controlador(semilla: 3, _sesion);
            controlador.Comenzar();

            Assert.That(controlador.Humano.MonstruosRestantes, Is.EqualTo(seleccion.Length));
            Assert.That(TodasLasCartasDelHumano(controlador), Is.EquivalentTo(seleccion));
        }

        /// <summary>
        /// Una sesion cableada pero sin mazo elegido no cambia nada: sigue el
        /// reparto aleatorio de siempre.
        /// </summary>
        [Test]
        public void ConSesionSinMazoElegidoSigueElRepartoAleatorio()
        {
            _sesion = NuevaSesionDeTest();

            MatchController controlador = Controlador(semilla: 4, _sesion);
            controlador.Comenzar();

            Assert.That(controlador.HayPartida, Is.True);
            Assert.That(controlador.Humano.MonstruosRestantes,
                Is.EqualTo(ConstructorDeMazos.CartasPorMazo));
        }

        [Test]
        public void SinCatalogoNoSeMontaLaPartidaPeroNoRevienta()
        {
            _objeto = new GameObject("MatchController");
            MatchController controlador = _objeto.AddComponent<MatchController>();

            LogAssert.ignoreFailingMessages = true;
            controlador.Comenzar();
            LogAssert.ignoreFailingMessages = false;

            Assert.That(controlador.HayPartida, Is.False);
        }

        /// <summary>
        /// Selecciona 10 cartas del catalogo real respetando el maximo de 2
        /// copias, sin importar si tiene 10 monstruos distintos o menos
        /// (reparte copias extra por rondas, igual que la cuenta nueva de
        /// <c>SesionDeJuego</c>).
        /// </summary>
        private static string[] SeleccionDeDiezCartas(CardCatalog catalogo)
        {
            List<string> distintos = catalogo.Monsters
                .Where(monstruo => monstruo != null)
                .Select(monstruo => monstruo.CardId)
                .ToList();

            List<string> seleccion = new();
            for (int copia = 0;
                 seleccion.Count < ConstructorDeMazos.CartasPorMazo
                 && copia < ConstructorDeMazos.MaxCopiasPorCarta;
                 copia++)
            {
                foreach (string cardId in distintos)
                {
                    if (seleccion.Count >= ConstructorDeMazos.CartasPorMazo)
                    {
                        break;
                    }

                    seleccion.Add(cardId);
                }
            }

            Assert.That(seleccion.Count, Is.EqualTo(ConstructorDeMazos.CartasPorMazo),
                "el catalogo del proyecto no da para un mazo de 10 con maximo 2 copias por carta");

            return seleccion.ToArray();
        }

        private static string ManoDe(MatchController controlador)
            => $"{controlador.Humano.Mano[0]?.Definition.CardId}|" +
               $"{controlador.Humano.Mano[1]?.Definition.CardId}";

        /// <summary>CardIds del humano en mazo y mano, para comparar con una seleccion.</summary>
        private static string[] TodasLasCartasDelHumano(MatchController controlador)
        {
            System.Collections.Generic.List<string> cartas = controlador.Humano.Mazo.Cartas
                .Select(carta => carta.Definition.CardId)
                .ToList();

            for (int slot = 0; slot < Hand.Capacity; slot++)
            {
                CardInstance enMano = controlador.Humano.Mano[slot];
                if (enMano != null)
                {
                    cartas.Add(enMano.Definition.CardId);
                }
            }

            return cartas.ToArray();
        }

        /// <summary>
        /// Sesion de prueba con el guardado redirigido a un fichero temporal,
        /// para no tocar el guardado real del desarrollador.
        /// </summary>
        private SesionDeJuego NuevaSesionDeTest()
        {
            SesionDeJuego sesion = ScriptableObject.CreateInstance<SesionDeJuego>();
            _rutaTemporal = Path.Combine(Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json");
            sesion.UsarRutaDeGuardadoParaTests(_rutaTemporal);
            return sesion;
        }

        private MatchController Controlador(int semilla, SesionDeJuego sesion = null)
        {
            _objeto = new GameObject("MatchController");
            MatchController controlador = _objeto.AddComponent<MatchController>();

            SerializedObject serializado = new(controlador);
            serializado.FindProperty("catalogo").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<CardCatalog>(RutaCatalogo);
            serializado.FindProperty("semilla").intValue = semilla;
            serializado.FindProperty("sesion").objectReferenceValue = sesion;
            serializado.ApplyModifiedPropertiesWithoutUndo();

            return controlador;
        }
    }
}
