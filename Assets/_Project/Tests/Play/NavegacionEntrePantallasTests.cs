using System.Collections;
using ManaMaster.Unity.Navegacion;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ManaMaster.PlayTests
{
    /// <summary>
    /// El flujo real entre las 4 pantallas del roadmap (DESIGN.md §10): cada
    /// boton de navegacion carga de verdad la escena que promete.
    /// </summary>
    [TestFixture]
    public sealed class NavegacionEntrePantallasTests
    {
        [UnityTest]
        public IEnumerator BotonJugarDeInicioLlevaADuelo() => Navega("Inicio", "Duelo");

        [UnityTest]
        public IEnumerator BotonTiendaDeInicioLlevaATienda() => Navega("Inicio", "Tienda");

        [UnityTest]
        public IEnumerator BotonMazosDeInicioLlevaADeckbuild() => Navega("Inicio", "Deckbuild");

        [UnityTest]
        public IEnumerator BotonVolverDeTiendaLlevaAInicio() => Navega("Tienda", "Inicio");

        [UnityTest]
        public IEnumerator BotonCancelarDeDeckbuildLlevaAInicio() => Navega("Deckbuild", "Inicio");

        [UnityTest]
        public IEnumerator DueloTieneUnBotonDeVolverAlMenuQueLlevaAInicio()
        {
            yield return CargarEscena("Duelo");

            // El boton vive dentro del panel de resultado, que empieza apagado
            // (solo se enciende al terminar la partida): hay que buscarlo
            // incluyendo objetos inactivos.
            BotonDeNavegacion volver = BuscarPorDestino("Inicio");

            Assert.That(volver, Is.Not.Null, "Duelo no tiene boton de volver al menu");

            volver.Ir();
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Inicio"));
        }

        private static IEnumerator Navega(string escenaOrigen, string escenaDestino)
        {
            yield return CargarEscena(escenaOrigen);

            BotonDeNavegacion boton = BuscarPorDestino(escenaDestino);
            Assert.That(boton, Is.Not.Null,
                $"'{escenaOrigen}' no tiene un boton que lleve a '{escenaDestino}'");

            boton.Ir();
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(escenaDestino));
        }

        private static BotonDeNavegacion BuscarPorDestino(string nombreEscena)
        {
            foreach (BotonDeNavegacion boton in Object.FindObjectsByType<BotonDeNavegacion>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (boton.NombreEscena == nombreEscena)
                {
                    return boton;
                }
            }

            return null;
        }

        private static IEnumerator CargarEscena(string nombre)
        {
            yield return SceneManager.LoadSceneAsync(nombre, LoadSceneMode.Single);
            yield return null;
        }
    }
}
