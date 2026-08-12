using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Deckbuild;
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
    /// Genera la escena de Deckbuild: construccion del mazo de monstruos
    /// (DESIGN.md §8).
    /// </summary>
    /// <remarks>
    /// Funcional de verdad con las cartas de monstruo y de objeto que hay hoy
    /// en el catalogo: el mazo 10+10 de DESIGN.md §8 se guarda solo con las
    /// dos mitades completas (<see cref="ControladorDeckbuild.PuedeGuardar"/>).
    /// </remarks>
    public static class ConstructorDeEscenaDeckbuild
    {
        public const string RutaEscena = "Assets/_Project/Scenes/Deckbuild.unity";

        private static readonly Color ColorDeFondo = new(0.07f, 0.08f, 0.12f, 1f);
        private static readonly Vector2 TamanoCarta = new(130f, 180f);
        private static readonly Vector2 TamanoSelector = new(150f, 250f);
        private static readonly Vector2 TamanoCartaObjeto = new(100f, 140f);
        private static readonly Vector2 TamanoSelectorDeObjeto = new(120f, 210f);

        private const int Columnas = 5;
        private const float SeparacionX = 170f;
        private const float SeparacionY = 280f;
        private const float SeparacionXObjetos = 190f;

        [MenuItem("Mana Master/Reconstruir escena de deckbuild")]
        public static void Reconstruir()
        {
            Scene escena = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConstructorDeEscenaComun.Camara(ColorDeFondo);
            ConstructorDeEscenaComun.SistemaDeEventos();

            Canvas lienzo = ConstructorDeEscenaComun.Lienzo();

            ControladorDeckbuild controlador = Controlador();

            ConstructorDeInterfaz.Texto("Titulo", lienzo.transform,
                new Vector2(0f, 480f), new Vector2(700f, 70f), "Deckbuild", 44);

            CardCatalog catalogo = AssetDatabase.LoadAssetAtPath<CardCatalog>(
                ConstructorDeEscenaComun.RutaCatalogo);

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
                    float y = 180f - fila * SeparacionY;

                    Selector(lienzo.transform, new Vector2(x, y), controlador, definicion);
                }
            }

            ConstructorDeInterfaz.Texto("TituloObjetos", lienzo.transform,
                new Vector2(0f, -260f), new Vector2(700f, 40f), "Objetos", 26);

            if (catalogo != null)
            {
                for (int i = 0; i < catalogo.Items.Count; i++)
                {
                    ItemCardDefinition definicion = catalogo.Items[i];
                    if (definicion == null)
                    {
                        continue;
                    }

                    float x = (i - (catalogo.Items.Count - 1) * 0.5f) * SeparacionXObjetos;

                    SelectorObjeto(lienzo.transform, new Vector2(x, -370f), controlador, definicion);
                }
            }

            Pie(lienzo.transform, controlador);

            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(RutaEscena));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);

            ConstructorDeEscenaComun.AnadirABuildSettings(RutaEscena);

            Debug.Log($"[ConstructorDeEscenaDeckbuild] Escena regenerada en {RutaEscena}");
        }

        private static ControladorDeckbuild Controlador()
        {
            GameObject objeto = new("Deckbuild", typeof(ControladorDeckbuild));
            ControladorDeckbuild controlador = objeto.GetComponent<ControladorDeckbuild>();

            ConstructorDeInterfaz.Cablear(controlador,
                ("catalogo", AssetDatabase.LoadAssetAtPath<CardCatalog>(
                    ConstructorDeEscenaComun.RutaCatalogo)),
                ("sesion", AssetDatabase.LoadAssetAtPath<SesionDeJuego>(
                    ConstructorDeEscenaComun.RutaSesion)));

            return controlador;
        }

        private static void Selector(
            Transform padre, Vector2 posicion,
            ControladorDeckbuild controlador, MonsterCardDefinition definicion)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                $"Selector_{definicion.name}", padre, posicion, TamanoSelector);

            SelectorDeCarta selector = raiz.gameObject.AddComponent<SelectorDeCarta>();

            VistaCartaMonstruo carta = Carta(raiz.transform, new Vector2(0f, 35f));

            Text copias = ConstructorDeInterfaz.Texto("Copias", raiz,
                new Vector2(0f, -85f), new Vector2(140f, 30f), "0/2", 20);

            Button quitar = ConstructorDeInterfaz.Boton("BotonQuitar", raiz,
                new Vector2(-50f, -120f), new Vector2(60f, 50f), "-", out _);
            Button anadir = ConstructorDeInterfaz.Boton("BotonAnadir", raiz,
                new Vector2(50f, -120f), new Vector2(60f, 50f), "+", out _);

            ConstructorDeInterfaz.Cablear(selector,
                ("definicion", definicion),
                ("controlador", controlador),
                ("vista", carta),
                ("copias", copias),
                ("anadir", anadir),
                ("quitar", quitar));
        }

        private static void SelectorObjeto(
            Transform padre, Vector2 posicion,
            ControladorDeckbuild controlador, ItemCardDefinition definicion)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                $"SelectorObjeto_{definicion.name}", padre, posicion, TamanoSelectorDeObjeto);

            SelectorDeObjeto selector = raiz.gameObject.AddComponent<SelectorDeObjeto>();

            VistaCartaObjeto carta = CartaObjeto(raiz.transform, new Vector2(0f, 25f));

            Text copias = ConstructorDeInterfaz.Texto("Copias", raiz,
                new Vector2(0f, -70f), new Vector2(110f, 26f), "0/2", 16);

            Button quitar = ConstructorDeInterfaz.Boton("BotonQuitar", raiz,
                new Vector2(-40f, -95f), new Vector2(50f, 40f), "-", out _);
            Button anadir = ConstructorDeInterfaz.Boton("BotonAnadir", raiz,
                new Vector2(40f, -95f), new Vector2(50f, 40f), "+", out _);

            ConstructorDeInterfaz.Cablear(selector,
                ("definicion", definicion),
                ("controlador", controlador),
                ("vista", carta),
                ("copias", copias),
                ("anadir", anadir),
                ("quitar", quitar));
        }

        /// <summary>Objeto dibujado: fondo, arte y los tres bonus.</summary>
        private static VistaCartaObjeto CartaObjeto(Transform padre, Vector2 posicion)
        {
            RectTransform raiz = ConstructorDeInterfaz.Panel("Carta", padre, posicion,
                    TamanoCartaObjeto, new Color(0.15f, 0.17f, 0.24f, 1f), recibeClics: false)
                .rectTransform;

            VistaCartaObjeto vista = raiz.gameObject.AddComponent<VistaCartaObjeto>();

            Image arte = ConstructorDeInterfaz.Panel("Arte", raiz,
                new Vector2(0f, 24f), new Vector2(80f, 56f), Color.white,
                recibeClics: false);
            arte.preserveAspect = true;

            Text nombreObjeto = ConstructorDeInterfaz.Texto("Nombre", raiz,
                new Vector2(0f, 58f), new Vector2(96f, 22f), "", 12);
            Text bonusAtaque = ConstructorDeInterfaz.Texto("BonusAtaque", raiz,
                new Vector2(-28f, -50f), new Vector2(28f, 20f), "", 14);
            Text bonusVida = ConstructorDeInterfaz.Texto("BonusVida", raiz,
                new Vector2(0f, -50f), new Vector2(28f, 20f), "", 14);
            Text bonusCura = ConstructorDeInterfaz.Texto("BonusCura", raiz,
                new Vector2(28f, -50f), new Vector2(28f, 20f), "", 14);

            ConstructorDeInterfaz.Cablear(vista,
                ("nombre", nombreObjeto),
                ("bonusAtaque", bonusAtaque),
                ("bonusVida", bonusVida),
                ("bonusCura", bonusCura),
                ("arte", arte));

            return vista;
        }

        private static void Pie(Transform padre, ControladorDeckbuild controlador)
        {
            Text contador = ConstructorDeInterfaz.Texto("Contador", padre,
                new Vector2(-820f, -480f), new Vector2(200f, 50f), "0/10", 26);

            Button guardar = ConstructorDeInterfaz.Boton("BotonGuardar", padre,
                new Vector2(700f, -480f), new Vector2(220f, 70f),
                "Guardar mazo", out _);

            GameObject vistaObjeto = new("VistaSeleccionMazo",
                typeof(VistaSeleccionMazo));
            VistaSeleccionMazo vista = vistaObjeto.GetComponent<VistaSeleccionMazo>();
            ConstructorDeInterfaz.Cablear(vista,
                ("controlador", controlador),
                ("contador", contador),
                ("guardar", guardar));

            Button cancelar = ConstructorDeInterfaz.Boton("BotonCancelar", padre,
                new Vector2(-820f, 460f), new Vector2(200f, 60f), "Cancelar", out _);
            BotonDeNavegacion navegacion = cancelar.gameObject.AddComponent<BotonDeNavegacion>();
            ConstructorDeInterfaz.CablearString(navegacion, "nombreEscena", "Inicio");
            // Sin AddPersistentListener: BotonDeNavegacion ya se cablea solo
            // en OnEnable (mismo motivo que en ConstructorDeEscenaInicio).
        }

        /// <summary>Carta dibujada: fondo, arte y los cinco numeros.</summary>
        private static VistaCartaMonstruo Carta(Transform padre, Vector2 posicion)
        {
            RectTransform raiz = ConstructorDeInterfaz.Panel("Carta", padre, posicion,
                    TamanoCarta, new Color(0.15f, 0.17f, 0.24f, 1f), recibeClics: false)
                .rectTransform;

            VistaCartaMonstruo vista = raiz.gameObject.AddComponent<VistaCartaMonstruo>();

            Image arte = ConstructorDeInterfaz.Panel("Arte", raiz,
                new Vector2(0f, 18f), new Vector2(112f, 96f), Color.white,
                recibeClics: false);
            arte.preserveAspect = true;

            Text nombreCarta = ConstructorDeInterfaz.Texto("Nombre", raiz,
                new Vector2(0f, 76f), new Vector2(126f, 26f), "", 16);
            Text mana = ConstructorDeInterfaz.Texto("Mana", raiz,
                new Vector2(-48f, 76f), new Vector2(30f, 26f), "", 20);
            Text ataque = ConstructorDeInterfaz.Texto("Ataque", raiz,
                new Vector2(-44f, -64f), new Vector2(36f, 28f), "", 22);
            Text cura = ConstructorDeInterfaz.Texto("Cura", raiz,
                new Vector2(0f, -64f), new Vector2(36f, 28f), "", 22);
            Text vida = ConstructorDeInterfaz.Texto("Vida", raiz,
                new Vector2(44f, -64f), new Vector2(36f, 28f), "", 22);

            ConstructorDeInterfaz.Cablear(vista,
                ("nombre", nombreCarta),
                ("ataque", ataque),
                ("mana", mana),
                ("cura", cura),
                ("vida", vida),
                ("arte", arte));

            return vista;
        }
    }
}
