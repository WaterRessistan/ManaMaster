using UnityEditor;
using UnityEngine;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Reconstruye las 4 escenas del flujo en orden, de una vez.
    /// </summary>
    /// <remarks>
    /// Cada escena se genera desde cero por su cuenta; esto es solo la forma
    /// normal de comprobar que el flujo entero sigue siendo coherente despues
    /// de tocar algo compartido, como <c>ConstructorDeInterfaz</c> o
    /// <c>ConstructorDeEscenaComun</c>.
    /// </remarks>
    public static class ConstructorDeTodasLasEscenas
    {
        [MenuItem("Mana Master/Reconstruir todas las escenas")]
        public static void ReconstruirTodas()
        {
            ConstructorDeEscenaInicio.Reconstruir();
            ConstructorDeEscenaTienda.Reconstruir();
            ConstructorDeEscenaDeckbuild.Reconstruir();
            ConstructorDeEscenaDuelo.Reconstruir();

            Debug.Log("[ConstructorDeTodasLasEscenas] Las 4 escenas se han regenerado.");
        }
    }
}
