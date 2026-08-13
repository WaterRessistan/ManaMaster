using System.Collections;
using UnityEngine;

namespace ManaMaster.Unity.Navegacion
{
    /// <summary>
    /// Velo negro a pantalla completa que funde la entrada y la salida de
    /// cada escena, para que cambiar de pantalla no sea un corte seco.
    /// </summary>
    /// <remarks>
    /// No hace falta <c>DontDestroyOnLoad</c> ni ningun estado que sobreviva
    /// entre escenas (CLAUDE.md: sin estado static mutable): el fundido de
    /// salida ocurre en la escena vieja (que se destruye igual al cargar la
    /// siguiente) y el de entrada lo hace cada escena nueva por su cuenta al
    /// arrancar.
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class VistaTransicionDePantalla : MonoBehaviour
    {
        private const float Duracion = 0.2f;

        private CanvasGroup _grupo;

        private void Awake()
        {
            _grupo = GetComponent<CanvasGroup>();
            _grupo.alpha = 1f;
            _grupo.blocksRaycasts = true;
        }

        private void Start()
        {
            StartCoroutine(Fundir(1f, 0f));
        }

        /// <summary>
        /// Tapa la pantalla y, ya tapada, carga la escena indicada.
        /// </summary>
        /// <remarks>
        /// La corrutina corre sobre este objeto (siempre activo mientras la
        /// escena viva) y no sobre el boton que la dispara: un boton puede
        /// estar dentro de un panel que empieza apagado (p. ej. el de
        /// resultado en Duelo), y Unity no deja arrancar una corrutina en un
        /// GameObject inactivo.
        /// </remarks>
        public void IrA(string escena)
        {
            StartCoroutine(FundirYCargar(escena));
        }

        private IEnumerator FundirYCargar(string escena)
        {
            yield return Fundir(_grupo.alpha, 1f);
            UnityEngine.SceneManagement.SceneManager.LoadScene(escena);
        }

        private IEnumerator Fundir(float desde, float hasta)
        {
            _grupo.blocksRaycasts = true;

            float transcurrido = 0f;
            while (transcurrido < Duracion)
            {
                transcurrido += Time.unscaledDeltaTime;
                _grupo.alpha = Mathf.Lerp(desde, hasta, transcurrido / Duracion);
                yield return null;
            }

            _grupo.alpha = hasta;
            // Solo bloquea clics mientras tapa la pantalla: a alpha 0 no debe
            // robarle el raycast a los botones de la escena.
            _grupo.blocksRaycasts = hasta > 0f;
        }
    }
}
