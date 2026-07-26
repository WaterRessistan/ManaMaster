using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentToReturnTo = null;
    private CanvasGroup canvasGroup;
    private Transform canvasRoot;

    void Awake()
    {
        // Cache the CanvasGroup for raycast toggling
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // Añade un CanvasGroup al GameObject si no existe
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Encuentra el Canvas raíz para elevar el elemento durante el drag
        var canvas = GetComponentInParent<Canvas>();
        canvasRoot = canvas != null ? canvas.transform : transform.root;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Guardamos el padre actual para volver al soltar
        parentToReturnTo = transform.parent;

        // Reparent al Canvas raíz para que quede siempre encima
        transform.SetParent(canvasRoot);
        transform.SetAsLastSibling();

        // Desactivamos raycasts para que no bloquee otros eventos UI
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Seguimos la posición del cursor
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restauramos al contenedor original
        transform.SetParent(parentToReturnTo);
        transform.SetAsLastSibling();

        // Reactivamos los raycasts
        canvasGroup.blocksRaycasts = true;
    }

    // (Opcional) Si necesitas inicialización adicional
    void Start() { }

    // (Opcional) Lógica por frame
    void Update() { }
}
