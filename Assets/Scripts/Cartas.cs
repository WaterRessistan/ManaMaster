using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class Cartas
{
    public int id;
    public int ataque;
    public int curar;
    public int costemana;
    public bool rango;
    public int vida;
    public string nombre;
    public Sprite spriteImage;

    public Cartas(){}

    public Cartas(int iden, string name, int atack, int cura, int costmana, bool range, int health, Sprite img)
    {
        id = iden;    
        nombre = name;
        ataque = atack;
        curar = cura;
        costemana = costmana;
        rango = range;
        vida = health;
        spriteImage = img;
    }
}
