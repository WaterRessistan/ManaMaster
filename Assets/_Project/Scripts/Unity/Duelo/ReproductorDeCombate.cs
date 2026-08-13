using System.Collections;
using System.Collections.Generic;
using ManaMaster.Core.Cards;
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
        [SerializeField, Min(0f)] private float pausaEntreGolpes = 0.4375f; // 0.35s base, 25% mas lento

        [Tooltip("Pausa extra al caer un monstruo, para que se vea compactar.")]
        [SerializeField, Min(0f)] private float pausaTrasUnaMuerte = 0.6875f; // 0.55s base, 25% mas lento

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

            ReproducirImpacto(vistaAtacante, vistaDefensora, fotograma);
        }

        /// <summary>
        /// "Juice" de combate: golpe de escala y numero flotante sobre la
        /// carta que recibe el dano o la curacion de este fotograma, mas la
        /// embestida del atacante contra el objetivo en un ataque. Es solo
        /// feedback visual, no cambia ninguna regla.
        /// </summary>
        private static void ReproducirImpacto(
            VistaArena vistaAtacante, VistaArena vistaDefensora, FotogramaCombate fotograma)
        {
            switch (fotograma.Evento)
            {
                case AtaqueResuelto ataque:
                {
                    // A diferencia de CuracionAplicada, el evento ya trae los
                    // dos carriles: no hace falta buscar la carta en la lista.
                    VistaCartaMonstruo vistaObjetivo =
                        vistaDefensora?.VistaEnCarril(ataque.CarrilObjetivo);
                    vistaObjetivo?.ReproducirImpacto(ataque.Dano, esCuracion: false);

                    if (vistaObjetivo != null)
                    {
                        vistaAtacante?.VistaEnCarril(ataque.CarrilAtacante)
                            ?.ReproducirEmbestida(vistaObjetivo.transform.position);
                    }

                    break;
                }

                case CuracionAplicada curacion:
                    BuscarVista(vistaAtacante, vistaDefensora, fotograma, curacion.Objetivo)
                        ?.ReproducirImpacto(curacion.Cantidad, esCuracion: true);
                    break;
            }
        }

        private static VistaCartaMonstruo BuscarVista(
            VistaArena vistaAtacante,
            VistaArena vistaDefensora,
            FotogramaCombate fotograma,
            CardInstance objetivo)
        {
            int carrilAtacante = IndiceDe(fotograma.ArenaAtacante, objetivo);
            if (carrilAtacante >= 0)
            {
                return vistaAtacante?.VistaEnCarril(carrilAtacante);
            }

            int carrilDefensora = IndiceDe(fotograma.ArenaDefensora, objetivo);
            return carrilDefensora >= 0 ? vistaDefensora?.VistaEnCarril(carrilDefensora) : null;
        }

        private static int IndiceDe(IReadOnlyList<CardInstance> arena, CardInstance objetivo)
        {
            for (int i = 0; i < arena.Count; i++)
            {
                if (ReferenceEquals(arena[i], objetivo))
                {
                    return i;
                }
            }

            return -1;
        }

        private float PausaDe(EventoCombate evento)
            => evento is MonstruoDerrotado ? pausaTrasUnaMuerte : pausaEntreGolpes;
    }
}
