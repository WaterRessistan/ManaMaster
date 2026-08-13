using System.Collections;
using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Dibuja una carta de monstruo. Solo dibuja.
    /// </summary>
    /// <remarks>
    /// Sustituye a <c>DisplayCard</c>, que era vista y modelo a la vez:
    /// duplicaba los datos de la carta y ademas se sorteaba a si misma cual
    /// mostrar al arrancar. Aqui no se decide nada: se le da una
    /// <see cref="CardInstance"/> y la pinta.
    /// </remarks>
    public sealed class VistaCartaMonstruo : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private Text nombre;
        [SerializeField] private Text ataque;
        [SerializeField] private Text mana;
        [SerializeField] private Text cura;
        [SerializeField] private Text vida;

        [Header("Arte")]
        [SerializeField] private Image arte;

        [Tooltip("Se enciende con el arte del objeto equipado, o se apaga si no lleva ninguno.")]
        [SerializeField] private Image iconoObjeto;

        [Tooltip("Numero flotante que sube y se desvanece con cada golpe o curacion. Oculto hasta que se usa.")]
        [SerializeField] private Text textoFlotante;

        [Header("Tipo de ataque")]
        [Tooltip("Espada: se enciende si el monstruo solo ataca cuerpo a cuerpo.")]
        [SerializeField] private GameObject iconoMelee;
        [Tooltip("Flecha: se enciende si el monstruo solo ataca a distancia.")]
        [SerializeField] private GameObject iconoRango;
        [Tooltip("Arco y flecha cruzados: se enciende si el monstruo puede las dos cosas.")]
        [SerializeField] private GameObject iconoHibrido;

        // "Juice" de combate (DESIGN.md: sin regla propia, es solo feedback
        // visual): golpe de escala + numero flotante sobre la carta afectada,
        // y embestida del atacante contra el objetivo. Duracion compartida
        // por las tres para que atacante y objetivo queden sincronizados.
        private const float DuracionDeGolpe = 0.4375f; // 0.35s base, 25% mas lento a peticion del usuario
        private const float EscalaMaximaDeImpacto = 1.18f;
        private const float DistanciaFlotante = 40f;
        private const float FraccionDeEmbestida = 0.18f;
        private const float DistanciaMaximaDeEmbestida = 45f;

        private static readonly Color ColorDeCuracion = new(0.35f, 0.9f, 0.4f, 1f);
        private static readonly Color ColorDeDano = new(0.95f, 0.3f, 0.3f, 1f);

        private Vector2 _posicionInicialFlotante;
        private Vector2 _posicionAnchoredOriginal;
        private Coroutine _animacionDeImpacto;
        private Coroutine _animacionDeEmbestida;

        /// <summary>
        /// Vida a mostrar en lugar de la actual, o null para usar la real.
        /// </summary>
        /// <remarks>
        /// Lo usa el reproductor de combate: cuando el log llega a la interfaz
        /// las vidas ya son las finales, asi que para enseñar el golpe a golpe
        /// hay que poder pintar una vida pasada.
        /// </remarks>
        private int? _vidaForzada;

        private void Awake()
        {
            if (textoFlotante != null)
            {
                _posicionInicialFlotante = textoFlotante.rectTransform.anchoredPosition;
            }

            _posicionAnchoredOriginal = ((RectTransform)transform).anchoredPosition;
        }

        /// <summary>Carta representada, o null si el hueco esta vacio.</summary>
        public CardInstance Carta { get; private set; }

        public bool TieneCarta => Carta != null;

        /// <summary>Si el badge de objeto equipado esta encendido ahora mismo.</summary>
        public bool MuestraIconoDeObjeto => iconoObjeto != null && iconoObjeto.enabled;

        /// <summary>Muestra la carta con su vida real y enciende el objeto.</summary>
        public void Mostrar(CardInstance carta) => Mostrar(carta, null);

        /// <summary>Muestra la carta con una vida concreta, no la actual.</summary>
        public void MostrarConVida(CardInstance carta, int vidaMostrada)
            => Mostrar(carta, vidaMostrada);

        private void Mostrar(CardInstance carta, int? vidaMostrada)
        {
            Carta = carta;
            _vidaForzada = vidaMostrada;

            if (carta == null)
            {
                Ocultar();
                return;
            }

            gameObject.SetActive(true);
            Refrescar();
        }

        /// <summary>Vacia el hueco y apaga el objeto.</summary>
        public void Ocultar()
        {
            Carta = null;
            _vidaForzada = null;

            // Desactivar el GameObject corta sola cualquier corrutina de
            // impacto o embestida en marcha (Unity lo hace al desactivar),
            // pero sin resetear escala y posicion se quedaria a medio golpe
            // la proxima vez que se muestre esta carta.
            _animacionDeImpacto = null;
            _animacionDeEmbestida = null;
            transform.localScale = Vector3.one;
            ((RectTransform)transform).anchoredPosition = _posicionAnchoredOriginal;
            if (textoFlotante != null)
            {
                textoFlotante.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Vuelve a leer el estado de la carta. Hay que llamarlo cuando la vida
        /// cambia: la vista no se entera sola.
        /// </summary>
        public void Refrescar()
        {
            if (Carta == null)
            {
                return;
            }

            IMonsterCard definicion = Carta.Definition;

            // Ataque y cura salen de Carta, no de la definicion: si lleva un
            // objeto equipado, Carta.Attack/HealPerTurn ya incluyen su bonus
            // (ver CardInstance), y es lo que el jugador va a notar en combate.
            Escribir(nombre, definicion.DisplayName);
            Escribir(ataque, Carta.Attack.ToString());
            Escribir(mana, definicion.ManaCost.ToString());
            Escribir(cura, Carta.HealPerTurn.ToString());
            Escribir(vida, (_vidaForzada ?? Carta.CurrentHealth).ToString());

            if (iconoObjeto != null)
            {
                Sprite iconoDelObjeto = (Carta.EquippedItem as CardDefinition)?.Artwork;
                iconoObjeto.sprite = iconoDelObjeto;
                iconoObjeto.enabled = Carta.EquippedItem != null;
            }

            // Chequeo explicito, no "?.": un GameObject sin cablear en el
            // Inspector no es null de verdad (es la referencia "perdida" de
            // Unity), y "?." no la detecta y revienta en SetActive.
            bool melee = definicion.CanAttackMelee;
            bool rango = definicion.CanAttackRanged;
            bool hibrido = melee && rango;

            if (iconoMelee != null)
            {
                iconoMelee.SetActive(melee && !hibrido);
            }

            if (iconoRango != null)
            {
                iconoRango.SetActive(rango && !hibrido);
            }

            if (iconoHibrido != null)
            {
                iconoHibrido.SetActive(hibrido);
            }

            if (arte == null)
            {
                return;
            }

            // El arte es un Sprite y por eso no cabe en IMonsterCard: el motor de
            // reglas se compila sin UnityEngine. Se lee de la definicion concreta.
            Sprite dibujo = (definicion as CardDefinition)?.Artwork;
            arte.sprite = dibujo;
            arte.enabled = dibujo != null;
        }

        /// <summary>
        /// Golpe de escala y numero flotante sobre esta carta, para que un
        /// ataque o una curacion se note ademas de leerse en la vida.
        /// </summary>
        /// <param name="cantidad">Dano o curacion aplicados. Si es 0 no hace nada.</param>
        public void ReproducirImpacto(int cantidad, bool esCuracion)
        {
            if (cantidad == 0 || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (_animacionDeImpacto != null)
            {
                StopCoroutine(_animacionDeImpacto);
            }

            _animacionDeImpacto = StartCoroutine(AnimarImpacto(cantidad, esCuracion));
        }

        private IEnumerator AnimarImpacto(int cantidad, bool esCuracion)
        {
            Vector3 escalaOriginal = transform.localScale;

            if (textoFlotante != null)
            {
                textoFlotante.text = (esCuracion ? "+" : "-") + Mathf.Abs(cantidad);
                textoFlotante.color = esCuracion ? ColorDeCuracion : ColorDeDano;
                textoFlotante.rectTransform.anchoredPosition = _posicionInicialFlotante;
                textoFlotante.gameObject.SetActive(true);
            }

            float transcurrido = 0f;
            while (transcurrido < DuracionDeGolpe)
            {
                transcurrido += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(transcurrido / DuracionDeGolpe);

                // Sube a mitad de camino y vuelve a la escala normal: un
                // golpe, no un latido.
                float golpe = Mathf.Sin(t * Mathf.PI);
                transform.localScale = escalaOriginal * (1f + (EscalaMaximaDeImpacto - 1f) * golpe);

                if (textoFlotante != null)
                {
                    textoFlotante.rectTransform.anchoredPosition =
                        _posicionInicialFlotante + Vector2.up * (DistanciaFlotante * t);

                    Color color = textoFlotante.color;
                    color.a = 1f - t;
                    textoFlotante.color = color;
                }

                yield return null;
            }

            transform.localScale = escalaOriginal;
            if (textoFlotante != null)
            {
                textoFlotante.gameObject.SetActive(false);
            }

            _animacionDeImpacto = null;
        }

        /// <summary>
        /// Embestida del atacante: se lanza una fraccion del camino hacia el
        /// objetivo y vuelve, para que se vea un choque en vez de que solo
        /// reaccione la carta golpeada.
        /// </summary>
        public void ReproducirEmbestida(Vector3 posicionObjetivoMundo)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (_animacionDeEmbestida != null)
            {
                StopCoroutine(_animacionDeEmbestida);
                ((RectTransform)transform).anchoredPosition = _posicionAnchoredOriginal;
            }

            _animacionDeEmbestida = StartCoroutine(AnimarEmbestida(posicionObjetivoMundo));
        }

        private IEnumerator AnimarEmbestida(Vector3 posicionObjetivoMundo)
        {
            RectTransform rect = (RectTransform)transform;
            Vector3 posicionOriginal = rect.position;
            Vector3 direccion = posicionObjetivoMundo - posicionOriginal;

            float distancia = Mathf.Min(
                direccion.magnitude * FraccionDeEmbestida, DistanciaMaximaDeEmbestida);
            Vector3 posicionEmbestida = posicionOriginal + direccion.normalized * distancia;

            float transcurrido = 0f;
            while (transcurrido < DuracionDeGolpe)
            {
                transcurrido += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(transcurrido / DuracionDeGolpe);
                float avance = Mathf.Sin(t * Mathf.PI);
                rect.position = Vector3.Lerp(posicionOriginal, posicionEmbestida, avance);

                yield return null;
            }

            rect.position = posicionOriginal;
            _animacionDeEmbestida = null;
        }

        private static void Escribir(Text campo, string valor)
        {
            if (campo != null)
            {
                campo.text = valor;
            }
        }
    }
}
