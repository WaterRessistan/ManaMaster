using System;
using ManaMaster.Core.Board;
using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Todo lo que un jugador tiene durante la partida: mana, mazo, mano y arena.
    /// </summary>
    /// <remarks>
    /// Sustituye al <c>Jugador</c> MonoBehaviour, cuyo mana llego a ser una
    /// variable <c>static</c> compartida por los dos jugadores. Aqui no hay nada
    /// estatico ni nada de Unity: dos PlayerState son dos partidas de estado
    /// completamente independientes, que es lo que permite simular miles de
    /// duelos seguidos para el balanceo.
    /// </remarks>
    public sealed class PlayerState
    {
        public PlayerState(string nombre, Deck mazo)
        {
            Nombre = string.IsNullOrWhiteSpace(nombre) ? "Jugador" : nombre;
            Mazo = mazo ?? throw new ArgumentNullException(nameof(mazo));
            Mano = new Hand();
            Arena = new Arena();

            Mano.Refill(Mazo);
        }

        public string Nombre { get; }

        /// <summary>
        /// Mana disponible. Se acumula de un turno al siguiente y no tiene tope
        /// (DESIGN.md §7).
        /// </summary>
        public int Mana { get; private set; }

        public Deck Mazo { get; }

        public Hand Mano { get; }

        public Arena Arena { get; }

        /// <summary>
        /// Cartas de monstruo que le quedan en total: mazo, mano y arena.
        /// Cuando llega a 0, este jugador ha perdido (DESIGN.md §9).
        /// </summary>
        public int MonstruosRestantes => Mazo.Count + Mano.Count + Arena.Count;

        public bool SinMonstruos => MonstruosRestantes == 0;

        public void GanarMana(int cantidad)
        {
            if (cantidad <= 0)
            {
                return;
            }

            Mana += cantidad;
        }

        /// <summary>
        /// Descuenta el coste si hay mana suficiente. Si no lo hay no toca nada
        /// y devuelve false.
        /// </summary>
        public bool TryGastarMana(int coste)
        {
            if (coste < 0 || coste > Mana)
            {
                return false;
            }

            Mana -= coste;
            return true;
        }

        /// <summary>
        /// Indica si alguna carta de la mano se puede pagar y colocar ahora mismo.
        /// </summary>
        /// <remarks>
        /// Es la mitad de la segunda condicion de derrota del §9: quedarse sin
        /// monstruos en la arena y sin mana para desplegar ninguno.
        /// </remarks>
        public bool PuedeDesplegarAlguna()
        {
            if (Arena.IsFull)
            {
                return false;
            }

            for (int slot = 0; slot < Hand.Capacity; slot++)
            {
                CardInstance carta = Mano[slot];
                if (carta != null && carta.Definition.ManaCost <= Mana)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Juega la carta de un hueco de la mano en la posicion de insercion
        /// indicada, empujando hacia atras lo que hubiera (DESIGN.md §3).
        /// </summary>
        /// <remarks>
        /// El hueco de la mano se rellena al momento desde el mazo. Si algo
        /// falla no se modifica nada: el mana solo se gasta cuando el despliegue
        /// es seguro.
        /// </remarks>
        public ResultadoDespliegue TryDesplegar(int huecoMano, int carril)
        {
            CardInstance carta = Mano[huecoMano];
            if (carta == null)
            {
                return ResultadoDespliegue.HuecoVacio;
            }

            if (Arena.IsFull)
            {
                return ResultadoDespliegue.ArenaLlena;
            }

            if (!Arena.CanInsertAt(carril))
            {
                return ResultadoDespliegue.CarrilInvalido;
            }

            if (carta.Definition.ManaCost > Mana)
            {
                return ResultadoDespliegue.ManaInsuficiente;
            }

            TryGastarMana(carta.Definition.ManaCost);
            Arena.Insert(carril, Mano.Take(huecoMano));
            Mano.Refill(Mazo);

            return ResultadoDespliegue.Ok;
        }
    }
}
