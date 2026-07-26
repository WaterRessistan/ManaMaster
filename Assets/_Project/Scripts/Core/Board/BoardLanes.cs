namespace ManaMaster.Core.Board
{
    /// <summary>
    /// Constantes y consultas sobre los tres carriles de la arena.
    /// </summary>
    /// <remarks>
    /// Los carriles se indexan de 0 a 2. El carril 0 es el PRINCIPAL, donde
    /// atacan los monstruos cuerpo a cuerpo. Los carriles 1 y 2 son los
    /// TRASEROS, desde donde atacan los monstruos a distancia.
    ///
    /// En la interfaz se muestran al jugador como "carril 1, 2 y 3".
    /// </remarks>
    public static class BoardLanes
    {
        /// <summary>Numero maximo de monstruos desplegados por jugador.</summary>
        public const int Count = 3;

        /// <summary>Indice del carril principal (cuerpo a cuerpo).</summary>
        public const int Principal = 0;

        /// <summary>Indice del primer carril trasero.</summary>
        public const int PrimerTrasero = 1;

        public static bool IsValid(int laneIndex)
            => laneIndex >= 0 && laneIndex < Count;

        /// <summary>El carril desde el que atacan los monstruos a distancia.</summary>
        public static bool IsRear(int laneIndex)
            => laneIndex >= PrimerTrasero && laneIndex < Count;

        /// <summary>El carril desde el que atacan los monstruos cuerpo a cuerpo.</summary>
        public static bool IsFront(int laneIndex)
            => laneIndex == Principal;

        /// <summary>
        /// Carril trasero enfrentado en espejo. Los ataques a distancia se
        /// cruzan: el carril 1 apunta al 2 del rival y viceversa.
        /// </summary>
        public static int MirrorRear(int rearLaneIndex)
        {
            if (!IsRear(rearLaneIndex))
            {
                return rearLaneIndex;
            }

            // Con Count = 3 los traseros son {1, 2}: 1 <-> 2.
            return PrimerTrasero + (Count - 1 - rearLaneIndex);
        }

        /// <summary>Etiqueta que ve el jugador ("Carril 1" para el indice 0).</summary>
        public static string ToDisplayName(int laneIndex)
            => $"Carril {laneIndex + 1}";
    }
}
