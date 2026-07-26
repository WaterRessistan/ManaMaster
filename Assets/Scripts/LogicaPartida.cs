using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; //no se si son necesarias estas dos biblioteca
using System.Collections.Generic;

public class LogicaPartida: MonoBehaviour
{
    public Jugador jugador1,jugardor2;
    private static bool finpartida = false;
    
    public List<Eventos> eventosPartida = new List<Eventos>();

    void Start()
    {

    }

    void Update()
    {
        SinCartas();
    }
   public void SinCartas(){
         /*if((jugador1.barajamonstruosJugador.baraja.Count == 0 && jugador1.cartasEnJuego()==0 ) || 
        jugardor2.barajamonstruosJugador.baraja.Count == 0 && jugador2.cartasEnJuego()==0 ){
            Debug.Log("No quedan cartas en la baraja algun jugador");
            finpartida = true;
        }
        if(finpartida){
            Debug.Log("Fin de la partida");
            // Aquí podrías implementar la lógica para finalizar la partida, como mostrar un mensaje o reiniciar el juego
            // Por ejemplo:
            // SceneManager.LoadScene("GameOverScene");
        }*/
    }
    public void comprobarmuertes(){
        
    }


}
