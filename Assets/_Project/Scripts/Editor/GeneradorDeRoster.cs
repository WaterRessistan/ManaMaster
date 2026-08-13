using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Herramientas
{
    /// <summary>
    /// Amplia el roster con las cartas que faltaban (DESIGN.md §13: 0
    /// épicas de monstruo, 0 legendarias de objeto).
    /// </summary>
    /// <remarks>
    /// Igual que <see cref="GeneradorDeArtePixel"/>, es un item de menu
    /// aparte, no parte de <see cref="ConstructorDeTodasLasEscenas"/>: se
    /// ejecuta una vez, el resultado (los .asset nuevos y el CardCatalog
    /// ampliado) se commitea como contenido normal, y no debe volver a
    /// correr en cada reconstruccion de escenas.
    /// </remarks>
    public static class GeneradorDeRoster
    {
        private const string RutaMonstruos = "Assets/_Project/Content/Cards/Monstruos/";
        private const string RutaObjetos = "Assets/_Project/Content/Cards/Objetos/";

        [MenuItem("Mana Master/Generar roster nuevo (Fase 6)")]
        public static void Generar()
        {
            CardCatalog catalogo = AssetDatabase.LoadAssetAtPath<CardCatalog>(
                ConstructorDeEscenaComun.RutaCatalogo);

            if (catalogo == null)
            {
                Debug.LogError("[GeneradorDeRoster] No encuentro el CardCatalog.");
                return;
            }

            MonsterCardDefinition duendeArquero = CrearMonstruo(
                "DuendeArquero", "Duende Arquero", CardRarity.Comun,
                mana: 1, vida: 2, ataque: 2, cura: 0, melee: false, rango: true);

            MonsterCardDefinition trasgoVeloz = CrearMonstruo(
                "TrasgoVeloz", "Trasgo Veloz", CardRarity.Rara,
                mana: 2, vida: 5, ataque: 3, cura: 0, melee: true, rango: true);

            MonsterCardDefinition behemot = CrearMonstruo(
                "Behemot", "Behemot", CardRarity.Epica,
                mana: 4, vida: 14, ataque: 4, cura: 0, melee: true, rango: false);

            MonsterCardDefinition sombraLetal = CrearMonstruo(
                "SombraLetal", "Sombra Letal", CardRarity.Epica,
                mana: 3, vida: 6, ataque: 6, cura: 0, melee: false, rango: true);

            ItemCardDefinition pocionMayor = CrearObjeto(
                "PocionMayor", "Poción Mayor", CardRarity.Rara, mana: 1,
                descripcion: "+5 a la vida máxima del monstruo que lo lleve. Es " +
                             "una poción: no ocupa el hueco de objeto.",
                bonusAtaque: 0, bonusVida: 5, bonusCura: 0, esPocion: true);

            ItemCardDefinition anilloDelVacio = CrearObjeto(
                "AnilloDelVacio", "Anillo del Vacío", CardRarity.Legendaria, mana: 2,
                descripcion: "+2 al ataque y +4 a la vida máxima del monstruo que lo lleve.",
                bonusAtaque: 2, bonusVida: 4, bonusCura: 0, esPocion: false);

            AnadirACatalogo(catalogo, "monsters",
                duendeArquero, trasgoVeloz, behemot, sombraLetal);
            AnadirACatalogo(catalogo, "items", pocionMayor, anilloDelVacio);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GeneradorDeRoster] Roster ampliado: 4 monstruos y 2 objetos nuevos.");
        }

        private static MonsterCardDefinition CrearMonstruo(
            string nombreAsset, string displayName, CardRarity rareza,
            int mana, int vida, int ataque, int cura, bool melee, bool rango)
        {
            MonsterCardDefinition carta = ScriptableObject.CreateInstance<MonsterCardDefinition>();

            SerializedObject serializado = new(carta);
            serializado.FindProperty("displayName").stringValue = displayName;
            serializado.FindProperty("rarity").enumValueIndex = (int)rareza;
            serializado.FindProperty("manaCost").intValue = mana;
            serializado.FindProperty("maxHealth").intValue = vida;
            serializado.FindProperty("attack").intValue = ataque;
            serializado.FindProperty("healPerTurn").intValue = cura;
            serializado.FindProperty("canAttackMelee").boolValue = melee;
            serializado.FindProperty("canAttackRanged").boolValue = rango;
            serializado.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(carta, $"{RutaMonstruos}{nombreAsset}.asset");
            return carta;
        }

        private static ItemCardDefinition CrearObjeto(
            string nombreAsset, string displayName, CardRarity rareza, int mana,
            string descripcion, int bonusAtaque, int bonusVida, int bonusCura, bool esPocion)
        {
            ItemCardDefinition carta = ScriptableObject.CreateInstance<ItemCardDefinition>();

            SerializedObject serializado = new(carta);
            serializado.FindProperty("displayName").stringValue = displayName;
            serializado.FindProperty("rarity").enumValueIndex = (int)rareza;
            serializado.FindProperty("manaCost").intValue = mana;
            serializado.FindProperty("effectDescription").stringValue = descripcion;
            serializado.FindProperty("bonusAttack").intValue = bonusAtaque;
            serializado.FindProperty("bonusMaxHealth").intValue = bonusVida;
            serializado.FindProperty("bonusHealPerTurn").intValue = bonusCura;
            serializado.FindProperty("esPocion").boolValue = esPocion;
            serializado.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(carta, $"{RutaObjetos}{nombreAsset}.asset");
            return carta;
        }

        private static void AnadirACatalogo(CardCatalog catalogo, string campo, params Object[] cartas)
        {
            SerializedObject serializado = new(catalogo);
            SerializedProperty lista = serializado.FindProperty(campo);

            int inicio = lista.arraySize;
            lista.arraySize += cartas.Length;
            for (int i = 0; i < cartas.Length; i++)
            {
                lista.GetArrayElementAtIndex(inicio + i).objectReferenceValue = cartas[i];
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalogo);
        }
    }
}
