using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Deckbuild
{
    /// <summary>
    /// Un objeto del catalogo en la rejilla de deckbuild, con sus copias
    /// elegidas (0 a 2).
    /// </summary>
    /// <remarks>
    /// Mismo papel que <see cref="SelectorDeCarta"/> para monstruos, pero mas
    /// simple: <see cref="VistaCartaObjeto"/> ya acepta la definicion
    /// directamente (un objeto no tiene estado de partida como la vida, asi
    /// que no hace falta ninguna instancia desechable).
    /// </remarks>
    public sealed class SelectorDeObjeto : MonoBehaviour
    {
        [SerializeField] private ItemCardDefinition definicion;
        [SerializeField] private ControladorDeckbuild controlador;
        [SerializeField] private VistaCartaObjeto vista;
        [SerializeField] private Text copias;
        [SerializeField] private Button anadir;
        [SerializeField] private Button quitar;

        /// <summary>CardId del objeto representado, o null si no se cableo ninguno.</summary>
        public string CardId => definicion != null ? definicion.CardId : null;

        private void Awake()
        {
            if (vista != null && definicion != null)
            {
                vista.Mostrar(definicion);
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
                controlador.AnadirObjeto(definicion.CardId);
            }
        }

        /// <summary>Conectado al boton de quitar.</summary>
        public void AlPulsarQuitar()
        {
            if (controlador != null && definicion != null)
            {
                controlador.QuitarObjeto(definicion.CardId);
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
                $"{controlador.CopiasObjeto(definicion.CardId)}/{ConstructorDeMazos.MaxCopiasPorCarta} " +
                $"(posees {poseidas})";
        }
    }
}
