using ManaMaster.Core.Board;
using ManaMaster.Core.Cards;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Estado de partida de un monstruo concreto: vida, dano, curacion y desde
    /// que carriles puede atacar (DESIGN.md §4, §6 y §7).
    /// </summary>
    [TestFixture]
    public sealed class CardInstanceTests
    {
        [Test]
        public void UnMonstruoNaceConSuVidaMaxima()
        {
            CardInstance monstruo = new(new CartaDePrueba { MaxHealth = 5 });

            Assert.That(monstruo.CurrentHealth, Is.EqualTo(5));
            Assert.That(monstruo.IsAlive, Is.True);
            Assert.That(monstruo.IsDamaged, Is.False);
        }

        [Test]
        public void SinDefinicionNoSePuedeCrear()
        {
            Assert.That(() => new CardInstance(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ElDanoRestaVidaYDevuelveLoInfligido()
        {
            CardInstance monstruo = new(new CartaDePrueba { MaxHealth = 5 });

            Assert.That(monstruo.ReceiveDamage(2), Is.EqualTo(2));
            Assert.That(monstruo.CurrentHealth, Is.EqualTo(3));
            Assert.That(monstruo.IsDamaged, Is.True);
        }

        /// <summary>
        /// DESIGN.md §4: al llegar a 0 el monstruo queda fuera de combate. La vida
        /// no baja de ahi, y el exceso de dano no se contabiliza.
        /// </summary>
        [Test]
        public void LaVidaNoBajaDeCero()
        {
            CardInstance monstruo = new(new CartaDePrueba { MaxHealth = 3 });

            Assert.That(monstruo.ReceiveDamage(10), Is.EqualTo(3));
            Assert.That(monstruo.CurrentHealth, Is.EqualTo(0));
            Assert.That(monstruo.IsAlive, Is.False);
        }

        [Test]
        public void ElDanoNoPositivoNoHaceNada()
        {
            CardInstance monstruo = new(new CartaDePrueba { MaxHealth = 3 });

            Assert.That(monstruo.ReceiveDamage(0), Is.EqualTo(0));
            Assert.That(monstruo.ReceiveDamage(-4), Is.EqualTo(0));
            Assert.That(monstruo.CurrentHealth, Is.EqualTo(3));
        }

        /// <summary>DESIGN.md §6: la curacion no puede superar la vida maxima.</summary>
        [Test]
        public void LaCuracionNoSuperaLaVidaMaxima()
        {
            CardInstance monstruo = new(new CartaDePrueba { MaxHealth = 5 });
            monstruo.ReceiveDamage(1);

            Assert.That(monstruo.ReceiveHealing(10), Is.EqualTo(1));
            Assert.That(monstruo.CurrentHealth, Is.EqualTo(5));
        }

        [Test]
        public void UnMonstruoMuertoNoSeCura()
        {
            CardInstance monstruo = new(new CartaDePrueba { MaxHealth = 2 });
            monstruo.ReceiveDamage(2);

            Assert.That(monstruo.ReceiveHealing(5), Is.EqualTo(0));
            Assert.That(monstruo.CurrentHealth, Is.EqualTo(0));
            Assert.That(monstruo.IsAlive, Is.False);
        }

        /// <summary>DESIGN.md §4: melee puro solo ataca desde el carril principal.</summary>
        [Test]
        public void ElMeleePuroSoloAtacaDesdeElCarrilPrincipal()
        {
            CardInstance monstruo = new(new CartaDePrueba
            {
                CanAttackMelee = true,
                CanAttackRanged = false
            });

            Assert.That(monstruo.CanAttackFrom(BoardLanes.Principal), Is.True);
            Assert.That(monstruo.CanAttackFrom(1), Is.False);
            Assert.That(monstruo.CanAttackFrom(2), Is.False);
        }

        /// <summary>DESIGN.md §4: rango puro solo ataca desde los traseros.</summary>
        [Test]
        public void ElRangoPuroSoloAtacaDesdeLosCarrilesTraseros()
        {
            CardInstance monstruo = new(new CartaDePrueba
            {
                CanAttackMelee = false,
                CanAttackRanged = true
            });

            Assert.That(monstruo.CanAttackFrom(BoardLanes.Principal), Is.False);
            Assert.That(monstruo.CanAttackFrom(1), Is.True);
            Assert.That(monstruo.CanAttackFrom(2), Is.True);
        }

        /// <summary>DESIGN.md §4: las cartas mixtas atacan desde cualquier carril.</summary>
        [Test]
        public void LaCartaMixtaAtacaDesdeCualquierCarril()
        {
            CardInstance monstruo = new(new CartaDePrueba
            {
                CanAttackMelee = true,
                CanAttackRanged = true
            });

            for (int carril = 0; carril < BoardLanes.Count; carril++)
            {
                Assert.That(monstruo.CanAttackFrom(carril), Is.True,
                    $"deberia poder atacar desde el carril {carril}");
            }
        }

        [Test]
        public void UnCarrilInexistenteNuncaPermiteAtacar()
        {
            CardInstance monstruo = new(new CartaDePrueba
            {
                CanAttackMelee = true,
                CanAttackRanged = true
            });

            Assert.That(monstruo.CanAttackFrom(-1), Is.False);
            Assert.That(monstruo.CanAttackFrom(BoardLanes.Count), Is.False);
        }

        [Test]
        public void SoloCuraQuienTieneCuraMayorQueCero()
        {
            CardInstance curandero = new(new CartaDePrueba { HealPerTurn = 2 });
            CardInstance normal = new(new CartaDePrueba { HealPerTurn = 0 });

            Assert.That(curandero.IsHealer, Is.True);
            Assert.That(normal.IsHealer, Is.False);
        }

        // ------------------------------------------------------------------
        // Objetos (DESIGN.md §4)
        // ------------------------------------------------------------------

        [Test]
        public void EmpiezaSinObjetoEquipado()
        {
            CardInstance monstruo = new(new CartaDePrueba());

            Assert.That(monstruo.EquippedItem, Is.Null);
        }

        [Test]
        public void EquiparUnObjetoSumaSusBonusAlAtaqueVidaYCura()
        {
            CardInstance monstruo = new(new CartaDePrueba
            {
                MaxHealth = 5, Attack = 2, HealPerTurn = 1
            });

            bool equipado = monstruo.TryEquip(
                Fabrica.Objeto("Espada", bonusAtaque: 2, bonusVida: 3, bonusCura: 1));

            Assert.That(equipado, Is.True);
            Assert.That(monstruo.Attack, Is.EqualTo(4));
            Assert.That(monstruo.MaxHealth, Is.EqualTo(8));
            Assert.That(monstruo.HealPerTurn, Is.EqualTo(2));
        }

        /// <summary>
        /// La vida extra se nota ya mismo: no hace falta esperar a la
        /// siguiente curacion para poder usarla.
        /// </summary>
        [Test]
        public void LaVidaMaximaExtraSeSumaTambienALaVidaActualAlEquipar()
        {
            CardInstance monstruo = new(new CartaDePrueba { MaxHealth = 5 });
            monstruo.ReceiveDamage(3);

            monstruo.TryEquip(Fabrica.Objeto("Escudo", bonusVida: 2));

            Assert.That(monstruo.CurrentHealth, Is.EqualTo(4));
            Assert.That(monstruo.MaxHealth, Is.EqualTo(7));
        }

        /// <summary>DESIGN.md §4: como mucho un objeto, no se puede sustituir.</summary>
        [Test]
        public void NoSePuedeEquiparUnSegundoObjeto()
        {
            CardInstance monstruo = new(new CartaDePrueba());
            IItemCard primero = Fabrica.Objeto("Espada", bonusAtaque: 1);
            IItemCard segundo = Fabrica.Objeto("Escudo", bonusVida: 1);

            Assert.That(monstruo.TryEquip(primero), Is.True);
            Assert.That(monstruo.TryEquip(segundo), Is.False);
            Assert.That(monstruo.EquippedItem, Is.SameAs(primero));
        }

        [Test]
        public void EquiparSinObjetoEsUnError()
        {
            CardInstance monstruo = new(new CartaDePrueba());

            Assert.That(() => monstruo.TryEquip(null), Throws.ArgumentNullException);
        }

        /// <summary>
        /// DESIGN.md §7: el sacrificio devuelve la mitad del coste redondeando
        /// hacia abajo, asi que una carta de coste 1 no devuelve nada. Es un
        /// valor pendiente de la fase de balanceo, pero hoy la regla es esta.
        /// </summary>
        [Test]
        public void ElSacrificioDevuelveLaMitadDelCosteRedondeandoHaciaAbajo()
        {
            Assert.That(new CardInstance(new CartaDePrueba { ManaCost = 1 })
                .SacrificeManaValue, Is.EqualTo(0));
            Assert.That(new CardInstance(new CartaDePrueba { ManaCost = 4 })
                .SacrificeManaValue, Is.EqualTo(2));
            Assert.That(new CardInstance(new CartaDePrueba { ManaCost = 5 })
                .SacrificeManaValue, Is.EqualTo(2));
        }
    }
}
