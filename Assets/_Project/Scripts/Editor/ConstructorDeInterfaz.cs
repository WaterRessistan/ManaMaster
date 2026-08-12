using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Ladrillos para construir interfaces desde codigo.
    /// </summary>
    /// <remarks>
    /// Las escenas de este proyecto se generan con scripts y no se cablean a
    /// mano en el Inspector: asi la jerarquia se revisa en el diff, se puede
    /// regenerar si se rompe y no depende de que alguien recuerde que arrastro
    /// donde.
    /// </remarks>
    public static class ConstructorDeInterfaz
    {
        private const string RutaFuente = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";

        /// <summary>Objeto de interfaz vacio, hijo del padre indicado.</summary>
        public static RectTransform Nodo(
            string nombre, Transform padre, Vector2 posicion, Vector2 tamano)
        {
            GameObject objeto = new(nombre, typeof(RectTransform));
            RectTransform rect = (RectTransform)objeto.transform;

            rect.SetParent(padre, worldPositionStays: false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = tamano;
            rect.anchoredPosition = posicion;

            return rect;
        }

        public static Image Panel(
            string nombre,
            Transform padre,
            Vector2 posicion,
            Vector2 tamano,
            Color color,
            bool recibeClics = true)
        {
            RectTransform rect = Nodo(nombre, padre, posicion, tamano);

            Image imagen = rect.gameObject.AddComponent<Image>();
            imagen.color = color;
            imagen.raycastTarget = recibeClics;

            return imagen;
        }

        public static Text Texto(
            string nombre,
            Transform padre,
            Vector2 posicion,
            Vector2 tamano,
            string contenido,
            int cuerpo = 22,
            TextAnchor alineacion = TextAnchor.MiddleCenter)
        {
            RectTransform rect = Nodo(nombre, padre, posicion, tamano);

            Text texto = rect.gameObject.AddComponent<Text>();
            texto.font = Fuente();
            texto.text = contenido;
            texto.fontSize = cuerpo;
            texto.alignment = alineacion;
            texto.color = Color.white;
            texto.raycastTarget = false;
            texto.horizontalOverflow = HorizontalWrapMode.Overflow;
            texto.verticalOverflow = VerticalWrapMode.Overflow;

            return texto;
        }

        public static Button Boton(
            string nombre,
            Transform padre,
            Vector2 posicion,
            Vector2 tamano,
            string etiqueta,
            out Text textoEtiqueta)
        {
            Image fondo = Panel(nombre, padre, posicion, tamano,
                new Color(0.20f, 0.28f, 0.42f, 1f));

            Button boton = fondo.gameObject.AddComponent<Button>();
            boton.targetGraphic = fondo;

            textoEtiqueta = Texto(
                "Etiqueta", fondo.transform, Vector2.zero, tamano, etiqueta);

            return boton;
        }

        /// <summary>
        /// Fuente de la interfaz. Se usa la del proyecto y no la incrustada del
        /// motor, que ha cambiado de nombre entre versiones de Unity.
        /// </summary>
        public static Font Fuente()
        {
            Font fuente = AssetDatabase.LoadAssetAtPath<Font>(RutaFuente);

            if (fuente == null)
            {
                fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return fuente;
        }

        /// <summary>
        /// Asigna referencias a campos privados con <c>[SerializeField]</c>.
        /// </summary>
        /// <remarks>
        /// Es la unica forma de cablear desde codigo sin abrir esos campos al
        /// resto del proyecto solo para poder construir la escena.
        /// </remarks>
        public static void Cablear(Object destino, params (string campo, Object valor)[] enlaces)
        {
            SerializedObject serializado = new(destino);

            foreach ((string campo, Object valor) in enlaces)
            {
                SerializedProperty propiedad = serializado.FindProperty(campo);

                if (propiedad == null)
                {
                    Debug.LogError(
                        $"[ConstructorDeInterfaz] '{destino.GetType().Name}' no " +
                        $"tiene el campo serializado '{campo}'.");
                    continue;
                }

                propiedad.objectReferenceValue = valor;
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Asigna un array de referencias a un campo privado.</summary>
        public static void CablearLista(Object destino, string campo, params Object[] valores)
        {
            SerializedObject serializado = new(destino);
            SerializedProperty propiedad = serializado.FindProperty(campo);

            if (propiedad == null)
            {
                Debug.LogError(
                    $"[ConstructorDeInterfaz] '{destino.GetType().Name}' no tiene " +
                    $"el campo serializado '{campo}'.");
                return;
            }

            propiedad.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++)
            {
                propiedad.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Asigna un booleano a un campo privado.</summary>
        public static void CablearBool(Object destino, string campo, bool valor)
        {
            SerializedObject serializado = new(destino);
            serializado.FindProperty(campo).boolValue = valor;
            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Asigna un entero a un campo privado.</summary>
        public static void CablearInt(Object destino, string campo, int valor)
        {
            SerializedObject serializado = new(destino);
            serializado.FindProperty(campo).intValue = valor;
            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Asigna una cadena a un campo privado.</summary>
        public static void CablearString(Object destino, string campo, string valor)
        {
            SerializedObject serializado = new(destino);
            serializado.FindProperty(campo).stringValue = valor;
            serializado.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
