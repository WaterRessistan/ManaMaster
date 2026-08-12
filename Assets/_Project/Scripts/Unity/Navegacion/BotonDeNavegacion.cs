using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManaMaster.Unity.Navegacion
{
    /// <summary>
    /// Boton que carga otra escena del flujo (Inicio, Tienda, Deckbuild, Duelo).
    /// </summary>
    /// <remarks>
    /// El nombre de la escena se cablea al generar la interfaz, igual que el
    /// resto de referencias del proyecto (ver <c>ConstructorDeInterfaz</c>): no
    /// hay un enum de pantallas porque anadir una pantalla nueva no debe tocar
    /// codigo de las demas.
    /// </remarks>
    [RequireComponent(typeof(Button))]
    public sealed class BotonDeNavegacion : MonoBehaviour
    {
        [Tooltip("Nombre de la escena a cargar, tal como esta en Build Settings.")]
        [SerializeField] private string nombreEscena;

        public string NombreEscena => nombreEscena;

        private Button _boton;

        private void Awake()
        {
            _boton = GetComponent<Button>();
        }

        private void OnEnable() => _boton.onClick.AddListener(Ir);

        private void OnDisable() => _boton.onClick.RemoveListener(Ir);

        /// <summary>Conectado al boton.</summary>
        public void Ir()
        {
            if (string.IsNullOrEmpty(nombreEscena))
            {
                Debug.LogError(
                    "[BotonDeNavegacion] No se ha cableado el nombre de la escena.",
                    this);
                return;
            }

            SceneManager.LoadScene(nombreEscena);
        }
    }
}
