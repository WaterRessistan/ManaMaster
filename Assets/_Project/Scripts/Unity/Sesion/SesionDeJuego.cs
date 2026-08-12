using System;
using System.Collections.Generic;
using System.IO;
using ManaMaster.Unity.Cards;
using UnityEngine;

namespace ManaMaster.Unity.Sesion
{
    /// <summary>
    /// Datos del jugador que cruzan de una escena a otra y sobreviven a
    /// cerrar el juego: diamantes, coleccion de cartas y mazo elegido.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Es el mismo mecanismo que <c>CardCatalog</c>: un asset unico que cada
    /// escena referencia por <c>[SerializeField]</c> y que el editor cablea al
    /// generarlas. No es un singleton <c>DontDestroyOnLoad</c> ni un campo
    /// <c>static</c> (CLAUDE.md prohibe el estado estatico mutable, que fue el
    /// problema central de la Fase 1).
    /// </para>
    /// <para>
    /// Los datos de juego siguen sin <c>[SerializeField]</c> a proposito:
    /// Unity nunca los escribe en el <c>.asset</c> versionado en git — mezclar
    /// datos de partida del jugador con un asset compartido del repositorio
    /// seria un error grave. La persistencia real es un fichero JSON aparte,
    /// en <see cref="Application.persistentDataPath"/>, gestionado a mano por
    /// esta clase (Fase 4). Es local y no se protege de manipulacion a
    /// proposito: no hay online todavia, asi que no hay a quien perjudicar
    /// tramposeando con el propio guardado. Eso cambia con el servidor
    /// autoritativo de la Fase 9.
    /// </para>
    /// <para>
    /// <see cref="HideFlags.DontUnloadUnusedAsset"/> es obligatorio: al cargar
    /// una escena en modo <c>Single</c>, Unity descarga los assets que la
    /// escena entrante no referencia. Deckbuild guarda el mazo y vuelve a
    /// Inicio, que no referencia esta sesion — sin esta flag, el salto por
    /// Inicio la descargaria y Duelo cargaria una instancia nueva y vacia,
    /// perdiendo el mazo elegido en el camino.
    /// </para>
    /// <para>
    /// Solo las mutaciones escriben a disco: leer (cargar una pantalla) nunca
    /// deja rastro en el guardado, solo pulsar comprar, terminar una partida o
    /// guardar un mazo. Eso es lo que permite que un test cargue una escena
    /// real sin tocar el guardado del desarrollador, siempre que redirija la
    /// ruta con <see cref="UsarRutaDeGuardadoParaTests"/> antes de disparar la
    /// accion que muta.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Mana Master/Sesion de juego", fileName = "SesionDeJuego")]
    public sealed class SesionDeJuego : ScriptableObject
    {
        private const string NombreDeFichero = "perfil.json";
        private const int DiamantesDeCuentaNueva = 500;

        [Tooltip("Solo para inicializar la cuenta nueva: 1 copia de cada " +
                 "monstruo y un mazo ya listo con esas mismas 10 cartas.")]
        [SerializeField] private CardCatalog catalogo;

        private readonly List<string> _mazoHumano = new();
        private readonly Dictionary<string, int> _coleccion = new();
        private int _diamantes;
        private bool _cargado;
        private string _rutaDeGuardadoParaTests;

        /// <summary>Diamantes, coleccion o mazo elegido han cambiado.</summary>
        public event Action Cambiada;

        private void OnEnable() => hideFlags |= HideFlags.DontUnloadUnusedAsset;

        public int Diamantes
        {
            get { AsegurarCargado(); return _diamantes; }
        }

        /// <summary>CardId de las cartas elegidas en Deckbuild, en orden de eleccion.</summary>
        public IReadOnlyList<string> MazoHumano
        {
            get { AsegurarCargado(); return _mazoHumano; }
        }

        /// <summary>Si hay un mazo elegido en esta sesion.</summary>
        public bool TieneMazoElegido
        {
            get { AsegurarCargado(); return _mazoHumano.Count > 0; }
        }

        /// <summary>Copias que el jugador posee de esa carta (0 si ninguna).</summary>
        public int CopiasEnColeccion(string cardId)
        {
            AsegurarCargado();
            return _coleccion.TryGetValue(cardId, out int copias) ? copias : 0;
        }

        /// <summary>Guarda el mazo elegido en Deckbuild, sustituyendo el anterior.</summary>
        public void FijarMazoHumano(IEnumerable<string> cardIds)
        {
            if (cardIds == null)
            {
                throw new ArgumentNullException(nameof(cardIds));
            }

            AsegurarCargado();
            _mazoHumano.Clear();
            _mazoHumano.AddRange(cardIds);
            GuardarYAvisar();
        }

        /// <summary>Olvida el mazo elegido, volviendo al reparto aleatorio.</summary>
        public void LimpiarMazoHumano()
        {
            AsegurarCargado();
            _mazoHumano.Clear();
            GuardarYAvisar();
        }

        /// <summary>Diamantes ganados jugando (DESIGN.md §10): victoria, derrota o empate.</summary>
        public void GanarDiamantes(int cantidad)
        {
            if (cantidad <= 0)
            {
                return;
            }

            AsegurarCargado();
            _diamantes += cantidad;
            GuardarYAvisar();
        }

        /// <summary>Descuenta el coste si hay diamantes suficientes. Si no, no toca nada.</summary>
        public bool TryGastarDiamantes(int coste)
        {
            AsegurarCargado();

            if (coste < 0 || coste > _diamantes)
            {
                return false;
            }

            _diamantes -= coste;
            GuardarYAvisar();
            return true;
        }

        /// <summary>Anade copias de una carta a la coleccion (comprada suelta o de un sobre).</summary>
        public void AnadirAColeccion(string cardId, int copias = 1)
        {
            if (string.IsNullOrEmpty(cardId) || copias <= 0)
            {
                return;
            }

            AsegurarCargado();
            _coleccion[cardId] =
                (_coleccion.TryGetValue(cardId, out int actuales) ? actuales : 0) + copias;
            GuardarYAvisar();
        }

        /// <summary>
        /// Redirige el guardado a otra ruta y olvida lo que hubiera en
        /// memoria, para que el siguiente acceso recargue desde ahi (o
        /// arranque una cuenta nueva si la ruta no existe). Solo para tests:
        /// la partida real siempre usa
        /// <see cref="Application.persistentDataPath"/>.
        /// </summary>
        /// <remarks>
        /// Olvidar el estado en memoria es imprescindible: este asset
        /// sobrevive a los cambios de escena a proposito
        /// (<see cref="HideFlags.DontUnloadUnusedAsset"/>), asi que en un
        /// mismo Play de PlayMode varios tests seguidos comparten la misma
        /// instancia. Sin este reinicio, el saldo de un test contaminaria al
        /// siguiente aunque cada uno redirigiera a su propio fichero.
        /// </remarks>
        public void UsarRutaDeGuardadoParaTests(string ruta)
        {
            _rutaDeGuardadoParaTests = ruta;
            _cargado = false;
            _diamantes = 0;
            _mazoHumano.Clear();
            _coleccion.Clear();
        }

        private void GuardarYAvisar()
        {
            Guardar();
            Cambiada?.Invoke();
        }

        private void AsegurarCargado()
        {
            if (_cargado)
            {
                return;
            }

            _cargado = true;

            string ruta = RutaDeGuardado();
            if (!File.Exists(ruta))
            {
                IniciarCuentaNueva();
                return;
            }

            Datos datos = JsonUtility.FromJson<Datos>(File.ReadAllText(ruta));
            _diamantes = datos.diamantes;
            _mazoHumano.AddRange(datos.mazoHumano);
            foreach (EntradaColeccion entrada in datos.coleccion)
            {
                _coleccion[entrada.cardId] = entrada.copias;
            }
        }

        /// <summary>
        /// Cuenta nueva (DESIGN.md §10): 500 diamantes y, si hay catalogo, 1
        /// copia de cada monstruo formando ya un mazo jugable.
        /// </summary>
        private void IniciarCuentaNueva()
        {
            _diamantes = DiamantesDeCuentaNueva;

            if (catalogo == null)
            {
                return;
            }

            foreach (MonsterCardDefinition monstruo in catalogo.Monsters)
            {
                if (monstruo == null)
                {
                    continue;
                }

                _coleccion[monstruo.CardId] = 1;
                _mazoHumano.Add(monstruo.CardId);
            }
        }

        private void Guardar()
        {
            Datos datos = new()
            {
                diamantes = _diamantes,
                mazoHumano = new List<string>(_mazoHumano),
            };

            foreach (KeyValuePair<string, int> entrada in _coleccion)
            {
                datos.coleccion.Add(
                    new EntradaColeccion { cardId = entrada.Key, copias = entrada.Value });
            }

            File.WriteAllText(RutaDeGuardado(), JsonUtility.ToJson(datos));
        }

        private string RutaDeGuardado()
            => string.IsNullOrEmpty(_rutaDeGuardadoParaTests)
                ? Path.Combine(Application.persistentDataPath, NombreDeFichero)
                : _rutaDeGuardadoParaTests;

        [Serializable]
        private sealed class Datos
        {
            public int diamantes;
            public List<string> mazoHumano = new();
            public List<EntradaColeccion> coleccion = new();
        }

        [Serializable]
        private sealed class EntradaColeccion
        {
            public string cardId;
            public int copias;
        }
    }
}
