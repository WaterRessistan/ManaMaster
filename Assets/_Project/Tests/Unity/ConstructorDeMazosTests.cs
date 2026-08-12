using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;
using ManaMaster.Core.Util;
using ManaMaster.Unity.Cards;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ManaMaster.Unity.Tests
{
    /// <summary>
    /// El puente entre los assets del catalogo y los mazos del motor.
    /// </summary>
    /// <remarks>
    /// Estos tests solo corren dentro de Unity (`scripts/unity-check.sh`),
    /// porque necesitan ScriptableObjects de verdad. Los del dominio, que son
    /// la mayoria, viven en Tests/Core y corren tambien con `dotnet test`.
    /// </remarks>
    [TestFixture]
    public sealed class ConstructorDeMazosTests
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
        public void RepartePorDefectoLasDiezCartasDelReglamento()
        {
            Deck mazo = ConstructorDeMazos.Aleatorio(
                CatalogoDePrueba(8), new SystemRandom(1));

            Assert.That(ConstructorDeMazos.CartasPorMazo, Is.EqualTo(10));
            Assert.That(mazo.Count, Is.EqualTo(10));
        }

        /// <summary>DESIGN.md §8: como mucho 2 copias de cada carta.</summary>
        [Test]
        public void NuncaMeteMasDeDosCopiasDeLaMismaCarta()
        {
            // Seis cartas distintas para diez huecos: obliga a repetir.
            Deck mazo = ConstructorDeMazos.Aleatorio(
                CatalogoDePrueba(6), new SystemRandom(7));

            Dictionary<string, int> copias = new();
            foreach (CardInstance carta in mazo.Cartas)
            {
                string id = carta.Definition.CardId;
                copias[id] = copias.TryGetValue(id, out int n) ? n + 1 : 1;
            }

            foreach (KeyValuePair<string, int> entrada in copias)
            {
                Assert.That(entrada.Value,
                    Is.LessThanOrEqualTo(ConstructorDeMazos.MaxCopiasPorCarta),
                    $"'{entrada.Key}' aparece {entrada.Value} veces");
            }
        }

        [Test]
        public void ConPocasCartasReparteLasQueHaya()
        {
            // Dos cartas distintas dan como mucho cuatro con el limite de copias.
            Deck mazo = ConstructorDeMazos.Aleatorio(
                CatalogoDePrueba(2), new SystemRandom(1));

            Assert.That(mazo.Count, Is.EqualTo(4));
        }

        [Test]
        public void UnCatalogoSinMonstruosEsUnError()
        {
            Assert.That(
                () => ConstructorDeMazos.Aleatorio(
                    CatalogoDePrueba(0), new SystemRandom(1)),
                Throws.ArgumentException);
        }

        /// <summary>
        /// Cada copia es una instancia propia: danar una en partida no puede
        /// tocar ni al asset ni a las demas copias.
        /// </summary>
        [Test]
        public void LasCopiasDeUnaMismaCartaSonIndependientes()
        {
            Deck mazo = ConstructorDeMazos.Aleatorio(
                CatalogoDePrueba(1), new SystemRandom(1));

            CardInstance primera = mazo.Cartas[0];
            CardInstance segunda = mazo.Cartas[1];

            Assert.That(primera.Definition.CardId,
                Is.EqualTo(segunda.Definition.CardId), "son la misma carta");
            Assert.That(primera, Is.Not.SameAs(segunda));

            primera.ReceiveDamage(1);

            Assert.That(segunda.CurrentHealth, Is.EqualTo(segunda.MaxHealth));
        }

        [Test]
        public void ConLaMismaSemillaSaleElMismoMazo()
        {
            CardCatalog catalogo = CatalogoDePrueba(8);

            Deck primero = ConstructorDeMazos.Aleatorio(catalogo, new SystemRandom(42));
            Deck segundo = ConstructorDeMazos.Aleatorio(catalogo, new SystemRandom(42));

            for (int i = 0; i < primero.Count; i++)
            {
                Assert.That(segundo.Cartas[i].Definition.CardId,
                    Is.EqualTo(primero.Cartas[i].Definition.CardId));
            }
        }

        [Test]
        public void DesdeSeleccionConstruyeElMazoEnElOrdenDado()
        {
            CardCatalog catalogo = CatalogoDePrueba(10);
            string[] seleccion =
            {
                "Monstruo0", "Monstruo0", "Monstruo1", "Monstruo2", "Monstruo3",
                "Monstruo4", "Monstruo5", "Monstruo6", "Monstruo7", "Monstruo8",
            };

            Deck mazo = ConstructorDeMazos.DesdeSeleccion(catalogo, seleccion);

            Assert.That(mazo.Count, Is.EqualTo(10));
            for (int i = 0; i < seleccion.Length; i++)
            {
                Assert.That(mazo.Cartas[i].Definition.CardId, Is.EqualTo(seleccion[i]));
            }
        }

        [Test]
        public void DesdeSeleccionConUnNumeroDeCartasDistintoDeDiezEsUnError()
        {
            CardCatalog catalogo = CatalogoDePrueba(10);

            Assert.That(
                () => ConstructorDeMazos.DesdeSeleccion(catalogo, new[] { "Monstruo0" }),
                Throws.ArgumentException);
        }

        [Test]
        public void DesdeSeleccionConUnCardIdQueNoExisteEsUnError()
        {
            CardCatalog catalogo = CatalogoDePrueba(10);
            string[] seleccion =
            {
                "NoExiste", "Monstruo0", "Monstruo1", "Monstruo2", "Monstruo3",
                "Monstruo4", "Monstruo5", "Monstruo6", "Monstruo7", "Monstruo8",
            };

            Assert.That(
                () => ConstructorDeMazos.DesdeSeleccion(catalogo, seleccion),
                Throws.ArgumentException);
        }

        [Test]
        public void DesdeSeleccionConMasDeDosCopiasEsUnError()
        {
            CardCatalog catalogo = CatalogoDePrueba(10);
            string[] seleccion =
            {
                "Monstruo0", "Monstruo0", "Monstruo0", "Monstruo1", "Monstruo2",
                "Monstruo3", "Monstruo4", "Monstruo5", "Monstruo6", "Monstruo7",
            };

            Assert.That(
                () => ConstructorDeMazos.DesdeSeleccion(catalogo, seleccion),
                Throws.ArgumentException);
        }

        /// <summary>
        /// Catalogo de mentira con N monstruos distintos. El CardId es el nombre
        /// del asset, asi que basta con darles nombres distintos.
        /// </summary>
        private CardCatalog CatalogoDePrueba(int monstruos)
        {
            CardCatalog catalogo = ScriptableObject.CreateInstance<CardCatalog>();
            catalogo.name = "CatalogoDePrueba";
            _creados.Add(catalogo);

            SerializedObject serializado = new(catalogo);
            SerializedProperty lista = serializado.FindProperty("monsters");
            lista.arraySize = monstruos;

            for (int i = 0; i < monstruos; i++)
            {
                MonsterCardDefinition definicion =
                    ScriptableObject.CreateInstance<MonsterCardDefinition>();
                definicion.name = $"Monstruo{i}";
                _creados.Add(definicion);

                lista.GetArrayElementAtIndex(i).objectReferenceValue = definicion;
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();

            return catalogo;
        }
    }
}
