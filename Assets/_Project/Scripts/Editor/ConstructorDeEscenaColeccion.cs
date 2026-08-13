using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Coleccion;
using ManaMaster.Unity.Duelo;
using ManaMaster.Unity.Navegacion;
using ManaMaster.Unity.Sesion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Genera la escena de Coleccion: galeria de todas las cartas del
    /// catalogo, atenuadas si el jugador no posee ninguna copia.
    /// </summary>
    public static class ConstructorDeEscenaColeccion
    {
        public const string RutaEscena = "Assets/_Project/Scenes/Coleccion.unity";

        private static readonly Color ColorDeFondo = new(0.07f, 0.08f, 0.12f, 1f);

        private const int ColumnasMonstruos = 8;
        private const int ColumnasObjetos = 7;
        private const float SeparacionXMonstruos = 170f;
        private const float SeparacionYMonstruos = 250f;
        private const float SeparacionXObjetos = 190f;

        private const float AlturaResumen = 400f;
        private const float AlturaTituloMonstruos = 340f;
        private const float AlturaFilaMonstruos = 260f;
        private const float AlturaTituloObjetos = -160f;
        private const float AlturaFilaObjetos = -280f;

        [MenuItem("Mana Master/Reconstruir escena de coleccion")]
        public static void Reconstruir()
        {
            Scene escena = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConstructorDeEscenaComun.Camara(ColorDeFondo);
            ConstructorDeEscenaComun.SistemaDeEventos();

            Canvas lienzo = ConstructorDeEscenaComun.Lienzo();

            ConstructorDeInterfaz.Texto("Titulo", lienzo.transform,
                new Vector2(0f, 460f), new Vector2(700f, 80f), "Colección", 48);

            SesionDeJuego sesion = AssetDatabase.LoadAssetAtPath<SesionDeJuego>(
                ConstructorDeEscenaComun.RutaSesion);
            CardCatalog catalogo = AssetDatabase.LoadAssetAtPath<CardCatalog>(
                ConstructorDeEscenaComun.RutaCatalogo);

            Resumen(lienzo.transform, sesion, catalogo);

            ConstructorDeInterfaz.Texto("TituloMonstruos", lienzo.transform,
                new Vector2(0f, AlturaTituloMonstruos), new Vector2(700f, 40f), "Monstruos", 26);

            if (catalogo != null)
            {
                for (int i = 0; i < catalogo.Monsters.Count; i++)
                {
                    MonsterCardDefinition definicion = catalogo.Monsters[i];
                    if (definicion == null)
                    {
                        continue;
                    }

                    int columna = i % ColumnasMonstruos;
                    int fila = i / ColumnasMonstruos;

                    float x = (columna - (ColumnasMonstruos - 1) * 0.5f) * SeparacionXMonstruos;
                    float y = AlturaFilaMonstruos - fila * SeparacionYMonstruos;

                    EntradaMonstruo(lienzo.transform, new Vector2(x, y), sesion, definicion);
                }
            }

            ConstructorDeInterfaz.Texto("TituloObjetos", lienzo.transform,
                new Vector2(0f, AlturaTituloObjetos), new Vector2(700f, 40f), "Objetos", 26);

            if (catalogo != null)
            {
                for (int i = 0; i < catalogo.Items.Count; i++)
                {
                    ItemCardDefinition definicion = catalogo.Items[i];
                    if (definicion == null)
                    {
                        continue;
                    }

                    int columna = i % ColumnasObjetos;
                    int fila = i / ColumnasObjetos;

                    float x = (columna - (ColumnasObjetos - 1) * 0.5f) * SeparacionXObjetos;
                    float y = AlturaFilaObjetos - fila * 200f;

                    EntradaObjeto(lienzo.transform, new Vector2(x, y), sesion, definicion);
                }
            }

            Volver(lienzo.transform);
            ConstructorDeEscenaComun.Transicion(lienzo);

            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(RutaEscena));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);

            ConstructorDeEscenaComun.AnadirABuildSettings(RutaEscena);

            Debug.Log($"[ConstructorDeEscenaColeccion] Escena regenerada en {RutaEscena}");
        }

        private static void Resumen(Transform padre, SesionDeJuego sesion, CardCatalog catalogo)
        {
            Text texto = ConstructorDeInterfaz.Texto("Resumen", padre,
                new Vector2(0f, AlturaResumen), new Vector2(700f, 40f), "", 22);

            ResumenDeColeccion resumen = texto.gameObject.AddComponent<ResumenDeColeccion>();
            ConstructorDeInterfaz.Cablear(resumen,
                ("catalogo", catalogo), ("sesion", sesion), ("texto", texto));
        }

        private static void EntradaMonstruo(
            Transform padre, Vector2 posicion, SesionDeJuego sesion, MonsterCardDefinition definicion)
        {
            RectTransform contenedor = ConstructorDeInterfaz.Nodo(
                $"Entrada_{definicion.name}", padre, posicion, new Vector2(150f, 230f));

            CanvasGroup atenuado = contenedor.gameObject.AddComponent<CanvasGroup>();

            VistaCartaMonstruo carta = ConstructorDeCartas.Monstruo(
                "Carta", contenedor, new Vector2(0f, 35f));

            Text copias = ConstructorDeInterfaz.Texto("Copias", contenedor,
                new Vector2(0f, -85f), new Vector2(140f, 30f), "", 18);

            EntradaDeColeccionDeMonstruo entrada =
                contenedor.gameObject.AddComponent<EntradaDeColeccionDeMonstruo>();
            ConstructorDeInterfaz.Cablear(entrada,
                ("definicion", definicion),
                ("sesion", sesion),
                ("vista", carta),
                ("copias", copias),
                ("atenuado", atenuado));
        }

        private static void EntradaObjeto(
            Transform padre, Vector2 posicion, SesionDeJuego sesion, ItemCardDefinition definicion)
        {
            RectTransform contenedor = ConstructorDeInterfaz.Nodo(
                $"Entrada_{definicion.name}", padre, posicion, new Vector2(120f, 190f));

            CanvasGroup atenuado = contenedor.gameObject.AddComponent<CanvasGroup>();

            VistaCartaObjeto carta = ConstructorDeCartas.Objeto(
                "Carta", contenedor, new Vector2(0f, 25f));

            Text copias = ConstructorDeInterfaz.Texto("Copias", contenedor,
                new Vector2(0f, -70f), new Vector2(110f, 26f), "", 16);

            EntradaDeColeccionDeObjeto entrada =
                contenedor.gameObject.AddComponent<EntradaDeColeccionDeObjeto>();
            ConstructorDeInterfaz.Cablear(entrada,
                ("definicion", definicion),
                ("sesion", sesion),
                ("vista", carta),
                ("copias", copias),
                ("atenuado", atenuado));
        }

        private static void Volver(Transform padre)
        {
            Button volver = ConstructorDeInterfaz.Boton("BotonVolver", padre,
                new Vector2(-820f, 460f), new Vector2(200f, 60f), "Volver", out _);

            BotonDeNavegacion navegacion = volver.gameObject.AddComponent<BotonDeNavegacion>();
            ConstructorDeInterfaz.CablearString(navegacion, "nombreEscena", "Inicio");
            // Sin AddPersistentListener: BotonDeNavegacion ya se cablea solo
            // en OnEnable (ver el comentario equivalente en las otras escenas).
        }
    }
}
