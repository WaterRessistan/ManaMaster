using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using ManaMaster.Unity.Sesion;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Coleccion
{
    /// <summary>
    /// Una carta de objeto en la pantalla de Coleccion: mismo papel que
    /// <see cref="EntradaDeColeccionDeMonstruo"/>, pero para objetos.
    /// </summary>
    public sealed class EntradaDeColeccionDeObjeto : MonoBehaviour
    {
        [SerializeField] private ItemCardDefinition definicion;
        [SerializeField] private SesionDeJuego sesion;
        [SerializeField] private VistaCartaObjeto vista;
        [SerializeField] private Text copias;
        [SerializeField] private CanvasGroup atenuado;

        private void OnEnable()
        {
            if (vista == null || definicion == null)
            {
                return;
            }

            vista.Mostrar(definicion);

            int poseidas = sesion != null ? sesion.CopiasEnColeccion(definicion.CardId) : 0;

            if (copias != null)
            {
                copias.text = $"{poseidas}/{ConstructorDeMazos.MaxCopiasPorCarta}";
            }

            if (atenuado != null)
            {
                atenuado.alpha = poseidas > 0 ? 1f : 0.35f;
            }
        }
    }
}
