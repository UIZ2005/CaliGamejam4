using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class MesaMadera : MonoBehaviour
{
    [Header("Configuración de Corte")]
    public float tiempoParaCortar = 3f;
    public GameObject prefabTablaMadera;
    
    [Header("Efectos y UI")]
    public GameObject canvasMesa; 
    public Slider barraProgreso;
    
    [Tooltip("El RectTransform de la Imagen que va a palpitar")]
    public RectTransform imagenAnimada;
    public float incrementoEscala = 0.3f;
    public float velocidadAnimacion = 12f;

    [Header("Sonidos")]
    public AudioSource audioTrabajo;
    public AudioClip sonidoCortar;

    private float progresoActual = 0f;
    private moveplayer jugadorCortando = null;
    private Rigidbody rbJugador = null;
    private Vector3 escalaOriginalImagen;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        
        if (canvasMesa != null) canvasMesa.SetActive(true); 
        
        barraProgreso.gameObject.SetActive(false);
        barraProgreso.value = 0;

        if (imagenAnimada != null)
        {
            imagenAnimada.gameObject.SetActive(false);
            escalaOriginalImagen = imagenAnimada.localScale;
        }

        // Configuración automática del AudioSource para que se repita
        if (audioTrabajo != null && sonidoCortar != null)
        {
            audioTrabajo.clip = sonidoCortar;
            audioTrabajo.loop = true;
            audioTrabajo.playOnAwake = false;
        }
    }

    public void IniciarCorte(moveplayer jugador)
    {
        if (jugadorCortando == jugador) return;

        jugadorCortando = jugador;
        rbJugador = jugador.GetComponent<Rigidbody>();
        
        barraProgreso.gameObject.SetActive(true);
        
        if (imagenAnimada != null)
        {
            imagenAnimada.gameObject.SetActive(true);
            imagenAnimada.localScale = escalaOriginalImagen;
        }
    }

    void Update()
    {
        if (jugadorCortando != null)
        {
            // Validaciones para cancelar el proceso
            if (jugadorCortando.TieneObjetoAgarrado() || 
                Vector3.Distance(transform.position, jugadorCortando.transform.position) > 2.5f ||
                rbJugador.linearVelocity.magnitude > 0.05f) 
            {
                PausarCorte();
                return; 
            }

            // Progreso del corte
            progresoActual += Time.deltaTime;
            barraProgreso.value = progresoActual / tiempoParaCortar;

            // Animación visual
            if (imagenAnimada != null)
            {
                float factorSuavizado = (Mathf.Sin(Time.time * velocidadAnimacion) + 1f) / 2f;
                float escalaActual = factorSuavizado * incrementoEscala;
                imagenAnimada.localScale = escalaOriginalImagen + new Vector3(escalaActual, escalaActual, escalaActual);
            }

            // Reproducir sonido en Loop mientras trabaja
            if (audioTrabajo != null && !audioTrabajo.isPlaying)
            {
                audioTrabajo.Play();
            }

            // Completado
            if (progresoActual >= tiempoParaCortar)
            {
                EntregarMadera();
            }
        }
    }

    private void EntregarMadera()
    {
        GameObject nuevaMadera = Instantiate(prefabTablaMadera, jugadorCortando.puntoAgarrePoder.position, Quaternion.identity);
        
        if (nuevaMadera.TryGetComponent(out GrabObject grab))
        {
            jugadorCortando.ForzarAgarre(grab);
        }

        progresoActual = 0f;
        barraProgreso.value = 0;
        barraProgreso.gameObject.SetActive(false); 
        
        if (imagenAnimada != null)
        {
            imagenAnimada.gameObject.SetActive(false); 
        }

        // Apagar sonido al finalizar
        if (audioTrabajo != null) audioTrabajo.Stop();
            
        jugadorCortando = null;
        rbJugador = null;
    }

    private void PausarCorte()
    {
        jugadorCortando = null;
        rbJugador = null;
        
        if (imagenAnimada != null)
        {
            imagenAnimada.gameObject.SetActive(false);
            imagenAnimada.localScale = escalaOriginalImagen; 
        }

        // Apagar sonido al interrumpir el trabajo
        if (audioTrabajo != null) audioTrabajo.Stop();
    }
}