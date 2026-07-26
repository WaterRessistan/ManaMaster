using UnityEngine;
using UnityEngine.UI;

public class IA : Jugador
{
    private PlayerDeck barajaMonstruosJugador;
    private static int mana = 0;
    public Tablero tablero;

    public void addmana(int cantidad)
    {
        mana += cantidad;
        Debug.Log("Mana actual: " + mana);
    }
    public void accionIA()
    {
        // Implementar lógica de IA aquí
        Debug.Log("La IA está tomando una acción.");
        // Por ejemplo, la IA podría atacar o jugar una carta

    }
    
    void Start()
    {
        inicializarIA();
        Debug.Log("IA inicializada");
        // Aquí podrías inicializar la baraja de la IA o cualquier otro componente necesario
    }

    private void inicializarIA()
    {
        // Aquí podrías inicializar la IA, como asignar una baraja de cartas específica
        barajaMonstruosJugador = new PlayerDeck();
        Debug.Log("Baraja de IA inicializada.");
    }

}
