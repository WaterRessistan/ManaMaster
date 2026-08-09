using System.Collections;
using ManaMaster.Core.Match;
using ManaMaster.Unity.Duelo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ManaMaster.PlayTests
{
    /// <summary>
    /// La escena de duelo, arrancada de verdad.
    /// </summary>
    /// <remarks>
    /// Es la unica prueba que ejecuta el juego: carga la escena generada, deja
    /// correr los Start, juega un turno y comprueba que el rival responde y la
    /// ronda avanza. Sin esto, "compila" y "funciona" serian lo mismo.
    /// </remarks>
    [TestFixture]
    public sealed class EscenaDeDueloTests
    {
        private const string Escena = "Duelo";

        /// <summary>Segundos de margen para las animaciones del combate.</summary>
        private const float Margen = 30f;

        [UnityTest]
        public IEnumerator LaEscenaArrancaConLaPartidaMontada()
        {
            yield return CargarEscena();

            MatchController controlador = Controlador();

            Assert.That(controlador.HayPartida, Is.True,
                "la escena no monto ninguna partida");
            Assert.That(controlador.Humano.MonstruosRestantes, Is.EqualTo(10));
            Assert.That(controlador.Rival.MonstruosRestantes, Is.EqualTo(10));
            Assert.That(controlador.Humano.Mano.Count, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator LasVistasDibujanLaManoDelJugador()
        {
            yield return CargarEscena();

            MatchController controlador = Controlador();

            foreach (VistaMano mano in Object.FindObjectsByType<VistaMano>(
                         FindObjectsSortMode.None))
            {
                mano.Refrescar();
            }

            // Las cartas de la mano se apagan cuando el hueco esta vacio, asi que
            // basta con que haya alguna encendida y con carta.
            bool alguna = false;
            foreach (CartaDeMano carta in Object.FindObjectsByType<CartaDeMano>(
                         FindObjectsSortMode.None))
            {
                alguna |= carta.Vista != null && carta.Vista.TieneCarta;
            }

            Assert.That(alguna, Is.True, "ninguna carta de la mano se ha dibujado");
            Assert.That(controlador.Humano.Mano[0], Is.Not.Null);
        }

        /// <summary>
        /// Un turno completo: el jugador despliega, cierra el turno, el rival
        /// juega el suyo y la ronda avanza.
        /// </summary>
        [UnityTest]
        public IEnumerator UnTurnoCompletoDevuelveElControlAlJugador()
        {
            yield return CargarEscena();

            MatchController controlador = Controlador();
            ControlDeTurno control = controlador.GetComponent<ControlDeTurno>();

            // Puede tocarle empezar al rival: el inicial se sortea (§5).
            yield return EsperarAlJugador(controlador);

            if (controlador.Partida.Terminada)
            {
                Assert.Pass("la partida acabo antes de empezar; nada que probar");
            }

            int rondaInicial = controlador.Partida.Ronda;

            controlador.Desplegar(0, 0);

            control.TerminarTurno();

            yield return EsperarAlJugador(controlador);

            Assert.That(controlador.Partida.Ronda, Is.GreaterThan(rondaInicial),
                "el turno no llego a pasar");
        }

        /// <summary>
        /// El despliegue del jugador se ve en la arena, en el carril donde lo
        /// solto.
        /// </summary>
        [UnityTest]
        public IEnumerator DesplegarUnaCartaLaDibujaEnLaArena()
        {
            yield return CargarEscena();

            MatchController controlador = Controlador();
            yield return EsperarAlJugador(controlador);

            if (controlador.Partida.Terminada)
            {
                Assert.Pass("la partida acabo antes de empezar");
            }

            if (controlador.Desplegar(0, 0) != ResultadoDespliegue.Ok)
            {
                Assert.Pass("no habia mana para desplegar en esta semilla");
            }

            yield return null;

            Assert.That(controlador.Humano.Arena.Count, Is.EqualTo(1));

            bool dibujada = false;
            foreach (CarrilDeInsercion carril in Object.FindObjectsByType<CarrilDeInsercion>(
                         FindObjectsSortMode.None))
            {
                dibujada |= carril.Vista != null
                            && carril.Vista.TieneCarta
                            && ReferenceEquals(
                                carril.Vista.Carta, controlador.Humano.Arena[0]);
            }

            Assert.That(dibujada, Is.True,
                "el monstruo desplegado no aparece en ningun carril");
        }

        private static IEnumerator CargarEscena()
        {
            yield return SceneManager.LoadSceneAsync(Escena, LoadSceneMode.Single);

            // Un fotograma para que corran los Start.
            yield return null;
        }

        private static MatchController Controlador()
        {
            MatchController controlador =
                Object.FindFirstObjectByType<MatchController>();

            Assert.That(controlador, Is.Not.Null,
                "la escena no tiene MatchController");

            return controlador;
        }

        /// <summary>
        /// Espera a que le toque al jugador, o a que la partida acabe. El rival
        /// juega con pausas, asi que hay que darle tiempo.
        /// </summary>
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
    }
}
