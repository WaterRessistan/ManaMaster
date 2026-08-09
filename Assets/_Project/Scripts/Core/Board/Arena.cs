using System;
using System.Collections.Generic;
using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Board
{
    /// <summary>
    /// Los tres carriles desplegados de un jugador.
    /// </summary>
    /// <remarks>
    /// <para>
    /// INVARIANTE: la arena nunca tiene huecos (DESIGN.md §2). Si hay N
    /// monstruos, ocupan exactamente los carriles 0..N-1. Todo lo que pueda
    /// abrir un hueco (una muerte, un sacrificio) lo cierra al momento.
    /// </para>
    /// <para>
    /// Es el nucleo de las dos mecanicas que dan forma al juego: la insercion
    /// con empuje, que es la unica manera de sacar a un monstruo de rango del
    /// carril principal, y la compactacion, que hace que matar en el carril 1
    /// adelante a la carta de detras y cambie el objetivo del siguiente
    /// atacante (DESIGN.md §3 y §6).
    /// </para>
    /// </remarks>
    public sealed class Arena
    {
        private readonly CardInstance[] _carriles = new CardInstance[BoardLanes.Count];

        /// <summary>Monstruos desplegados.</summary>
        public int Count
        {
            get
            {
                int ocupados = 0;
                foreach (CardInstance monstruo in _carriles)
                {
                    if (monstruo != null)
                    {
                        ocupados++;
                    }
                }

                return ocupados;
            }
        }

        public bool IsEmpty => Count == 0;

        public bool IsFull => Count == BoardLanes.Count;

        /// <summary>Monstruo en ese carril, o null si esta libre.</summary>
        public CardInstance this[int lane]
            => BoardLanes.IsValid(lane) ? _carriles[lane] : null;

        /// <summary>
        /// Monstruos desplegados en orden de carril, sin huecos.
        /// </summary>
        public IReadOnlyList<CardInstance> Desplegados
        {
            get
            {
                List<CardInstance> desplegados = new(BoardLanes.Count);
                foreach (CardInstance monstruo in _carriles)
                {
                    if (monstruo != null)
                    {
                        desplegados.Add(monstruo);
                    }
                }

                return desplegados;
            }
        }

        /// <summary>
        /// Indica si se puede insertar en ese carril.
        /// </summary>
        /// <remarks>
        /// Las posiciones validas van de 0 a <see cref="Count"/> incluido: se
        /// puede colocar delante de cualquier monstruo o justo detras del
        /// ultimo, pero nunca dejando un hueco por delante (DESIGN.md §3).
        /// </remarks>
        public bool CanInsertAt(int lane)
            => !IsFull && lane >= 0 && lane <= Count && BoardLanes.IsValid(lane);

        /// <summary>
        /// Coloca un monstruo empujando hacia atras lo que hubiera de ese carril
        /// en adelante.
        /// </summary>
        /// <exception cref="InvalidOperationException">La arena esta llena.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// El carril dejaria un hueco o no existe.
        /// </exception>
        public void Insert(int lane, CardInstance monstruo)
        {
            if (monstruo == null)
            {
                throw new ArgumentNullException(nameof(monstruo));
            }

            if (IsFull)
            {
                throw new InvalidOperationException(
                    "La arena ya tiene los tres monstruos desplegados.");
            }

            if (!CanInsertAt(lane))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lane),
                    lane,
                    $"Posicion de insercion invalida: con {Count} monstruos " +
                    $"desplegados solo valen los carriles 0 a {Count}.");
            }

            // Se captura antes del bucle: al escribir en el array, Count cambia.
            int ocupados = Count;
            for (int destino = ocupados; destino > lane; destino--)
            {
                _carriles[destino] = _carriles[destino - 1];
            }

            _carriles[lane] = monstruo;
        }

        /// <summary>Carril en el que esta ese monstruo, o -1 si no esta.</summary>
        public int IndexOf(CardInstance monstruo)
        {
            if (monstruo == null)
            {
                return -1;
            }

            for (int lane = 0; lane < _carriles.Length; lane++)
            {
                if (ReferenceEquals(_carriles[lane], monstruo))
                {
                    return lane;
                }
            }

            return -1;
        }

        public bool Contains(CardInstance monstruo) => IndexOf(monstruo) >= 0;

        /// <summary>
        /// Saca el monstruo de ese carril y cierra el hueco. Devuelve el
        /// monstruo retirado, o null si el carril estaba libre.
        /// </summary>
        public CardInstance RemoveAt(int lane)
        {
            if (!BoardLanes.IsValid(lane))
            {
                return null;
            }

            CardInstance retirado = _carriles[lane];
            if (retirado == null)
            {
                return null;
            }

            _carriles[lane] = null;
            Compact();
            return retirado;
        }

        /// <summary>Saca ese monstruo concreto y cierra el hueco.</summary>
        public bool Remove(CardInstance monstruo)
        {
            int lane = IndexOf(monstruo);
            return lane >= 0 && RemoveAt(lane) != null;
        }

        /// <summary>
        /// Retira los monstruos sin vida y cierra los huecos, en orden de carril.
        /// </summary>
        /// <remarks>
        /// El combate la llama despues de cada ataque, no al final de la fase:
        /// el atacante siguiente tiene que ver el tablero ya compactado
        /// (DESIGN.md §6).
        /// </remarks>
        public IReadOnlyList<CardInstance> RemoveDead()
        {
            List<CardInstance> caidos = new();

            for (int lane = 0; lane < _carriles.Length; lane++)
            {
                CardInstance monstruo = _carriles[lane];
                if (monstruo != null && !monstruo.IsAlive)
                {
                    caidos.Add(monstruo);
                    _carriles[lane] = null;
                }
            }

            if (caidos.Count > 0)
            {
                Compact();
            }

            return caidos;
        }

        /// <summary>
        /// Cierra los huecos desplazando los monstruos hacia delante y conservando
        /// su orden. Devuelve true si algo llego a moverse.
        /// </summary>
        public bool Compact()
        {
            bool huboMovimiento = false;
            int destino = 0;

            for (int origen = 0; origen < _carriles.Length; origen++)
            {
                if (_carriles[origen] == null)
                {
                    continue;
                }

                if (origen != destino)
                {
                    _carriles[destino] = _carriles[origen];
                    _carriles[origen] = null;
                    huboMovimiento = true;
                }

                destino++;
            }

            return huboMovimiento;
        }
    }
}
