using System;
using System.Collections.Generic;
using ManaMaster.Core.Board;
using ManaMaster.Core.Cards;

namespace ManaMaster.Core.Combat
{
    /// <summary>
    /// Foto de una arena en un momento dado: quien esta desplegado y con cuanta
    /// vida.
    /// </summary>
    /// <remarks>
    /// Hace falta porque el combate se resuelve entero de golpe. Cuando la
    /// interfaz recibe el log, los monstruos ya tienen su vida final y los
    /// muertos ya no estan en la arena, asi que sin una foto previa no habria
    /// forma de reproducir el combate paso a paso.
    /// </remarks>
    public sealed class InstantaneaDeArena
    {
        private InstantaneaDeArena(
            IReadOnlyList<CardInstance> monstruos,
            IReadOnlyDictionary<CardInstance, int> vidas)
        {
            Monstruos = monstruos;
            Vidas = vidas;
        }

        /// <summary>Monstruos en orden de carril, sin huecos.</summary>
        public IReadOnlyList<CardInstance> Monstruos { get; }

        /// <summary>Vida que tenia cada uno al tomar la foto.</summary>
        public IReadOnlyDictionary<CardInstance, int> Vidas { get; }

        public static InstantaneaDeArena Tomar(Arena arena)
        {
            if (arena == null)
            {
                throw new ArgumentNullException(nameof(arena));
            }

            List<CardInstance> monstruos = new(arena.Desplegados);
            Dictionary<CardInstance, int> vidas = new(monstruos.Count);

            foreach (CardInstance monstruo in monstruos)
            {
                vidas[monstruo] = monstruo.CurrentHealth;
            }

            return new InstantaneaDeArena(monstruos, vidas);
        }
    }
}
