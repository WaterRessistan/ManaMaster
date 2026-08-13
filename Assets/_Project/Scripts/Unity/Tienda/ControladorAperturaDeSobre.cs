using System.Collections.Generic;
using ManaMaster.Core.Cards;
using ManaMaster.Unity.Cards;
using ManaMaster.Unity.Duelo;
using UnityEngine;
using UnityEngine.UI;

namespace ManaMaster.Unity.Tienda
{
    /// <summary>
    /// Pantalla superpuesta que revela, una a una, las cartas de un sobre ya
    /// comprado, y termina en un resumen.
    /// </summary>
    /// <remarks>
    /// Puramente cosmetico: la coleccion del jugador ya se actualizo en
    /// <see cref="VistaOfertaTienda.Comprar"/> antes de llamar a
    /// <see cref="Mostrar"/>, asi que cerrar esta pantalla sin llegar al
    /// resumen no pierde ni duplica ninguna carta.
    /// </remarks>
    public sealed class ControladorAperturaDeSobre : MonoBehaviour
    {
        [SerializeField] private GameObject panelRaiz;

        [Header("Revelado")]
        [SerializeField] private GameObject panelRevelado;
        [SerializeField] private GameObject dorso;
        [SerializeField] private VistaCartaMonstruo carta;
        [SerializeField] private Text instruccion;
        [SerializeField] private Button botonAvanzar;

        [Header("Resumen")]
        [SerializeField] private GameObject panelResumen;
        [SerializeField] private Text[] filasResumen;
        [SerializeField] private Button botonCerrar;

        private readonly List<MonsterCardDefinition> _cartas = new();

        /// <summary>-1 = dorso sin revelar; 0..N-1 = indice de la carta revelada.</summary>
        private int _indice;

        private void OnEnable()
        {
            if (botonAvanzar != null)
            {
                botonAvanzar.onClick.AddListener(AlPulsarAvanzar);
            }

            if (botonCerrar != null)
            {
                botonCerrar.onClick.AddListener(Cerrar);
            }
        }

        private void OnDisable()
        {
            if (botonAvanzar != null)
            {
                botonAvanzar.onClick.RemoveListener(AlPulsarAvanzar);
            }

            if (botonCerrar != null)
            {
                botonCerrar.onClick.RemoveListener(Cerrar);
            }
        }

        /// <summary>Abre la pantalla con las cartas que acaba de dar el sobre.</summary>
        public void Mostrar(IReadOnlyList<string> cardIds, CardCatalog catalogo)
        {
            if (panelRaiz == null || catalogo == null || cardIds == null)
            {
                return;
            }

            _cartas.Clear();
            foreach (string cardId in cardIds)
            {
                MonsterCardDefinition definicion = catalogo.FindMonster(cardId);
                if (definicion != null)
                {
                    _cartas.Add(definicion);
                }
            }

            if (_cartas.Count == 0)
            {
                return;
            }

            _indice = -1;

            panelRaiz.SetActive(true);
            if (panelRevelado != null)
            {
                panelRevelado.SetActive(true);
            }

            if (panelResumen != null)
            {
                panelResumen.SetActive(false);
            }

            MostrarDorso();
        }

        /// <summary>Conectado al boton grande de "toca para continuar".</summary>
        public void AlPulsarAvanzar()
        {
            if (_cartas.Count == 0)
            {
                return;
            }

            if (_indice < _cartas.Count - 1)
            {
                _indice++;
                MostrarCartaActual();
            }
            else
            {
                MostrarResumen();
            }
        }

        /// <summary>Conectado al boton "Cerrar" del resumen.</summary>
        public void Cerrar()
        {
            if (panelRaiz != null)
            {
                panelRaiz.SetActive(false);
            }
        }

        private void MostrarDorso()
        {
            if (dorso != null)
            {
                dorso.SetActive(true);
            }

            if (carta != null)
            {
                carta.Ocultar();
            }

            ActualizarInstruccion();
        }

        private void MostrarCartaActual()
        {
            if (dorso != null)
            {
                dorso.SetActive(false);
            }

            if (carta != null)
            {
                carta.Mostrar(new CardInstance(_cartas[_indice]));
            }

            ActualizarInstruccion();
        }

        private void ActualizarInstruccion()
        {
            if (instruccion == null)
            {
                return;
            }

            instruccion.text = _indice < 0
                ? $"Toca para revelar (1/{_cartas.Count})"
                : $"Toca para continuar ({_indice + 1}/{_cartas.Count})";
        }

        private void MostrarResumen()
        {
            if (panelRevelado != null)
            {
                panelRevelado.SetActive(false);
            }

            if (filasResumen != null)
            {
                for (int i = 0; i < filasResumen.Length; i++)
                {
                    if (filasResumen[i] == null)
                    {
                        continue;
                    }

                    if (i < _cartas.Count)
                    {
                        MonsterCardDefinition definicion = _cartas[i];
                        filasResumen[i].gameObject.SetActive(true);
                        filasResumen[i].text = $"{definicion.DisplayName} ({Etiqueta(definicion.Rarity)})";
                        filasResumen[i].color = ColoresDeRareza.De(definicion.Rarity);
                    }
                    else
                    {
                        filasResumen[i].gameObject.SetActive(false);
                    }
                }
            }

            if (panelResumen != null)
            {
                panelResumen.SetActive(true);
            }
        }

        private static string Etiqueta(CardRarity rareza) => rareza switch
        {
            CardRarity.Comun => "Comun",
            CardRarity.Rara => "Rara",
            CardRarity.Epica => "Epica",
            CardRarity.Legendaria => "Legendaria",
            _ => rareza.ToString(),
        };
    }
}
