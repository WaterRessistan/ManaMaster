using System.Collections;
using System.Collections.Generic;
using ManaMaster.Core.Combat;
using UnityEngine;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Enseña el combate golpe a golpe en lugar de saltar al resultado.
    /// </summary>
    /// <remarks>
    /// El §6 pide que, al morir un monstruo, la compactacion se vea "con una
    /// breve pausa" antes de que ataque el carril siguiente, porque es lo que
    /// hace entendible que un atacante encadene bajas. El motor resuelve el
    /// combate entero de golpe, asi que aqui se rehace la secuencia a partir
    /// del guion y se dibuja fotograma a fotograma.
    /// </remarks>
    public sealed class ReproductorDeCombate : MonoBehaviour
    {
        [SerializeField] private VistaArena arenaHumano;
        [SerializeField] private VistaArena arenaRival;

        [Header("Ritmo")]
        [Tooltip("Pausa tras cada golpe o curacion.")]
        [SerializeField, Min(0f)] private float pausaEntreGolpes = 0.35f;

        [Tooltip("Pausa extra al caer un monstruo, para que se vea compactar.")]
        [SerializeField, Min(0f)] private float pausaTrasUnaMuerte = 0.55f;

        /// <summary>Hay un combate animandose ahora mismo.</summary>
        public bool Reproduciendo { get; private set; }

        /// <summary>
        /// Reproduce el combate ya resuelto a partir de las fotos de antes.
        /// </summary>
        /// <param name="atacaElHumano">
        /// De quien era el turno: decide cual de las dos arenas es la atacante.
        /// </param>
        public IEnumerator Reproducir(
            InstantaneaDeArena fotoAtacante,
            InstantaneaDeArena fotoDefensora,
            IReadOnlyList<EventoCombate> eventos,
            bool atacaElHumano)
        {
            if (eventos == null || eventos.Count == 0)
            {
                yield break;
            }

            VistaArena vistaAtacante = atacaElHumano ? arenaHumano : arenaRival;
            VistaArena vistaDefensora = atacaElHumano ? arenaRival : arenaHumano;

            IReadOnlyList<FotogramaCombate> guion =
                GuionDeCombate.Construir(fotoAtacante, fotoDefensora, eventos);

            Reproduciendo = true;

            try
            {
                foreach (FotogramaCombate fotograma in guion)
                {
                    Dibujar(vistaAtacante, vistaDefensora, fotograma);

                    // El fotograma inicial es el estado que ya se estaba viendo:
                    // no merece pausa propia.
                    if (fotograma.Evento == null)
                    {
                        continue;
                    }

                    yield return new WaitForSeconds(PausaDe(fotograma.Evento));
                }
            }
            finally
            {
                Reproduciendo = false;
            }
        }

        private static void Dibujar(
            VistaArena vistaAtacante,
            VistaArena vistaDefensora,
            FotogramaCombate fotograma)
        {
            if (vistaAtacante != null)
            {
                vistaAtacante.MostrarFotograma(
                    fotograma.ArenaAtacante, fotograma.Vidas);
            }

            if (vistaDefensora != null)
            {
                vistaDefensora.MostrarFotograma(
                    fotograma.ArenaDefensora, fotograma.Vidas);
            }
        }

        private float PausaDe(EventoCombate evento)
            => evento is MonstruoDerrotado ? pausaTrasUnaMuerte : pausaEntreGolpes;
    }
}
