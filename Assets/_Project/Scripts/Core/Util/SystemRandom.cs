using System;

namespace ManaMaster.Core.Util
{
    /// <summary>
    /// Implementacion de <see cref="IRandom"/> sobre <see cref="Random"/>.
    /// </summary>
    /// <remarks>
    /// Acepta una semilla para poder repetir una partida tal cual, que es lo que
    /// hace falta para reproducir un fallo o para las simulaciones de balanceo.
    /// </remarks>
    public sealed class SystemRandom : IRandom
    {
        private readonly Random _random;

        public SystemRandom()
            : this(Environment.TickCount)
        {
        }

        public SystemRandom(int seed)
        {
            Seed = seed;
            _random = new Random(seed);
        }

        /// <summary>Semilla con la que se creo, para poder repetir la partida.</summary>
        public int Seed { get; }

        public int Next(int maxExclusive)
            => maxExclusive <= 0 ? 0 : _random.Next(maxExclusive);
    }
}
