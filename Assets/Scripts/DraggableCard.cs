using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Permite arrastrar una carta desde la mano hasta la arena.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). Antes <c>Awake()</c> hacía <c>return</c> antes de
/// cachear el RectTransform, el CanvasGroup y el Canvas si en ese instante no
/// era el turno del jugador 1. Esas referencias se quedaban a null PARA SIEMPRE,
/// y el primer arrastre posterior lanzaba NullReferenceException. Un filtro de
/// turno no puede ir nunca en <c>Awake()</c>.
///
/// Además el prefab no tenía CanvasGroup pese a declararlo en RequireComponent,
/// y sin <c>blocksRaycasts = false</c> el carril de debajo no llega a recibir el
/// evento de soltado.
///
/// La validación de si la carta se puede jugar (turno, propietario, maná, orden
/// de carriles) la hace <see cref="BoardSlot"/> al soltar. En la Fase 3 se
/// añadirá el filtro previo para no dejar ni siquiera arrastrar una carta que no
/// es tuya.
/// </remarks>
[RequireComponent(typeof(RectTransform))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rect;
    private CanvasGroup _group;
    private Canvas _canvas;

    private Transform _originalParent;
    private Vector2 _originalAnchoredPos;
    private Vector3 _originalScale;

    /// <summary>
    /// Baraja que debe reponer este hueco de la mano cuando termine el arrastre,
    /// o null si la carta no llegó a jugarse. La reposición no puede hacerse en
    /// el momento del soltado: si la baraja está vacía desactiva el objeto, y
    /// desactivarlo a mitad de arrastre impide que llegue a ejecutarse el resto
    /// de <see cref="OnEndDrag"/>.
    /// </summary>
    private PlayerDeck _barajaAReponer;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _originalScale = transform.localScale;

        // El prefab puede no traer CanvasGroup; sin él no se pueden desactivar
        // los raycasts durante el arrastre y el carril nunca recibiría el drop.
        if (!TryGetComponent(out _group))
        {
            _group = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_canvas == null)
        {
            Debug.LogError("[DraggableCard] La carta no está dentro de un Canvas.", this);
            return;
        }

        _originalParent = transform.parent;
        _originalAnchoredPos = _rect.anchoredPosition;
        _originalScale = transform.localScale;
        _barajaAReponer = null;

        _group.blocksRaycasts = false;
        _group.alpha = 0.6f;

        // Por encima del resto de la interfaz mientras se arrastra.
        transform.SetParent(_canvas.transform, worldPositionStays: true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null)
        {
            return;
        }

        _rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _group.blocksRaycasts = true;
        _group.alpha = 1f;

        // Esta carta vuelve SIEMPRE a la mano: lo que se despliega en el carril
        // es una copia, no ella misma. Así el hueco de la mano sigue existiendo
        // y la baraja puede reponerlo.
        transform.SetParent(_originalParent, worldPositionStays: false);
        _rect.anchoredPosition = _originalAnchoredPos;
        transform.localScale = _originalScale;

        if (_barajaAReponer == null)
        {
            return;
        }

        PlayerDeck baraja = _barajaAReponer;
        _barajaAReponer = null;

        // Va lo último: si la baraja está vacía, esta llamada desactiva el hueco.
        baraja.ColocarEnMano(gameObject);
    }

    /// <summary>
    /// Lo llama <see cref="BoardSlot"/> cuando despliega esta carta. La baraja
    /// repondrá el hueco en cuanto termine el arrastre.
    /// </summary>
    public void MarcarComoJugada(PlayerDeck baraja) => _barajaAReponer = baraja;
}
