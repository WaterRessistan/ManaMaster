namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Quien decide las jugadas de un jugador durante su fase principal.
    /// </summary>
    /// <remarks>
    /// Lo implementa la IA. El jugador humano no lo necesita: sus acciones las
    /// construye la interfaz al arrastrar y soltar. Vive en el Core y no en
    /// Unity para que la IA se pueda probar sin abrir el editor y para poder
    /// enfrentarla contra si misma en las simulaciones de balanceo.
    /// </remarks>
    public interface IMatchAgent
    {
        /// <summary>
        /// Siguiente jugada del jugador activo. Se le vuelve a preguntar despues
        /// de cada jugada hasta que devuelve
        /// <see cref="TipoAccion.TerminarTurno"/>.
        /// </summary>
        AccionTurno DecidirAccion(MatchState partida);
    }
}
