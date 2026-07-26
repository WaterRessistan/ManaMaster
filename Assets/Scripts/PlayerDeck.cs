using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour {
    public List<Cartas> baraja = new List<Cartas>();
    public int deckSize = 10;
    public GameObject cardInDeck1, cardInDeck2;

    void Start() {
        // 1) Limpiamos la baraja por si acaso
        baraja.Clear();

        // 2) Creamos una copia del mazo completo (sin el elemento 0 si es "None")
        List<Cartas> temp = new List<Cartas>(CardDatabase.cardList);
        if (temp.Count > 0) {
            temp.RemoveAt(0);
        }

        // 3) Barajado Fisher–Yates
        for (int i = temp.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            Cartas aux = temp[i];
            temp[i] = temp[j];
            temp[j] = aux;
        }

        // 4) Tomamos las deckSize primeras cartas (o todas si hay menos)
        int countToTake = Mathf.Min(deckSize, temp.Count);
        for (int i = 0; i < countToTake; i++) {
            baraja.Add(temp[i]);
        }

        // 5) Asignamos las dos primeras cartas al DisplayCard
        if (baraja.Count > 0) {
            cardInDeck1.GetComponent<DisplayCard>().displayId = baraja[0].id;
        }
        if (baraja.Count > 1) {
            cardInDeck2.GetComponent<DisplayCard>().displayId = baraja[1].id;
        }
    }

    void Update() {
        // Desactivamos cartas extras según deckSize
        if (deckSize == 1) {
            cardInDeck2.SetActive(false);
        }
        if (deckSize == 0) {
            cardInDeck1.SetActive(false);
            cardInDeck2.SetActive(false);
        }
    }
}
