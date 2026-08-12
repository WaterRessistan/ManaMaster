using System.IO;
using ManaMaster.Unity.Sesion;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Crea el asset unico de <see cref="SesionDeJuego"/> si todavia no existe.
    /// </summary>
    /// <remarks>
    /// Es un asset, no una escena, asi que no lo regenera
    /// <c>ConstructorDeTodasLasEscenas</c>: se crea una vez y a partir de ahi
    /// los constructores de escena solo lo referencian (igual que hacen con
    /// <c>CardCatalog.asset</c>).
    /// </remarks>
    public static class CreadorDeSesionDeJuego
    {
        public const string RutaAsset = "Assets/_Project/Content/Session/SesionDeJuego.asset";

        [MenuItem("Mana Master/Crear asset de sesion de juego (si falta)")]
        public static void CrearSiFalta()
        {
            if (AssetDatabase.LoadAssetAtPath<SesionDeJuego>(RutaAsset) != null)
            {
                Debug.Log($"[CreadorDeSesionDeJuego] Ya existe {RutaAsset}.");
                return;
            }

            string carpeta = Path.GetDirectoryName(RutaAsset)!.Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(carpeta))
            {
                Directory.CreateDirectory(carpeta);
                AssetDatabase.Refresh();
            }

            SesionDeJuego sesion = ScriptableObject.CreateInstance<SesionDeJuego>();
            AssetDatabase.CreateAsset(sesion, RutaAsset);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CreadorDeSesionDeJuego] Creado {RutaAsset}.");
        }
    }
}
