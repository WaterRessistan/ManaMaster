using ManaMaster.Unity.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Deckbuild
{
    /// <summary>
    /// Contador "X/10" y estado del boton de guardar de la pantalla de
    /// deckbuild.
    /// </summary>
    public sealed class VistaSeleccionMazo : MonoBehaviour
    {
        [SerializeField] private ControladorDeckbuild controlador;
        [SerializeField] private Text contador;
        [SerializeField] private Button guardar;

        private void OnEnable()
        {
            if (controlador != null)
            {
                controlador.SeleccionCambiada += Refrescar;
            }

            if (guardar != null)
            {
                guardar.onClick.AddListener(AlPulsarGuardar);
            }

            Refrescar();
        }

        private void OnDisable()
        {
            if (controlador != null)
            {
                controlador.SeleccionCambiada -= Refrescar;
            }

            if (guardar != null)
            {
                guardar.onClick.RemoveListener(AlPulsarGuardar);
            }
        }

        private void AlPulsarGuardar() => controlador?.Guardar();

        private void Refrescar()
        {
            if (controlador == null)
            {
                return;
            }

            if (contador != null)
            {
                contador.text = $"{controlador.Total}/{ConstructorDeMazos.CartasPorMazo}";
            }

            if (guardar != null)
            {
                guardar.interactable = controlador.PuedeGuardar;
            }
        }
    }
}
