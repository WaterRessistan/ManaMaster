using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using UnityEngine;

/// <summary>
/// Mazo de monstruos de un jugador y las cartas visibles en su mano.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). El barajado Fisher-Yates ya era correcto, pero su
/// resultado no se usaba: <c>DisplayCard</c> se sorteaba su propia carta al
/// arrancar. Además la copia del mazo era superficial, así que todas las cartas
/// compartían el mismo objeto de datos con la base de datos global.
///
/// Ahora el mazo se construye con <see cref="CardInstance"/> independientes a
/// partir del catálogo, y es el mazo quien decide qué se ve en la mano.
///
/// FASE 2: el mazo real es de 10 monstruos + 10 objetos con un máximo de 2
/// copias por carta, y lo definirá el jugador desde la pantalla de deckbuild.
/// Aquí se genera uno aleatorio solo para poder probar la escena.
/// </remarks>
public class PlayerDeck : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Si se deja vacío se busca el CardCatalog del CardDatabase de la escena.")]
    [SerializeField] private CardCatalog catalog;
    [SerializeField, Min(0)] private int deckSize = 10;

    [Header("Mano (2 cartas visibles)")]
    // Nombres conservados: la escena ya los tiene cableados.
    [SerializeField] private GameObject cardInDeck1;
    [SerializeField] private GameObject cardInDeck2;

    private readonly List<CardInstance> _baraja = new();

    /// <summary>Cartas que quedan por robar.</summary>
    public IReadOnlyList<CardInstance> Baraja => _baraja;

    public int CartasRestantes => _baraja.Count;
    public bool EstaVacia => _baraja.Count == 0;

    private void Start()
    {
        ConstruirBaraja();
        RepartirMano();
    }

    private void ConstruirBaraja()
    {
        _baraja.Clear();

        CardCatalog catalogo = ResolverCatalogo();
        if (catalogo == null || catalogo.Monsters.Count == 0)
        {
            Debug.LogError(
                "[PlayerDeck] No hay catálogo de cartas disponible: la baraja " +
                "se queda vacía.", this);
            return;
        }

        // Copia barajable de las definiciones disponibles.
        List<MonsterCardDefinition> disponibles = new(catalogo.Monsters);
        Barajar(disponibles);

        // Se instancia una CardInstance por carta: cada copia tiene su propia
        // vida y dañar una en partida ya no altera el asset compartido.
        for (int i = 0; i < deckSize; i++)
        {
            MonsterCardDefinition definicion = disponibles[i % disponibles.Count];
            if (definicion != null)
            {
                _baraja.Add(new CardInstance(definicion));
            }
        }
    }

    private CardCatalog ResolverCatalogo()
    {
        if (catalog != null)
        {
            return catalog;
        }

        CardDatabase database = FindFirstObjectByType<CardDatabase>();
        return database != null ? database.Catalog : null;
    }

    /// <summary>Reparte las dos cartas visibles de la mano.</summary>
    private void RepartirMano()
    {
        ColocarEnMano(cardInDeck1);
        ColocarEnMano(cardInDeck2);
    }

    /// <summary>
    /// Pone la siguiente carta de la baraja en el hueco indicado. Si la baraja
    /// está vacía, el hueco se desactiva.
    /// </summary>
    public void ColocarEnMano(GameObject hueco)
    {
        if (hueco == null)
        {
            return;
        }

        if (!hueco.TryGetComponent(out DisplayCard vista))
        {
            Debug.LogWarning(
                $"[PlayerDeck] '{hueco.name}' no tiene componente DisplayCard.", this);
            return;
        }

        if (EstaVacia)
        {
            vista.Limpiar();
            hueco.SetActive(false);
            return;
        }

        CardInstance siguiente = Robar();
        vista.Mostrar(siguiente);
        hueco.SetActive(true);
    }

    /// <summary>Saca la carta superior de la baraja.</summary>
    public CardInstance Robar()
    {
        if (EstaVacia)
        {
            return null;
        }

        CardInstance carta = _baraja[0];
        _baraja.RemoveAt(0);
        return carta;
    }

    private static void Barajar<T>(IList<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }
}
