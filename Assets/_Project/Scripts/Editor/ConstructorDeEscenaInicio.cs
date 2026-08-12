using ManaMaster.Unity.Navegacion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Genera la escena de Inicio: el punto de entrada del flujo (DESIGN.md §10).
    /// </summary>
    /// <remarks>
    /// Es la pantalla con menos codigo propio: solo botones que cargan otra
    /// escena. "Opciones" no es una escena del roadmap, asi que se resuelve con
    /// un panel que se enciende y se apaga sin salir de aqui.
    /// </remarks>
    public static class ConstructorDeEscenaInicio
    {
        public const string RutaEscena = "Assets/_Project/Scenes/Inicio.unity";

        private static readonly Color ColorDeFondo = new(0.07f, 0.08f, 0.12f, 1f);
        private static readonly Vector2 TamanoBoton = new(360f, 80f);

        [MenuItem("Mana Master/Reconstruir escena de inicio")]
        public static void Reconstruir()
        {
            Scene escena = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConstructorDeEscenaComun.Camara(ColorDeFondo);
            ConstructorDeEscenaComun.SistemaDeEventos();

            Canvas lienzo = ConstructorDeEscenaComun.Lienzo();

            ConstructorDeInterfaz.Texto("Titulo", lienzo.transform,
                new Vector2(0f, 320f), new Vector2(700f, 90f), "Mana Master", 56);

            // Nombres de escena tal como los definen sus propios constructores
            // (ConstructorDeEscenaTienda.RutaEscena, ...Deckbuild...): en
            // literal porque Inicio se genera antes de que esas clases existan
            // en el orden de trabajo de la Fase 5.
            NavegacionA("BotonJugar", lienzo.transform, new Vector2(0f, 120f), "Jugar", "Duelo");
            NavegacionA("BotonTienda", lienzo.transform, new Vector2(0f, 20f), "Tienda", "Tienda");
            NavegacionA("BotonMazos", lienzo.transform, new Vector2(0f, -80f), "Mazos", "Deckbuild");

            Opciones(lienzo.transform);

            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(RutaEscena));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);

            ConstructorDeEscenaComun.AnadirABuildSettings(RutaEscena);

            Debug.Log($"[ConstructorDeEscenaInicio] Escena regenerada en {RutaEscena}");
        }

        /// <summary>Boton que carga otra escena por su nombre (Build Settings).</summary>
        private static void NavegacionA(
            string nombre, Transform padre, Vector2 posicion, string etiqueta,
            string nombreEscenaDestino)
        {
            Button boton = ConstructorDeInterfaz.Boton(
                nombre, padre, posicion, TamanoBoton, etiqueta, out _);

            BotonDeNavegacion navegacion = boton.gameObject.AddComponent<BotonDeNavegacion>();
            ConstructorDeInterfaz.CablearString(navegacion, "nombreEscena", nombreEscenaDestino);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                boton.onClick, navegacion.Ir);
        }

        /// <summary>
        /// Boton "Opciones" que enciende un panel-aviso, sin cambiar de escena
        /// (DESIGN.md §10 no especifica su contenido todavia).
        /// </summary>
        private static void Opciones(Transform padre)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "PanelOpciones", padre, Vector2.zero, new Vector2(520f, 260f));
            Image panel = ConstructorDeInterfaz.Panel("Fondo", raiz.transform,
                Vector2.zero, new Vector2(520f, 260f),
                new Color(0.06f, 0.07f, 0.11f, 0.96f));

            ConstructorDeInterfaz.Texto("Aviso", panel.transform,
                new Vector2(0f, 30f), new Vector2(460f, 100f),
                "Proximamente", 32);

            Button cerrar = ConstructorDeInterfaz.Boton("BotonCerrar",
                panel.transform, new Vector2(0f, -80f), new Vector2(220f, 60f),
                "Cerrar", out _);

            AlternarPanel alternar = raiz.gameObject.AddComponent<AlternarPanel>();
            ConstructorDeInterfaz.Cablear(alternar, ("panel", raiz.gameObject));

            raiz.gameObject.SetActive(false);

            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                cerrar.onClick, alternar.Alternar);

            Button abrir = ConstructorDeInterfaz.Boton("BotonOpciones", padre,
                new Vector2(0f, -180f), TamanoBoton, "Opciones", out _);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                abrir.onClick, alternar.Alternar);
        }
    }
}
