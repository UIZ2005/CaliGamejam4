using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    
    // --- NUEVA VARIABLE DE CONSTRUCCIÓN ---
    private bool construccionCompletada;

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
        if (!jugando) return;

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();
            TerminarNivel();
            return;
        }

        ActualizarUI();

        if (terminarAlCumplirMetas && MetasCumplidas())
            TerminarNivel();
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
        
        // --- REINICIAMOS LA CONSTRUCCIÓN AL INICIAR EL NIVEL ---
        construccionCompletada = false;

        OcultarPaneles();
        Time.timeScale = 1f;
        jugando = true;

        ActualizarUI();
        OnNivelEmpieza?.Invoke(NivelActual);
        Debug.Log($"Empieza {N.nombre}");
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

        // --- ACTUALIZAMOS EL MOTIVO DE GAME OVER PARA INCLUIR CONSTRUCCIÓN ---
        string motivo = "";
        if (comidas < N.metaComidas) 
            motivo += $"Faltaron {N.metaComidas - comidas} comidas por servir.\n";
        if (!construccionCompletada) 
            motivo += "La construcción no se terminó a tiempo.";

        if (textoMotivoGameOver) textoMotivoGameOver.text = motivo;
        Debug.Log("Perdió: " + motivo);
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
        
        // Comprobar si ya ganaron al servir la comida
        if (terminarAlCumplirMetas && MetasCumplidas()) TerminarNivel();
    }
    
    // --- NUEVO MÉTODO PARA AVISAR QUE LA CONSTRUCCIÓN TERMINÓ ---
    public void RegistrarConstruccionCompletada()
    {
        if (!jugando) return;
        construccionCompletada = true;
        Debug.Log($"Construcción completada para el nivel {NivelActual}");
        
        // Comprobar si ya ganaron al poner el último ladrillo
        if (terminarAlCumplirMetas && MetasCumplidas()) TerminarNivel();
    }

    private bool MetasCumplidas()
    {
        // --- AHORA EXIGE AMBAS COSAS ---
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
            textoComidas.text = $"Comidas: {comidas}/{N.metaComidas}";
    }
}