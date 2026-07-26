using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Partida : MonoBehaviour
{
    private Jugador jugador1;
    private Jugador jugador2;
    private Tablero tablero;
    
    //private Tablero slotsJugador1;
    //private Tablero slotsJugador2;


    void Start()
    {
        inicializarPartida(); // Inicializa la partida y los jugadores
    }

    void Update()
    {

    }

    public void inicializarPartida()
    {
        jugador1 = new GameObject("Jugador1").AddComponent<Jugador>();
        jugador2 = new GameObject("Jugador2").AddComponent<Jugador>();
        //tablero = new Tablero(); // Asegúrate de que Tablero no sea un MonoBehaviour
        //slotsJugador1 = new Tablero();
        //slotsJugador2 = new Tablero();

        //slotsJugador1.boardSlotPrincipal   = new GameObject("SlotPrincipalJugador1").AddComponent<BoardSlot>();
        //slotsJugador1.boardSlotSecundario  = new GameObject("SlotSecundarioJugador1").AddComponent<BoardSlot>();
        //slotsJugador1.boardSlotTerciario   = new GameObject("SlotTerciarioJugador1").AddComponent<BoardSlot>();

        //slotsJugador2.boardSlotPrincipal   = new GameObject("SlotPrincipalJugador2").AddComponent<BoardSlot>();
        //slotsJugador2.boardSlotSecundario  = new GameObject("SlotSecundarioJugador2").AddComponent<BoardSlot>();
        //slotsJugador2.boardSlotTerciario   = new GameObject("SlotTerciarioJugador2").AddComponent<BoardSlot>();

        Debug.Log("Partida inicializada");
    } 

    /*public void colocarCarta(Cartas carta, BoardSlot slot)
    {
        Tablero slotsJugadorActual = turnoJugador1 ? slotsJugador1 : slotsJugador2;

        if (ContarCartas(slotsJugadorActual) >= 3)
        {
            Debug.Log("No puedes colocar más de 3 cartas.");
            return;
        }

        if (slot.IsEmpty())
        {
            slot.PlaceCard(carta);

            for (int i = 0; i < ContarCartas(slotsJugador1); i++)
            {
                if (slotsJugadorActual.boardSlotPrincipal == null || slotsJugadorActual.boardSlotPrincipal.IsEmpty())
                {
                    slotsJugadorActual.boardSlotPrincipal = slot;
                    break;
                }
                else if (slotsJugadorActual.boardSlotSecundario == null || slotsJugadorActual.boardSlotSecundario.IsEmpty())
                {
                    slotsJugadorActual.boardSlotSecundario = slot;
                    break;
                }
                else if (slotsJugadorActual.boardSlotTerciario == null || slotsJugadorActual.boardSlotTerciario.IsEmpty())
                {
                    slotsJugadorActual.boardSlotTerciario = slot;
                    break;
                }
            }
            if (ContarCartas(slotsJugadorActual) >= 3)
            {
                Debug.Log("No puedes colocar más de 3 cartas.");
                return;
            }
        }
        else
        {
            Debug.Log("El slot ya tiene una carta.");
        }
    }

    public void quitarCarta(BoardSlot slot)
    {
        if (!slot.IsEmpty())
        {
            slot.RemoveCard();
        }
        else
        {
            Debug.Log("El slot ya está vacío");
        }
    }
*/

/*
    public void accion()
    {
        if (turnoJugador1)
        {
            if (slotsJugador1?.boardSlotPrincipal?.currentCard != null)
            {
                if (slotsJugador1.boardSlotPrincipal.currentCard.ataque > 0)
                {
                    if (slotsJugador2?.boardSlotPrincipal?.currentCard != null)
                    {
                        slotsJugador2.boardSlotPrincipal.currentCard.vida -= slotsJugador1.boardSlotPrincipal.currentCard.ataque;
                    }
                    else
                    {
                        Debug.LogWarning("El slot principal del jugador 2 o su carta es null.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("El slot principal del jugador 1 o su carta es null.");
            }
        }
        else
        {
            if (slotsJugador2?.boardSlotPrincipal?.currentCard != null)
            {
                if (slotsJugador2.boardSlotPrincipal.currentCard.ataque > 0)
                {
                    if (slotsJugador1?.boardSlotPrincipal?.currentCard != null)
                    {
                        slotsJugador1.boardSlotPrincipal.currentCard.vida -= slotsJugador2.boardSlotPrincipal.currentCard.ataque;
                    }
                    else
                    {
                        Debug.LogWarning("El slot principal del jugador 1 o su carta es null.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("El slot principal del jugador 2 o su carta es null.");
            }
        }
    }*/

    /*public int ContarCartas(Tablero tablerojugador)
    {
        if (tablerojugador == null)
        {
            Debug.LogError("El objeto Tablero es null.");
            return 0;
        }
        int contador = 0;
        if (tablerojugador.boardSlotPrincipal.currentCard != null)   contador++;
        if (tablerojugador.boardSlotSecundario.currentCard != null)  contador++;
        if (tablerojugador.boardSlotTerciario.currentCard != null)   contador++;
        if (turnoActual == 1) return 0;
        return contador;
    }

*/
}