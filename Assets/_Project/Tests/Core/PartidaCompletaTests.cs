using System.Collections.Generic;
using ManaMaster.Core.Agents;
using ManaMaster.Core.Cards;
using ManaMaster.Core.Match;
using ManaMaster.Core.Util;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Partidas enteras jugadas por dos agentes.
    /// </summary>
    /// <remarks>
    /// Es la prueba de que las piezas encajan: despliegue, combate, muertes,
    /// compactacion, turnos y derrota funcionando juntas durante decenas de
    /// rondas. Un motor que se contradiga o se atasque se cae aqui aunque todos
    /// los tests unitarios esten en verde.
    /// </remarks>
    [TestFixture]
    public sealed class PartidaCompletaTests
    {
        [Test]
        public void UnaPartidaEntreDosAgentesLlegaASuFin()
        {
            MatchState partida = Duelo(semilla: 7);

            ResultadoPartida resultado = MatchRunner.Jugar(
                partida, new AgenteHeuristico(), new AgenteHeuristico());

            Assert.That(resultado, Is.Not.EqualTo(ResultadoPartida.EnCurso),
                "la partida se ha quedado atascada");
            Assert.That(partida.Ganador, Is.Not.Null);
            Assert.That(partida.Ronda, Is.GreaterThan(1));
        }

        /// <summary>
        /// Cincuenta partidas distintas: si alguna combinacion deja el motor en
        /// bucle o rompe una invariante, sale aqui.
        /// </summary>
        [Test]
        public void CincuentaPartidasSeguidasTerminanTodas()
        {
            for (int semilla = 0; semilla < 50; semilla++)
            {
                MatchState partida = Duelo(semilla);

                ResultadoPartida resultado = MatchRunner.Jugar(
                    partida, new AgenteHeuristico(), new AgenteHeuristico());

                Assert.That(resultado, Is.Not.EqualTo(ResultadoPartida.EnCurso),
                    $"la partida con semilla {semilla} no termino");
            }
        }

        /// <summary>
        /// Una partida sin salida acaba en tablas al llegar al tope de rondas
        /// (DESIGN.md §9).
        /// </summary>
        /// <remarks>
        /// El caso: las dos arenas llenas, con lo que nadie puede desplegar, y
        /// la curacion igualando al dano que se hacen, con lo que no muere
        /// nadie. Sin el tope la partida no acabaria nunca. Pasa en algo menos
        /// del 0,5 % de las simulaciones.
        /// </remarks>
        [Test]
        public void UnaPartidaSinSalidaAcabaEnTablas()
        {
            MatchState partida = Duelo(semilla: 76);

            ResultadoPartida resultado = MatchRunner.Jugar(
                partida, new AgenteHeuristico(), new AgenteHeuristico());

            Assert.That(resultado, Is.EqualTo(ResultadoPartida.Empate));
            Assert.That(partida.Ronda, Is.EqualTo(MatchState.MaxRondas));
            Assert.That(partida.Ganador, Is.Null, "en tablas no gana nadie");
            Assert.That(partida.Jugador1.Arena.IsFull, Is.True);
            Assert.That(partida.Jugador2.Arena.IsFull, Is.True);
        }

        /// <summary>
        /// El tope tiene que dejar margen de sobra: las partidas que se deciden
        /// duran entre 11 y 36 rondas.
        /// </summary>
        [Test]
        public void ElTopeDeRondasNoCortaPartidasNormales()
        {
            for (int semilla = 0; semilla < 50; semilla++)
            {
                MatchState partida = Duelo(semilla);

                MatchRunner.Jugar(partida, new AgenteHeuristico(), new AgenteHeuristico());

                Assert.That(partida.Resultado,
                    Is.Not.EqualTo(ResultadoPartida.Empate),
                    $"la partida con semilla {semilla} se corto por el tope");
            }
        }

        /// <summary>
        /// La misma semilla da la misma partida. Es lo que permitira repetir un
        /// fallo y lo que hara fiables las simulaciones de balanceo.
        /// </summary>
        [Test]
        public void LaMismaSemillaDaLaMismaPartida()
        {
            MatchState primera = Duelo(semilla: 123);
            MatchState segunda = Duelo(semilla: 123);

            MatchRunner.Jugar(primera, new AgenteHeuristico(), new AgenteHeuristico());
            MatchRunner.Jugar(segunda, new AgenteHeuristico(), new AgenteHeuristico());

            Assert.That(segunda.Resultado, Is.EqualTo(primera.Resultado));
            Assert.That(segunda.Ronda, Is.EqualTo(primera.Ronda));
        }

        [Test]
        public void ElPerdedorSeQuedaSinMonstruosOSinPoderDesplegar()
        {
            MatchState partida = Duelo(semilla: 42);

            MatchRunner.Jugar(partida, new AgenteHeuristico(), new AgenteHeuristico());

            Assert.That(partida.Ganador, Is.Not.Null, "esta partida no es de tablas");

            PlayerState perdedor = ReferenceEquals(partida.Ganador, partida.Jugador1)
                ? partida.Jugador2
                : partida.Jugador1;

            bool sinMonstruos = perdedor.SinMonstruos;
            bool ahogado = perdedor.Arena.IsEmpty && !perdedor.PuedeDesplegarAlguna();

            Assert.That(sinMonstruos || ahogado, Is.True,
                "el perdedor no cumple ninguna clausula del §9");
        }

        [Test]
        public void SinAgentesNoSePuedeJugar()
        {
            MatchState partida = Duelo(semilla: 1);
            AgenteHeuristico agente = new();

            Assert.That(() => MatchRunner.Jugar(partida, null, agente),
                Throws.ArgumentNullException);
            Assert.That(() => MatchRunner.Jugar(partida, agente, null),
                Throws.ArgumentNullException);
            Assert.That(() => MatchRunner.Jugar(null, agente, agente),
                Throws.ArgumentNullException);
        }

        private static MatchState Duelo(int semilla)
            => new(
                new PlayerState("Ana", MazoVariado(semilla)),
                new PlayerState("Beto", MazoVariado(semilla + 1000)),
                new SystemRandom(semilla));

        /// <summary>
        /// Diez monstruos con perfiles distintos, parecidos a los del catalogo:
        /// cuerpo a cuerpo, rango y algun curandero.
        /// </summary>
        private static Deck MazoVariado(int semilla)
        {
            List<CardInstance> cartas = new();

            for (int i = 0; i < 3; i++)
            {
                cartas.Add(Fabrica.Monstruo($"Melee{i}", coste: 2, vida: 4, ataque: 3));
                cartas.Add(Fabrica.Monstruo($"Rango{i}", coste: 2, vida: 3, ataque: 3,
                    melee: false, rango: true));
                cartas.Add(Fabrica.Monstruo($"Barato{i}", coste: 1, vida: 2, ataque: 1));
            }

            cartas.Add(Fabrica.Monstruo("Curandero", coste: 4, vida: 6, ataque: 2,
                cura: 1));

            Deck mazo = new(cartas);
            mazo.Shuffle(new SystemRandom(semilla));

            return mazo;
        }
    }
}
