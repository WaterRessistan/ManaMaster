using UnityEngine;

public class Tablero : MonoBehaviour
{
    public BoardSlot boardSlotPrincipal;
    public BoardSlot boardSlotSecundario;
    public BoardSlot boardSlotTerciario;

    void Start()
    {
        Debug.Log("Tablero inicializado");
    }

    void Update()
    {

    }

    private void OrganizarTablero()
    {
        if (!boardSlotPrincipal.IsOccupied()) {
            //implementar logica para que este slot siempre este ocupado
        }
    }

}
