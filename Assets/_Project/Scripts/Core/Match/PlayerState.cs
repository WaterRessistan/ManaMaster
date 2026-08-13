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
        /// Mazo de objetos. Vacio por defecto: solo lo rellena de verdad
        /// <see cref="IniciarObjetos"/>, para no romper el constructor ni los
        /// tests que ya montan un <see cref="PlayerState"/> sin objetos.
        /// </summary>
        public ItemDeck MazoDeObjetos { get; private set; } = new(Array.Empty<IItemCard>());

        public ItemHand ManoDeObjetos { get; } = new();

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

        /// <summary>
        /// Retira voluntariamente un monstruo propio de la arena y recupera la
        /// mitad de su coste, redondeando hacia abajo (DESIGN.md §7).
        /// </summary>
        /// <remarks>
        /// El monstruo sale de la partida definitivamente: no vuelve al mazo ni
        /// a la mano, asi que sacrificar acerca a la derrota del §9. El hueco se
        /// cierra al momento.
        ///
        /// Con la formula actual un monstruo de coste 1 devuelve 0 de mana. Esta
        /// en el §7 como pendiente de la fase de balanceo.
        /// </remarks>
        /// <returns>Mana recuperado, o -1 si ese carril estaba vacio.</returns>
        public int TrySacrificar(int carril)
        {
            CardInstance monstruo = Arena[carril];
            if (monstruo == null)
            {
                return -1;
            }

            int manaRecuperado = monstruo.SacrificeManaValue;

            Arena.RemoveAt(carril);
            GanarMana(manaRecuperado);

            return manaRecuperado;
        }

        /// <summary>
        /// Monta el mazo de objetos de verdad y reparte la primera mano. Solo
        /// hace falta llamarlo cuando el jugador va a usar objetos (Fase 7);
        /// sin esta llamada, <see cref="ManoDeObjetos"/> se queda vacia.
        /// </summary>
        public void IniciarObjetos(ItemDeck mazo)
        {
            MazoDeObjetos = mazo ?? throw new ArgumentNullException(nameof(mazo));
            ManoDeObjetos.Refill(MazoDeObjetos);
        }

        /// <summary>
        /// Equipa un objeto de la mano sobre un monstruo propio en la arena
        /// (DESIGN.md §4). El hueco de la mano se rellena al momento desde el
        /// mazo, igual que al desplegar un monstruo.
        /// </summary>
        /// <remarks>
        /// Las pociones (<see cref="IItemCard.EsPocion"/>) no pasan por
        /// <see cref="CardInstance.TryEquip"/>: se aplican con
        /// <see cref="CardInstance.UsarPocion"/>, que no ocupa el hueco de
        /// objeto y por eso nunca falla con <see cref="ResultadoEquipar.YaLlevaObjeto"/>.
        /// </remarks>
        public ResultadoEquipar TryEquipar(int huecoManoObjeto, int carril)
        {
            IItemCard objeto = ManoDeObjetos[huecoManoObjeto];
            if (objeto == null)
            {
                return ResultadoEquipar.HuecoVacio;
            }

            CardInstance monstruo = Arena[carril];
            if (monstruo == null)
            {
                return ResultadoEquipar.CarrilVacio;
            }

            bool aplicado = objeto.EsPocion
                ? monstruo.UsarPocion(objeto)
                : monstruo.TryEquip(objeto);

            if (!aplicado)
            {
                return ResultadoEquipar.YaLlevaObjeto;
            }

            ManoDeObjetos.Take(huecoManoObjeto);
            ManoDeObjetos.Refill(MazoDeObjetos);

            return ResultadoEquipar.Ok;
        }
    }
}
