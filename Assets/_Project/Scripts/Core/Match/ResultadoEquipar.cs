namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Por que salio o no salio equipar un objeto (DESIGN.md §4).
    /// </summary>
    public enum ResultadoEquipar
    {
        /// <summary>El objeto quedo equipado.</summary>
        Ok = 0,

        /// <summary>Ese hueco de la mano de objetos esta vacio o no existe.</summary>
        HuecoVacio = 1,

        /// <summary>No hay ningun monstruo en ese carril.</summary>
        CarrilVacio = 2,

        /// <summary>
        /// Ese monstruo ya lleva un objeto. No se puede quitar ni sustituir.
        /// </summary>
        YaLlevaObjeto = 3
    }
}
