using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using ManaMaster.Unity.Sesion;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Coleccion
{
    /// <summary>
    /// Una carta de monstruo en la pantalla de Coleccion: se atenua si el
    /// jugador no posee ninguna copia.
    /// </summary>
    /// <remarks>
    /// A diferencia de la Tienda (donde la plantilla es la misma para
    /// cualquiera y se hornea en el editor), la posesion es estado del
    /// jugador: tiene que leerse en tiempo real de <see cref="SesionDeJuego"/>
    /// en vez de hornearse al reconstruir la escena, o todo el mundo veria
    /// la coleccion de quien la reconstruyo por ultima vez.
    /// </remarks>
    public sealed class EntradaDeColeccionDeMonstruo : MonoBehaviour
    {
        [SerializeField] private MonsterCardDefinition definicion;
        [SerializeField] private SesionDeJuego sesion;
        [SerializeField] private VistaCartaMonstruo vista;
        [SerializeField] private Text copias;
        [SerializeField] private CanvasGroup atenuado;

        private void OnEnable()
        {
            if (vista == null || definicion == null)
            {
                return;
            }

            vista.Mostrar(new CardInstance(definicion));

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
