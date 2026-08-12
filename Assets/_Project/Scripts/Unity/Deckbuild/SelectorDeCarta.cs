using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Deckbuild
{
    /// <summary>
    /// Una carta del catalogo en la rejilla de deckbuild, con sus copias
    /// elegidas (0 a 2).
    /// </summary>
    /// <remarks>
    /// Reutiliza <see cref="VistaCartaMonstruo"/> para dibujarla, que exige una
    /// <see cref="CardInstance"/>. Como aqui no hay ninguna partida en curso de
    /// la que sacarla, se crea una desechable en <see cref="Awake"/> solo para
    /// alimentar la vista: esta clase no representa ningun monstruo en juego,
    /// solo la plantilla del catalogo.
    /// </remarks>
    public sealed class SelectorDeCarta : MonoBehaviour
    {
        [SerializeField] private MonsterCardDefinition definicion;
        [SerializeField] private ControladorDeckbuild controlador;
        [SerializeField] private VistaCartaMonstruo vista;
        [SerializeField] private Text copias;
        [SerializeField] private Button anadir;
        [SerializeField] private Button quitar;

        /// <summary>CardId de la carta representada, o null si no se cableo ninguna.</summary>
        public string CardId => definicion != null ? definicion.CardId : null;

        private void Awake()
        {
            if (vista != null && definicion != null)
            {
                vista.Mostrar(new CardInstance(definicion));
            }
        }

        private void OnEnable()
        {
            if (controlador != null)
            {
                controlador.SeleccionCambiada += Refrescar;
            }

            if (anadir != null)
            {
                anadir.onClick.AddListener(AlPulsarAnadir);
            }

            if (quitar != null)
            {
                quitar.onClick.AddListener(AlPulsarQuitar);
            }

            Refrescar();
        }

        private void OnDisable()
        {
            if (controlador != null)
            {
                controlador.SeleccionCambiada -= Refrescar;
            }

            if (anadir != null)
            {
                anadir.onClick.RemoveListener(AlPulsarAnadir);
            }

            if (quitar != null)
            {
                quitar.onClick.RemoveListener(AlPulsarQuitar);
            }
        }

        /// <summary>Conectado al boton de anadir.</summary>
        public void AlPulsarAnadir()
        {
            if (controlador != null && definicion != null)
            {
                controlador.Anadir(definicion.CardId);
            }
        }

        /// <summary>Conectado al boton de quitar.</summary>
        public void AlPulsarQuitar()
        {
            if (controlador != null && definicion != null)
            {
                controlador.Quitar(definicion.CardId);
            }
        }

        private void Refrescar()
        {
            if (copias == null || controlador == null || definicion == null)
            {
                return;
            }

            int poseidas = controlador.Sesion != null
                ? controlador.Sesion.CopiasEnColeccion(definicion.CardId)
                : 0;

            copias.text =
                $"{controlador.Copias(definicion.CardId)}/{ConstructorDeMazos.MaxCopiasPorCarta} " +
                $"(posees {poseidas})";
        }
    }
}
