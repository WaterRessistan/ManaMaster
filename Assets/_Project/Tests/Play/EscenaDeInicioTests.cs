using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ManaMaster.Unity.Navegacion;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ManaMaster.PlayTests
{
    /// <summary>
    /// La escena de Inicio, arrancada de verdad: el hub del que salen las
    /// otras tres pantallas (DESIGN.md §10).
    /// </summary>
    [TestFixture]
    public sealed class EscenaDeInicioTests
    {
        private const string Escena = "Inicio";

        [UnityTest]
        public IEnumerator LaEscenaTieneUnBotonACadaPantalla()
        {
            yield return CargarEscena();

            List<string> destinos = Object.FindObjectsByType<BotonDeNavegacion>(
                    FindObjectsSortMode.None)
                .Select(boton => boton.NombreEscena)
                .ToList();

            Assert.That(destinos, Is.EquivalentTo(
                new[] { "Duelo", "Tienda", "Deckbuild", "Coleccion" }));
        }

        [UnityTest]
        public IEnumerator OpcionesEmpiezaCerradoYAlternarLoAbre()
        {
            yield return CargarEscena();

            AlternarPanel alternar =
                Object.FindFirstObjectByType<AlternarPanel>(FindObjectsInactive.Include);

            Assert.That(alternar, Is.Not.Null, "la escena no tiene panel de Opciones");
            Assert.That(alternar.gameObject.activeSelf, Is.False,
                "el panel de Opciones deberia empezar apagado");

            alternar.Alternar();

            Assert.That(alternar.gameObject.activeSelf, Is.True);
        }

        private static IEnumerator CargarEscena()
        {
            yield return SceneManager.LoadSceneAsync(Escena, LoadSceneMode.Single);
            yield return null;
        }
    }
}
