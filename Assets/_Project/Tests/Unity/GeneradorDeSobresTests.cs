using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Util;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Tienda;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Unity.Tests
{
    /// <summary>Apertura de sobres segun las probabilidades de DESIGN.md §10.</summary>
    [TestFixture]
    public sealed class GeneradorDeSobresTests
    {
        private readonly List<Object> _creados = new();

        [TearDown]
        public void Limpiar()
        {
            foreach (Object creado in _creados)
            {
                Object.DestroyImmediate(creado);
            }

            _creados.Clear();
        }

        [Test]
        public void AbreExactamenteTresCartasDelCatalogo()
        {
            CardCatalog catalogo = CatalogoDePrueba(
                (CardRarity.Comun, 5), (CardRarity.Rara, 3));

            List<string> sobre = GeneradorDeSobres.Abrir(catalogo, new SystemRandom(1));

            Assert.That(sobre.Count, Is.EqualTo(GeneradorDeSobres.CartasPorSobre));
            foreach (string cardId in sobre)
            {
                Assert.That(catalogo.FindMonster(cardId), Is.Not.Null,
                    $"'{cardId}' no esta en el catalogo");
            }
        }

        [Test]
        public void SiempreHayAlMenosUnaRaraOSuperiorCuandoElCatalogoTiene()
        {
            CardCatalog catalogo = CatalogoDePrueba(
                (CardRarity.Comun, 5), (CardRarity.Rara, 2), (CardRarity.Epica, 1));

            for (int semilla = 0; semilla < 200; semilla++)
            {
                List<string> sobre = GeneradorDeSobres.Abrir(catalogo, new SystemRandom(semilla));

                bool hayRaraOSuperior = false;
                foreach (string cardId in sobre)
                {
                    if (catalogo.FindMonster(cardId).Rarity >= CardRarity.Rara)
                    {
                        hayRaraOSuperior = true;
                        break;
                    }
                }

                Assert.That(hayRaraOSuperior, Is.True, $"semilla {semilla} no dio ninguna Rara+");
            }
        }

        [Test]
        public void SinCartasRaraOSuperiorNoFuerzaNadaYNoRevienta()
        {
            CardCatalog catalogo = CatalogoDePrueba((CardRarity.Comun, 4));

            List<string> sobre = GeneradorDeSobres.Abrir(catalogo, new SystemRandom(1));

            Assert.That(sobre.Count, Is.EqualTo(GeneradorDeSobres.CartasPorSobre));
        }

        [Test]
        public void RedistribuyeElPesoDeLasRarezasQueFaltanEnElCatalogo()
        {
            // Sin epicas ni legendarias en el catalogo: su peso combinado
            // (5%) tiene que repartirse entre comun y rara sin reventar.
            CardCatalog catalogo = CatalogoDePrueba(
                (CardRarity.Comun, 3), (CardRarity.Rara, 3));

            for (int semilla = 0; semilla < 50; semilla++)
            {
                List<string> sobre = GeneradorDeSobres.Abrir(catalogo, new SystemRandom(semilla));
                Assert.That(sobre.Count, Is.EqualTo(GeneradorDeSobres.CartasPorSobre));
            }
        }

        [Test]
        public void ConLaMismaSemillaSaleElMismoSobre()
        {
            CardCatalog catalogo = CatalogoDePrueba(
                (CardRarity.Comun, 5), (CardRarity.Rara, 3), (CardRarity.Epica, 1));

            List<string> primero = GeneradorDeSobres.Abrir(catalogo, new SystemRandom(42));
            List<string> segundo = GeneradorDeSobres.Abrir(catalogo, new SystemRandom(42));

            Assert.That(segundo, Is.EqualTo(primero));
        }

        [Test]
        public void UnCatalogoSinMonstruosEsUnError()
        {
            CardCatalog catalogo = CatalogoDePrueba();

            Assert.That(
                () => GeneradorDeSobres.Abrir(catalogo, new SystemRandom(1)),
                Throws.ArgumentException);
        }

        /// <summary>Catalogo de mentira con N monstruos por cada rareza pedida.</summary>
        private CardCatalog CatalogoDePrueba(params (CardRarity rareza, int cuantos)[] grupos)
        {
            CardCatalog catalogo = ScriptableObject.CreateInstance<CardCatalog>();
            catalogo.name = "CatalogoDePrueba";
            _creados.Add(catalogo);

            List<MonsterCardDefinition> definiciones = new();
            int indice = 0;
            foreach ((CardRarity rareza, int cuantos) in grupos)
            {
                for (int i = 0; i < cuantos; i++)
                {
                    MonsterCardDefinition definicion =
                        ScriptableObject.CreateInstance<MonsterCardDefinition>();
                    definicion.name = $"Monstruo{indice++}_{rareza}";
                    _creados.Add(definicion);

                    SerializedObject serializadoCarta = new(definicion);
                    serializadoCarta.FindProperty("rarity").enumValueIndex = (int)rareza;
                    serializadoCarta.ApplyModifiedPropertiesWithoutUndo();

                    definiciones.Add(definicion);
                }
            }

            SerializedObject serializado = new(catalogo);
            SerializedProperty lista = serializado.FindProperty("monsters");
            lista.arraySize = definiciones.Count;
            for (int i = 0; i < definiciones.Count; i++)
            {
                lista.GetArrayElementAtIndex(i).objectReferenceValue = definiciones[i];
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();

            return catalogo;
        }
    }
}
