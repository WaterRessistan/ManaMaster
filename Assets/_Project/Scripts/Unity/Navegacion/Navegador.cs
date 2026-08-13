using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManaMaster.Unity.Navegacion
{
    /// <summary>
    /// Punto unico para cambiar de escena, con el fundido de
    /// <see cref="VistaTransicionDePantalla"/> si la escena actual tiene una.
    /// </summary>
    /// <remarks>
    /// Antes cada sitio que navegaba llamaba a <c>SceneManager.LoadScene</c>
    /// por su cuenta (<c>BotonDeNavegacion</c> y
    /// <c>ControladorDeckbuild.Guardar</c>); con el fundido de por medio, los
    /// dos delegan aqui. La corrutina del fundido la arranca la propia
    /// <see cref="VistaTransicionDePantalla"/> sobre si misma, no sobre quien
    /// llama: no hace falta que el que llama sea un GameObject activo.
    /// </remarks>
    public static class Navegador
    {
        public static void Ir(string escena)
        {
            VistaTransicionDePantalla transicion =
                Object.FindFirstObjectByType<VistaTransicionDePantalla>();

            if (transicion != null)
            {
                transicion.IrA(escena);
            }
            else
            {
                SceneManager.LoadScene(escena);
            }
        }
    }
}
