using System.Collections;
using System.IO;
using ManaMaster.Core.Agents;
using ManaMaster.Core.Match;
using ManaMaster.Unity.Duelo;
using ManaMaster.Unity.Sesion;
using ManaMaster.Unity.Tienda;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ManaMaster.PlayTests
{
    /// <summary>
    /// La economia real de la Fase 4: comprar en la Tienda gasta diamantes de
    /// verdad, y terminar una partida en Duelo los reparte (DESIGN.md §10).
    /// </summary>
    /// <remarks>
    /// Cada test redirige la sesion real a un fichero temporal antes de
    /// disparar la primera mutacion, para no tocar el guardado del
    /// desarrollador (ver <c>SesionDeJuego.UsarRutaDeGuardadoParaTests</c>).
    /// </remarks>
    [TestFixture]
    public sealed class TiendaYRecompensasTests
    {
        private const float MargenSegundos = 90f;

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
        public IEnumerator ComprarUnaCartaSueltaGastaSuPrecioYAmpliaLaColeccion()
        {
            yield return CargarEscena("Tienda");

            VistaOfertaTienda oferta = OfertaDeCartaSuelta();
            Redirigir(oferta.Sesion);

            int diamantesAntes = oferta.Sesion.Diamantes;
            int copiasAntes = oferta.Sesion.CopiasEnColeccion(oferta.CardId);

            oferta.Comprar();

            Assert.That(oferta.Sesion.Diamantes, Is.EqualTo(diamantesAntes - oferta.Precio));
            Assert.That(oferta.Sesion.CopiasEnColeccion(oferta.CardId), Is.EqualTo(copiasAntes + 1));
        }

        [UnityTest]
        public IEnumerator ComprarUnObjetoGastaSuPrecioYAmpliaLaColeccion()
        {
            yield return CargarEscena("Tienda");

            VistaOfertaTienda oferta = OfertaDeObjeto();
            Redirigir(oferta.Sesion);

            int diamantesAntes = oferta.Sesion.Diamantes;
            int copiasAntes = oferta.Sesion.CopiasEnColeccion(oferta.CardId);

            oferta.Comprar();

            Assert.That(oferta.Sesion.Diamantes, Is.EqualTo(diamantesAntes - oferta.Precio));
            Assert.That(oferta.Sesion.CopiasEnColeccion(oferta.CardId), Is.EqualTo(copiasAntes + 1));
        }

        [UnityTest]
        public IEnumerator AbrirUnSobreGastaSuPrecio()
        {
            yield return CargarEscena("Tienda");

            VistaOfertaTienda sobre = OfertaDeSobre();
            Redirigir(sobre.Sesion);

            int diamantesAntes = sobre.Sesion.Diamantes;

            sobre.Comprar();

            Assert.That(sobre.Sesion.Diamantes, Is.EqualTo(diamantesAntes - sobre.Precio));
        }

        [UnityTest]
        public IEnumerator ComprarSinDiamantesSuficientesNoGastaNiAmplia()
        {
            yield return CargarEscena("Tienda");

            VistaOfertaTienda oferta = OfertaDeCartaSuelta();
            Redirigir(oferta.Sesion);

            Assert.That(oferta.Precio, Is.GreaterThan(0), "un precio de 0 dejaria esto en bucle");

            // Deja el saldo por debajo del precio, gastando lo que haga falta.
            // Acotado: si el precio fuera 0 por algun error, esto fallaria
            // rapido en vez de colgarse.
            int intentos = 0;
            while (oferta.Sesion.Diamantes >= oferta.Precio && intentos++ < 1000)
            {
                oferta.Sesion.TryGastarDiamantes(oferta.Precio);
            }

            Assert.That(intentos, Is.LessThan(1000), "no se vacio el saldo en un numero razonable de intentos");

            int diamantesAntes = oferta.Sesion.Diamantes;
            int copiasAntes = oferta.Sesion.CopiasEnColeccion(oferta.CardId);

            oferta.Comprar();

            Assert.That(oferta.Sesion.Diamantes, Is.EqualTo(diamantesAntes));
            Assert.That(oferta.Sesion.CopiasEnColeccion(oferta.CardId), Is.EqualTo(copiasAntes));
        }

        [UnityTest]
        public IEnumerator TerminarUnaPartidaReparteDiamantesAlHumano()
        {
            yield return CargarEscena("Duelo");

            MatchController controlador = Object.FindFirstObjectByType<MatchController>();
            Assert.That(controlador, Is.Not.Null, "Duelo no tiene MatchController");
            Assert.That(controlador.Sesion, Is.Not.Null, "Duelo no tiene sesion cableada");

            Redirigir(controlador.Sesion);

            int diamantesAntes = controlador.Sesion.Diamantes;

            yield return JugarHastaElFinal(controlador);

            Assert.That(controlador.Partida.Terminada, Is.True, "la partida no termino a tiempo");

            // El reparto ocurre dentro de VistaResultado.Refrescar(), que
            // solo reacciona cuando se reanudan los avisos tras la ultima
            // animacion de combate.
            yield return EsperarAQueTerminenLasAnimaciones(controlador);

            int esperado = controlador.Partida.Resultado == ResultadoPartida.Empate
                ? 30
                : ReferenceEquals(controlador.Partida.Ganador, controlador.Humano) ? 50 : 15;

            Assert.That(controlador.Sesion.Diamantes, Is.EqualTo(diamantesAntes + esperado));
        }

        private static VistaOfertaTienda OfertaDeCartaSuelta()
        {
            foreach (VistaOfertaTienda oferta in Object.FindObjectsByType<VistaOfertaTienda>(
                         FindObjectsSortMode.None))
            {
                if (oferta.CardId != null)
                {
                    return oferta;
                }
            }

            Assert.Fail("la Tienda no tiene ninguna oferta de carta suelta");
            return null;
        }

        private static VistaOfertaTienda OfertaDeObjeto()
        {
            foreach (VistaOfertaTienda oferta in Object.FindObjectsByType<VistaOfertaTienda>(
                         FindObjectsSortMode.None))
            {
                if (oferta.EsCartaDeObjeto)
                {
                    return oferta;
                }
            }

            Assert.Fail("la Tienda no tiene ninguna oferta de objeto");
            return null;
        }

        private static VistaOfertaTienda OfertaDeSobre()
        {
            foreach (VistaOfertaTienda oferta in Object.FindObjectsByType<VistaOfertaTienda>(
                         FindObjectsSortMode.None))
            {
                if (oferta.CardId == null)
                {
                    return oferta;
                }
            }

            Assert.Fail("la Tienda no tiene oferta de sobre");
            return null;
        }

        private void Redirigir(SesionDeJuego sesion)
        {
            _rutaTemporal = Path.Combine(
                Path.GetTempPath(), $"manamaster-test-{System.Guid.NewGuid()}.json");
            sesion.UsarRutaDeGuardadoParaTests(_rutaTemporal);
        }

        private static IEnumerator JugarHastaElFinal(MatchController controlador)
        {
            AgenteHeuristico manosDelHumano = new();
            float limite = Time.realtimeSinceStartup + MargenSegundos;

            while (Time.realtimeSinceStartup < limite && !controlador.Partida.Terminada)
            {
                if (controlador.Ocupado)
                {
                    yield return null;
                    continue;
                }

                if (!controlador.EsTurnoDelHumano)
                {
                    controlador.JugarTurnoDelRival();
                    yield return null;
                    continue;
                }

                AccionTurno accion = manosDelHumano.DecidirAccion(controlador.Partida);

                if (accion.Tipo != TipoAccion.Desplegar
                    || controlador.Desplegar(accion.HuecoMano, accion.Carril)
                       != ResultadoDespliegue.Ok)
                {
                    controlador.TerminarTurno();
                }

                yield return null;
            }
        }

        private static IEnumerator EsperarAQueTerminenLasAnimaciones(MatchController controlador)
        {
            float limite = Time.realtimeSinceStartup + MargenSegundos;

            while (controlador.Ocupado && Time.realtimeSinceStartup < limite)
            {
                yield return null;
            }

            yield return null;
        }

        private static IEnumerator CargarEscena(string nombre)
        {
            yield return SceneManager.LoadSceneAsync(nombre, LoadSceneMode.Single);
            yield return null;
        }
    }
}
