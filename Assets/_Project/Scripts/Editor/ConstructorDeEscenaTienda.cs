using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using ManaMaster.Unity.Navegacion;
using ManaMaster.Unity.Sesion;
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
    /// Los precios son los provisionales del §10. "Comprar" gasta diamantes
    /// de verdad de la sesion del jugador (Fase 4). Cada oferta enseña la
    /// carta de verdad (con <see cref="ConstructorDeCartas"/> o
    /// <see cref="VistaCartaObjeto"/>) en vez de solo un nombre en texto.
    /// </remarks>
    public static class ConstructorDeEscenaTienda
    {
        public const string RutaEscena = "Assets/_Project/Scenes/Tienda.unity";
        private const string RutaArteSobre = "Assets/_Project/Content/Art/Sobre.png";

        private static readonly Color ColorDeFondo = new(0.07f, 0.08f, 0.12f, 1f);
        private static readonly Vector2 TamanoOfertaSobre = new(220f, 220f);

        // Las cartas de la Tienda se dibujan a menor escala que en Duelo o
        // Deckbuild (ConstructorDeCartas usa siempre 130x180 / 100x140 por
        // dentro): con 13 monstruos y 7 objetos no cabria una sola fila, asi
        // que se encogen para poder ensenar dos filas de monstruos sin
        // desbordar el lienzo de 1920x1080.
        private const float EscalaCartaMonstruo = 0.78f;
        private const float EscalaCartaObjeto = 0.8f;

        private const int ColumnasMonstruos = 7;
        private const float SeparacionX = 145f;
        private const float SeparacionY = 235f;
        private const float SeparacionXObjetos = 190f;

        // De arriba a abajo: sobre, titulo, filas de monstruos, titulo, fila
        // de objetos. Es una aproximacion sin verificacion visual, igual que
        // el resto de la interfaz de esta sesion: si algo queda apretado o
        // se sale del lienzo, se ajusta al verlo en el editor.
        private const float AlturaSobre = 290f;
        private const float AlturaTituloCartas = 160f;
        private const float AlturaFilaMonstruos = 10f;
        private const float AlturaTituloObjetos = -370f;
        private const float AlturaFilaObjetos = -470f;

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

            SesionDeJuego sesion = AssetDatabase.LoadAssetAtPath<SesionDeJuego>(
                ConstructorDeEscenaComun.RutaSesion);
            CardCatalog catalogo = AssetDatabase.LoadAssetAtPath<CardCatalog>(
                ConstructorDeEscenaComun.RutaCatalogo);

            Diamantes(lienzo.transform, sesion);

            ControladorAperturaDeSobre apertura = PanelDeApertura(lienzo.transform);

            Sprite arteDelSobre = AssetDatabase.LoadAssetAtPath<Sprite>(RutaArteSobre);
            OfertaDeSobre(lienzo.transform, new Vector2(0f, AlturaSobre),
                sesion, catalogo, apertura, arteDelSobre);

            ConstructorDeInterfaz.Texto("TituloCartasSueltas", lienzo.transform,
                new Vector2(0f, AlturaTituloCartas), new Vector2(700f, 50f), "Cartas sueltas", 28);

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

                    float x = (columna - (ColumnasMonstruos - 1) * 0.5f) * SeparacionX;
                    float y = AlturaFilaMonstruos - fila * SeparacionY;

                    OfertaDeMonstruo(lienzo.transform, new Vector2(x, y), sesion, definicion);
                }
            }

            ConstructorDeInterfaz.Texto("TituloObjetos", lienzo.transform,
                new Vector2(0f, AlturaTituloObjetos), new Vector2(700f, 50f), "Objetos", 28);

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

                    OfertaDeObjeto(lienzo.transform,
                        new Vector2(x, AlturaFilaObjetos), sesion, definicion);
                }
            }

            Volver(lienzo.transform);
            ConstructorDeEscenaComun.Transicion(lienzo);

            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(RutaEscena));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);

            ConstructorDeEscenaComun.AnadirABuildSettings(RutaEscena);

            Debug.Log($"[ConstructorDeEscenaTienda] Escena regenerada en {RutaEscena}");
        }

        /// <summary>
        /// Oferta de una carta de monstruo: la carta de verdad (con su
        /// indicador de tipo de ataque) mas precio y boton de compra debajo.
        /// </summary>
        private static VistaOfertaTienda OfertaDeMonstruo(
            Transform padre, Vector2 posicion, SesionDeJuego sesion, MonsterCardDefinition definicion)
        {
            RectTransform contenedor = ConstructorDeInterfaz.Nodo(
                $"Oferta_{definicion.name}", padre, posicion, new Vector2(150f, 235f));

            VistaCartaMonstruo carta = ConstructorDeCartas.Monstruo(
                "Carta", contenedor, new Vector2(0f, 35f));
            carta.transform.localScale = Vector3.one * EscalaCartaMonstruo;
            // Horneado en el editor: esta pantalla no tiene ninguna partida
            // en curso de la que sacar una CardInstance real, asi que se crea
            // una desechable solo para pintar la plantilla (mismo truco que
            // SelectorDeCarta.Awake en Deckbuild).
            carta.Mostrar(new CardInstance(definicion));

            Text precioTexto = ConstructorDeInterfaz.Texto("Precio", contenedor,
                new Vector2(0f, -60f), new Vector2(140f, 28f), "", 18);

            Button comprar = ConstructorDeInterfaz.Boton("BotonComprar", contenedor,
                new Vector2(0f, -96f), new Vector2(130f, 42f), "Comprar", out _);

            VistaOfertaTienda vista = contenedor.gameObject.AddComponent<VistaOfertaTienda>();
            ConstructorDeInterfaz.Cablear(vista,
                ("precio", precioTexto),
                ("comprar", comprar),
                ("sesion", sesion),
                ("carta", definicion));

            // Sin "nombre": la carta ya enseña el suyo, un texto aparte
            // solo repetiria lo mismo.
            vista.Mostrar(definicion.DisplayName);

            return vista;
        }

        /// <summary>Oferta de una carta de objeto: arte y los tres bonus, precio y compra.</summary>
        private static VistaOfertaTienda OfertaDeObjeto(
            Transform padre, Vector2 posicion, SesionDeJuego sesion, ItemCardDefinition definicion)
        {
            RectTransform contenedor = ConstructorDeInterfaz.Nodo(
                $"Oferta_{definicion.name}", padre, posicion, new Vector2(130f, 190f));

            VistaCartaObjeto carta = ConstructorDeCartas.Objeto(
                "Carta", contenedor, new Vector2(0f, 25f));
            carta.transform.localScale = Vector3.one * EscalaCartaObjeto;
            carta.Mostrar(definicion);

            Text precioTexto = ConstructorDeInterfaz.Texto("Precio", contenedor,
                new Vector2(0f, -48f), new Vector2(110f, 24f), "", 15);

            Button comprar = ConstructorDeInterfaz.Boton("BotonComprar", contenedor,
                new Vector2(0f, -80f), new Vector2(95f, 36f), "Comprar", out _);

            VistaOfertaTienda vista = contenedor.gameObject.AddComponent<VistaOfertaTienda>();
            ConstructorDeInterfaz.Cablear(vista,
                ("precio", precioTexto),
                ("comprar", comprar),
                ("sesion", sesion),
                ("carta", definicion));

            vista.Mostrar(definicion.DisplayName);

            return vista;
        }

        /// <summary>
        /// Oferta del sobre: el pixel art en vez de una carta (no es una
        /// carta concreta), mismo precio/boton que las demas, y cableada al
        /// panel de apertura para que comprarlo dispare la animacion.
        /// </summary>
        private static VistaOfertaTienda OfertaDeSobre(
            Transform padre, Vector2 posicion, SesionDeJuego sesion, CardCatalog catalogo,
            ControladorAperturaDeSobre apertura, Sprite arteDelSobre)
        {
            RectTransform contenedor = ConstructorDeInterfaz.Nodo(
                "Oferta_Sobre", padre, posicion, TamanoOfertaSobre);

            Image arte = ConstructorDeInterfaz.Panel("Arte", contenedor,
                new Vector2(0f, 60f), new Vector2(90f, 90f), Color.white,
                recibeClics: false);
            arte.sprite = arteDelSobre;
            arte.preserveAspect = true;
            arte.enabled = arteDelSobre != null;

            Text nombre = ConstructorDeInterfaz.Texto("Nombre", contenedor,
                new Vector2(0f, 5f), new Vector2(200f, 30f), "", 18);
            Text precioTexto = ConstructorDeInterfaz.Texto("Precio", contenedor,
                new Vector2(0f, -32f), new Vector2(200f, 28f), "", 18);

            Button comprar = ConstructorDeInterfaz.Boton("BotonComprar", contenedor,
                new Vector2(0f, -68f), new Vector2(160f, 46f), "Comprar", out _);

            VistaOfertaTienda vista = contenedor.gameObject.AddComponent<VistaOfertaTienda>();
            ConstructorDeInterfaz.Cablear(vista,
                ("nombre", nombre),
                ("precio", precioTexto),
                ("comprar", comprar),
                ("sesion", sesion),
                ("catalogo", catalogo),
                ("apertura", apertura));

            vista.Mostrar("Sobre (3 cartas, ≥1 Rara)");

            return vista;
        }

        /// <summary>
        /// Pantalla superpuesta que revela las 3 cartas del sobre y termina
        /// en un resumen. Empieza apagada; <see cref="VistaOfertaTienda"/> la
        /// enciende al comprar el sobre.
        /// </summary>
        private static ControladorAperturaDeSobre PanelDeApertura(Transform padre)
        {
            Vector2 pantallaCompleta = new(1920f, 1080f);

            Image fondo = ConstructorDeInterfaz.Panel(
                "PanelApertura", padre, Vector2.zero, pantallaCompleta,
                new Color(0f, 0f, 0f, 0.85f));
            fondo.gameObject.SetActive(false);
            RectTransform raiz = fondo.rectTransform;

            ControladorAperturaDeSobre controlador =
                raiz.gameObject.AddComponent<ControladorAperturaDeSobre>();

            RectTransform panelRevelado = ConstructorDeInterfaz.Nodo(
                "Revelado", raiz, Vector2.zero, pantallaCompleta);

            Vector2 tamanoDorso = new(130f * 2.2f, 180f * 2.2f);
            Image dorso = ConstructorDeInterfaz.Panel("Dorso", panelRevelado,
                new Vector2(0f, 40f), tamanoDorso,
                new Color(0.20f, 0.16f, 0.30f, 1f), recibeClics: false);
            ConstructorDeInterfaz.Panel("Marco", dorso.transform, Vector2.zero,
                tamanoDorso - new Vector2(24f, 24f),
                new Color(0.30f, 0.24f, 0.42f, 1f), recibeClics: false);

            VistaCartaMonstruo cartaRevelada = ConstructorDeCartas.Monstruo(
                "CartaRevelada", panelRevelado, new Vector2(0f, 40f));
            cartaRevelada.transform.localScale = Vector3.one * 2.2f;
            // SetActive simple, no Ocultar(): a esta altura de la
            // construccion de la escena, VistaCartaMonstruo.Awake puede no
            // haber corrido todavia, y Ocultar() reescribe anchoredPosition
            // con lo que tenga cacheado en ese momento (podria ser el
            // (0, 0) por defecto en vez de esta posicion). Sin tocar
            // posiciones, solo hace falta que empiece apagada.
            cartaRevelada.gameObject.SetActive(false);

            Text instruccion = ConstructorDeInterfaz.Texto("Instruccion", panelRevelado,
                new Vector2(0f, -260f), new Vector2(700f, 50f), "", 26);

            Button botonAvanzar = ConstructorDeInterfaz.Boton("BotonAvanzar", panelRevelado,
                new Vector2(0f, -330f), new Vector2(320f, 60f), "Toca para continuar", out _);

            RectTransform panelResumen = ConstructorDeInterfaz.Nodo(
                "Resumen", raiz, Vector2.zero, pantallaCompleta);
            panelResumen.gameObject.SetActive(false);

            ConstructorDeInterfaz.Texto("TituloResumen", panelResumen,
                new Vector2(0f, 220f), new Vector2(600f, 60f), "Cartas obtenidas", 32);

            Text[] filas = new Text[GeneradorDeSobres.CartasPorSobre];
            for (int i = 0; i < filas.Length; i++)
            {
                filas[i] = ConstructorDeInterfaz.Texto($"Fila{i + 1}", panelResumen,
                    new Vector2(0f, 130f - i * 60f), new Vector2(560f, 44f), "", 26);
            }

            Button botonCerrar = ConstructorDeInterfaz.Boton("BotonCerrar", panelResumen,
                new Vector2(0f, -180f), new Vector2(220f, 60f), "Cerrar", out _);

            ConstructorDeInterfaz.Cablear(controlador,
                ("panelRaiz", raiz.gameObject),
                ("panelRevelado", panelRevelado.gameObject),
                ("dorso", dorso.gameObject),
                ("carta", cartaRevelada),
                ("instruccion", instruccion),
                ("botonAvanzar", botonAvanzar),
                ("panelResumen", panelResumen.gameObject),
                ("botonCerrar", botonCerrar));
            ConstructorDeInterfaz.CablearLista(controlador, "filasResumen", filas);

            return controlador;
        }

        private static void Diamantes(Transform padre, SesionDeJuego sesion)
        {
            Text texto = ConstructorDeInterfaz.Texto("Diamantes", padre,
                new Vector2(820f, 460f), new Vector2(240f, 60f), "", 28,
                TextAnchor.MiddleRight);

            VistaDiamantes vista = texto.gameObject.AddComponent<VistaDiamantes>();
            ConstructorDeInterfaz.Cablear(vista, ("sesion", sesion), ("texto", texto));
        }

        private static void Volver(Transform padre)
        {
            Button volver = ConstructorDeInterfaz.Boton("BotonVolver", padre,
                new Vector2(-820f, 460f), new Vector2(200f, 60f), "Volver", out _);

            BotonDeNavegacion navegacion = volver.gameObject.AddComponent<BotonDeNavegacion>();
            ConstructorDeInterfaz.CablearString(navegacion, "nombreEscena", "Inicio");
            // Sin AddPersistentListener: BotonDeNavegacion ya se cablea solo
            // en OnEnable (ver el comentario de OfertaDeMonstruo mas arriba).
        }
    }
}
