using UnityEngine;
using UnityEngine.EventSystems;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Una de las cartas visibles de la mano del jugador. Se puede arrastrar a
    /// la arena.
    /// </summary>
    /// <remarks>
    /// La carta vuelve siempre a su sitio al soltarla: quien decide si se
    /// despliega es <see cref="CarrilDeInsercion"/> preguntando al motor, y
    /// quien redibuja la mano es <see cref="VistaMano"/> cuando el motor avisa
    /// de que algo ha cambiado. Asi la vista nunca se adelanta a las reglas.
    /// </remarks>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class CartaDeMano : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private MatchController controlador;

        [Tooltip("Hueco de la mano que representa esta carta.")]
        [SerializeField, Min(0)] private int hueco;

        [SerializeField] private VistaCartaMonstruo vista;

        [Tooltip("Arena propia, para resaltar las posiciones validas al arrastrar.")]
        [SerializeField] private VistaArena arenaPropia;

        private RectTransform _rect;
        private CanvasGroup _grupo;
        private Canvas _lienzo;

        private Transform _padreOriginal;
        private Vector2 _posicionOriginal;
        private bool _arrastreValido;

        public int Hueco => hueco;

        public VistaCartaMonstruo Vista => vista;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _grupo = GetComponent<CanvasGroup>();
            _lienzo = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _arrastreValido = PuedeArrastrarse();
            if (!_arrastreValido)
            {
                return;
            }

            _padreOriginal = transform.parent;
            _posicionOriginal = _rect.anchoredPosition;

            // Sin esto el carril de debajo nunca recibe el evento de soltado.
            _grupo.blocksRaycasts = false;
            _grupo.alpha = 0.6f;

            transform.SetParent(_lienzo.transform, worldPositionStays: true);
            transform.SetAsLastSibling();

            if (arenaPropia != null)
            {
                arenaPropia.ResaltarPosicionesValidas(true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_arrastreValido)
            {
                _rect.anchoredPosition += eventData.delta / _lienzo.scaleFactor;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_arrastreValido)
            {
                return;
            }

            _arrastreValido = false;

            _grupo.blocksRaycasts = true;
            _grupo.alpha = 1f;

            transform.SetParent(_padreOriginal, worldPositionStays: false);
            _rect.anchoredPosition = _posicionOriginal;

            if (arenaPropia != null)
            {
                arenaPropia.ResaltarPosicionesValidas(false);
            }
        }

        private bool PuedeArrastrarse()
        {
            if (_lienzo == null)
            {
                Debug.LogError("[CartaDeMano] La carta no esta dentro de un Canvas.", this);
                return false;
            }

            if (controlador == null || !controlador.EsTurnoDelHumano || controlador.Ocupado)
            {
                return false;
            }

            return vista != null && vista.TieneCarta;
        }
    }
}
