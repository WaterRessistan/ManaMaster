using ManaMaster.Core.Agents;
using ManaMaster.Core.Match;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
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

        [TearDown]
        public void Limpiar()
        {
            if (_objeto != null)
            {
                Object.DestroyImmediate(_objeto);
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

        private static string ManoDe(MatchController controlador)
            => $"{controlador.Humano.Mano[0]?.Definition.CardId}|" +
               $"{controlador.Humano.Mano[1]?.Definition.CardId}";

        private MatchController Controlador(int semilla)
        {
            _objeto = new GameObject("MatchController");
            MatchController controlador = _objeto.AddComponent<MatchController>();

            SerializedObject serializado = new(controlador);
            serializado.FindProperty("catalogo").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<CardCatalog>(RutaCatalogo);
            serializado.FindProperty("semilla").intValue = semilla;
            serializado.ApplyModifiedPropertiesWithoutUndo();

            return controlador;
        }
    }
}
