using System;
using System.Collections.Generic;
using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Combat
{
    /// <summary>
    /// Un instante del combate tal como hay que dibujarlo.
    /// </summary>
    public sealed class FotogramaCombate
    {
        internal FotogramaCombate(
            IReadOnlyList<CardInstance> arenaAtacante,
            IReadOnlyList<CardInstance> arenaDefensora,
            IReadOnlyDictionary<CardInstance, int> vidas,
            EventoCombate evento)
        {
            ArenaAtacante = arenaAtacante;
            ArenaDefensora = arenaDefensora;
            Vidas = vidas;
            Evento = evento;
        }

        /// <summary>Monstruos del atacante, en orden de carril y sin huecos.</summary>
        public IReadOnlyList<CardInstance> ArenaAtacante { get; }

        public IReadOnlyList<CardInstance> ArenaDefensora { get; }

        /// <summary>Vida que hay que mostrar en este instante.</summary>
        public IReadOnlyDictionary<CardInstance, int> Vidas { get; }

        /// <summary>
        /// Evento que llevo hasta aqui, o null en el fotograma inicial.
        /// </summary>
        public EventoCombate Evento { get; }
    }

    /// <summary>
    /// Convierte el log del combate en una secuencia de instantes dibujables.
    /// </summary>
    /// <remarks>
    /// <para>
    /// El motor resuelve el combate de una vez y devuelve lo que paso. Este
    /// guion rehace el camino: parte de la foto anterior al combate y va
    /// aplicando los eventos uno a uno, de modo que la interfaz puede enseñar
    /// cada golpe, cada muerte y cada compactacion en su momento, con la pausa
    /// que pide el DESIGN.md §6.
    /// </para>
    /// <para>
    /// Es C# puro y sin Unity a proposito: asi se prueba en el mismo bucle
    /// rapido que las reglas, que es donde estan los casos dificiles (matar en
    /// el carril 1 y que avance el de detras).
    /// </para>
    /// </remarks>
    public static class GuionDeCombate
    {
        /// <summary>
        /// Fotogramas del combate, empezando por el estado previo.
        /// </summary>
        public static IReadOnlyList<FotogramaCombate> Construir(
            InstantaneaDeArena atacante,
            InstantaneaDeArena defensor,
            IReadOnlyList<EventoCombate> eventos)
        {
            if (atacante == null)
            {
                throw new ArgumentNullException(nameof(atacante));
            }

            if (defensor == null)
            {
                throw new ArgumentNullException(nameof(defensor));
            }

            if (eventos == null)
            {
                throw new ArgumentNullException(nameof(eventos));
            }

            List<CardInstance> arenaAtacante = new(atacante.Monstruos);
            List<CardInstance> arenaDefensora = new(defensor.Monstruos);

            Dictionary<CardInstance, int> vidas = new();
            foreach (var entrada in atacante.Vidas)
            {
                vidas[entrada.Key] = entrada.Value;
            }

            foreach (var entrada in defensor.Vidas)
            {
                vidas[entrada.Key] = entrada.Value;
            }

            List<FotogramaCombate> guion = new(eventos.Count + 1)
            {
                Fotograma(arenaAtacante, arenaDefensora, vidas, evento: null)
            };

            foreach (EventoCombate evento in eventos)
            {
                Aplicar(evento, arenaAtacante, arenaDefensora, vidas);
                guion.Add(Fotograma(arenaAtacante, arenaDefensora, vidas, evento));
            }

            return guion;
        }

        private static void Aplicar(
            EventoCombate evento,
            List<CardInstance> arenaAtacante,
            List<CardInstance> arenaDefensora,
            Dictionary<CardInstance, int> vidas)
        {
            switch (evento)
            {
                case CuracionAplicada curacion:
                    vidas[curacion.Objetivo] = curacion.VidaResultante;
                    break;

                case AtaqueResuelto ataque:
                    vidas[ataque.Objetivo] = ataque.VidaResultante;
                    break;

                // Quitarlo de la lista ya cierra el hueco: los de detras suben
                // una posicion, que es exactamente la compactacion del §6. El
                // evento ArenaCompactada que viene detras solo marca la pausa.
                case MonstruoDerrotado derrota:
                    if (!arenaDefensora.Remove(derrota.Monstruo))
                    {
                        arenaAtacante.Remove(derrota.Monstruo);
                    }

                    break;
            }
        }

        private static FotogramaCombate Fotograma(
            List<CardInstance> arenaAtacante,
            List<CardInstance> arenaDefensora,
            Dictionary<CardInstance, int> vidas,
            EventoCombate evento)
            => new(
                new List<CardInstance>(arenaAtacante),
                new List<CardInstance>(arenaDefensora),
                new Dictionary<CardInstance, int>(vidas),
                evento);
    }
}
