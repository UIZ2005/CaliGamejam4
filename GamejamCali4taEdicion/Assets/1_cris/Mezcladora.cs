using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class Mezcladora : MonoBehaviour
{
    [Header("Configuración de Mezcla")]
    public GameObject prefabConcreto;
    public Transform puntoAparicionConcreto;
    public float tiempoMezclado = 3f;
    public float incrementoEscala = 0.3f;
    public float velocidadAnimacion = 5f;

    [Header("Interfaz (UI)")]
    public GameObject canvasMezcladora;
    public Text textoMateriales;
    public Slider barraProgreso;

    [Header("Sonidos")]
    public AudioSource audioMaquina;
    public AudioClip sonidoTerminar;
    public AudioClip sonidoRecibirMaterial; // <-- NUEVO: Sonido al depositar agua/cemento
    [Tooltip("Tiempo en segundos para apagar el motor suavemente")]
    public float tiempoFadeOut = 0.2f;

    private int necesitaAgua = 1;
    private int necesitaCemento = 1;
    private bool estaMezclando = false;
    private Vector3 escalaOriginal;

    void Start()
    {
        escalaOriginal = transform.localScale;
        GetComponent<BoxCollider>().isTrigger = true;
        ResetearMezcladora();
    }

    private void OnTriggerStay(Collider other)
    {
        if (estaMezclando || other.isTrigger) return;

        if (necesitaAgua > 0 && other.TryGetComponent(out CubetaAgua cubeta))
        {
            if (cubeta.estaLlena)
            {
                cubeta.Vaciar();
                necesitaAgua--;
                SonarRecepcion(); // <-- Dispara el sonido
                ActualizarUI();
                ComprobarMezcla();
            }
        }
        else if (necesitaCemento > 0 && other.TryGetComponent(out ItemCemento cemento))
        {
            Destroy(cemento.gameObject);
            necesitaCemento--;
            SonarRecepcion(); // <-- Dispara el sonido
            ActualizarUI();
            ComprobarMezcla();
        }
    }

    private void SonarRecepcion()
    {
        if (audioMaquina != null && sonidoRecibirMaterial != null)
        {
            audioMaquina.PlayOneShot(sonidoRecibirMaterial);
        }
    }

    void ActualizarUI()
    {
        string texto = "";
        if (necesitaAgua > 0) texto += "Agua: " + necesitaAgua + "\n";
        if (necesitaCemento > 0) texto += "Cemento: " + necesitaCemento;
        textoMateriales.text = texto;
    }

    void ComprobarMezcla()
    {
        if (necesitaAgua == 0 && necesitaCemento == 0)
        {
            StartCoroutine(RutinaMezclar());
        }
    }

    private IEnumerator RutinaMezclar()
    {
        estaMezclando = true;
        textoMateriales.gameObject.SetActive(false);
        barraProgreso.gameObject.SetActive(true);
        barraProgreso.value = 0;

        if (audioMaquina != null)
        {
            audioMaquina.volume = 1f;
            audioMaquina.Play();
        }

        float tiempo = 0;
        while (tiempo < tiempoMezclado)
        {
            tiempo += Time.deltaTime;
            barraProgreso.value = tiempo / tiempoMezclado;

            float factorSuavizado = (Mathf.Sin(tiempo * velocidadAnimacion) + 1f) / 2f;
            float escalaActual = factorSuavizado * incrementoEscala;
            transform.localScale = escalaOriginal + new Vector3(escalaActual, escalaActual, escalaActual);

            if (audioMaquina != null && tiempo > (tiempoMezclado - tiempoFadeOut))
            {
                float tiempoRestante = tiempoMezclado - tiempo;
                audioMaquina.volume = tiempoRestante / tiempoFadeOut;
            }
            yield return null;
        }

        transform.localScale = escalaOriginal;
        if (audioMaquina != null)
        {
            audioMaquina.Stop();
            audioMaquina.volume = 1f;
        }

        if (prefabConcreto != null && puntoAparicionConcreto != null)
        {
            Vector3 variacionPosicion = new Vector3(Random.Range(-0.15f, 0.15f), 0.5f, Random.Range(-0.15f, 0.15f));
            GameObject nuevoConcreto = Instantiate(prefabConcreto, puntoAparicionConcreto.position + variacionPosicion, puntoAparicionConcreto.rotation);

            if (nuevoConcreto.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (audioMaquina != null && sonidoTerminar != null)
                audioMaquina.PlayOneShot(sonidoTerminar);
        }

        ResetearMezcladora();
    }

    void ResetearMezcladora()
    {
        estaMezclando = false;
        necesitaAgua = 1;
        necesitaCemento = 1;
        barraProgreso.gameObject.SetActive(false);
        textoMateriales.gameObject.SetActive(true);
        ActualizarUI();
    }
}