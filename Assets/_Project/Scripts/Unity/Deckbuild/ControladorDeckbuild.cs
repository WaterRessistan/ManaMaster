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
    /// Desde la Fase 4, <see cref="Anadir"/> tambien exige poseer la carta:
    /// una coleccion que no restringe nada no seria una coleccion. Desde la
    /// Fase 7, la seleccion de objetos vive aqui tambien, con las mismas
    /// reglas que la de monstruos (existe en el catalogo, se posee, maximo 2
    /// copias, maximo 10 en total): el mazo 10+10 de DESIGN.md §8 solo se
    /// puede guardar con las dos mitades completas.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class ControladorDeckbuild : MonoBehaviour
    {
        [SerializeField] private CardCatalog catalogo;
        [SerializeField] private SesionDeJuego sesion;

        private readonly List<string> _elegidas = new();
        private readonly List<string> _elegidasObjetos = new();

        /// <summary>La seleccion ha cambiado y las vistas deben redibujar.</summary>
        public event Action SeleccionCambiada;

        /// <summary>Sesion cableada, o null si no se cableo ninguna.</summary>
        public SesionDeJuego Sesion => sesion;

        public int Total => _elegidas.Count;

        public int TotalObjetos => _elegidasObjetos.Count;

        public bool PuedeGuardar
            => Total == ConstructorDeMazos.CartasPorMazo
               && TotalObjetos == ConstructorDeMazos.CartasPorMazoDeObjetos;

        public int Copias(string cardId) => Contar(_elegidas, cardId);

        public int CopiasObjeto(string cardId) => Contar(_elegidasObjetos, cardId);

        private static int Contar(List<string> elegidas, string cardId)
        {
            int copias = 0;
            foreach (string elegido in elegidas)
            {
                if (elegido == cardId)
                {
                    copias++;
                }
            }

            return copias;
        }

        /// <summary>
        /// Anade una copia de la carta si existe en el catalogo, el jugador la
        /// posee, queda hueco en el mazo y no se supera el maximo de copias.
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

            if (sesion != null && Copias(cardId) >= sesion.CopiasEnColeccion(cardId))
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
        /// Anade una copia de objeto si existe en el catalogo, el jugador lo
        /// posee, queda hueco en el mazo de objetos y no se supera el maximo
        /// de copias. Mismas reglas que <see cref="Anadir"/>, para el otro
        /// lado del mazo 10+10.
        /// </summary>
        public bool AnadirObjeto(string cardId)
        {
            if (catalogo == null || catalogo.FindItem(cardId) == null)
            {
                return false;
            }

            if (TotalObjetos >= ConstructorDeMazos.CartasPorMazoDeObjetos)
            {
                return false;
            }

            if (CopiasObjeto(cardId) >= ConstructorDeMazos.MaxCopiasPorCarta)
            {
                return false;
            }

            if (sesion != null && CopiasObjeto(cardId) >= sesion.CopiasEnColeccion(cardId))
            {
                return false;
            }

            _elegidasObjetos.Add(cardId);
            SeleccionCambiada?.Invoke();
            return true;
        }

        /// <summary>Quita una copia elegida de objeto, si habia alguna.</summary>
        public bool QuitarObjeto(string cardId)
        {
            if (!_elegidasObjetos.Remove(cardId))
            {
                return false;
            }

            SeleccionCambiada?.Invoke();
            return true;
        }

        /// <summary>
        /// Con el mazo 10+10 completo, lo guarda en la sesion y vuelve al
        /// menu. Sin las dos mitades completas no hace nada.
        /// </summary>
        public void Guardar()
        {
            if (!PuedeGuardar || sesion == null)
            {
                return;
            }

            sesion.FijarMazoHumano(_elegidas);
            sesion.FijarMazoObjetos(_elegidasObjetos);
            SceneManager.LoadScene("Inicio");
        }
    }
}
