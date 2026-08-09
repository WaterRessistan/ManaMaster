using ManaMaster.Core.Board;

namespace ManaMaster.Core.Combat
{
    /// <summary>
    /// A quien golpea un atacante segun el carril desde el que ataca.
    /// </summary>
    /// <remarks>
    /// Reglas del DESIGN.md §6:
    ///
    /// - Carril 1 (cuerpo a cuerpo) -> carril 1 del rival.
    /// - Carriles 2 y 3 (distancia) -> carriles traseros del rival, CRUZADOS:
    ///   mi 2 golpea a su 3 y mi 3 golpea a su 2.
    ///
    /// Y las tres sustituciones para los atacantes a distancia:
    ///
    /// 1. Si el rival solo tiene un trasero ocupado, ambos rangos van a ese.
    /// 2. Si no tiene ningun trasero ocupado, van a su carril 1.
    /// 3. Si no tiene nada en la arena, el ataque no hace nada.
    ///
    /// Las tres salen de la invariante de que la arena no tiene huecos: con N
    /// monstruos desplegados estan ocupados exactamente los carriles 0..N-1, asi
    /// que el numero de monstruos del rival basta para saber a quien se apunta.
    /// </remarks>
    public static class CombatTargeting
    {
        /// <summary>Ningun objetivo posible.</summary>
        public const int SinObjetivo = -1;

        /// <summary>
        /// Carril del defensor al que apunta un atacante situado en
        /// <paramref name="carrilAtacante"/>, o <see cref="SinObjetivo"/>.
        /// </summary>
        public static int ResolverObjetivo(Arena defensora, int carrilAtacante)
        {
            if (defensora == null
                || defensora.IsEmpty
                || !BoardLanes.IsValid(carrilAtacante))
            {
                // Tercera sustitucion: sin nada enfrente, el ataque se pierde.
                return SinObjetivo;
            }

            // Cuerpo a cuerpo: siempre contra el carril principal. Por la
            // invariante, si hay alguien en la arena el carril 0 esta ocupado.
            if (BoardLanes.IsFront(carrilAtacante))
            {
                return BoardLanes.Principal;
            }

            switch (defensora.Count)
            {
                // Segunda sustitucion: el rival solo tiene el principal.
                case 1:
                    return BoardLanes.Principal;

                // Primera sustitucion: un unico trasero ocupado, los dos rangos
                // van contra el.
                case 2:
                    return BoardLanes.PrimerTrasero;

                // Los dos traseros ocupados: ataque cruzado.
                default:
                    return BoardLanes.MirrorRear(carrilAtacante);
            }
        }
    }
}
