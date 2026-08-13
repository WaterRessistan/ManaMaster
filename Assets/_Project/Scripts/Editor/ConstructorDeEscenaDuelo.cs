using System.Collections.Generic;
using ManaMaster.Core.Board;
using ManaMaster.Core.Match;
using ManaMaster.Unity.Cards;
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
    /// Genera la escena de duelo entera desde codigo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reconstruirla es seguro: borra lo que hubiera y la deja igual que la
    /// ultima vez. Lo que se pierde al regenerar son los retoques hechos a mano
    /// en el editor, asi que los cambios de fondo se hacen aqui.
    /// </para>
    /// <para>
    /// La disposicion sigue el tablero del DESIGN.md §2: los traseros del rival
    /// arriba del todo, su principal debajo, y en espejo los del jugador.
    /// </para>
    /// </remarks>
    public static class ConstructorDeEscenaDuelo
    {
        public const string RutaEscena = "Assets/_Project/Scenes/Duelo.unity";
        private const string RutaTablero = "Assets/_Project/Content/Art/Tablero.png";

        private static readonly Color ColorDeFondo = new(0.07f, 0.08f, 0.12f, 1f);

        private static readonly Vector2 TamanoCarta = new(130f, 180f);
        private static readonly Vector2 TamanoCarril = new(150f, 200f);
        private static readonly Vector2 TamanoCartaObjeto = new(100f, 140f);
        private static readonly Vector2 TamanoTablero = new(1600f, 900f);

        // Una sola fila por jugador (el tablero original de Tablero.png), no
        // una altura para el principal y otra para los traseros. Medido sobre
        // la imagen: ver Contexto del plan de este cambio.
        private const float AlturaFilaRival = 288f;
        private const float AlturaFilaJugador = -286f;
        private const float AlturaMano = -450f;

        private const float SeparacionCarriles = 299f;

        [MenuItem("Mana Master/Reconstruir escena de duelo")]
        public static void Reconstruir()
        {
            Scene escena = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConstructorDeEscenaComun.Camara(ColorDeFondo);
            ConstructorDeEscenaComun.SistemaDeEventos();

            Canvas lienzo = Lienzo();

            MatchController controlador = Partida();

            VistaArena arenaRival = Arena(
                lienzo.transform, controlador, esDelRival: true);
            VistaArena arenaJugador = Arena(
                lienzo.transform, controlador, esDelRival: false);

            Mano(lienzo.transform, controlador, arenaJugador);
            ManoDeObjetos(lienzo.transform, controlador, arenaJugador);
            VistaMarcador marcador = Marcador(lienzo.transform, controlador);
            Resultado(lienzo.transform, controlador);

            CablearControlador(controlador, arenaJugador, arenaRival, marcador);

            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(RutaEscena));

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, RutaEscena);

            ConstructorDeEscenaComun.AnadirABuildSettings(RutaEscena);

            Debug.Log($"[ConstructorDeEscenaDuelo] Escena regenerada en {RutaEscena}");
        }

        private static Canvas Lienzo()
        {
            Canvas lienzo = ConstructorDeEscenaComun.Lienzo();

            Tablero(lienzo.transform);

            // Linea divisoria entre los dos bandos.
            ConstructorDeInterfaz.Panel("LineaCentral", lienzo.transform,
                new Vector2(0f, 55f), new Vector2(1400f, 4f),
                new Color(1f, 1f, 1f, 0.15f), recibeClics: false);

            return lienzo;
        }

        /// <summary>
        /// Fondo con la imagen del tablero, detras de los carriles. Su tamano
        /// y posicion son los mismos que se usaron para medir
        /// <see cref="AlturaFilaRival"/>, <see cref="AlturaFilaJugador"/> y
        /// <see cref="SeparacionCarriles"/>, asi que los carriles caen
        /// encima de sus huecos en la imagen.
        /// </summary>
        private static void Tablero(Transform padre)
        {
            Image fondo = ConstructorDeInterfaz.Panel(
                "Tablero", padre, Vector2.zero, TamanoTablero,
                Color.white, recibeClics: false);
            fondo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaTablero);
            fondo.preserveAspect = true;
        }

        private static MatchController Partida()
        {
            GameObject objeto = new("Partida",
                typeof(MatchController), typeof(ReproductorDeCombate),
                typeof(ControlDeTurno));

            MatchController controlador = objeto.GetComponent<MatchController>();

            ConstructorDeInterfaz.Cablear(controlador,
                ("catalogo", AssetDatabase.LoadAssetAtPath<CardCatalog>(
                    ConstructorDeEscenaComun.RutaCatalogo)),
                ("sesion", AssetDatabase.LoadAssetAtPath<SesionDeJuego>(
                    ConstructorDeEscenaComun.RutaSesion)));

            return controlador;
        }

        private static VistaArena Arena(
            Transform padre, MatchController controlador, bool esDelRival)
        {
            string lado = esDelRival ? "Rival" : "Jugador";

            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                $"Arena{lado}", padre, Vector2.zero, new Vector2(600f, 400f));

            VistaArena vista = raiz.gameObject.AddComponent<VistaArena>();

            CarrilDeInsercion[] carriles = new CarrilDeInsercion[BoardLanes.Count];
            for (int carril = 0; carril < BoardLanes.Count; carril++)
            {
                carriles[carril] = Carril(
                    raiz, controlador, carril, esDelRival);
            }

            ConstructorDeInterfaz.Cablear(vista, ("controlador", controlador));
            ConstructorDeInterfaz.CablearBool(vista, "esDelRival", esDelRival);
            ConstructorDeInterfaz.CablearLista(vista, "carriles", carriles);

            return vista;
        }

        private static CarrilDeInsercion Carril(
            Transform padre, MatchController controlador, int carril, bool esDelRival)
        {
            Image zona = ConstructorDeInterfaz.Panel(
                $"Carril{carril + 1}", padre,
                PosicionDeCarril(carril, esDelRival), TamanoCarril,
                new Color(1f, 1f, 1f, 0.05f));

            CarrilDeInsercion insercion =
                zona.gameObject.AddComponent<CarrilDeInsercion>();

            Image resaltado = ConstructorDeInterfaz.Panel(
                "Resaltado", zona.transform, Vector2.zero, TamanoCarril,
                new Color(0.45f, 0.85f, 1f, 0.35f), recibeClics: false);
            resaltado.enabled = false;

            VistaCartaMonstruo carta = Carta("Carta", zona.transform, Vector2.zero);

            ConstructorDeInterfaz.Cablear(insercion,
                ("controlador", controlador),
                ("vista", carta),
                ("resaltado", resaltado));
            ConstructorDeInterfaz.CablearInt(insercion, "carril", carril);

            return insercion;
        }

        /// <summary>
        /// Coloca cada carril segun el tablero del §2: una sola fila por
        /// jugador, con el principal centrado entre los dos traseros. Los
        /// traseros van cruzados entre bandos: el 2 del jugador queda enfrente
        /// del 3 del rival, que es justo a quien ataca.
        /// </summary>
        private static Vector2 PosicionDeCarril(int carril, bool esDelRival)
        {
            float altura = esDelRival ? AlturaFilaRival : AlturaFilaJugador;

            if (carril == BoardLanes.Principal)
            {
                return new Vector2(0f, altura);
            }

            // Carril 2 a la derecha del rival y a la izquierda del jugador.
            float lado = carril == BoardLanes.PrimerTrasero ? 1f : -1f;
            if (!esDelRival)
            {
                lado = -lado;
            }

            return new Vector2(lado * SeparacionCarriles, altura);
        }

        private static void Mano(
            Transform padre, MatchController controlador, VistaArena arenaJugador)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "ManoJugador", padre, Vector2.zero, new Vector2(400f, 200f));

            VistaMano vista = raiz.gameObject.AddComponent<VistaMano>();

            List<Object> huecos = new();
            for (int hueco = 0; hueco < Hand.Capacity; hueco++)
            {
                float desplazamiento = (hueco - (Hand.Capacity - 1) * 0.5f) * 150f;

                Image fondo = ConstructorDeInterfaz.Panel(
                    $"Hueco{hueco + 1}", raiz,
                    new Vector2(desplazamiento, AlturaMano), TamanoCarta,
                    new Color(0.15f, 0.17f, 0.24f, 1f));

                fondo.gameObject.AddComponent<CanvasGroup>();
                CartaDeMano carta = fondo.gameObject.AddComponent<CartaDeMano>();

                VistaCartaMonstruo vistaCarta =
                    Carta("Contenido", fondo.transform, Vector2.zero, conFondo: false);

                ConstructorDeInterfaz.Cablear(carta,
                    ("controlador", controlador),
                    ("vista", vistaCarta),
                    ("arenaPropia", arenaJugador));
                ConstructorDeInterfaz.CablearInt(carta, "hueco", hueco);

                huecos.Add(carta);
            }

            ConstructorDeInterfaz.Cablear(vista, ("controlador", controlador));
            ConstructorDeInterfaz.CablearBool(vista, "esDelRival", false);
            ConstructorDeInterfaz.CablearLista(vista, "huecos", huecos.ToArray());
        }

        /// <summary>
        /// Mano de objetos del jugador, junto a la de monstruos. Solo la
        /// suya: el Rival no recibe mazo de objetos (ver
        /// <c>MatchController.Comenzar</c>), asi que no hace falta ocultarla
        /// como se hace con la mano de monstruos del rival.
        /// </summary>
        private static void ManoDeObjetos(
            Transform padre, MatchController controlador, VistaArena arenaJugador)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "ManoDeObjetos", padre, new Vector2(560f, AlturaMano), new Vector2(260f, 200f));

            ConstructorDeInterfaz.Texto("TituloObjetos", raiz,
                new Vector2(0f, 90f), new Vector2(240f, 30f), "Objetos", 16);

            VistaManoDeObjetos vista = raiz.gameObject.AddComponent<VistaManoDeObjetos>();

            List<Object> huecos = new();
            for (int hueco = 0; hueco < ItemHand.Capacity; hueco++)
            {
                float desplazamiento = (hueco - (ItemHand.Capacity - 1) * 0.5f) * 120f;

                Image fondo = ConstructorDeInterfaz.Panel(
                    $"Hueco{hueco + 1}", raiz,
                    new Vector2(desplazamiento, 0f), TamanoCartaObjeto,
                    new Color(0.15f, 0.17f, 0.24f, 1f));

                fondo.gameObject.AddComponent<CanvasGroup>();
                CartaDeObjeto carta = fondo.gameObject.AddComponent<CartaDeObjeto>();

                VistaCartaObjeto vistaObjeto = CartaObjetoDibujada(fondo.transform);

                ConstructorDeInterfaz.Cablear(carta,
                    ("controlador", controlador),
                    ("vista", vistaObjeto),
                    ("arenaPropia", arenaJugador));
                ConstructorDeInterfaz.CablearInt(carta, "hueco", hueco);

                huecos.Add(carta);
            }

            ConstructorDeInterfaz.Cablear(vista, ("controlador", controlador));
            ConstructorDeInterfaz.CablearLista(vista, "huecos", huecos.ToArray());
        }

        private static VistaMarcador Marcador(
            Transform padre, MatchController controlador)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "Marcador", padre, Vector2.zero, new Vector2(400f, 400f));

            VistaMarcador vista = raiz.gameObject.AddComponent<VistaMarcador>();

            Text manaRival = ConstructorDeInterfaz.Texto("ManaRival", raiz,
                new Vector2(-700f, 300f), new Vector2(300f, 40f), "Mana: 0",
                26, TextAnchor.MiddleLeft);
            Text manaJugador = ConstructorDeInterfaz.Texto("ManaJugador", raiz,
                new Vector2(-700f, -200f), new Vector2(300f, 40f), "Mana: 0",
                26, TextAnchor.MiddleLeft);
            Text ronda = ConstructorDeInterfaz.Texto("Ronda", raiz,
                new Vector2(-700f, 55f), new Vector2(300f, 40f), "Ronda: 1",
                26, TextAnchor.MiddleLeft);
            Text turno = ConstructorDeInterfaz.Texto("Turno", raiz,
                new Vector2(0f, 500f), new Vector2(600f, 50f), "",
                32, TextAnchor.MiddleCenter);

            Button terminar = ConstructorDeInterfaz.Boton("BotonTerminarTurno", raiz,
                new Vector2(700f, -300f), new Vector2(260f, 70f),
                "Terminar turno", out _);

            ControlDeTurno control = controlador.GetComponent<ControlDeTurno>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                terminar.onClick, control.TerminarTurno);

            ConstructorDeInterfaz.Cablear(vista,
                ("controlador", controlador),
                ("manaHumano", manaJugador),
                ("manaRival", manaRival),
                ("ronda", ronda),
                ("turno", turno),
                ("terminarTurno", terminar));

            return vista;
        }

        private static void Resultado(Transform padre, MatchController controlador)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "Resultado", padre, Vector2.zero, Vector2.zero);

            VistaResultado vista = raiz.gameObject.AddComponent<VistaResultado>();

            Image panel = ConstructorDeInterfaz.Panel("Panel", raiz,
                Vector2.zero, new Vector2(640f, 340f),
                new Color(0.06f, 0.07f, 0.11f, 0.96f));

            Text titulo = ConstructorDeInterfaz.Texto("Titulo", panel.transform,
                new Vector2(0f, 90f), new Vector2(600f, 70f), "", 48);
            Text detalle = ConstructorDeInterfaz.Texto("Detalle", panel.transform,
                new Vector2(0f, 10f), new Vector2(560f, 80f), "", 24);

            Button revancha = ConstructorDeInterfaz.Boton("BotonRevancha",
                panel.transform, new Vector2(-140f, -100f), new Vector2(250f, 70f),
                "Otra partida", out _);

            Button volverAlMenu = ConstructorDeInterfaz.Boton("BotonVolverAlMenu",
                panel.transform, new Vector2(140f, -100f), new Vector2(250f, 70f),
                "Volver al menu", out _);
            BotonDeNavegacion navegacion =
                volverAlMenu.gameObject.AddComponent<BotonDeNavegacion>();
            ConstructorDeInterfaz.CablearString(navegacion, "nombreEscena", "Inicio");
            // Sin AddPersistentListener: BotonDeNavegacion ya se cablea solo
            // en OnEnable (mismo motivo que en ConstructorDeEscenaInicio).

            panel.gameObject.SetActive(false);

            ConstructorDeInterfaz.Cablear(vista,
                ("controlador", controlador),
                ("panel", panel.gameObject),
                ("titulo", titulo),
                ("detalle", detalle),
                ("revancha", revancha));
        }

        private static void CablearControlador(
            MatchController controlador,
            VistaArena arenaJugador,
            VistaArena arenaRival,
            VistaMarcador marcador)
        {
            ReproductorDeCombate reproductor =
                controlador.GetComponent<ReproductorDeCombate>();
            ControlDeTurno control = controlador.GetComponent<ControlDeTurno>();

            ConstructorDeInterfaz.Cablear(reproductor,
                ("arenaHumano", arenaJugador),
                ("arenaRival", arenaRival));

            ConstructorDeInterfaz.Cablear(control,
                ("controlador", controlador),
                ("reproductor", reproductor));
        }

        /// <summary>Carta dibujada: fondo, arte y los cinco numeros.</summary>
        private static VistaCartaMonstruo Carta(
            string nombre, Transform padre, Vector2 posicion, bool conFondo = true)
        {
            RectTransform raiz = conFondo
                ? ConstructorDeInterfaz.Panel(nombre, padre, posicion, TamanoCarta,
                    new Color(0.15f, 0.17f, 0.24f, 1f), recibeClics: false)
                    .rectTransform
                : ConstructorDeInterfaz.Nodo(nombre, padre, posicion, TamanoCarta);

            VistaCartaMonstruo vista =
                raiz.gameObject.AddComponent<VistaCartaMonstruo>();

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

            // Badge del objeto equipado, asomando por la esquina. Apagado
            // hasta que VistaCartaMonstruo.Refrescar lo encienda.
            Image iconoObjeto = ConstructorDeInterfaz.Panel("IconoObjeto", raiz,
                new Vector2(50f, 78f), new Vector2(28f, 28f), Color.white,
                recibeClics: false);
            iconoObjeto.preserveAspect = true;
            iconoObjeto.enabled = false;

            // Numero flotante del "juice" de combate. Oculto hasta que
            // VistaCartaMonstruo.ReproducirImpacto lo encienda.
            Text textoFlotante = ConstructorDeInterfaz.Texto("TextoFlotante", raiz,
                new Vector2(0f, 18f), new Vector2(120f, 40f), "", 26);
            textoFlotante.gameObject.SetActive(false);

            ConstructorDeInterfaz.Cablear(vista,
                ("nombre", nombreCarta),
                ("ataque", ataque),
                ("mana", mana),
                ("cura", cura),
                ("vida", vida),
                ("arte", arte),
                ("iconoObjeto", iconoObjeto),
                ("textoFlotante", textoFlotante));

            return vista;
        }

        /// <summary>Contenido de una carta de objeto: arte y los tres bonus.</summary>
        private static VistaCartaObjeto CartaObjetoDibujada(Transform padre)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "Contenido", padre, Vector2.zero, TamanoCartaObjeto);

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
    }
}
