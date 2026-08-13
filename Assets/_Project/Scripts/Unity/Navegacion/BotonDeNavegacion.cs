using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Navegacion
{
    /// <summary>
    /// Boton que carga otra escena del flujo (Inicio, Tienda, Deckbuild,
    /// Coleccion, Duelo).
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

        // Con el fundido de por medio hay una ventana de tiempo entre el
        // clic y la carga real de la escena; sin esta bandera, un segundo
        // clic en esa ventana arrancaba una segunda corrutina de fundido.
        private bool _yendo;

        private void Awake()
        {
            _boton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _yendo = false;
            _boton.onClick.AddListener(Ir);
        }

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

            if (_yendo)
            {
                return;
            }

            _yendo = true;
            Navegador.Ir(nombreEscena);
        }
    }
}
