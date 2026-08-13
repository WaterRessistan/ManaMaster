using System.IO;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Dibuja a mano, pixel a pixel, el arte del sobre de la Tienda.
    /// </summary>
    /// <remarks>
    /// No hay ninguna herramienta de generacion de imagenes en este entorno:
    /// esto es lo mas parecido a "arte" que se puede producir sin salir del
    /// codigo, con el mismo espiritu que los iconos de espada/flecha de
    /// <see cref="ConstructorDeCartas"/> pero como textura en vez de paneles
    /// de UI. Es un item de menu aparte, no parte de la reconstruccion de
    /// escenas: asi no se pisa el resultado si mas adelante se sustituye por
    /// arte de verdad (mismo trato que <c>Content/Art/Tablero.png</c>).
    /// </remarks>
    public static class GeneradorDeArtePixel
    {
        private const string RutaSobre = "Assets/_Project/Content/Art/Sobre.png";
        private const int Tamano = 32;

        [MenuItem("Mana Master/Generar arte pixel del sobre")]
        public static void GenerarSobre()
        {
            Texture2D textura = new(Tamano, Tamano, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
            };

            Color transparente = new(0f, 0f, 0f, 0f);
            for (int y = 0; y < Tamano; y++)
            {
                for (int x = 0; x < Tamano; x++)
                {
                    textura.SetPixel(x, y, transparente);
                }
            }

            Color borde = new(0.24f, 0.16f, 0.10f, 1f);
            Color cuerpo = new(0.77f, 0.64f, 0.38f, 1f);
            Color solapa = new(0.59f, 0.47f, 0.27f, 1f);
            Color selloOscuro = new(0.5f, 0.08f, 0.08f, 1f);
            Color selloClaro = new(0.75f, 0.18f, 0.18f, 1f);

            const int x1 = 3, x2 = 28, yBase = 7, yTapa = 23, xApex = 15, yApex = 13;

            RellenarRectangulo(textura, x1, yBase, x2, yTapa, cuerpo);
            RellenarSolapa(textura, x1, x2, xApex, yTapa, yApex, solapa);
            Contorno(textura, x1, yBase, x2, yTapa, borde);
            LineaDiagonal(textura, x1, yTapa, xApex, yApex, borde);
            LineaDiagonal(textura, x2, yTapa, xApex, yApex, borde);

            RellenarCirculo(textura, xApex, yApex, 5, selloOscuro);
            RellenarCirculo(textura, xApex, yApex, 3, selloClaro);

            textura.Apply();

            byte[] png = textura.EncodeToPNG();
            Directory.CreateDirectory(Path.GetDirectoryName(RutaSobre));
            File.WriteAllBytes(RutaSobre, png);

            AssetDatabase.ImportAsset(RutaSobre);
            ConfigurarImportacion();

            Debug.Log($"[GeneradorDeArtePixel] Arte del sobre generado en {RutaSobre}");
        }

        /// <summary>Nitido y sin desenfocar al escalarlo, como corresponde a pixel art.</summary>
        private static void ConfigurarImportacion()
        {
            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(RutaSobre);
            if (importador == null)
            {
                Debug.LogError($"[GeneradorDeArtePixel] No se pudo importar '{RutaSobre}'.");
                return;
            }

            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.filterMode = FilterMode.Point;
            importador.textureCompression = TextureImporterCompression.Uncompressed;
            importador.spritePixelsPerUnit = Tamano;
            importador.mipmapEnabled = false;
            importador.alphaIsTransparency = true;

            EditorUtility.SetDirty(importador);
            importador.SaveAndReimport();
        }

        private static void RellenarRectangulo(
            Texture2D tex, int x1, int y1, int x2, int y2, Color color)
        {
            for (int y = y1; y <= y2; y++)
            {
                for (int x = x1; x <= x2; x++)
                {
                    Pintar(tex, x, y, color);
                }
            }
        }

        private static void Contorno(Texture2D tex, int x1, int y1, int x2, int y2, Color color)
        {
            for (int x = x1; x <= x2; x++)
            {
                Pintar(tex, x, y1, color);
                Pintar(tex, x, y2, color);
            }

            for (int y = y1; y <= y2; y++)
            {
                Pintar(tex, x1, y, color);
                Pintar(tex, x2, y, color);
            }
        }

        /// <summary>
        /// Triangulo de la solapa: del borde superior del sobre hacia el
        /// vertice donde va el sello, un lado por cada mitad del ancho.
        /// </summary>
        private static void RellenarSolapa(
            Texture2D tex, int x1, int x2, int xApex, int yBase, int yApex, Color color)
        {
            for (int x = x1; x <= x2; x++)
            {
                float t = x <= xApex
                    ? (float)(x - x1) / (xApex - x1)
                    : (float)(x2 - x) / (x2 - xApex);

                int yLimite = Mathf.RoundToInt(Mathf.Lerp(yApex, yBase, t));

                for (int y = yLimite; y <= yBase; y++)
                {
                    Pintar(tex, x, y, color);
                }
            }
        }

        /// <summary>Bresenham simple: sirve para las dos costuras de la solapa.</summary>
        private static void LineaDiagonal(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = -Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            int x = x0;
            int y = y0;

            while (true)
            {
                Pintar(tex, x, y, color);

                if (x == x1 && y == y1)
                {
                    break;
                }

                int error2 = 2 * error;
                if (error2 >= dy)
                {
                    error += dy;
                    x += sx;
                }

                if (error2 <= dx)
                {
                    error += dx;
                    y += sy;
                }
            }
        }

        private static void RellenarCirculo(Texture2D tex, int cx, int cy, int radio, Color color)
        {
            for (int y = cy - radio; y <= cy + radio; y++)
            {
                for (int x = cx - radio; x <= cx + radio; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    if (dx * dx + dy * dy <= radio * radio)
                    {
                        Pintar(tex, x, y, color);
                    }
                }
            }
        }

        private static void Pintar(Texture2D tex, int x, int y, Color color)
        {
            if (x >= 0 && x < Tamano && y >= 0 && y < Tamano)
            {
                tex.SetPixel(x, y, color);
            }
        }
    }
}
