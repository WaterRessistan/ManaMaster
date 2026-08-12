using System.Collections;
using System.Collections.Generic;
using ManaMaster.Core.Match;
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
    [TestFixture]
    public sealed class DeckbuildAlDueloTests
    {
        [UnityTest]
        public IEnumerator ElMazoElegidoEnDeckbuildEsElQueJuegaElHumanoEnDuelo()
        {
            yield return CargarEscena("Deckbuild");

            ControladorDeckbuild controlador =
                Object.FindFirstObjectByType<ControladorDeckbuild>();
            Assert.That(controlador, Is.Not.Null, "Deckbuild no tiene ControladorDeckbuild");

            SelectorDeCarta[] selectores = Object.FindObjectsByType<SelectorDeCarta>(
                FindObjectsSortMode.None);
            Assert.That(selectores.Length, Is.GreaterThanOrEqualTo(5),
                "hacen falta al menos 5 cartas distintas para un mazo de 10 con maximo 2 copias");

            // Dos copias de las cinco primeras cartas: diez en total, dentro
            // del maximo de copias por carta (DESIGN.md §8).
            List<string> elegidas = new();
            for (int i = 0; i < 5; i++)
            {
                selectores[i].AlPulsarAnadir();
                selectores[i].AlPulsarAnadir();
                elegidas.Add(selectores[i].CardId);
                elegidas.Add(selectores[i].CardId);
            }

            Assert.That(controlador.PuedeGuardar, Is.True);

            controlador.Guardar();
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Inicio"),
                "guardar el mazo deberia volver al menu");

            yield return CargarEscena("Duelo");

            MatchController partida = Object.FindFirstObjectByType<MatchController>();
            Assert.That(partida.HayPartida, Is.True);
            Assert.That(partida.Humano.MonstruosRestantes, Is.EqualTo(10));
            Assert.That(TodasLasCartasDelHumano(partida), Is.EquivalentTo(elegidas));

            // No dejar un mazo elegido en el asset compartido: otros tests
            // (EscenaDeDueloTests) esperan el camino "sin sesion" por defecto.
            partida.Sesion?.LimpiarMazoHumano();
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
