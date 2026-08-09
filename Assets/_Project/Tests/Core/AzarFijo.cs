using System.Collections.Generic;
using ManaMaster.Core.Util;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Azar de mentira que devuelve los valores que le digas, en orden.
    /// </summary>
    /// <remarks>
    /// Cuando se le acaban los valores vuelve a empezar. Si no se le da
    /// ninguno, devuelve siempre 0.
    /// </remarks>
    internal sealed class AzarFijo : IRandom
    {
        private readonly IReadOnlyList<int> _valores;
        private int _siguiente;

        public AzarFijo(params int[] valores)
        {
            _valores = valores == null || valores.Length == 0
                ? new[] { 0 }
                : valores;
        }

        /// <summary>Cuantas veces se le ha pedido un numero.</summary>
        public int Llamadas { get; private set; }

        public int Next(int maxExclusive)
        {
            Llamadas++;

            if (maxExclusive <= 0)
            {
                return 0;
            }

            int valor = _valores[_siguiente % _valores.Count];
            _siguiente++;

            return valor % maxExclusive;
        }
    }
}
