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

    // 1, 2 o 3. Útil para probar un nivel directo
    [SerializeField] private int nivelInicial = 1;
    
    [SerializeField] private bool terminarAlCumplirMetas = false;

    
    [SerializeField] private Text textoTiempo;
    [SerializeField] private Text textoComidas;

    [Header("Paneles")]
     public GameObject panelInicioNivel;
    public TMP_Text textoTituloNivel;
    public GameObject panelGameOver;
    public TMP_Text textoMotivoGameOver;
    public GameObject panelVictoria;

    private int indice;              
    private float tiempoRestante;
    private int comidas;
    private bool jugando;

    public int NivelActual => indice + 1;   // 1, 2, 3
    public bool Jugando => jugando;
    public System.Action<int> OnNivelEmpieza;  // por si otros scripts quieren reaccionar

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
        Time.timeScale = 0f; // todo quieto mientras se muestra el panel

        OcultarPaneles();
        ActivarObjetosDelNivel(i);

        if (panelInicioNivel) panelInicioNivel.SetActive(true);
        if (textoTituloNivel) textoTituloNivel.text = N.nombre;
    }

    
    public void EmpezarNivel()
    {
        tiempoRestante = N.duracionSegundos;
        comidas = 0;

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

        string motivo = $"altaron {N.metaComidas - comidas} comidas por servir.";

        if (textoMotivoGameOver) textoMotivoGameOver.text = motivo;
        Debug.Log("Gperdio " + motivo);
    }

    // Botón "Reintentar" o "Jugar de nuevo"  recarga la escena
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
    }

    private bool MetasCumplidas()
    {
        return comidas >= N.metaComidas;
    }

    

    private void ActivarObjetosDelNivel(int i)
    {
        // Primero se apagan los de todos los niveles y luego se prende el actual,
        // así un objeto que está en varios niveles queda prendido.
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
