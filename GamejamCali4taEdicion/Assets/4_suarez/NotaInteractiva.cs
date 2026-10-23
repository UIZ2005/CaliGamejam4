
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NotaInteractiva : MonoBehaviour
{
    [Header("Camara")]
    public Camera camara;

    [Header("Objetos que aparecen")]
    public GameObject[] objetosAActivar;

    [Header("Flotacion de la nota")]
    public float alturaFlotacion = 0.12f;

    [Header("Caida de los objetos")]
    public float alturaCaida = 2.0f;
    public float duracionCaida = 0.8f;

    [Header("Sonidos")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoHover;
    public AudioClip sonidoAterrizaje;

    [Range(0f, 1f)]
    public float volumenHover = 0.8f;

    [Range(0f, 1f)]
    public float volumenAterrizaje = 0.8f;

    [Header("Animacion del menu")]
    public Animaciones animaciones;

    private Vector3 posicionInicial;
    private Vector3[] posicionesFinales;
    private bool cursorEncima;
    private bool abierta;

    // Cuenta cuantos objetos faltan por aterrizar.
    private int objetosPendientes;

    void Start()
    {
        posicionInicial = transform.position;

        if (camara == null)
            camara = Camera.main;

        if (fuenteAudio == null)
            fuenteAudio = GetComponent<AudioSource>();

        posicionesFinales = new Vector3[objetosAActivar.Length];

        for (int i = 0; i < objetosAActivar.Length; i++)
        {
            GameObject objeto = objetosAActivar[i];

            if (objeto != null)
            {
                posicionesFinales[i] = objeto.transform.position;
                objeto.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (camara == null || Mouse.current == null)
            return;

        Ray rayo = camara.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        bool estabaEncima = cursorEncima;

        cursorEncima =
            Physics.Raycast(rayo, out RaycastHit hit, 1000f)
            && hit.collider.transform == transform;

        if (cursorEncima != estabaEncima)
        {
            transform.position = posicionInicial +
                (cursorEncima && !abierta
                    ? Vector3.up * alturaFlotacion
                    : Vector3.zero);

            if (cursorEncima && !abierta)
            {
                ReproducirSonido(sonidoHover, volumenHover);
            }
        }

        if (cursorEncima &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            AbrirNota();
        }
    }

    void AbrirNota()
    {
        if (abierta)
            return;

        abierta = true;
        cursorEncima = false;
        transform.position = posicionInicial;

        // Contar los objetos validos antes de iniciar las caidas.
        objetosPendientes = 0;

        for (int i = 0; i < objetosAActivar.Length; i++)
        {
            if (objetosAActivar[i] != null)
                objetosPendientes++;
        }

        // Evitar esperar indefinidamente si no hay objetos.
        if (objetosPendientes == 0)
        {
            Debug.LogWarning(
                "NotaInteractiva: no hay objetos para activar."
            );
            return;
        }

        for (int i = 0; i < objetosAActivar.Length; i++)
        {
            GameObject objeto = objetosAActivar[i];

            if (objeto == null)
                continue;

            Vector3 destino = posicionesFinales[i];

            objeto.transform.position =
                destino + Vector3.up * alturaCaida;

            Rigidbody rb = objeto.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            objeto.SetActive(true);

            StartCoroutine(CaerObjeto(objeto.transform, destino));
        }

        Debug.Log(
            "Nota abierta: comienza la caida de los objetos."
        );
    }

    IEnumerator CaerObjeto(Transform objeto, Vector3 destino)
    {
        Vector3 inicio = objeto.position;
        float tiempo = 0f;

        while (tiempo < duracionCaida)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionCaida
            );

            float suavizado = Mathf.SmoothStep(
                0f, 1f, progreso
            );

            objeto.position = Vector3.Lerp(
                inicio, destino, suavizado
            );

            yield return null;
        }

        objeto.position = destino;

        // Mantener el sonido de aterrizaje existente.
        ReproducirSonido(
            sonidoAterrizaje,
            volumenAterrizaje
        );

        // Este objeto ya termino de caer.
        objetosPendientes--;

        // Solo cuando TODOS han aterrizado, mostrar el menu.
        if (objetosPendientes == 0)
        {
            Debug.Log(
                "Todos los objetos aterrizaron. Se activa el menu."
            );

            if (animaciones != null)
            {
                animaciones.MostrarCanvasConAnimacion();
            }
            else
            {
                Debug.LogError(
                    "NotaInteractiva: falta asignar el componente Animaciones."
                );
            }
        }
    }

    void ReproducirSonido(AudioClip clip, float volumen)
    {
        if (fuenteAudio != null && clip != null)
        {
            fuenteAudio.PlayOneShot(clip, volumen);
        }
    }
}
