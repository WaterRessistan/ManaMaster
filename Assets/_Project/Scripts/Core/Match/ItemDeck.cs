using System;
using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Util;

namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Las cartas de objeto que a un jugador le quedan por robar.
    /// </summary>
    /// <remarks>
    /// Mismo patron que <see cref="Deck"/>, pero de <see cref="IItemCard"/>
    /// directo: a diferencia de un monstruo, un objeto no tiene estado mutable
    /// propio (nada como la vida actual), asi que no hace falta envolverlo en
    /// una instancia por copia — dos copias del mismo objeto son la misma
    /// referencia repetida en la lista, y eso no es un problema porque cada
    /// una se puede equipar en un monstruo distinto igualmente.
    /// </remarks>
    public sealed class ItemDeck
    {
        private readonly List<IItemCard> _objetos;

        public ItemDeck(IEnumerable<IItemCard> objetos)
        {
            if (objetos == null)
            {
                throw new ArgumentNullException(nameof(objetos));
            }

            _objetos = new List<IItemCard>(objetos);

            if (_objetos.Contains(null))
            {
                throw new ArgumentException(
                    "El mazo de objetos no puede tener huecos vacios.", nameof(objetos));
            }
        }

        public int Count => _objetos.Count;

        public bool IsEmpty => _objetos.Count == 0;

        /// <summary>Objetos restantes, de arriba abajo.</summary>
        public IReadOnlyList<IItemCard> Objetos => _objetos;

        /// <summary>Saca el objeto de arriba, o null si el mazo esta vacio.</summary>
        public IItemCard Draw()
        {
            if (IsEmpty)
            {
                return null;
            }

            IItemCard objeto = _objetos[0];
            _objetos.RemoveAt(0);
            return objeto;
        }

        /// <summary>Baraja el mazo con Fisher-Yates.</summary>
        public void Shuffle(IRandom random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            for (int i = _objetos.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_objetos[i], _objetos[j]) = (_objetos[j], _objetos[i]);
            }
        }
    }
}
