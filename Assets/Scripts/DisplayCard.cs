using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pinta una carta de monstruo en la interfaz.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). Antes este componente era a la vez vista y modelo:
/// duplicaba los ocho campos de la carta y, en <c>Start()</c>, se asignaba una
/// carta AL AZAR. Como <c>PlayerDeck</c> también escribía en <c>displayId</c>
/// desde su propio <c>Start()</c> y el orden entre componentes es indeterminado,
/// el barajado del mazo no llegaba nunca a verse.
///
/// Ahora es solo vista: no decide qué carta muestra, se la dan desde fuera.
/// </remarks>
public class DisplayCard : MonoBehaviour
{
    [Header("Referencias de UI")]
    // Nombres conservados: el prefab Carta ya los tiene cableados.
    [SerializeField] private Text textonombre;
    [SerializeField] private Text numataque;
    [SerializeField] private Text nummana;
    [SerializeField] private Text numcura;
    [SerializeField] private Text numvida;
    [SerializeField] private Image artImage;

    /// <summary>Monstruo representado, con su estado de partida.</summary>
    public CardInstance Carta { get; private set; }

    public bool TieneCarta => Carta != null;

    public int CosteMana => TieneCarta ? Carta.Definition.ManaCost : 0;

    /// <summary>Muestra una instancia concreta, conservando su vida actual.</summary>
    public void Mostrar(CardInstance instancia)
    {
        Carta = instancia;
        Refrescar();
    }

    /// <summary>Crea una instancia nueva a partir de la definición y la muestra.</summary>
    public void Mostrar(MonsterCardDefinition definicion)
    {
        Mostrar(definicion != null ? new CardInstance(definicion) : null);
    }

    public void Limpiar()
    {
        Carta = null;
        Refrescar();
    }

    /// <summary>Vuelve a leer el estado de la carta y actualiza los textos.</summary>
    public void Refrescar()
    {
        if (Carta == null)
        {
            SetTexto(textonombre, string.Empty);
            SetTexto(numataque, string.Empty);
            SetTexto(nummana, string.Empty);
            SetTexto(numcura, string.Empty);
            SetTexto(numvida, string.Empty);

            if (artImage != null)
            {
                artImage.sprite = null;
                artImage.enabled = false;
            }

            return;
        }

        IMonsterCard definicion = Carta.Definition;

        SetTexto(textonombre, definicion.DisplayName);
        SetTexto(numataque, definicion.Attack.ToString());
        SetTexto(nummana, definicion.ManaCost.ToString());
        SetTexto(numcura, definicion.HealPerTurn.ToString());
        SetTexto(numvida, Carta.CurrentHealth.ToString());

        // El arte es un Sprite, así que no cabe en IMonsterCard: el motor de
        // reglas se compila sin UnityEngine. Se lee de la definición concreta.
        Sprite arte = (definicion as CardDefinition)?.Artwork;

        if (artImage != null)
        {
            artImage.sprite = arte;
            artImage.enabled = arte != null;
        }
    }

    private static void SetTexto(Text campo, string valor)
    {
        if (campo != null)
        {
            campo.text = valor;
        }
    }
}
