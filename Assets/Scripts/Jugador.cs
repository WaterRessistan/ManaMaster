using UnityEngine;
using UnityEngine.UI;

public class Jugador : MonoBehaviour
{
    public PlayerDeck barajaMonstruosJugador;
    public static int mana = 0;
    public Text contadorMana;
    public Tablero tablero;

    public void addmana(int cantidad)
    {
        mana += cantidad;
        Debug.Log("Mana actual: " + mana);
    }

    public void ActualizarMana()
    {
        contadorMana.text = "Maná: " + mana.ToString();
    }
    public int cartasEnJuego(){
        int contador = 0;
        if(tablero != null){
            if(tablero.boardSlotPrincipal != null && tablero.boardSlotPrincipal.IsOccupied()) contador++;
            if(tablero.boardSlotSecundario != null && tablero.boardSlotSecundario.IsOccupied()) contador++;
            if(tablero.boardSlotTerciario != null && tablero.boardSlotTerciario.IsOccupied()) contador++;
        }
        return contador;

    }
        /*public void atacar(Jugador oponenteJugador) {
        int numeroCartas = tablero.NumcartasEnJuego();
        if(oponenteJugador==null || oponenteJugador.tablero.NumcartasEnJuego() == 0){
            Debug.Log("No hay cartas del oponente en juego para atacar.");
            return;
        }
        if(numeroCartas !=0){
            for(int i=0;i<numeroCartas;i++){
                //Hacer funcion de ataque para cada carta en juego
                ataquecarta(tablero.cartasEnJuego()[i]);
            }
        }else{
            Debug.Log("No hay cartas en juego para atacar.");
            return;
        }
    }
    public void ataquecarta(Cartas cartaAtacante,Jugador oponenteJugador) {
        if(oponenteJugador.tablero.NumcartasEnJuego() > 0){
            oponenteJugador.tablero.Slotprincipal.currentcard.vida = oponenteJugador.tablero.Slotprincipal.currentcard.vida - cartaAtacante.ataque;
        }
    }*/
}
