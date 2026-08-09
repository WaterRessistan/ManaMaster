using ManaMaster.Core.Board;
using ManaMaster.Core.Match;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManaMaster.Unity.Duelo
{
    /// <summary>
    /// Un carril de la arena propia: muestra al monstruo que lo ocupa y acepta
    /// que se suelten cartas encima.
    /// </summary>
    /// <remarks>
    /// El carril sobre el que sueltas es la posicion de insercion, y lo que
    /// hubiera de ahi hacia atras retrocede (DESIGN.md §3). Quien decide si esa
    /// posicion vale es el motor, no este componente: aqui solo se pregunta.
    /// </remarks>
    [RequireComponent(typeof(RectTransform))]
    public sealed class CarrilDeInsercion : MonoBehaviour, IDropHandler
    {
        [SerializeField] private MatchController controlador;

        [Tooltip("0 = principal, 1 y 2 = traseros.")]
        [SerializeField, Min(0)] private int carril;

        [SerializeField] private VistaCartaMonstruo vista;

        [Header("Resaltado")]
        [Tooltip("Se enciende mientras arrastras una carta que puede entrar aqui.")]
        [SerializeField] private Image resaltado;

        public int Carril => carril;

        public VistaCartaMonstruo Vista => vista;

        private void Awake()
        {
            Resaltar(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (controlador == null || eventData.pointerDrag == null)
            {
                return;
            }

            if (!eventData.pointerDrag.TryGetComponent(out CartaDeMano carta))
            {
                return;
            }

            ResultadoDespliegue resultado =
                controlador.Desplegar(carta.Hueco, carril);

            if (resultado != ResultadoDespliegue.Ok)
            {
                Debug.Log($"[CarrilDeInsercion] No se pudo desplegar en " +
                          $"{BoardLanes.ToDisplayName(carril)}: {Explicar(resultado)}");
            }
        }

        /// <summary>Enciende o apaga la marca de posicion valida.</summary>
        public void Resaltar(bool encendido)
        {
            if (resaltado != null)
            {
                resaltado.enabled = encendido;
            }
        }

        private static string Explicar(ResultadoDespliegue resultado) => resultado switch
        {
            ResultadoDespliegue.ManaInsuficiente => "no hay mana suficiente",
            ResultadoDespliegue.ArenaLlena => "ya hay tres monstruos",
            ResultadoDespliegue.CarrilInvalido => "ese carril dejaria un hueco",
            ResultadoDespliegue.HuecoVacio => "no hay carta en ese hueco",
            _ => resultado.ToString()
        };
    }
}
