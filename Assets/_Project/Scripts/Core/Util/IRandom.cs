namespace ManaMaster.Core.Util
{
    /// <summary>
    /// Fuente de azar del motor de reglas.
    /// </summary>
    /// <remarks>
    /// El dominio nunca llama al azar directamente: lo recibe. Asi el barajado,
    /// el sorteo del jugador inicial y, mas adelante, la apertura de sobres se
    /// pueden fijar en los tests y reproducir una partida entera. Ademas
    /// <c>UnityEngine.Random</c> no existe aqui: el Core se compila sin Unity.
    /// </remarks>
    public interface IRandom
    {
        /// <summary>
        /// Entero entre 0 (incluido) y <paramref name="maxExclusive"/> (excluido).
        /// </summary>
        int Next(int maxExclusive);
    }
}
