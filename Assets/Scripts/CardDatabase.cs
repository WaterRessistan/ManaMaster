using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour 
{
    public static List<Cartas> cardList = new List<Cartas>();

    void Awake()
    {
        cardList.Add(new Cartas(0, "None", 0, 0, 0, false, 0, Resources.Load<Sprite>("tras")));
        cardList.Add(new Cartas(1, "Baraja", 1, 0, 1, false, 2, Resources.Load<Sprite>("m1")));
        cardList.Add(new Cartas(2, "Monstruo", 3, 0, 2, false, 5, Resources.Load<Sprite>("m2")));
        cardList.Add(new Cartas(3, "Monstruito", 2, 1, 4, false, 6, Resources.Load<Sprite>("m3")));
        cardList.Add(new Cartas(4, "Fantasma", 3, 0, 2, false, 4, Resources.Load<Sprite>("m4")));
        cardList.Add(new Cartas(5, "Link", 2, 1, 2, false, 8, Resources.Load<Sprite>("m5")));
        cardList.Add(new Cartas(6, "Zombie", 1, 0, 1, false, 2, Resources.Load<Sprite>("m6")));
        cardList.Add(new Cartas(7, "Fantasmon", 2, 1, 2, false, 3, Resources.Load<Sprite>("m7")));
        cardList.Add(new Cartas(8, "Golem", 3, 0, 2, false, 4, Resources.Load<Sprite>("m8")));
        cardList.Add(new Cartas(9, "Azul", 2, 1, 2, false, 8, Resources.Load<Sprite>("azul")));
        cardList.Add(new Cartas(10, "Perro rojo", 2, 1, 2, false, 8, Resources.Load<Sprite>("perrorojo")));
    }
}
