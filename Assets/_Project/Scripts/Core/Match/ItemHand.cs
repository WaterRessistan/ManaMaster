using System;
using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Las dos cartas de objeto visibles de un jugador.
    /// </summary>
    /// <remarks>
    /// Mismo patron que <see cref="Hand"/>: al usar un objeto el hueco se
    /// rellena al momento desde <see cref="ItemDeck"/> (DESIGN.md §8).
    /// </remarks>
    public sealed class ItemHand
    {
        /// <summary>Objetos visibles a la vez.</summary>
        public const int Capacity = 2;

        private readonly IItemCard[] _huecos = new IItemCard[Capacity];

        /// <summary>Objeto en ese hueco, o null si esta vacio.</summary>
        public IItemCard this[int slot]
            => IsValidSlot(slot) ? _huecos[slot] : null;

        public int Count
        {
            get
            {
                int objetos = 0;
                foreach (IItemCard objeto in _huecos)
                {
                    if (objeto != null)
                    {
                        objetos++;
                    }
                }

                return objetos;
            }
        }

        public bool IsEmpty => Count == 0;

        public static bool IsValidSlot(int slot) => slot >= 0 && slot < Capacity;

        /// <summary>
        /// Rellena los huecos vacios robando del mazo. Devuelve cuantos
        /// objetos entraron.
        /// </summary>
        public int Refill(ItemDeck mazo)
        {
            if (mazo == null)
            {
                throw new ArgumentNullException(nameof(mazo));
            }

            int repartidos = 0;

            for (int slot = 0; slot < _huecos.Length; slot++)
            {
                if (_huecos[slot] != null || mazo.IsEmpty)
                {
                    continue;
                }

                _huecos[slot] = mazo.Draw();
                repartidos++;
            }

            return repartidos;
        }

        /// <summary>
        /// Saca el objeto de ese hueco y lo deja vacio. Devuelve null si el
        /// hueco ya estaba vacio o no existe.
        /// </summary>
        public IItemCard Take(int slot)
        {
            if (!IsValidSlot(slot))
            {
                return null;
            }

            IItemCard objeto = _huecos[slot];
            _huecos[slot] = null;
            return objeto;
        }
    }
}
