using ManaMaster.Core.Match;
using NUnit.Framework;

namespace ManaMaster.Core.Tests
{
    /// <summary>
    /// Estado de un jugador: mana, mano, mazo y despliegue (DESIGN.md §3 y §7).
    /// </summary>
    [TestFixture]
    public sealed class PlayerStateTests
    {
        [Test]
        public void EmpiezaSinManaYConLaManoRepartida()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));

            Assert.That(jugador.Mana, Is.EqualTo(0));
            Assert.That(jugador.Mano.Count, Is.EqualTo(2));
            Assert.That(jugador.Mazo.Count, Is.EqualTo(8));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
        }

        [Test]
        public void ElManaSeAcumulaYNoTieneTope()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));

            for (int ronda = 0; ronda < 10; ronda++)
            {
                jugador.GanarMana(3);
            }

            Assert.That(jugador.Mana, Is.EqualTo(30));
        }

        [Test]
        public void NoSeGastaManaQueNoSeTiene()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(3);

            Assert.That(jugador.TryGastarMana(4), Is.False);
            Assert.That(jugador.Mana, Is.EqualTo(3));

            Assert.That(jugador.TryGastarMana(3), Is.True);
            Assert.That(jugador.Mana, Is.EqualTo(0));
        }

        [Test]
        public void DesplegarPagaElCosteColocaYReponeLaMano()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10, coste: 2));
            jugador.GanarMana(3);

            ResultadoDespliegue resultado = jugador.TryDesplegar(huecoMano: 0, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.Ok));
            Assert.That(jugador.Mana, Is.EqualTo(1));
            Assert.That(jugador.Arena.Count, Is.EqualTo(1));
            Assert.That(jugador.Mano.Count, Is.EqualTo(2), "la mano se repone al momento");
            Assert.That(jugador.Mazo.Count, Is.EqualTo(7));
        }

        [Test]
        public void ElDespliegueEmpujaComoEnElReglamento()
        {
            // La mano arranca con [A, B] y el mazo guarda [C, D]. Ojo al hueco
            // que se juega: al vaciarlo se repone al momento con la carta
            // siguiente del mazo, no se desplaza la otra carta de la mano.
            PlayerState jugador = new("Ana", Fabrica.Mazo(
                Fabrica.Monstruo("A"),
                Fabrica.Monstruo("B"),
                Fabrica.Monstruo("C"),
                Fabrica.Monstruo("D")));
            jugador.GanarMana(10);

            jugador.TryDesplegar(huecoMano: 0, carril: 0);   // A al carril 1
            Assert.That(Fabrica.Disposicion(jugador.Arena), Is.EqualTo("A - -"));

            jugador.TryDesplegar(huecoMano: 1, carril: 1);   // B detras de A
            Assert.That(Fabrica.Disposicion(jugador.Arena), Is.EqualTo("A B -"));

            // C entra por delante y empuja a los dos hacia atras: el ejemplo
            // literal del DESIGN.md §3.
            jugador.TryDesplegar(huecoMano: 0, carril: 0);

            Assert.That(Fabrica.Disposicion(jugador.Arena), Is.EqualTo("C A B"));
        }

        [Test]
        public void SinManaSuficienteNoSeTocaNada()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10, coste: 5));
            jugador.GanarMana(3);

            ResultadoDespliegue resultado = jugador.TryDesplegar(0, 0);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.ManaInsuficiente));
            Assert.That(jugador.Mana, Is.EqualTo(3));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
            Assert.That(jugador.Mano.Count, Is.EqualTo(2));
        }

        [Test]
        public void NoSePuedeDesplegarDejandoUnHueco()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(10);

            ResultadoDespliegue resultado = jugador.TryDesplegar(0, carril: 2);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.CarrilInvalido));
            Assert.That(jugador.Arena.IsEmpty, Is.True);
            Assert.That(jugador.Mana, Is.EqualTo(10));
        }

        [Test]
        public void ConLaArenaLlenaNoSeDespliegaMas()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(10);
            jugador.TryDesplegar(0, 0);
            jugador.TryDesplegar(0, 1);
            jugador.TryDesplegar(0, 2);

            ResultadoDespliegue resultado = jugador.TryDesplegar(0, 0);

            Assert.That(resultado, Is.EqualTo(ResultadoDespliegue.ArenaLlena));
            Assert.That(jugador.Arena.Count, Is.EqualTo(3));
        }

        [Test]
        public void UnHuecoVacioDeLaManoNoDespliegaNada()
        {
            PlayerState jugador = new("Ana", Fabrica.Mazo(Fabrica.Monstruo("A")));
            jugador.GanarMana(10);

            // Solo habia una carta: el segundo hueco esta vacio.
            Assert.That(jugador.TryDesplegar(1, 0),
                Is.EqualTo(ResultadoDespliegue.HuecoVacio));
            Assert.That(jugador.TryDesplegar(99, 0),
                Is.EqualTo(ResultadoDespliegue.HuecoVacio));
        }

        /// <summary>DESIGN.md §9: cuentan los monstruos de mazo, mano y arena.</summary>
        [Test]
        public void LosMonstruosRestantesSonMazoMasManoMasArena()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(10);

            Assert.That(jugador.MonstruosRestantes, Is.EqualTo(10));

            jugador.TryDesplegar(0, 0);

            Assert.That(jugador.MonstruosRestantes, Is.EqualTo(10),
                "desplegar mueve la carta de sitio, no la elimina");
            Assert.That(jugador.SinMonstruos, Is.False);
        }

        [Test]
        public void SabeSiPuedePagarAlgunaCartaDeLaMano()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10, coste: 3));

            jugador.GanarMana(2);
            Assert.That(jugador.PuedeDesplegarAlguna(), Is.False);

            jugador.GanarMana(1);
            Assert.That(jugador.PuedeDesplegarAlguna(), Is.True);
        }

        [Test]
        public void ConLaArenaLlenaNoPuedeDesplegarAunqueLeSobreMana()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.GanarMana(50);
            jugador.TryDesplegar(0, 0);
            jugador.TryDesplegar(0, 1);
            jugador.TryDesplegar(0, 2);

            Assert.That(jugador.PuedeDesplegarAlguna(), Is.False);
        }

        // ------------------------------------------------------------------
        // Objetos (DESIGN.md §4)
        // ------------------------------------------------------------------

        [Test]
        public void SinIniciarObjetosLaManoDeObjetosEstaVacia()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));

            Assert.That(jugador.ManoDeObjetos.IsEmpty, Is.True);
            Assert.That(jugador.MazoDeObjetos.IsEmpty, Is.True);
        }

        [Test]
        public void IniciarObjetosRepartaLaPrimeraMano()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));

            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(
                Fabrica.Objeto("Espada"), Fabrica.Objeto("Escudo"), Fabrica.Objeto("Amuleto")));

            Assert.That(jugador.ManoDeObjetos.Count, Is.EqualTo(2));
            Assert.That(jugador.MazoDeObjetos.Count, Is.EqualTo(1));
        }

        [Test]
        public void IniciarObjetosSinMazoEsUnError()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));

            Assert.That(() => jugador.IniciarObjetos(null), Throws.ArgumentNullException);
        }

        [Test]
        public void EquiparConsumeElHuecoDeLaManoYLaRellena()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto"));
            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(
                Fabrica.Objeto("Espada"), Fabrica.Objeto("Escudo"), Fabrica.Objeto("Amuleto")));

            ResultadoEquipar resultado = jugador.TryEquipar(huecoManoObjeto: 0, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.Ok));
            Assert.That(jugador.Arena[0].EquippedItem?.CardId, Is.EqualTo("Espada"));
            Assert.That(jugador.ManoDeObjetos.Count, Is.EqualTo(2), "el hueco se rellena al momento");
            Assert.That(jugador.MazoDeObjetos.Count, Is.EqualTo(0));
        }

        [Test]
        public void EquiparConUnHuecoDeManoVacioNoHaceNada()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto"));
            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(Fabrica.Objeto("Espada")));
            jugador.TryEquipar(0, 0);   // vacia el unico hueco con objeto

            ResultadoEquipar resultado = jugador.TryEquipar(huecoManoObjeto: 1, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.HuecoVacio));
        }

        [Test]
        public void EquiparSobreUnCarrilVacioNoHaceNada()
        {
            PlayerState jugador = new("Ana", Fabrica.MazoDe(10));
            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(Fabrica.Objeto("Espada")));

            ResultadoEquipar resultado = jugador.TryEquipar(huecoManoObjeto: 0, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.CarrilVacio));
            Assert.That(jugador.ManoDeObjetos[0]?.CardId, Is.EqualTo("Espada"),
                "no deberia haberse gastado el objeto");
        }

        [Test]
        public void NoSePuedeEquiparUnSegundoObjetoAlMismoMonstruo()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto"));
            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(
                Fabrica.Objeto("Espada"), Fabrica.Objeto("Escudo")));
            jugador.TryEquipar(0, 0);

            // El hueco 0 quedo vacio (el mazo ya no tenia con que rellenarlo):
            // el segundo intento usa el hueco 1, que aun tiene "Escudo".
            ResultadoEquipar resultado = jugador.TryEquipar(huecoManoObjeto: 1, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.YaLlevaObjeto));
            Assert.That(jugador.Arena[0].EquippedItem?.CardId, Is.EqualTo("Espada"),
                "el primer objeto no se sustituye");
        }

        // ------------------------------------------------------------------
        // Pociones (DESIGN.md §4)
        // ------------------------------------------------------------------

        [Test]
        public void UnaPocionNoOcupaElHuecoDeObjetoNiFallaConYaLlevaObjeto()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", vida: 5));
            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(
                Fabrica.Objeto("Espada", bonusAtaque: 1),
                Fabrica.Objeto("Pocion", bonusVida: 2, pocion: true)));

            // El monstruo ya lleva un objeto normal: si la pocion pasara por
            // el mismo camino que TryEquip, fallaria con YaLlevaObjeto.
            jugador.TryEquipar(huecoManoObjeto: 0, carril: 0);
            ResultadoEquipar resultado = jugador.TryEquipar(huecoManoObjeto: 1, carril: 0);

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.Ok),
                "una pocion nunca deberia fallar con YaLlevaObjeto");
            Assert.That(jugador.Arena[0].EquippedItem?.CardId, Is.EqualTo("Espada"),
                "la pocion no toca el objeto ya equipado");
        }

        [Test]
        public void UnaPocionYUnObjetoNormalConvivenEnCualquierOrden()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", vida: 5));
            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(
                Fabrica.Objeto("Pocion", bonusVida: 2, pocion: true),
                Fabrica.Objeto("Espada", bonusAtaque: 1)));

            jugador.TryEquipar(huecoManoObjeto: 0, carril: 0);   // pocion primero
            ResultadoEquipar resultado = jugador.TryEquipar(huecoManoObjeto: 1, carril: 0); // objeto normal despues

            Assert.That(resultado, Is.EqualTo(ResultadoEquipar.Ok));
            Assert.That(jugador.Arena[0].EquippedItem?.CardId, Is.EqualTo("Espada"),
                "la pocion no ocupo el hueco de objeto, asi que el objeto normal se puede equipar despues");
            Assert.That(jugador.Arena[0].MaxHealth, Is.EqualTo(7));
        }

        [Test]
        public void ManoDeObjetosSeSigueRellenandoAlUsarUnaPocion()
        {
            PlayerState jugador = Fabrica.Jugador("Ana", Fabrica.Monstruo("Bruto", vida: 5));
            jugador.IniciarObjetos(Fabrica.MazoDeObjetos(
                Fabrica.Objeto("Pocion", bonusVida: 2, pocion: true), Fabrica.Objeto("Escudo"), Fabrica.Objeto("Amuleto")));

            jugador.TryEquipar(huecoManoObjeto: 0, carril: 0);

            Assert.That(jugador.ManoDeObjetos.Count, Is.EqualTo(2), "el hueco se rellena al momento");
            Assert.That(jugador.MazoDeObjetos.Count, Is.EqualTo(0));
        }
    }
}
