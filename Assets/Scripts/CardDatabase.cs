using ManaMaster.Unity.Cards;
using UnityEngine;

/// <summary>
/// Expone el catálogo de cartas a los componentes de la escena.
/// </summary>
/// <remarks>
/// TRANSITORIO (Fase 1). Antes este componente construía una
/// <c>static List&lt;Cartas&gt;</c> en <c>Awake()</c> con las 11 cartas escritas
/// a mano y cargaba los sprites por nombre desde <c>Resources</c>. Eso provocaba
/// tres fallos: la lista se duplicaba al recargar la escena (por ser estática),
/// tres rutas de sprite no existían y fallaban en silencio, y todas las copias
/// de una carta compartían el mismo objeto de datos.
///
/// Ahora las cartas son assets (<see cref="MonsterCardDefinition"/>) reunidos en
/// un <see cref="CardCatalog"/>, y este componente solo sirve de punto de acceso
/// desde la escena. En la Fase 2 lo sustituirá la inyección desde el
/// MatchController.
/// </remarks>
public class CardDatabase : MonoBehaviour
{
    [Tooltip("Asignar Assets/_Project/Content/Cards/CardCatalog.asset")]
    [SerializeField] private CardCatalog catalog;

    public CardCatalog Catalog => catalog;

    private void Awake()
    {
        if (catalog == null)
        {
            Debug.LogError(
                "[CardDatabase] No hay ningún CardCatalog asignado. Arrastra " +
                "Assets/_Project/Content/Cards/CardCatalog.asset al Inspector " +
                "de este componente.", this);
        }
    }
}
