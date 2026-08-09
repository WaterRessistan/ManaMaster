using System;
using System.Collections.Generic;
using ManaMaster.Core.Board;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;

namespace ManaMaster.Core.Combat
{
    /// <summary>
    /// La fase de combate: primero cura, despues ataca (DESIGN.md §5 y §6).
    /// </summary>
    /// <remarks>
    /// Solo actua el jugador activo. La arena del atacante no cambia durante su
    /// propia fase de combate (nadie le devuelve el golpe), asi que recorrer sus
    /// carriles de 0 a 2 es seguro; la del defensor si cambia, y por eso se
    /// recalcula el objetivo antes de cada ataque.
    /// </remarks>
    public static class CombatResolver
    {
        /// <summary>
        /// Resuelve la fase de combate del jugador activo y devuelve, en orden,
        /// todo lo que ha pasado.
        /// </summary>
        public static IReadOnlyList<EventoCombate> Resolver(
            PlayerState atacante, PlayerState defensor)
        {
            if (atacante == null)
            {
                throw new ArgumentNullException(nameof(atacante));
            }

            if (defensor == null)
            {
                throw new ArgumentNullException(nameof(defensor));
            }

            List<EventoCombate> eventos = new();

            Curar(atacante, eventos);
            Atacar(atacante, defensor, eventos);

            return eventos;
        }

        /// <summary>
        /// Cada curandero restaura vida a TODOS los aliados en arena, incluido el
        /// mismo, sin pasar de la vida maxima (DESIGN.md §6).
        /// </summary>
        private static void Curar(PlayerState jugador, List<EventoCombate> eventos)
        {
            IReadOnlyList<CardInstance> aliados = jugador.Arena.Desplegados;

            foreach (CardInstance curador in aliados)
            {
                if (!curador.IsHealer)
                {
                    continue;
                }

                foreach (CardInstance aliado in aliados)
                {
                    int curado = aliado.ReceiveHealing(curador.HealPerTurn);
                    if (curado > 0)
                    {
                        eventos.Add(new CuracionAplicada(
                            curador, aliado, curado, aliado.CurrentHealth));
                    }
                }
            }
        }

        /// <summary>
        /// Los ataques se resuelven en orden de carril: primero el 1, luego el 2
        /// y luego el 3 (DESIGN.md §6).
        /// </summary>
        private static void Atacar(
            PlayerState atacante, PlayerState defensor, List<EventoCombate> eventos)
        {
            for (int carril = 0; carril < BoardLanes.Count; carril++)
            {
                CardInstance monstruo = atacante.Arena[carril];

                if (monstruo == null || !monstruo.IsAlive)
                {
                    continue;
                }

                // Un monstruo en un carril desde el que no puede atacar sigue
                // siendo objetivo valido: simplemente no ataca (DESIGN.md §4).
                if (!monstruo.CanAttackFrom(carril) || monstruo.Attack <= 0)
                {
                    continue;
                }

                ResolverAtaque(monstruo, carril, defensor, eventos);
            }
        }

        private static void ResolverAtaque(
            CardInstance monstruo,
            int carril,
            PlayerState defensor,
            List<EventoCombate> eventos)
        {
            // El objetivo se calcula AHORA y no al empezar la fase: las muertes
            // de los ataques anteriores ya han compactado la arena rival.
            int carrilObjetivo =
                CombatTargeting.ResolverObjetivo(defensor.Arena, carril);

            if (carrilObjetivo == CombatTargeting.SinObjetivo)
            {
                eventos.Add(new AtaqueSinObjetivo(monstruo, carril));
                return;
            }

            CardInstance objetivo = defensor.Arena[carrilObjetivo];
            if (objetivo == null)
            {
                eventos.Add(new AtaqueSinObjetivo(monstruo, carril));
                return;
            }

            int dano = objetivo.ReceiveDamage(monstruo.Attack);
            eventos.Add(new AtaqueResuelto(
                monstruo, carril, objetivo, carrilObjetivo,
                dano, objetivo.CurrentHealth));

            if (objetivo.IsAlive)
            {
                return;
            }

            eventos.Add(new MonstruoDerrotado(objetivo, carrilObjetivo, defensor));

            // La compactacion ocurre en este momento, antes de que ataque el
            // carril siguiente (DESIGN.md §6).
            defensor.Arena.RemoveDead();

            // Solo se ha movido algo si quedaba alguien por detras del carril
            // que acaba de vaciarse: matar en el ultimo carril no adelanta nada.
            if (defensor.Arena.Count > carrilObjetivo)
            {
                eventos.Add(new ArenaCompactada(defensor));
            }
        }
    }
}
