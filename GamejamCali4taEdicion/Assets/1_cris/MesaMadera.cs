using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class MesaMadera : MonoBehaviour
{
    [Header("Configuración de Corte")]
    public float tiempoParaCortar = 3f;
    public GameObject prefabTablaMadera;
    
    [Header("Efectos y UI")]
    public GameObject canvasMesa; // Se mantiene para tener la referencia
    public Slider barraProgreso;
    
    [Tooltip("El RectTransform de la Imagen que va a palpitar")]
    public RectTransform imagenAnimada;
    public float incrementoEscala = 0.3f;
    public float velocidadAnimacion = 12f;

    private float progresoActual = 0f;
    private moveplayer jugadorCortando = null;
    private Rigidbody rbJugador = null;
    private Vector3 escalaOriginalImagen;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        
        // Aseguramos que el Canvas principal esté encendido siempre
        if (canvasMesa != null) canvasMesa.SetActive(true); 
        
        // Al iniciar (0%), ocultamos tanto la barra como la imagen
        barraProgreso.gameObject.SetActive(false);
        barraProgreso.value = 0;

        if (imagenAnimada != null)
        {
            imagenAnimada.gameObject.SetActive(false);
            escalaOriginalImagen = imagenAnimada.localScale;
        }
    }

    public void IniciarCorte(moveplayer jugador)
    {
        // Si ya hay alguien trabajando, ignoramos
        if (jugadorCortando == jugador) return;

        jugadorCortando = jugador;
        rbJugador = jugador.GetComponent<Rigidbody>();
        
        // Mostrar la barra de progreso y la imagen porque inició el proceso
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
            // 1. Cancelación ESTRICTA: 
            // - Si agarra un objeto
            // - Si se aleja (aunque como cortamos el movimiento, esto es por si lo empujan)
            // - Si SE MUEVE aunque sea un poquito (velocidad > 0.05f cancela inmediatamente)
            if (jugadorCortando.TieneObjetoAgarrado() || 
                Vector3.Distance(transform.position, jugadorCortando.transform.position) > 2.5f ||
                rbJugador.linearVelocity.magnitude > 0.05f) 
            {
                PausarCorte();
                return; // Corta la ejecución de este Update para que no sume tiempo
            }

            // 2. Si pasó las validaciones de arriba, está 100% quieto trabajando
            progresoActual += Time.deltaTime;
            barraProgreso.value = progresoActual / tiempoParaCortar;

            // Animar la imagen (Latido de caricatura)
            if (imagenAnimada != null)
            {
                float factorSuavizado = (Mathf.Sin(Time.time * velocidadAnimacion) + 1f) / 2f;
                float escalaActual = factorSuavizado * incrementoEscala;
                imagenAnimada.localScale = escalaOriginalImagen + new Vector3(escalaActual, escalaActual, escalaActual);
            }

            // 3. Si terminó el progreso
            if (progresoActual >= tiempoParaCortar)
            {
                EntregarMadera();
            }
        }
    }

    private void EntregarMadera()
    {
        // Instancia la tabla y se la entrega directo a la mano
        GameObject nuevaMadera = Instantiate(prefabTablaMadera, jugadorCortando.puntoAgarrePoder.position, Quaternion.identity);
        
        if (nuevaMadera.TryGetComponent(out GrabObject grab))
        {
            jugadorCortando.ForzarAgarre(grab);
        }

        // Como el proceso se ACABÓ, reseteamos el progreso y ocultamos TODO
        progresoActual = 0f;
        barraProgreso.value = 0;
        barraProgreso.gameObject.SetActive(false); // Ocultar barra al terminar
        
        if (imagenAnimada != null)
        {
            imagenAnimada.gameObject.SetActive(false); // Ocultar imagen al terminar
        }
            
        // Liberar al jugador
        jugadorCortando = null;
        rbJugador = null;
    }

    private void PausarCorte()
    {
        // Liberamos las variables. Esto obliga al jugador a presionar 'Interactuar' de nuevo
        jugadorCortando = null;
        rbJugador = null;
        
        // Como se INTERRUMPIÓ, ocultamos SOLO la imagen
        if (imagenAnimada != null)
        {
            imagenAnimada.gameObject.SetActive(false);
            imagenAnimada.localScale = escalaOriginalImagen; // Resetear escala visual
        }
        
        // ¡OJO! Aquí NO apagamos `barraProgreso.gameObject.SetActive(false);`
        // Esto garantiza que la barra de progreso se quede flotando y visible, 
        // indicando exactamente por dónde iba el trabajo.
    }
}