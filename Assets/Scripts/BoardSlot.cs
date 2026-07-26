// BoardSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class BoardSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData e)
    {
        if (IsOccupied() || (!SistemaTurnos.turnoJugador1)) return; // Si ya está ocupado o no es el turno del jugador 1, no hacemos nada

        var dragged = e.pointerDrag;
        if (dragged == null) return;

        // comprobamos que tenemos mana suficiente para jugar la carta
        var displayCard = dragged.GetComponent<DisplayCard>();
        if (displayCard == null)
        {
            Debug.LogWarning("No se encontró el componente DisplayCard en la carta arrastrada.");
            return;
        }
        if (displayCard.costemana > Jugador.mana)
        {
            Debug.LogWarning("No tienes suficiente mana para jugar esta carta.");
            return;
        }
        else
        {
            Jugador.mana -= displayCard.costemana; // Restamos el mana gastado
            Jugador jugador = FindObjectOfType<Jugador>();  //no me convence, que busca el primer jugador que encuentra
            if (jugador != null)
            {
                jugador.ActualizarMana(); // Actualizamos el contador de mana
            }
            else
            {
                Debug.LogWarning("No se encontró un objeto Jugador en la escena.");
            }
        }

        // 1) Avisamos a la carta que aquí se ha soltado
        var dragComp = dragged.GetComponent<DraggableCard>();
        if (dragComp != null) dragComp.MarkAsDropped();

        // 2) Reparent manteniendo posición/escala/rotación mundiales
        dragged.transform.SetParent(transform, worldPositionStays: true);

        // 3) Ajustamos su posición global al centro del slot
        var rtDragged = dragged.GetComponent<RectTransform>();
        var rtThis = GetComponent<RectTransform>();
        rtDragged.position = rtThis.position;

        // 4) Aseguramos la escala original (en caso de que algo la cambie)
        if (dragComp != null)
            dragged.transform.localScale = dragComp.transform.localScale;

    }
    public bool IsOccupied()
    {
        return transform.childCount > 0;
    }
}
