using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour
{
    public int displayId;

    [Header("Card Data")]
    public int id;
    public int ataque;
    public int curar;
    public int costemana;
    public bool rango;
    public int vida;
    public string nombre;
    public Sprite spriteImage;

    [Header("UI References")]
    public Text textonombre;
    public Text numataque;
    public Text nummana;
    public Text numcura;
    public Text numvida;
    public Image artImage;

    private Cartas currentCard;

    void Start()
    {
        if(CardDatabase.cardList.Count > 0) displayId = Random.Range(0, CardDatabase.cardList.Count);
        if (CardDatabase.cardList != null && CardDatabase.cardList.Count > displayId)
        {
            currentCard = CardDatabase.cardList[displayId];
            UpdateDisplay();
        }
        else
        {
            Debug.LogWarning("CardDatabase.cardList is null or displayId is out of range.");
        }
    }

    private void UpdateDisplay()
    {
        if (currentCard == null) return;

        id = currentCard.id;
        ataque = currentCard.ataque;
        curar = currentCard.curar;
        costemana = currentCard.costemana;
        rango = currentCard.rango;
        vida = currentCard.vida;
        nombre = currentCard.nombre;
        spriteImage = currentCard.spriteImage;

        // textonombre.text = " " + nombre; // Uncomment if you want to show the name
        if (numataque) numataque.text = " " + ataque;
        if (nummana) nummana.text = " " + costemana;
        if (numcura) numcura.text = " " + curar;
        if (numvida) numvida.text = " " + vida;
        if (artImage) artImage.sprite = spriteImage;
    }
}
