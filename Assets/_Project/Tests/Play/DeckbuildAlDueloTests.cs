using System.Collections;
using System.Collections.Generic;
using System.IO;
using ManaMaster.Core.Match;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Deckbuild;
using ManaMaster.Unity.Duelo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ManaMaster.PlayTests
{
    /// <summary>
    /// El mazo elegido en Deckbuild es de verdad el que juega el humano en
    /// Duelo, de punta a punta por el camino real de las escenas.
    /// </summary>
    /// <remarks>
    /// Redirige el guardado de la sesion real a un fichero temporal antes de
    /// tocar nada: desde la Fase 4, guardar un mazo escribe a disco de
    /// verdad, y este test no debe tocar el guardado del desarrollador (ver
    /// <c>SesionDeJuego.UsarRutaDeGuardadoParaTests</c>).
    /// </remarks>
    [TestFixture]
    public sealed class DeckbuildAlDueloTests
    {
        private string _rutaTemporal;

        [TearDown]
        public void Limpiar()
        {
            if (_rutaTemporal != null && File.Exists(_rutaTemporal))
            {
                File.Delete(_rutaTemporal);
            }
        }

        [UnityTest]
        public IEnumerator ElMazoElegidoEnDeckbuildEsElQueJuegaElHumanoEnDuelo()
        {
            yield return CargarEscena("Deckbuild");

            ControladorDeckbuild controlador =
                Object.FindFirstObjectByType<ControladorDeckbuild>();
            Assert.That(controlador, Is.Not.Null, "Deckbuild no tiene ControladorDeckbuild");
            Assert.That(controlador.Sesion, Is.Not.Null, "Deckbuild no tiene sesion cableada");

            // Redirigir antes de tocar nada: la sesion de la cuenta nueva (en
            // el fichero temporal) posee 1 copia de cada monstruo del
            // catalogo real, que es justo lo que este test necesita elegir.
            _rutaTemporal = Path.Combine(
                Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json");
            controlador.Sesion.UsarRutaDeGuardadoParaTests(_rutaTemporal);

            SelectorDeCarta[] selectores = Object.FindObjectsByType<SelectorDeCarta>(
                FindObjectsSortMode.None);
            Assert.That(selectores.Length, Is.EqualTo(ConstructorDeMazos.CartasPorMazo),
                $"hacen falta exactamente {ConstructorDeMazos.CartasPorMazo} cartas " +
                "distintas en el catalogo para este test (una copia de cada una)");

            List<string> elegidas = new();
            foreach (SelectorDeCarta selector in selectores)
            {
                selector.AlPulsarAnadir();
                elegidas.Add(selector.CardId);
            }

            Assert.That(controlador.PuedeGuardar, Is.True);

            controlador.Guardar();
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Inicio"),
                "guardar el mazo deberia volver al menu");

            yield return CargarEscena("Duelo");

            MatchController partida = Object.FindFirstObjectByType<MatchController>();
            partida.Sesion?.UsarRutaDeGuardadoParaTests(_rutaTemporal);

            Assert.That(partida.HayPartida, Is.True);
            Assert.That(partida.Humano.MonstruosRestantes, Is.EqualTo(10));
            Assert.That(TodasLasCartasDelHumano(partida), Is.EquivalentTo(elegidas));
        }

        private static List<string> TodasLasCartasDelHumano(MatchController controlador)
        {
            List<string> cartas = new();
            foreach (var carta in controlador.Humano.Mazo.Cartas)
            {
                cartas.Add(carta.Definition.CardId);
            }

            for (int slot = 0; slot < Hand.Capacity; slot++)
            {
                var enMano = controlador.Humano.Mano[slot];
                if (enMano != null)
                {
                    cartas.Add(enMano.Definition.CardId);
                }
            }

            return cartas;
        }

        private static IEnumerator CargarEscena(string nombre)
        {
            yield return SceneManager.LoadSceneAsync(nombre, LoadSceneMode.Single);
            yield return null;
        }
    }
}
