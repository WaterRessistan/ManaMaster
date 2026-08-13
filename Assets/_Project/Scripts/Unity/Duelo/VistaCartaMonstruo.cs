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

        // "Juice" de combate (DESIGN.md: sin regla propia, es solo feedback
        // visual): golpe de escala + numero flotante sobre la carta afectada.
        private const float DuracionDeImpacto = 0.35f;
        private const float EscalaMaximaDeImpacto = 1.18f;
        private const float DistanciaFlotante = 40f;

        private static readonly Color ColorDeCuracion = new(0.35f, 0.9f, 0.4f, 1f);
        private static readonly Color ColorDeDano = new(0.95f, 0.3f, 0.3f, 1f);

        private Vector2 _posicionInicialFlotante;
        private Coroutine _animacionDeImpacto;

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

            // Desactivar el GameObject para una corrutina de impacto en marcha
            // (Unity la corta sola), pero sin resetear la escala se quedaria
            // a medio golpe la proxima vez que se muestre esta carta.
            _animacionDeImpacto = null;
            transform.localScale = Vector3.one;
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
            while (transcurrido < DuracionDeImpacto)
            {
                transcurrido += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(transcurrido / DuracionDeImpacto);

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

        private static void Escribir(Text campo, string valor)
        {
            if (campo != null)
            {
                campo.text = valor;
            }
        }
    }
}
