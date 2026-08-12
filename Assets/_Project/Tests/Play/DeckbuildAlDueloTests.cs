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
    /// El mazo 10+10 elegido en Deckbuild es de verdad el que juega el
    /// humano en Duelo, de punta a punta por el camino real de las escenas.
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
            // el fichero temporal) posee coleccion y copias suficientes para
            // formar los dos mazos completos (SesionDeJuego reparte copias
            // extra por rondas si el catalogo tiene menos cartas distintas
            // que un mazo), sin importar el tamano exacto del catalogo.
            _rutaTemporal = Path.Combine(
                Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json");
            controlador.Sesion.UsarRutaDeGuardadoParaTests(_rutaTemporal);

            List<string> elegidas = ElegirHastaCompletar(
                Object.FindObjectsByType<SelectorDeCarta>(FindObjectsSortMode.None),
                selector => selector.CardId,
                selector => selector.AlPulsarAnadir(),
                selector => controlador.Sesion.CopiasEnColeccion(selector.CardId),
                () => controlador.Total,
                ConstructorDeMazos.CartasPorMazo);

            List<string> elegidasObjetos = ElegirHastaCompletar(
                Object.FindObjectsByType<SelectorDeObjeto>(FindObjectsSortMode.None),
                selector => selector.CardId,
                selector => selector.AlPulsarAnadir(),
                selector => controlador.Sesion.CopiasEnColeccion(selector.CardId),
                () => controlador.TotalObjetos,
                ConstructorDeMazos.CartasPorMazoDeObjetos);

            Assert.That(controlador.PuedeGuardar, Is.True,
                $"monstruos {controlador.Total}/{ConstructorDeMazos.CartasPorMazo}, " +
                $"objetos {controlador.TotalObjetos}/{ConstructorDeMazos.CartasPorMazoDeObjetos}");

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
            Assert.That(TodosLosObjetosDelHumano(partida), Is.EquivalentTo(elegidasObjetos));
        }

        /// <summary>
        /// Pulsa "anadir" en cada selector, tantas veces como copias se
        /// posean, hasta completar el mazo. Generico para no repetir el
        /// mismo bucle con <c>SelectorDeCarta</c> y <c>SelectorDeObjeto</c>.
        /// </summary>
        private static List<string> ElegirHastaCompletar<T>(
            T[] selectores,
            System.Func<T, string> cardId,
            System.Action<T> anadir,
            System.Func<T, int> copiasPoseidas,
            System.Func<int> totalElegido,
            int cartasPorMazo)
        {
            Assert.That(selectores.Length, Is.GreaterThan(0),
                $"Deckbuild no tiene ningun {typeof(T).Name}");

            List<string> elegidas = new();
            foreach (T selector in selectores)
            {
                int poseidas = copiasPoseidas(selector);
                for (int i = 0; i < poseidas && totalElegido() < cartasPorMazo; i++)
                {
                    anadir(selector);
                    elegidas.Add(cardId(selector));
                }
            }

            return elegidas;
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

        private static List<string> TodosLosObjetosDelHumano(MatchController controlador)
        {
            List<string> objetos = new();
            foreach (var objeto in controlador.Humano.MazoDeObjetos.Objetos)
            {
                objetos.Add(objeto.CardId);
            }

            for (int slot = 0; slot < ItemHand.Capacity; slot++)
            {
                var enMano = controlador.Humano.ManoDeObjetos[slot];
                if (enMano != null)
                {
                    objetos.Add(enMano.CardId);
                }
            }

            return objetos;
        }

        private static IEnumerator CargarEscena(string nombre)
        {
            yield return SceneManager.LoadSceneAsync(nombre, LoadSceneMode.Single);
            yield return null;
        }
    }
}
