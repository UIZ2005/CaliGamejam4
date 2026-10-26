using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para usar Corrutinas (WaitForSeconds)

public class NivelManager : MonoBehaviour
{
    public static NivelManager Instancia { get; private set; }

    [System.Serializable]
    public class Nivel
    {
        public string nombre = "Nivel 1";
        public float duracionSegundos = 180f;
        public int metaComidas = 8;
        public GameObject[] objetosDelNivel;
        public GameObject panelIngredientes;
    }

    [SerializeField] private Nivel[] niveles = new Nivel[3];
    [SerializeField] private int nivelInicial = 1;
    [SerializeField] private bool terminarAlCumplirMetas = false;

    [SerializeField] private Text textoTiempo;
    [SerializeField] private Text textoComidas;

    [Header("Paneles")]
    public GameObject panelInicioNivel;
    public Text textoTituloNivel;
    public GameObject panelGameOver;
    public Text textoMotivoGameOver;
    public GameObject panelVictoria;

    private int indice;              
    private float tiempoRestante;
    private int comidas;
    private bool jugando;
    
    // Variables de control de estado
    private bool construccionCompletada;
    private bool enTransicion; // Nueva variable para saber si estamos esperando los 2 segundos

    public int NivelActual => indice + 1;   // 1, 2, 3
    public bool Jugando => jugando;
    public System.Action<int> OnNivelEmpieza;



    private Nivel N => niveles[indice];

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        MostrarInicioNivel(Mathf.Clamp(nivelInicial - 1, 0, niveles.Length - 1));
    }

    private void Update()
    {
        // Si no estamos jugando (por ejemplo, si estamos en los 2 segundos de espera), el reloj no avanza
        if (!jugando) return;

        tiempoRestante -= Time.deltaTime;
        
        // ¿Qué pasa si el reloj llega a 0?
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();
            
            // Verificamos si en el último milisegundo alcanzaron a cumplir las metas
            if (MetasCumplidas())
            {
                IniciarTransicionVictoria();
            }
            else
            {
                TerminarNivel(); // Aquí sí pierden
            }
            return;
        }

        ActualizarUI();

        if (terminarAlCumplirMetas && MetasCumplidas())
        {
            IniciarTransicionVictoria();
        }
    }

    private void MostrarInicioNivel(int i)
    {
        indice = i;
        jugando = false;
        Time.timeScale = 0f; 

        OcultarPaneles();
        ActivarObjetosDelNivel(i);

        if (panelInicioNivel) panelInicioNivel.SetActive(true);
        if (textoTituloNivel) textoTituloNivel.text = N.nombre;
    }

    public void EmpezarNivel()
    {
        tiempoRestante = N.duracionSegundos;
        comidas = 0;
        
        construccionCompletada = false;
        enTransicion = false; // Reiniciamos el estado de transición

        LimpiarMaterialesSueltos();

        OcultarPaneles();
        Time.timeScale = 1f;
        jugando = true;

        ActualizarUI();
        OnNivelEmpieza?.Invoke(NivelActual);
        Debug.Log($"Empieza {N.nombre}");
    }

    // --- NUEVAS FUNCIONES PARA LA ESPERA DE 2 SEGUNDOS ---
    private void IniciarTransicionVictoria()
    {
        if (enTransicion) return; // Evita que se llame varias veces
        enTransicion = true;
        
        // ¡CRÍTICO! Al poner jugando en false, el Update detiene el cronómetro inmediatamente
        jugando = false; 
        
        // Iniciamos la cuenta regresiva de 2 segundos en segundo plano
        StartCoroutine(RutinaVictoria());
    }

    private IEnumerator RutinaVictoria()
    {
        // Espera exactamente 2 segundos sin pausar el juego para que se vean las animaciones
        yield return new WaitForSeconds(2f);
        
        // Después de 2 segundos, lanza la siguiente pantalla
        TerminarNivel();
    }
    // -----------------------------------------------------

    private void LimpiarMaterialesSueltos()
    {
        foreach (var item in FindObjectsOfType<ItemConcreto>()) Destroy(item.gameObject);
        foreach (var item in FindObjectsOfType<ItemLadrillo>()) Destroy(item.gameObject);
        foreach (var item in FindObjectsOfType<ItemCemento>()) Destroy(item.gameObject);
        foreach (var item in FindObjectsOfType<ItemMadera>()) Destroy(item.gameObject);

        foreach (var cubeta in FindObjectsOfType<CubetaAgua>())
        {
            cubeta.Vaciar();
        }
    }

    private void TerminarNivel()
    {
        jugando = false;
        Time.timeScale = 0f;

        if (!MetasCumplidas())
        {
            GameOver();
            return;
        }

        if (indice + 1 < niveles.Length)
        {
            MostrarInicioNivel(indice + 1);
        }
        else
        {
            OcultarPaneles();
            if (panelVictoria) panelVictoria.SetActive(true);
            Debug.Log("Ganaron");
        }
    }

    private void GameOver()
    {
        OcultarPaneles();
        if (panelGameOver) panelGameOver.SetActive(true);

        string motivo = "";
        if (comidas < N.metaComidas) 
            motivo += $"Faltaron {N.metaComidas - comidas} personas por atender.\n";
        if (!construccionCompletada) 
            motivo += "La construcción no se terminó a tiempo.";

        if (textoMotivoGameOver) textoMotivoGameOver.text = motivo;
        Debug.Log("Perdió: " + motivo);
        foreach (var n in niveles)
            if (n.panelIngredientes) n.panelIngredientes.SetActive(false);

    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RegistrarComida(TipoItem tipo)
    {
        if (!jugando) return;
        comidas++;
        Debug.Log($"Comedor: entregado {tipo}. {comidas}/{N.metaComidas}");
        
        if (terminarAlCumplirMetas && MetasCumplidas()) IniciarTransicionVictoria();
    }
    
    public void RegistrarConstruccionCompletada()
    {
        if (!jugando) return;
        construccionCompletada = true;
        Debug.Log($"Construcción completada para el nivel {NivelActual}");
        
        if (terminarAlCumplirMetas && MetasCumplidas()) IniciarTransicionVictoria();
    }

    private bool MetasCumplidas()
    {
        return (comidas >= N.metaComidas) && construccionCompletada;
    }

    private void ActivarObjetosDelNivel(int i)
    {
        foreach (var n in niveles)
            foreach (var go in n.objetosDelNivel)
                if (go) go.SetActive(false);

        foreach (var go in niveles[i].objetosDelNivel)
            if (go) go.SetActive(true);
    }

    private void OcultarPaneles()
    {
        if (panelInicioNivel) panelInicioNivel.SetActive(false);
        if (panelGameOver) panelGameOver.SetActive(false);
        if (panelVictoria) panelVictoria.SetActive(false);
    }

    private void ActualizarUI()
    {
        if (textoTiempo)
        {
            int t = Mathf.CeilToInt(tiempoRestante);
            textoTiempo.text = $"{t / 60}:{t % 60:00}";
        }
        if (textoComidas)
            textoComidas.text = $"{comidas}/{N.metaComidas}";
    }

    public void Siguiente()
    {
        if (N.panelIngredientes != null)
        {
            if (panelInicioNivel) panelInicioNivel.SetActive(false);
            N.panelIngredientes.SetActive(true);
        }
        else
        {
            EmpezarNivel();
        }
    }
    public void finaljuego()
    {
        SceneManager.LoadScene("New Scene");
    }
}