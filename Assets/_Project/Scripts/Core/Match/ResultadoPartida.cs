namespace ManaMaster.Core.Match
{
    /// <summary>Como esta la partida.</summary>
    public enum ResultadoPartida
    {
        EnCurso = 0,
        VictoriaJugador1 = 1,
        VictoriaJugador2 = 2,

        /// <summary>
        /// Tablas por agotarse el limite de rondas (DESIGN.md §9). Cada jugador
        /// se lleva una cantidad intermedia de diamantes.
        /// </summary>
        Empate = 3
    }
}
