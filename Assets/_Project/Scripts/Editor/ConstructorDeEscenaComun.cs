using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Piezas comunes a los cuatro constructores de escena.
    /// </summary>
    /// <remarks>
    /// Camara, EventSystem, Canvas base y el alta en Build Settings son
    /// identicos en Inicio, Tienda, Deckbuild y Duelo: vivian duplicados solo
    /// en <c>ConstructorDeEscenaDuelo</c> porque era la unica escena.
    /// </remarks>
    public static class ConstructorDeEscenaComun
    {
        public const string RutaCatalogo = "Assets/_Project/Content/Cards/CardCatalog.asset";
        public const string RutaSesion = "Assets/_Project/Content/Session/SesionDeJuego.asset";

        /// <summary>
        /// Orden fijo en Build Settings, con Inicio siempre en el indice 0
        /// porque es la escena de arranque. Regenerar cualquiera de las 4 en
        /// cualquier orden no debe desplazarla.
        /// </summary>
        private static readonly string[] OrdenCanonico =
        {
            "Assets/_Project/Scenes/Inicio.unity",
            "Assets/_Project/Scenes/Tienda.unity",
            "Assets/_Project/Scenes/Deckbuild.unity",
            "Assets/_Project/Scenes/Duelo.unity",
        };

        public static void Camara(Color fondo)
        {
            GameObject objeto = new("Camara", typeof(Camera));
            Camera camara = objeto.GetComponent<Camera>();

            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = fondo;
            camara.orthographic = true;
            camara.tag = "MainCamera";

            objeto.transform.position = new Vector3(0f, 0f, -10f);
        }

        /// <summary>
        /// El modulo de entrada tiene que ser el del Input System nuevo: el
        /// proyecto tiene desactivado el antiguo y <c>StandaloneInputModule</c>
        /// reventaria al arrancar.
        /// </summary>
        public static void SistemaDeEventos()
        {
            GameObject objeto = new("EventSystem", typeof(EventSystem));
            objeto.AddComponent<InputSystemUIInputModule>();
        }

        public static Canvas Lienzo()
        {
            GameObject objeto = new("Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            Canvas lienzo = objeto.GetComponent<Canvas>();
            lienzo.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler escalador = objeto.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920f, 1080f);
            escalador.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escalador.matchWidthOrHeight = 0.5f;

            return lienzo;
        }

        /// <summary>
        /// Anade la escena a Build Settings si falta, y reordena todo el
        /// conjunto segun <see cref="OrdenCanonico"/>.
        /// </summary>
        public static void AnadirABuildSettings(string rutaEscena)
        {
            HashSet<string> existentes = new();
            foreach (EditorBuildSettingsScene existente in EditorBuildSettings.scenes)
            {
                existentes.Add(existente.path);
            }

            existentes.Add(rutaEscena);

            List<EditorBuildSettingsScene> ordenadas = new();
            foreach (string ruta in OrdenCanonico)
            {
                if (existentes.Remove(ruta))
                {
                    ordenadas.Add(new EditorBuildSettingsScene(ruta, enabled: true));
                }
            }

            // Cualquier escena fuera del orden canonico no deberia darse, pero
            // si se da no se pierde en silencio: se anade detras.
            foreach (string ruta in existentes)
            {
                ordenadas.Add(new EditorBuildSettingsScene(ruta, enabled: true));
            }

            EditorBuildSettings.scenes = ordenadas.ToArray();
        }
    }
}
