
using System.Collections;
using UnityEngine;

public class Animaciones : MonoBehaviour
{
    [Header("Canvas del menu")]
    [SerializeField] private GameObject canvasUI;

    [Header("Elementos en orden de aparicion")]
    [SerializeField] private RectTransform tituloJuego;
    [SerializeField] private RectTransform botonJugar;
    [SerializeField] private RectTransform botonCreditos;

    [Header("Configuracion de la animacion")]
    [SerializeField] private float distanciaEntrada = 500f;
    [SerializeField] private float duracion = 0.45f;
    [SerializeField] private float pausaEntreElementos = 0.12f;

    [Header("Audio del menu")]
    [SerializeField] private AudioSource audioMenu;

    private RectTransform[] elementos;
    private Vector2[] posicionesFinales;
    private bool animacionIniciada;

    private void Awake()
    {
        elementos = new RectTransform[]
        {
            tituloJuego,
            botonJugar,
            botonCreditos
        };

        posicionesFinales = new Vector2[elementos.Length];

        // Guardar las posiciones originales de los elementos.
        for (int i = 0; i < elementos.Length; i++)
        {
            if (elementos[i] != null)
            {
                posicionesFinales[i] =
                    elementos[i].anchoredPosition;
            }
        }

        // Ocultar el menu al comenzar la escena.
        if (canvasUI != null)
        {
            canvasUI.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "Animaciones: falta asignar el Canvas del menu."
            );
        }

        // Comprobar que el audio este asignado.
        if (audioMenu == null)
        {
            Debug.LogWarning(
                "Animaciones: no se ha asignado el AudioSource del menu."
            );
        }
    }

    public void MostrarCanvasConAnimacion()
    {
        // Evitar que el menu y su sonido se inicien varias veces.
        if (animacionIniciada || canvasUI == null)
            return;

        animacionIniciada = true;

        // Preparar los elementos fuera de su posicion final.
        for (int i = 0; i < elementos.Length; i++)
        {
            if (elementos[i] != null)
            {
                elementos[i].anchoredPosition =
                    posicionesFinales[i]
                    + Vector2.left * distanciaEntrada;
            }
        }

        // Activar el Canvas despues de la caida de los objetos.
        canvasUI.SetActive(true);

        // Reproducir el audio del menu una sola vez.
        if (audioMenu != null && !audioMenu.isPlaying)
        {
            audioMenu.Play();
        }

        // Iniciar la animacion secuencial de los elementos.
        StartCoroutine(AnimarSecuencia());
    }

    private IEnumerator AnimarSecuencia()
    {
        for (int i = 0; i < elementos.Length; i++)
        {
            if (elementos[i] == null)
                continue;

            // Deslizar el elemento hasta su posicion original.
            yield return StartCoroutine(
                DeslizarElemento(
                    elementos[i],
                    posicionesFinales[i]
                )
            );

            // Pausa antes de que entre el siguiente elemento.
            if (i < elementos.Length - 1)
            {
                yield return new WaitForSeconds(
                    pausaEntreElementos
                );
            }
        }

        Debug.Log(
            "Animaciones: menu mostrado completamente."
        );
    }

    private IEnumerator DeslizarElemento(
        RectTransform elemento,
        Vector2 destino)
    {
        Vector2 inicio = elemento.anchoredPosition;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracion
            );

            // Movimiento suave al entrar y al detenerse.
            float suavizado =
                progreso * progreso * (3f - 2f * progreso);

            elemento.anchoredPosition =
                Vector2.Lerp(inicio, destino, suavizado);

            yield return null;
        }

        // Asegurar que termine en la posicion exacta.
        elemento.anchoredPosition = destino;
    }
}
