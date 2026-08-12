using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Navegacion;
using ManaMaster.Unity.Tienda;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Genera la escena de Tienda: sobres y cartas sueltas (DESIGN.md §10).
    /// </summary>
    /// <remarks>
    /// Fase 5: solo navegacion. Los precios son los provisionales del §10 y
    /// "Comprar" no gasta nada real todavia — no hay diamantes de mentira que
    /// mostrar ni coleccion que ampliar, eso llega con la Fase 4.
    /// </remarks>
    public static class ConstructorDeEscenaTienda
    {
        public const string RutaEscena = "Assets/_Project/Scenes/Tienda.unity";

        private static readonly Color ColorDeFondo = new(0.07f, 0.08f, 0.12f, 1f);
        private static readonly Vector2 TamanoOferta = new(220f, 160f);

        private const int Columnas = 5;
        private const float SeparacionX = 240f;
        private const float SeparacionY = 190f;

        [MenuItem("Mana Master/Reconstruir escena de tienda")]
        public static void Reconstruir()
        {
            Scene escena = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConstructorDeEscenaComun.Camara(ColorDeFondo);
            ConstructorDeEscenaComun.SistemaDeEventos();

            Canvas lienzo = ConstructorDeEscenaComun.Lienzo();

            ConstructorDeInterfaz.Texto("Titulo", lienzo.transform,
                new Vector2(0f, 460f), new Vector2(700f, 80f), "Tienda", 48);

            Oferta(lienzo.transform, new Vector2(0f, 300f),
                "Sobre (3 cartas, ≥1 Rara)", PreciosTienda.Sobre);

            CardCatalog catalogo = AssetDatabase.LoadAssetAtPath<CardCatalog>(
                ConstructorDeEscenaComun.RutaCatalogo);

            ConstructorDeInterfaz.Texto("TituloCartasSueltas", lienzo.transform,
                new Vector2(0f, 140f), new Vector2(700f, 50f), "Cartas sueltas", 28);

            if (catalogo != null)
            {
                for (int i = 0; i < catalogo.Monsters.Count; i++)
                {
                    MonsterCardDefinition definicion = catalogo.Monsters[i];
                    if (definicion == null)
                    {
                        continue;
                    }

                    int columna = i % Columnas;
                    int fila = i / Columnas;

                    float x = (columna - (Columnas - 1) * 0.5f) * SeparacionX;
                    float y = 20f - fila * SeparacionY;

                    Oferta(lienzo.transform, new Vector2(x, y),
                        definicion.DisplayName,
                        PreciosTienda.DeCartaSuelta(definicion.Rarity));
                }
            }

            Volver(lienzo.transform);

            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(RutaEscena));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);

            ConstructorDeEscenaComun.AnadirABuildSettings(RutaEscena);

            Debug.Log($"[ConstructorDeEscenaTienda] Escena regenerada en {RutaEscena}");
        }

        private static VistaOfertaTienda Oferta(
            Transform padre, Vector2 posicion, string etiqueta, int precio)
        {
            Image fondo = ConstructorDeInterfaz.Panel("Oferta", padre, posicion,
                TamanoOferta, new Color(0.15f, 0.17f, 0.24f, 1f));
            fondo.gameObject.name = $"Oferta_{etiqueta}";

            VistaOfertaTienda vista = fondo.gameObject.AddComponent<VistaOfertaTienda>();

            Text nombre = ConstructorDeInterfaz.Texto("Nombre", fondo.transform,
                new Vector2(0f, 45f), new Vector2(200f, 60f), etiqueta, 18);
            Text precioTexto = ConstructorDeInterfaz.Texto("Precio", fondo.transform,
                new Vector2(0f, -10f), new Vector2(200f, 30f), "", 20);

            Button comprar = ConstructorDeInterfaz.Boton("BotonComprar",
                fondo.transform, new Vector2(0f, -55f), new Vector2(160f, 50f),
                "Comprar", out _);

            ConstructorDeInterfaz.Cablear(vista,
                ("nombre", nombre),
                ("precio", precioTexto),
                ("comprar", comprar));

            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                comprar.onClick, vista.Comprar);

            vista.Mostrar(etiqueta, precio);

            return vista;
        }

        private static void Volver(Transform padre)
        {
            Button volver = ConstructorDeInterfaz.Boton("BotonVolver", padre,
                new Vector2(-820f, 460f), new Vector2(200f, 60f), "Volver", out _);

            BotonDeNavegacion navegacion = volver.gameObject.AddComponent<BotonDeNavegacion>();
            ConstructorDeInterfaz.CablearString(navegacion, "nombreEscena", "Inicio");
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                volver.onClick, navegacion.Ir);
        }
    }
}
