// DraggableCard.cs
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    RectTransform _rect;
    CanvasGroup   _group;
    Canvas        _canvas;
    Transform     _originalParent;
    Vector2       _originalAnchoredPos;
    Vector3       _originalScale;
    bool          _droppedInSlot;

    void Awake()
    {
        if (!SistemaTurnos.turnoJugador1) return;
        _rect          = GetComponent<RectTransform>();
        _group         = GetComponent<CanvasGroup>();
        _canvas        = GetComponentInParent<Canvas>();
        _originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!SistemaTurnos.turnoJugador1) return;
        // Guardamos parent, posición y scale
        _originalParent      = transform.parent;
        _originalAnchoredPos = _rect.anchoredPosition;
        _droppedInSlot       = false;

        // Feedback visual
        _group.blocksRaycasts = false;
        _group.alpha          = 0.6f;

        // Lo subimos encima de todo en el Canvas
        transform.SetParent(_canvas.transform, worldPositionStays: true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData e)
    {
        if (!SistemaTurnos.turnoJugador1) return;
        // Seguir el ratón
        _rect.anchoredPosition += e.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!SistemaTurnos.turnoJugador1) return;
        // Restaurar raycasts y opacidad
        _group.blocksRaycasts = true;
        _group.alpha          = 1f;

        if (!_droppedInSlot)
        {
            // Si no cayó en un slot, devolvemos todo
            transform.SetParent(_originalParent, worldPositionStays: false);
            _rect.anchoredPosition = _originalAnchoredPos;
            transform.localScale   = _originalScale;
        }
    }

    // Llamado por BoardSlot para confirmar que aquí sí cayó
    public void MarkAsDropped()
    {
        _droppedInSlot = true;
    }
}
