using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Sesion;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Coleccion
{
    /// <summary>Cabecera con cuantas cartas distintas posee el jugador.</summary>
    public sealed class ResumenDeColeccion : MonoBehaviour
    {
        [SerializeField] private CardCatalog catalogo;
        [SerializeField] private SesionDeJuego sesion;
        [SerializeField] private Text texto;

        private void OnEnable()
        {
            if (texto == null || catalogo == null)
            {
                return;
            }

            int monstruos = ContarPoseidas(catalogo.Monsters);
            int objetos = ContarPoseidas(catalogo.Items);

            texto.text = $"{monstruos}/{catalogo.Monsters.Count} monstruos · " +
                         $"{objetos}/{catalogo.Items.Count} objetos";
        }

        private int ContarPoseidas(System.Collections.Generic.IReadOnlyList<CardDefinition> cartas)
        {
            if (sesion == null)
            {
                return 0;
            }

            int total = 0;
            foreach (CardDefinition carta in cartas)
            {
                if (carta != null && sesion.CopiasEnColeccion(carta.CardId) > 0)
                {
                    total++;
                }
            }

            return total;
        }
    }
}
