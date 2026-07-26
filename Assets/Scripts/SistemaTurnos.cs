using UnityEngine;
using UnityEngine.UI;

public class SistemaTurnos : MonoBehaviour {
    public static bool turnoJugador1 = true;
    public int turnoActual = 1;
    public Text textoTurno;
    public Text contadorTurno;
    public Jugador jugador1;

    void Start() {
        ActualizarUI();
    }

    public void terminarTurno() {
        turnoJugador1 = !turnoJugador1;
        turnoActual++;
        Debug.Log(turnoJugador1 ? "Turno del Jugador 1" : "Turno del Jugador 2");
        ActualizarUI();
    }

    private void ActualizarUI() {
        textoTurno.text = "Turno del Jugador " + (turnoJugador1 ? "1" : "2");
        contadorTurno.text = "Turno: " + turnoActual.ToString();
        if (turnoJugador1) {
            jugador1.addmana(3);
            jugador1.ActualizarMana();
        }
    }
}
