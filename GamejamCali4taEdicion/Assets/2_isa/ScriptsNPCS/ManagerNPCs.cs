using System.Collections.Generic;
using UnityEngine;

public class ManagerNPCs : MonoBehaviour
{
    public static ManagerNPCs Instancia { get; private set; }

    [System.Serializable]
    public class MenuNivel
    {
        public TipoItem[] comidas = { TipoItem.Arepa };
        public TipoItem[] bebidas = { TipoItem.Cafe };

        [Tooltip("Probabilidad de que además pida una bebida (0 = nunca, 1 = siempre)")]
        [Range(0f, 1f)] public float probabilidadBebida = 0.6f;
    }

    [System.Serializable]
    public class IconoPedido
    {
        public TipoItem tipo;
        public Sprite icono;
    }

    
    
    [SerializeField] private NPC[] prefabsNPC;
    public int maxEnFila = 3;
    public float tiempoEntreNPCs = 6f;
    [Tooltip("Segundos antes de que llegue el primero")]
    public float tiempoPrimerNPC = 2f;

    
    
    [SerializeField] private Transform puntoAparicion;

    [Tooltip("El primer puesto de la fila, frente a la ventana. La fila crece hacia donde apunta su flecha AZUL")]
    [SerializeField] private Transform puntoVentana;
    
    [SerializeField] private Transform puntoSalida;
    
    [SerializeField] private float separacion = 1.2f;

    [Header("Menú por nivel (Element 0 = nivel 1)")]
    [SerializeField] private MenuNivel[] menuPorNivel = new MenuNivel[3];

    
    [SerializeField] private IconoPedido[] iconos;

    private readonly List<NPC> fila = new List<NPC>();
    private float timer;

    private void Awake()
    {
        Instancia = this;
        timer = tiempoPrimerNPC;
    }

    private void Start()
    {
        if (NivelManager.Instancia != null)
            NivelManager.Instancia.OnNivelEmpieza += Limpiar;
    }

    private void OnDestroy()
    {
        if (NivelManager.Instancia != null)
            NivelManager.Instancia.OnNivelEmpieza -= Limpiar;
    }

    private void Update()
    {
        if (NivelManager.Instancia != null && !NivelManager.Instancia.Jugando) return;

        timer -= Time.deltaTime;
        if (timer <= 0f && fila.Count < maxEnFila)
        {
            CrearNPC();
            timer = tiempoEntreNPCs;
        }
    }

    
    public bool Entregar(TipoItem tipo)
    {
        for (int i = 0; i < fila.Count; i++)
        {
            NPC npc = fila[i];
            if (!npc.Quiere(tipo)) continue;

            bool completo = npc.Recibir(tipo);
            if (completo)
            {
                npc.Salir(puntoSalida.position);
                fila.RemoveAt(i);
                ReordenarFila();

                if (NivelManager.Instancia != null)
                    NivelManager.Instancia.RegistrarComida(tipo);

                Debug.Log($"{npc.name} quedó servido y se va");
            }
            return true;
        }
        return false;
    }

    public Sprite IconoDe(TipoItem tipo)
    {
        foreach (var ic in iconos)
            if (ic.tipo == tipo) return ic.icono;
        return null;
    }

    // ===================== FILA =====================

    private void CrearNPC()
    {
        NPC prefab = ElegirPrefab();
        if (prefab == null) return;

        NPC npc = Instantiate(prefab, puntoAparicion.position, puntoAparicion.rotation);
        npc.PrefabOrigen = prefab.gameObject;
        npc.Iniciar(GenerarPedido(), this);

        fila.Add(npc);
        ReordenarFila();
    }

    // Elige al azar uno de los 4, sin repetir a alguien que ya está en la fila
    private NPC ElegirPrefab()
    {
        if (prefabsNPC == null || prefabsNPC.Length == 0) return null;

        var libres = new List<NPC>();
        foreach (var p in prefabsNPC)
        {
            bool yaEsta = false;
            foreach (var n in fila)
                if (n.PrefabOrigen == p.gameObject) { yaEsta = true; break; }
            if (!yaEsta) libres.Add(p);
        }

        var opciones = libres.Count > 0 ? libres : new List<NPC>(prefabsNPC);
        return opciones[Random.Range(0, opciones.Count)];
    }

    private List<TipoItem> GenerarPedido()
    {
        int nivel = NivelManager.Instancia != null ? NivelManager.Instancia.NivelActual : 1;
        MenuNivel menu = menuPorNivel[Mathf.Clamp(nivel - 1, 0, menuPorNivel.Length - 1)];

        var pedido = new List<TipoItem>();

        if (menu.comidas.Length > 0)
            pedido.Add(menu.comidas[Random.Range(0, menu.comidas.Length)]);

        bool quiereBebida = Random.value < menu.probabilidadBebida || pedido.Count == 0;
        if (menu.bebidas.Length > 0 && quiereBebida)
            pedido.Add(menu.bebidas[Random.Range(0, menu.bebidas.Length)]);

        return pedido;
    }

    // Cada NPC va a su puesto: el 0 frente a la ventana, los demás detrás
    private void ReordenarFila()
    {
        for (int i = 0; i < fila.Count; i++)
        {
            Vector3 puesto = puntoVentana.position + puntoVentana.forward * separacion * i;
            fila[i].IrA(puesto, -puntoVentana.forward); 
        }
    }

    // Al empezar un nivel nuevo se van todos y la fila arranca de cero
    private void Limpiar(int nivel)
    {
        foreach (var npc in fila)
            if (npc) Destroy(npc.gameObject);
        fila.Clear();
        timer = tiempoPrimerNPC;
    }

    // Para ver la fila en la escena
    private void OnDrawGizmosSelected()
    {
        if (puntoVentana == null) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < maxEnFila; i++)
            Gizmos.DrawWireSphere(puntoVentana.position + puntoVentana.forward * separacion * i, 0.3f);
    }
}
