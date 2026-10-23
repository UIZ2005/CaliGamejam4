using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class Mezcladora : MonoBehaviour
{
    [Header("Configuración de Mezcla")]
    public GameObject prefabConcreto;
    [Tooltip("El lugar exacto donde aparecerá el bloque terminado")]
    public Transform puntoAparicionConcreto; 
    public float tiempoMezclado = 3f;
    [Tooltip("Cuánto va a crecer la mezcladora durante la animación")]
    public float incrementoEscala = 0.3f;
    
    [Tooltip("Velocidad de la animación (Menor = Más suave y lento)")]
    public float velocidadAnimacion = 5f;

    [Header("Interfaz (UI)")]
    public GameObject canvasMezcladora;
    public Text textoMateriales; 
    public Slider barraProgreso;

    // Contadores internos
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
                ActualizarUI();
                ComprobarMezcla();
            }
        }
        else if (necesitaCemento > 0 && other.TryGetComponent(out ItemCemento cemento))
        {
            Destroy(cemento.gameObject);
            necesitaCemento--;
            ActualizarUI();
            ComprobarMezcla();
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

        float tiempo = 0;

        while (tiempo < tiempoMezclado)
        {
            tiempo += Time.deltaTime;
            barraProgreso.value = tiempo / tiempoMezclado;

            // Fórmula matemática suavizada (Onda Senoidal)
            // Esto oscila suavemente entre 0 y 1, multiplicándose por el incremento deseado
            float factorSuavizado = (Mathf.Sin(tiempo * velocidadAnimacion) + 1f) / 2f;
            float escalaActual = factorSuavizado * incrementoEscala;

            transform.localScale = escalaOriginal + new Vector3(escalaActual, escalaActual, escalaActual);

            yield return null; 
        }

        transform.localScale = escalaOriginal; 
        
        if(prefabConcreto != null && puntoAparicionConcreto != null)
        {
            Instantiate(prefabConcreto, puntoAparicionConcreto.position, puntoAparicionConcreto.rotation);
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