using System.Collections;
using System.IO;
using ManaMaster.Core.Match;
using ManaMaster.Unity.Duelo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ManaMaster.PlayTests
{
    /// <summary>
    /// Equipar un objeto de verdad en la escena de Duelo: el camino real que
    /// dispara <see cref="CarrilDeInsercion"/> al soltar una
    /// <see cref="ManaMaster.Unity.Duelo.CartaDeObjeto"/>, aqui invocado
    /// directamente sobre <see cref="MatchController"/> (mismo nivel al que
    /// ya se prueba <c>Desplegar</c> en otros tests de esta clase).
    /// </summary>
    [TestFixture]
    public sealed class EquiparObjetosTests
    {
        private const float Margen = 30f;

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
        public IEnumerator EquiparUnObjetoSeVeEnLaCartaYNoSePuedeRepetir()
        {
            yield return CargarEscena("Duelo");

            MatchController controlador = Object.FindFirstObjectByType<MatchController>();
            Assert.That(controlador, Is.Not.Null, "Duelo no tiene MatchController");

            if (controlador.Sesion != null)
            {
                _rutaTemporal = Path.Combine(
                    Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json");
                controlador.Sesion.UsarRutaDeGuardadoParaTests(_rutaTemporal);
            }

            yield return EsperarAlJugador(controlador);

            if (controlador.Partida.Terminada)
            {
                Assert.Pass("la partida acabo antes de empezar; nada que probar");
            }

            Assert.That(controlador.Humano.ManoDeObjetos.IsEmpty, Is.False,
                "el humano deberia tener objetos en la mano al empezar");

            // Hace falta un monstruo en la arena para poder equipar algo.
            if (!DesplegarAlguno(controlador))
            {
                Assert.Pass("no habia mana para desplegar en esta semilla");
            }

            int carril = 0;
            while (controlador.Humano.Arena[carril] == null)
            {
                carril++;
            }

            // Las pociones (DESIGN.md §4) nunca ocupan el hueco de objeto ni
            // bloquean nada, asi que para probar el bloqueo hace falta un
            // objeto normal concreto, no el primer hueco de la mano.
            int hueco = HuecoDeObjetoNormal(controlador);
            if (hueco < 0)
            {
                Assert.Pass("la mano no tenia ningun objeto normal en esta semilla; nada que probar");
            }

            ResultadoEquipar resultado = controlador.EquiparObjeto(hueco, carril);

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.Ok));
            Assert.That(controlador.Humano.Arena[carril].EquippedItem, Is.Not.Null);

            yield return null;

            VistaCartaMonstruo vista = VistaDelCarril(controlador, carril);
            Assert.That(vista, Is.Not.Null, "no se encontro la vista del carril equipado");
            Assert.That(vista.MuestraIconoDeObjeto, Is.True,
                "el badge de objeto equipado deberia estar encendido");

            // DESIGN.md §4: como mucho un objeto normal, no se puede sustituir.
            int segundoHueco = HuecoDeObjetoNormal(controlador);
            if (segundoHueco < 0)
            {
                Assert.Pass("no quedaba ningun objeto normal para probar el bloqueo; nada que probar");
            }

            ResultadoEquipar segundoIntento = controlador.EquiparObjeto(segundoHueco, carril);
            Assert.That(segundoIntento, Is.EqualTo(ResultadoEquipar.YaLlevaObjeto));
        }

        /// <summary>Primer hueco de la mano con un objeto que no sea pocion, o -1 si no hay.</summary>
        private static int HuecoDeObjetoNormal(MatchController controlador)
        {
            for (int hueco = 0; hueco < ItemHand.Capacity; hueco++)
            {
                var objeto = controlador.Humano.ManoDeObjetos[hueco];
                if (objeto != null && !objeto.EsPocion)
                {
                    return hueco;
                }
            }

            return -1;
        }

        private static bool DesplegarAlguno(MatchController controlador)
        {
            for (int hueco = 0; hueco < Hand.Capacity; hueco++)
            {
                for (int carril = 0; carril < 3; carril++)
                {
                    if (controlador.Desplegar(hueco, carril) == ResultadoDespliegue.Ok)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static VistaCartaMonstruo VistaDelCarril(MatchController controlador, int carril)
        {
            var carta = controlador.Humano.Arena[carril];

            foreach (CarrilDeInsercion carrilDeInsercion in Object.FindObjectsByType<CarrilDeInsercion>(
                         FindObjectsSortMode.None))
            {
                if (carrilDeInsercion.Vista != null
                    && ReferenceEquals(carrilDeInsercion.Vista.Carta, carta))
                {
                    return carrilDeInsercion.Vista;
                }
            }

            return null;
        }

        private static IEnumerator EsperarAlJugador(MatchController controlador)
        {
            float limite = Time.realtimeSinceStartup + Margen;

            while (Time.realtimeSinceStartup < limite
                   && !controlador.Partida.Terminada
                   && (!controlador.EsTurnoDelHumano || controlador.Ocupado))
            {
                yield return null;
            }

            Assert.That(Time.realtimeSinceStartup, Is.LessThan(limite),
                "el turno del rival no termino nunca");
        }

        private static IEnumerator CargarEscena(string nombre)
        {
            yield return SceneManager.LoadSceneAsync(nombre, LoadSceneMode.Single);
            yield return null;
        }
    }
}
