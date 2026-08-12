using System;
using System.Collections.Generic;
using UnityEngine;

namespace ManaMaster.Unity.Sesion
{
    /// <summary>
    /// Datos que cruzan de una escena a otra durante el flujo de menus.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Es el mismo mecanismo que <c>CardCatalog</c>: un asset unico que cada
    /// escena referencia por <c>[SerializeField]</c> y que el editor cablea al
    /// generarlas. No es un singleton <c>DontDestroyOnLoad</c> ni un campo
    /// <c>static</c> (CLAUDE.md prohibe el estado estatico mutable, que fue el
    /// problema central de la Fase 1).
    /// </para>
    /// <para>
    /// El mazo elegido vive en un campo <b>sin</b> <c>[SerializeField]</c> a
    /// proposito: Unity nunca lo escribe en el <c>.asset</c>, asi que no se
    /// ensucia jugando en el editor y se reinicia solo en cada recarga de
    /// dominio. Es la Fase 5, sin persistencia todavia (eso es la Fase 4); si
    /// algun dia se desactiva "Disable Domain Reload" en este proyecto, ese
    /// reinicio automatico deja de darse entre partidas manuales sucesivas del
    /// editor.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Mana Master/Sesion de juego", fileName = "SesionDeJuego")]
    public sealed class SesionDeJuego : ScriptableObject
    {
        private readonly List<string> _mazoHumano = new();

        /// <summary>CardId de las cartas elegidas en Deckbuild, en orden de eleccion.</summary>
        public IReadOnlyList<string> MazoHumano => _mazoHumano;

        /// <summary>Si hay un mazo elegido en esta sesion.</summary>
        public bool TieneMazoElegido => _mazoHumano.Count > 0;

        /// <summary>Guarda el mazo elegido en Deckbuild, sustituyendo el anterior.</summary>
        public void FijarMazoHumano(IEnumerable<string> cardIds)
        {
            if (cardIds == null)
            {
                throw new ArgumentNullException(nameof(cardIds));
            }

            _mazoHumano.Clear();
            _mazoHumano.AddRange(cardIds);
        }

        /// <summary>Olvida el mazo elegido, volviendo al reparto aleatorio.</summary>
        public void LimpiarMazoHumano() => _mazoHumano.Clear();
    }
}
