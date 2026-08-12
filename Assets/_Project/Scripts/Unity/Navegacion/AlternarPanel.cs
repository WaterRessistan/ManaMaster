using UnityEngine;

namespace ManaMaster.Unity.Navegacion
{
    /// <summary>
    /// Enciende o apaga un panel, sin cambiar de escena.
    /// </summary>
    /// <remarks>
    /// Se usa para paneles que no son una pantalla propia del flujo (DESIGN.md
    /// §10 solo nombra Inicio, Tienda, Deckbuild y Duelo) pero necesitan
    /// mostrarse y ocultarse, como el aviso "Proximamente" de Opciones.
    /// </remarks>
    public sealed class AlternarPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        /// <summary>Conectado al boton que abre o cierra el panel.</summary>
        public void Alternar()
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }
    }
}
