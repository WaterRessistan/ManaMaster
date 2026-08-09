using System;
using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Las dos cartas de monstruo visibles de un jugador.
    /// </summary>
    /// <remarks>
    /// No hay fase de robo: al jugar una carta el hueco se rellena al momento
    /// desde el mazo (DESIGN.md §8). Los huecos son posiciones fijas y no una
    /// lista que se encoge, para que la carta que no has jugado no se te mueva
    /// de sitio en pantalla; cuando el mazo se agota, el hueco simplemente se
    /// queda vacio.
    /// </remarks>
    public sealed class Hand
    {
        /// <summary>Cartas visibles a la vez.</summary>
        public const int Capacity = 2;

        private readonly CardInstance[] _huecos = new CardInstance[Capacity];

        /// <summary>Carta en ese hueco, o null si esta vacio.</summary>
        public CardInstance this[int slot]
            => IsValidSlot(slot) ? _huecos[slot] : null;

        public int Count
        {
            get
            {
                int cartas = 0;
                foreach (CardInstance carta in _huecos)
                {
                    if (carta != null)
                    {
                        cartas++;
                    }
                }

                return cartas;
            }
        }

        public bool IsEmpty => Count == 0;

        public static bool IsValidSlot(int slot) => slot >= 0 && slot < Capacity;

        /// <summary>
        /// Rellena los huecos vacios robando del mazo. Devuelve cuantas cartas
        /// entraron.
        /// </summary>
        public int Refill(Deck mazo)
        {
            if (mazo == null)
            {
                throw new ArgumentNullException(nameof(mazo));
            }

            int repartidas = 0;

            for (int slot = 0; slot < _huecos.Length; slot++)
            {
                if (_huecos[slot] != null || mazo.IsEmpty)
                {
                    continue;
                }

                _huecos[slot] = mazo.Draw();
                repartidas++;
            }

            return repartidas;
        }

        /// <summary>
        /// Saca la carta de ese hueco y lo deja vacio. Devuelve null si el hueco
        /// ya estaba vacio o no existe.
        /// </summary>
        public CardInstance Take(int slot)
        {
            if (!IsValidSlot(slot))
            {
                return null;
            }

            CardInstance carta = _huecos[slot];
            _huecos[slot] = null;
            return carta;
        }
    }
}
