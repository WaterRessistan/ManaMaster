using System;
using System.Collections.Generic;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Sesion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManaMaster.Unity.Deckbuild
{
    /// <summary>
    /// La seleccion de monstruos en curso, en memoria hasta que se guarda
    /// (DESIGN.md §8).
    /// </summary>
    /// <remarks>
    /// El lado de objetos del mazo no esta aqui: no hay cartas de objeto
    /// todavia en el catalogo (DESIGN.md §13), asi que ese hueco queda
    /// deshabilitado en la escena hasta que exista contenido de la Fase 6/7.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class ControladorDeckbuild : MonoBehaviour
    {
        [SerializeField] private CardCatalog catalogo;
        [SerializeField] private SesionDeJuego sesion;

        private readonly List<string> _elegidas = new();

        /// <summary>La seleccion ha cambiado y las vistas deben redibujar.</summary>
        public event Action SeleccionCambiada;

        /// <summary>Sesion cableada, o null si no se cableo ninguna.</summary>
        public SesionDeJuego Sesion => sesion;

        public int Total => _elegidas.Count;

        public bool PuedeGuardar => Total == ConstructorDeMazos.CartasPorMazo;

        public int Copias(string cardId)
        {
            int copias = 0;
            foreach (string elegido in _elegidas)
            {
                if (elegido == cardId)
                {
                    copias++;
                }
            }

            return copias;
        }

        /// <summary>
        /// Anade una copia de la carta si existe en el catalogo, queda hueco en
        /// el mazo y no se supera el maximo de copias.
        /// </summary>
        public bool Anadir(string cardId)
        {
            if (catalogo == null || catalogo.FindMonster(cardId) == null)
            {
                return false;
            }

            if (Total >= ConstructorDeMazos.CartasPorMazo)
            {
                return false;
            }

            if (Copias(cardId) >= ConstructorDeMazos.MaxCopiasPorCarta)
            {
                return false;
            }

            _elegidas.Add(cardId);
            SeleccionCambiada?.Invoke();
            return true;
        }

        /// <summary>Quita una copia elegida de la carta, si habia alguna.</summary>
        public bool Quitar(string cardId)
        {
            if (!_elegidas.Remove(cardId))
            {
                return false;
            }

            SeleccionCambiada?.Invoke();
            return true;
        }

        /// <summary>
        /// Con el mazo completo, lo guarda en la sesion y vuelve al menu. Sin
        /// mazo completo no hace nada.
        /// </summary>
        public void Guardar()
        {
            if (!PuedeGuardar || sesion == null)
            {
                return;
            }

            sesion.FijarMazoHumano(_elegidas);
            SceneManager.LoadScene("Inicio");
        }
    }
}
