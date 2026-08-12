using System.IO;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Sesion;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Crea el asset unico de <see cref="SesionDeJuego"/> si todavia no
    /// existe, y en cualquier caso le cablea el catalogo.
    /// </summary>
    /// <remarks>
    /// Es un asset, no una escena, asi que no lo regenera
    /// <c>ConstructorDeTodasLasEscenas</c>: se crea una vez y a partir de ahi
    /// los constructores de escena solo lo referencian (igual que hacen con
    /// <c>CardCatalog.asset</c>). El cableado del catalogo se repite siempre
    /// (no solo al crear) porque la Fase 4 lo añadió despues de que el asset
    /// ya existiera en los repositorios de quien venga de la Fase 5.
    /// </remarks>
    public static class CreadorDeSesionDeJuego
    {
        public const string RutaAsset = "Assets/_Project/Content/Session/SesionDeJuego.asset";

        [MenuItem("Mana Master/Crear asset de sesion de juego (si falta)")]
        public static void CrearSiFalta()
        {
            SesionDeJuego sesion = AssetDatabase.LoadAssetAtPath<SesionDeJuego>(RutaAsset);

            if (sesion == null)
            {
                string carpeta = Path.GetDirectoryName(RutaAsset)!.Replace('\\', '/');
                if (!AssetDatabase.IsValidFolder(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                    AssetDatabase.Refresh();
                }

                sesion = ScriptableObject.CreateInstance<SesionDeJuego>();
                AssetDatabase.CreateAsset(sesion, RutaAsset);

                Debug.Log($"[CreadorDeSesionDeJuego] Creado {RutaAsset}.");
            }

            ConstructorDeInterfaz.Cablear(sesion,
                ("catalogo", AssetDatabase.LoadAssetAtPath<CardCatalog>(
                    ConstructorDeEscenaComun.RutaCatalogo)));

            // DontUnloadUnusedAsset (ver el OnEnable de SesionDeJuego) es un
            // flag de tiempo de ejecucion: no debe quedar horneado en el
            // .asset guardado en git.
            sesion.hideFlags = HideFlags.None;
            EditorUtility.SetDirty(sesion);

            AssetDatabase.SaveAssets();
        }
    }
}
