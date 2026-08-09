using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;

namespace ManaMaster.Core.Combat
{
    /// <summary>
    /// Algo que ha pasado durante la fase de combate.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El resolvedor no anima nada ni sabe que existe una pantalla: devuelve la
    /// lista ordenada de lo que ha ocurrido y la vista la reproduce a su ritmo,
    /// con sus pausas. Es lo que permite que el §6 pida "una breve pausa" tras
    /// cada muerte sin que el motor tenga que saber de corrutinas, y que el
    /// combate se pueda simular miles de veces sin dibujar nada.
    /// </para>
    /// <para>
    /// Cada evento lleva las referencias a los monstruos implicados, no copias:
    /// la vista los usa para localizar el objeto que tiene que animar.
    /// </para>
    /// </remarks>
    public abstract class EventoCombate
    {
    }

    /// <summary>Un curandero ha restaurado vida a un aliado (o a si mismo).</summary>
    public sealed class CuracionAplicada : EventoCombate
    {
        public CuracionAplicada(
            CardInstance curador,
            CardInstance objetivo,
            int cantidad,
            int vidaResultante)
        {
            Curador = curador;
            Objetivo = objetivo;
            Cantidad = cantidad;
            VidaResultante = vidaResultante;
        }

        public CardInstance Curador { get; }

        public CardInstance Objetivo { get; }

        /// <summary>Vida realmente restaurada, ya recortada por el maximo.</summary>
        public int Cantidad { get; }

        public int VidaResultante { get; }

        public override string ToString()
            => $"{Curador.Definition.DisplayName} cura {Cantidad} a " +
               $"{Objetivo.Definition.DisplayName} ({VidaResultante} de vida)";
    }

    /// <summary>Un monstruo ha golpeado a otro.</summary>
    public sealed class AtaqueResuelto : EventoCombate
    {
        public AtaqueResuelto(
            CardInstance atacante,
            int carrilAtacante,
            CardInstance objetivo,
            int carrilObjetivo,
            int dano,
            int vidaResultante)
        {
            Atacante = atacante;
            CarrilAtacante = carrilAtacante;
            Objetivo = objetivo;
            CarrilObjetivo = carrilObjetivo;
            Dano = dano;
            VidaResultante = vidaResultante;
        }

        public CardInstance Atacante { get; }

        public int CarrilAtacante { get; }

        public CardInstance Objetivo { get; }

        /// <summary>Carril del defensor en el momento del golpe, ya compactado.</summary>
        public int CarrilObjetivo { get; }

        public int Dano { get; }

        public int VidaResultante { get; }

        public override string ToString()
            => $"{Atacante.Definition.DisplayName} golpea a " +
               $"{Objetivo.Definition.DisplayName} por {Dano} " +
               $"({VidaResultante} de vida)";
    }

    /// <summary>
    /// Un monstruo podia atacar pero no habia nada a lo que apuntar: el rival
    /// tiene la arena vacia (DESIGN.md §6, tercera regla de sustitucion).
    /// </summary>
    public sealed class AtaqueSinObjetivo : EventoCombate
    {
        public AtaqueSinObjetivo(CardInstance atacante, int carrilAtacante)
        {
            Atacante = atacante;
            CarrilAtacante = carrilAtacante;
        }

        public CardInstance Atacante { get; }

        public int CarrilAtacante { get; }

        public override string ToString()
            => $"{Atacante.Definition.DisplayName} ataca al vacio";
    }

    /// <summary>Un monstruo ha llegado a 0 de vida y sale de la partida.</summary>
    public sealed class MonstruoDerrotado : EventoCombate
    {
        public MonstruoDerrotado(
            CardInstance monstruo, int carril, PlayerState propietario)
        {
            Monstruo = monstruo;
            Carril = carril;
            Propietario = propietario;
        }

        public CardInstance Monstruo { get; }

        /// <summary>Carril que ocupaba al caer.</summary>
        public int Carril { get; }

        public PlayerState Propietario { get; }

        public override string ToString()
            => $"{Monstruo.Definition.DisplayName} cae en el carril {Carril + 1}";
    }

    /// <summary>
    /// Una arena ha cerrado sus huecos y las cartas de detras han avanzado.
    /// </summary>
    /// <remarks>
    /// Llega inmediatamente despues de la muerte que lo provoca y ANTES del
    /// ataque siguiente, porque el atacante que viene detras ya apunta sobre el
    /// tablero compactado (DESIGN.md §6).
    /// </remarks>
    public sealed class ArenaCompactada : EventoCombate
    {
        public ArenaCompactada(PlayerState propietario)
        {
            Propietario = propietario;
        }

        public PlayerState Propietario { get; }

        public override string ToString()
            => $"la arena de {Propietario.Nombre} se compacta";
    }
}
