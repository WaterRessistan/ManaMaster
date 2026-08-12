using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Tienda
{
    /// <summary>
    /// Una oferta de la tienda: nombre, precio y un boton "Comprar".
    /// </summary>
    /// <remarks>
    /// Fase 5: solo navegacion. No hay diamantes de mentira que gastar ni
    /// coleccion que ampliar (eso es la Fase 4) — el clic no tiene efecto,
    /// solo deja constancia en el log de que se registro.
    /// </remarks>
    public sealed class VistaOfertaTienda : MonoBehaviour
    {
        [SerializeField] private Text nombre;
        [SerializeField] private Text precio;
        [SerializeField] private Button comprar;

        private void OnEnable()
        {
            if (comprar != null)
            {
                comprar.onClick.AddListener(Comprar);
            }
        }

        private void OnDisable()
        {
            if (comprar != null)
            {
                comprar.onClick.RemoveListener(Comprar);
            }
        }

        public void Mostrar(string etiqueta, int precioEnDiamantes)
        {
            if (nombre != null)
            {
                nombre.text = etiqueta;
            }

            if (precio != null)
            {
                precio.text = $"{precioEnDiamantes} \U0001F48E";
            }
        }

        /// <summary>Conectado al boton. Todavia sin efecto real (Fase 4).</summary>
        public void Comprar()
        {
            Debug.Log($"[VistaOfertaTienda] Comprar '{(nombre != null ? nombre.text : "?")}': " +
                      "sin efecto todavia, la economia llega en la Fase 4.");
        }
    }
}
