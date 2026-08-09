using ManaMaster.Core.Board;
using ManaMaster.Core.Combat;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// A quien apunta cada atacante, incluidas las tres reglas de sustitucion
    /// del DESIGN.md §6.
    /// </summary>
    [TestFixture]
    public sealed class CombatTargetingTests
    {
        [Test]
        public void ElCuerpoACuerpoSiempreVaContraElCarrilPrincipal()
        {
            for (int defensores = 1; defensores <= BoardLanes.Count; defensores++)
            {
                Arena rival = ArenaCon(defensores);

                Assert.That(
                    CombatTargeting.ResolverObjetivo(rival, BoardLanes.Principal),
                    Is.EqualTo(BoardLanes.Principal),
                    $"con {defensores} defensores");
            }
        }

        /// <summary>
        /// Tercera sustitucion: si el rival no tiene nada, el ataque no hace nada.
        /// </summary>
        [Test]
        public void ContraUnaArenaVaciaNoHayObjetivo()
        {
            Arena vacia = new();

            Assert.That(CombatTargeting.ResolverObjetivo(vacia, 0),
                Is.EqualTo(CombatTargeting.SinObjetivo));
            Assert.That(CombatTargeting.ResolverObjetivo(vacia, 1),
                Is.EqualTo(CombatTargeting.SinObjetivo));
            Assert.That(CombatTargeting.ResolverObjetivo(vacia, 2),
                Is.EqualTo(CombatTargeting.SinObjetivo));
        }

        /// <summary>
        /// Con los dos traseros del rival ocupados, los ataques a distancia se
        /// cruzan: mi carril 2 golpea a su 3 y mi 3 a su 2.
        /// </summary>
        [Test]
        public void ConLosDosTraserosOcupadosLosRangosSeCruzan()
        {
            Arena rival = ArenaCon(3);

            Assert.That(CombatTargeting.ResolverObjetivo(rival, 1), Is.EqualTo(2));
            Assert.That(CombatTargeting.ResolverObjetivo(rival, 2), Is.EqualTo(1));
        }

        /// <summary>
        /// Primera sustitucion: con un solo trasero ocupado, los dos rangos van
        /// contra ese.
        /// </summary>
        [Test]
        public void ConUnSoloTraseroLosDosRangosVanContraEl()
        {
            Arena rival = ArenaCon(2);

            Assert.That(CombatTargeting.ResolverObjetivo(rival, 1), Is.EqualTo(1));
            Assert.That(CombatTargeting.ResolverObjetivo(rival, 2), Is.EqualTo(1));
        }

        /// <summary>
        /// Segunda sustitucion: sin ningun trasero ocupado, los rangos van contra
        /// el carril principal.
        /// </summary>
        [Test]
        public void SinTraserosLosRangosVanContraElCarrilPrincipal()
        {
            Arena rival = ArenaCon(1);

            Assert.That(CombatTargeting.ResolverObjetivo(rival, 1),
                Is.EqualTo(BoardLanes.Principal));
            Assert.That(CombatTargeting.ResolverObjetivo(rival, 2),
                Is.EqualTo(BoardLanes.Principal));
        }

        [Test]
        public void UnCarrilAtacanteInexistenteNoApuntaANada()
        {
            Arena rival = ArenaCon(3);

            Assert.That(CombatTargeting.ResolverObjetivo(rival, -1),
                Is.EqualTo(CombatTargeting.SinObjetivo));
            Assert.That(CombatTargeting.ResolverObjetivo(rival, BoardLanes.Count),
                Is.EqualTo(CombatTargeting.SinObjetivo));
        }

        private static Arena ArenaCon(int monstruos)
        {
            Arena arena = new();
            for (int carril = 0; carril < monstruos; carril++)
            {
                arena.Insert(carril, Fabrica.Monstruo($"D{carril + 1}"));
            }

            return arena;
        }
    }
}
