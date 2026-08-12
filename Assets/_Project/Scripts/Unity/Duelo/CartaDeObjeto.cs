using UnityEngine;
using UnityEngine.EventSystems;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Una de las cartas visibles de la mano de objetos. Se arrastra sobre un
    /// monstruo propio en la arena para equiparselo.
    /// </summary>
    /// <remarks>
    /// Mismo patron que <see cref="CartaDeMano"/>: siempre vuelve a su sitio
    /// al soltarla, y es <see cref="CarrilDeInsercion"/> quien decide si el
    /// equipamiento sale bien preguntando al motor.
    /// </remarks>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class CartaDeObjeto : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private MatchController controlador;

        [Tooltip("Hueco de la mano de objetos que representa esta carta.")]
        [SerializeField, Min(0)] private int hueco;

        [SerializeField] private VistaCartaObjeto vista;

        private RectTransform _rect;
        private CanvasGroup _grupo;
        private Canvas _lienzo;

        private Transform _padreOriginal;
        private Vector2 _posicionOriginal;
        private bool _arrastreValido;

        public int Hueco => hueco;

        public VistaCartaObjeto Vista => vista;

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

            _grupo.blocksRaycasts = false;
            _grupo.alpha = 0.6f;

            transform.SetParent(_lienzo.transform, worldPositionStays: true);
            transform.SetAsLastSibling();
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
        }

        private bool PuedeArrastrarse()
        {
            if (_lienzo == null)
            {
                Debug.LogError("[CartaDeObjeto] La carta no esta dentro de un Canvas.", this);
                return false;
            }

            if (controlador == null || !controlador.EsTurnoDelHumano)
            {
                return false;
            }

            return vista != null && vista.TieneObjeto;
        }
    }
}
