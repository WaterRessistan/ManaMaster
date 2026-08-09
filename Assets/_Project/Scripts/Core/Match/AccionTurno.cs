namespace ManaMaster.Core.Match
{
    /// <summary>Que puede hacer un jugador durante su fase principal.</summary>
    public enum TipoAccion
    {
        /// <summary>Cerrar la fase principal y pasar al combate.</summary>
        TerminarTurno = 0,

        /// <summary>Colocar una carta de la mano en un carril.</summary>
        Desplegar = 1,

        /// <summary>Retirar un monstruo propio para recuperar mana.</summary>
        Sacrificar = 2
    }

    /// <summary>
    /// Una jugada de la fase principal, ya decidida pero todavia sin aplicar.
    /// </summary>
    /// <remarks>
    /// Que la decision sea un dato y no una llamada directa es lo que permite
    /// que la IA y el jugador humano entren por el mismo sitio: la interfaz
    /// construye la misma <see cref="AccionTurno"/> al soltar una carta que el
    /// agente al decidirla, y el motor no distingue quien la mando.
    /// </remarks>
    public readonly struct AccionTurno
    {
        private AccionTurno(TipoAccion tipo, int huecoMano, int carril)
        {
            Tipo = tipo;
            HuecoMano = huecoMano;
            Carril = carril;
        }

        public TipoAccion Tipo { get; }

        /// <summary>Hueco de la mano, solo para <see cref="TipoAccion.Desplegar"/>.</summary>
        public int HuecoMano { get; }

        /// <summary>
        /// Carril: posicion de insercion al desplegar, o carril del monstruo al
        /// sacrificar.
        /// </summary>
        public int Carril { get; }

        public static AccionTurno Desplegar(int huecoMano, int carril)
            => new(TipoAccion.Desplegar, huecoMano, carril);

        public static AccionTurno Sacrificar(int carril)
            => new(TipoAccion.Sacrificar, -1, carril);

        public static AccionTurno TerminarTurno()
            => new(TipoAccion.TerminarTurno, -1, -1);

        public override string ToString() => Tipo switch
        {
            TipoAccion.Desplegar
                => $"desplegar la carta {HuecoMano} en el carril {Carril + 1}",
            TipoAccion.Sacrificar
                => $"sacrificar el monstruo del carril {Carril + 1}",
            _ => "terminar el turno"
        };
    }
}
