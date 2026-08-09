using System;
using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Util;

namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Las cartas de monstruo que a un jugador le quedan por robar.
    /// </summary>
    /// <remarks>
    /// Se roba siempre por arriba. Que la carta que entra en la mano sea
    /// "aleatoria" (DESIGN.md §8) sale de haber barajado el mazo al empezar, no
    /// de sortear en cada robo: asi una partida con la misma semilla se repite
    /// exactamente igual.
    /// </remarks>
    public sealed class Deck
    {
        private readonly List<CardInstance> _cartas;

        public Deck(IEnumerable<CardInstance> cartas)
        {
            if (cartas == null)
            {
                throw new ArgumentNullException(nameof(cartas));
            }

            _cartas = new List<CardInstance>(cartas);

            if (_cartas.Contains(null))
            {
                throw new ArgumentException(
                    "El mazo no puede tener huecos vacios.", nameof(cartas));
            }
        }

        public int Count => _cartas.Count;

        public bool IsEmpty => _cartas.Count == 0;

        /// <summary>Cartas restantes, de arriba abajo.</summary>
        public IReadOnlyList<CardInstance> Cartas => _cartas;

        /// <summary>Saca la carta de arriba, o null si el mazo esta vacio.</summary>
        public CardInstance Draw()
        {
            if (IsEmpty)
            {
                return null;
            }

            CardInstance carta = _cartas[0];
            _cartas.RemoveAt(0);
            return carta;
        }

        /// <summary>Baraja el mazo con Fisher-Yates.</summary>
        public void Shuffle(IRandom random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            for (int i = _cartas.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_cartas[i], _cartas[j]) = (_cartas[j], _cartas[i]);
            }
        }
    }
}
