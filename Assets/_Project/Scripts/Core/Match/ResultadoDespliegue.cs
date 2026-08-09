namespace ManaMaster.Core.Match
{
    /// <summary>
    /// Por que salio o no salio un despliegue.
    /// </summary>
    /// <remarks>
    /// El motor devuelve el motivo en lugar de un booleano para que la interfaz
    /// pueda decirle al jugador que le falta, y para que la IA descarte jugadas
    /// sin tener que replicar las reglas.
    /// </remarks>
    public enum ResultadoDespliegue
    {
        /// <summary>El monstruo entro en la arena.</summary>
        Ok = 0,

        /// <summary>Ese hueco de la mano esta vacio o no existe.</summary>
        HuecoVacio = 1,

        /// <summary>Ya hay tres monstruos desplegados.</summary>
        ArenaLlena = 2,

        /// <summary>La posicion pedida dejaria un hueco, o no existe.</summary>
        CarrilInvalido = 3,

        /// <summary>No hay mana suficiente para pagar el coste.</summary>
        ManaInsuficiente = 4
    }
}
