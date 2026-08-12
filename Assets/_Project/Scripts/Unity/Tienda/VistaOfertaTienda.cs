using ManaMaster.Core.Util;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Sesion;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Tienda
{
    /// <summary>
    /// Una oferta de la tienda: nombre, precio y un boton "Comprar" que gasta
    /// diamantes de verdad.
    /// </summary>
    /// <remarks>
    /// <see cref="carta"/> vacio significa que esta oferta es el sobre: en vez
    /// de anadir una carta concreta, abre <see cref="GeneradorDeSobres"/>
    /// sobre <see cref="catalogo"/>. Una sola clase para las dos porque solo
    /// hay una oferta de sobre frente a una por cada carta del catalogo — no
    /// compensa una jerarquia para esta unica diferencia.
    /// </remarks>
    public sealed class VistaOfertaTienda : MonoBehaviour
    {
        [SerializeField] private Text nombre;
        [SerializeField] private Text precio;
        [SerializeField] private Button comprar;
        [SerializeField] private SesionDeJuego sesion;

        [Tooltip("Carta concreta que se compra. Vacio = esta oferta es el sobre.")]
        [SerializeField] private MonsterCardDefinition carta;

        [Tooltip("Solo hace falta si 'carta' esta vacio, para elegir las del sobre.")]
        [SerializeField] private CardCatalog catalogo;

        private int _precio;

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
            _precio = precioEnDiamantes;

            if (nombre != null)
            {
                nombre.text = etiqueta;
            }

            if (precio != null)
            {
                precio.text = $"{precioEnDiamantes} \U0001F48E";
            }
        }

        /// <summary>Conectado al boton.</summary>
        public void Comprar()
        {
            if (sesion == null || !sesion.TryGastarDiamantes(_precio))
            {
                Debug.Log($"[VistaOfertaTienda] No hay diamantes suficientes para " +
                          $"'{(nombre != null ? nombre.text : "?")}'.");
                return;
            }

            if (carta != null)
            {
                sesion.AnadirAColeccion(carta.CardId);
                return;
            }

            if (catalogo == null)
            {
                Debug.LogError("[VistaOfertaTienda] Sobre sin catalogo cableado.", this);
                return;
            }

            IRandom azar = new SystemRandom(System.Environment.TickCount);
            foreach (string cardId in GeneradorDeSobres.Abrir(catalogo, azar))
            {
                sesion.AnadirAColeccion(cardId);
            }
        }
    }
}
