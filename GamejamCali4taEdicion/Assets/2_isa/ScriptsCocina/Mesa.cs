using UnityEngine;

public class Mesa : Estacion
{
    // gameobjets ubicaiocn deonde se ubicara la comida
    [SerializeField] private Transform[] slots;

    [Header("Rellenar solo (ej. cafetera). Dejar vacío para mesa normal")]
    [SerializeField] private ItemCocina prefabRelleno;
    [SerializeField] private float tiempoRelleno = 8f;
    [SerializeField] private bool empezarLlena = true;

    private ItemCocina[] items;
    private float timer;
    private bool rellenando;

    public bool EsDispensadorAuto => prefabRelleno != null;
    public bool Rellenando => rellenando;
    public float ProgresoRelleno => rellenando ? Mathf.Clamp01(timer / tiempoRelleno) : 1f; // para la barra visual

    private void Awake()
    {
        items = new ItemCocina[slots.Length];
    }

    private void Start()
    {
        
        for (int i = 0; i < slots.Length; i++)
        {
            ItemCocina inicial = slots[i].GetComponentInChildren<ItemCocina>();
            if (inicial != null)
            {
                items[i] = inicial;
                inicial.Grab.ColocarEn(slots[i]);
            }
        }

        if (EsDispensadorAuto && empezarLlena) Llenar();
    }

    private void Update()
    {
        if (!EsDispensadorAuto) return;

        if (!rellenando)
        {
            if (TodosVacios())
            {
                rellenando = true;
                timer = 0f;
                Debug.Log($"{name}: se acabó, preparando...");
            }
            return;
        }

        timer += Time.deltaTime;
        if (timer >= tiempoRelleno)
        {
            rellenando = false;
            Llenar();
            Debug.Log($"{name}: ¡listo!");
        }
    }

    private void Llenar()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (items[i] != null) continue;
            items[i] = Instantiate(prefabRelleno, slots[i].position, slots[i].rotation);
            items[i].Grab.ColocarEn(slots[i]);
        }
    }

    private bool TodosVacios()
    {
        foreach (var it in items)
            if (it != null) return false;
        return true;
    }

    // Manos vacías: me llevo el objeto más cercano 
    public override ItemCocina Entregar(Vector3 posJugador)
    {
        int i = SlotMasCercano(slots, posJugador, j => items[j] != null);
        if (i < 0) return null;

        ItemCocina item = items[i];
        items[i] = null;
        return item;
    }

    //  Traigo algo en la mano 
    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        // Cafetera: solo acepta devolver un café, y no mientras prepara
        if (EsDispensadorAuto)
        {
            if (item.tipo != prefabRelleno.tipo || rellenando) return ResultadoEstacion.Rechazado;
            return DejarEnLibre(item, posJugador);
        }

        // Traigo comida y en la mesa hay un plato vacío -> va encima del plato
        int iPlato = SlotMasCercano(slots, posJugador, i =>
            items[i] != null && items[i].PuedeRecibirComida(item));

        if (iPlato >= 0)
        {
            items[iPlato].RecibirComida(item);
            return ResultadoEstacion.ObjetoRecibido;
        }

        //  Traigo un plato vacío y en la mesa hay comida suelta -> el plato la recoge
        if (item.EsPlato)
        {
            int iComida = SlotMasCercano(slots, posJugador, i =>
                items[i] != null && item.PuedeRecibirComida(items[i]));

            if (iComida >= 0)
            {
                item.RecibirComida(items[iComida]);
                items[iComida] = null;
                return ResultadoEstacion.SeQuedaEnMano;
            }
        }

        // 3. Lo dejo en el espacio libre más cercano
        return DejarEnLibre(item, posJugador);
    }

    private ResultadoEstacion DejarEnLibre(ItemCocina item, Vector3 posJugador)
    {
        int libre = SlotMasCercano(slots, posJugador, i => items[i] == null);
        if (libre < 0)
        {
            Debug.Log($"{name}: no hay espacio");
            return ResultadoEstacion.Rechazado;
        }

        items[libre] = item;
        item.Grab.ColocarEn(slots[libre]);
        return ResultadoEstacion.ObjetoRecibido;
    }
}
