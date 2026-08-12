using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Sesion
{
    /// <summary>Muestra el saldo de diamantes y se refresca cuando cambia.</summary>
    public sealed class VistaDiamantes : MonoBehaviour
    {
        [SerializeField] private SesionDeJuego sesion;
        [SerializeField] private Text texto;

        private void OnEnable()
        {
            if (sesion != null)
            {
                sesion.Cambiada += Refrescar;
            }

            Refrescar();
        }

        private void OnDisable()
        {
            if (sesion != null)
            {
                sesion.Cambiada -= Refrescar;
            }
        }

        private void Refrescar()
        {
            if (texto != null && sesion != null)
            {
                texto.text = $"{sesion.Diamantes} \U0001F48E";
            }
        }
    }
}
