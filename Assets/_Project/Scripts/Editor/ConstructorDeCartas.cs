using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Dibuja el cuerpo visual de una carta (de monstruo o de objeto) desde codigo.
    /// </summary>
    /// <remarks>
    /// Antes vivia por separado y casi identico en
    /// <c>ConstructorDeEscenaDuelo</c>, <c>ConstructorDeEscenaDeckbuild</c> y
    /// <c>ConstructorDeEscenaTienda</c>. Triplicar (y ahora cuadruplicar, con
    /// la Coleccion) el dibujado ya no compensaba: las cuatro pantallas llaman
    /// a <see cref="Monstruo"/> y <see cref="Objeto"/>.
    /// </remarks>
    public static class ConstructorDeCartas
    {
        private static readonly Vector2 TamanoCarta = new(130f, 180f);
        private static readonly Vector2 TamanoCartaObjeto = new(100f, 140f);
        private static readonly Color ColorDeFondo = new(0.15f, 0.17f, 0.24f, 1f);

        /// <summary>Carta dibujada: fondo, arte, los cinco numeros y el indicador de ataque.</summary>
        public static VistaCartaMonstruo Monstruo(
            string nombre, Transform padre, Vector2 posicion, bool conFondo = true)
        {
            RectTransform raiz = conFondo
                ? ConstructorDeInterfaz.Panel(nombre, padre, posicion, TamanoCarta,
                    ColoresDeRareza.De(CardRarity.Comun), recibeClics: false)
                    .rectTransform
                : ConstructorDeInterfaz.Nodo(nombre, padre, posicion, TamanoCarta);

            // El marco (el propio fondo, coloreado por rareza en Refrescar) y
            // un "Fondo" interior mas pequeno encima, para que se vea como un
            // borde de color alrededor de la cara oscura de la carta. Solo
            // tiene sentido si esta carta tiene fondo propio: la mano de
            // Duelo ya vive dentro de un hueco con su propio color.
            Image marco = null;
            if (conFondo)
            {
                marco = raiz.GetComponent<Image>();
                ConstructorDeInterfaz.Panel("Fondo", raiz, Vector2.zero,
                    TamanoCarta - new Vector2(8f, 8f), ColorDeFondo, recibeClics: false);
            }

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

            // Fila de numeros mas arriba que antes (era y=-64): deja sitio
            // debajo para el corazon y el arma.
            Text ataque = ConstructorDeInterfaz.Texto("Ataque", raiz,
                new Vector2(-44f, -50f), new Vector2(36f, 28f), "", 22);
            Text cura = ConstructorDeInterfaz.Texto("Cura", raiz,
                new Vector2(0f, -50f), new Vector2(36f, 28f), "", 22);
            Text vida = ConstructorDeInterfaz.Texto("Vida", raiz,
                new Vector2(44f, -50f), new Vector2(36f, 28f), "", 22);

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

            RectTransform iconoCorazon = IconoDeCorazon(raiz);
            RectTransform iconoMelee = IconoDeEspada(raiz);
            RectTransform iconoRango = IconoDeFlecha(raiz);
            RectTransform iconoHibrido = IconoDeHibrido(raiz);

            ConstructorDeInterfaz.Cablear(vista,
                ("nombre", nombreCarta),
                ("ataque", ataque),
                ("mana", mana),
                ("cura", cura),
                ("vida", vida),
                ("arte", arte),
                ("marco", marco),
                ("iconoObjeto", iconoObjeto),
                ("textoFlotante", textoFlotante),
                ("iconoMelee", iconoMelee.gameObject),
                ("iconoRango", iconoRango.gameObject),
                ("iconoHibrido", iconoHibrido.gameObject));

            // El corazon no tiene campo en VistaCartaMonstruo: no hay logica
            // condicional, siempre esta encendido bajo la vida.
            iconoCorazon.gameObject.SetActive(true);

            return vista;
        }

        /// <summary>Carta de objeto dibujada: fondo, arte y los tres bonus.</summary>
        public static VistaCartaObjeto Objeto(
            string nombre, Transform padre, Vector2 posicion, bool conFondo = true)
        {
            RectTransform raiz = conFondo
                ? ConstructorDeInterfaz.Panel(nombre, padre, posicion, TamanoCartaObjeto,
                    ColoresDeRareza.De(CardRarity.Comun), recibeClics: false)
                    .rectTransform
                : ConstructorDeInterfaz.Nodo(nombre, padre, posicion, TamanoCartaObjeto);

            Image marco = null;
            if (conFondo)
            {
                marco = raiz.GetComponent<Image>();
                ConstructorDeInterfaz.Panel("Fondo", raiz, Vector2.zero,
                    TamanoCartaObjeto - new Vector2(6f, 6f), ColorDeFondo, recibeClics: false);
            }

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
                ("arte", arte),
                ("marco", marco));

            return vista;
        }

        /// <summary>
        /// Corazon bajo "Vida", aproximado con paneles en rombo (no hay
        /// sprite curvo disponible en este entorno): dos lobulos arriba y
        /// una punta mas grande debajo, superpuestos.
        /// </summary>
        private static RectTransform IconoDeCorazon(Transform padre)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "IconoCorazon", padre, new Vector2(44f, -78f), new Vector2(18f, 18f));

            Color rojo = new(0.85f, 0.25f, 0.3f, 1f);

            Image lobuloIzquierdo = ConstructorDeInterfaz.Panel("LobuloIzquierdo", raiz,
                new Vector2(-3.2f, 3f), new Vector2(8f, 8f), rojo, recibeClics: false);
            lobuloIzquierdo.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            Image lobuloDerecho = ConstructorDeInterfaz.Panel("LobuloDerecho", raiz,
                new Vector2(3.2f, 3f), new Vector2(8f, 8f), rojo, recibeClics: false);
            lobuloDerecho.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            Image punta = ConstructorDeInterfaz.Panel("Punta", raiz,
                new Vector2(0f, -3f), new Vector2(10f, 10f), rojo, recibeClics: false);
            punta.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            return raiz;
        }

        /// <summary>
        /// Icono de espada (melee en solitario): hoja, guarda y mango, sin
        /// arte porque no hay herramientas de imagen en este entorno. Es un
        /// dibujo hecho de paneles UI, como el resto de la escena. Empieza
        /// apagado; lo enciende <see cref="VistaCartaMonstruo.Refrescar"/>
        /// segun <c>CanAttackMelee</c> (y solo si no es hibrido).
        /// </summary>
        private static RectTransform IconoDeEspada(Transform padre)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "IconoMelee", padre, new Vector2(-44f, -78f), new Vector2(22f, 22f));

            ConstructorDeInterfaz.Panel("Hoja", raiz,
                new Vector2(0f, 3f), new Vector2(3f, 14f),
                new Color(0.85f, 0.87f, 0.92f, 1f), recibeClics: false);
            ConstructorDeInterfaz.Panel("Guarda", raiz,
                new Vector2(0f, -4f), new Vector2(10f, 2.5f),
                new Color(0.55f, 0.45f, 0.15f, 1f), recibeClics: false);
            ConstructorDeInterfaz.Panel("Mango", raiz,
                new Vector2(0f, -8f), new Vector2(3f, 5f),
                new Color(0.35f, 0.22f, 0.12f, 1f), recibeClics: false);

            // Se construye derecha y se rota entera: mas facil de calcular
            // que rotar cada pieza a su propio angulo.
            raiz.localRotation = Quaternion.Euler(0f, 0f, -40f);
            raiz.gameObject.SetActive(false);

            return raiz;
        }

        /// <summary>
        /// Icono de flecha (rango en solitario): asta y punta, en vez de un
        /// arco curvo que no se puede dibujar bien solo con rectangulos.
        /// Empieza apagado; lo enciende
        /// <see cref="VistaCartaMonstruo.Refrescar"/> segun
        /// <c>CanAttackRanged</c> (y solo si no es hibrido).
        /// </summary>
        private static RectTransform IconoDeFlecha(Transform padre)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "IconoRango", padre, new Vector2(-44f, -78f), new Vector2(22f, 22f));

            DibujarFlecha(raiz);

            raiz.gameObject.SetActive(false);

            return raiz;
        }

        /// <summary>
        /// Icono hibrido (melee y rango a la vez): la misma flecha cruzada
        /// por una vara recta en diagonal. Un arco de verdad no se puede
        /// lograr solo con rectangulos (mismo limite ya asumido con la
        /// flecha), asi que el "arco" se simplifica a una barra que cruza el
        /// asta, en vez de una curva. Empieza apagado; lo enciende
        /// <see cref="VistaCartaMonstruo.Refrescar"/> cuando el monstruo
        /// puede atacar de las dos formas.
        /// </summary>
        private static RectTransform IconoDeHibrido(Transform padre)
        {
            RectTransform raiz = ConstructorDeInterfaz.Nodo(
                "IconoHibrido", padre, new Vector2(-44f, -78f), new Vector2(22f, 22f));

            DibujarFlecha(raiz);

            Image vara = ConstructorDeInterfaz.Panel("Vara", raiz,
                Vector2.zero, new Vector2(18f, 2.2f),
                new Color(0.5f, 0.35f, 0.2f, 1f), recibeClics: false);
            vara.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 65f);

            raiz.gameObject.SetActive(false);

            return raiz;
        }

        private static void DibujarFlecha(Transform raiz)
        {
            ConstructorDeInterfaz.Panel("Asta", raiz,
                new Vector2(-2f, 0f), new Vector2(14f, 2.5f),
                new Color(0.5f, 0.35f, 0.2f, 1f), recibeClics: false);

            Image puntaArriba = ConstructorDeInterfaz.Panel("PuntaArriba", raiz,
                new Vector2(6f, 1.5f), new Vector2(6f, 2f),
                new Color(0.8f, 0.8f, 0.82f, 1f), recibeClics: false);
            puntaArriba.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 35f);

            Image puntaAbajo = ConstructorDeInterfaz.Panel("PuntaAbajo", raiz,
                new Vector2(6f, -1.5f), new Vector2(6f, 2f),
                new Color(0.8f, 0.8f, 0.82f, 1f), recibeClics: false);
            puntaAbajo.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -35f);
        }
    }
}
