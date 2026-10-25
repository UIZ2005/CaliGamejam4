using System.Collections.Generic;
using UnityEngine;

public class JarraLimonada : Estacion
{
    [SerializeField] private TipoItem[] receta = { TipoItem.Panela, TipoItem.Limon };

    
    
    [SerializeField] private ItemCocina prefabVaso;
    [SerializeField] private Transform[] slotsVasos;

    
    [SerializeField] private GameObject panelaDentro;
    [SerializeField] private GameObject limonDentro;

    
    
    [SerializeField] private Renderer agua;
    [SerializeField] private Color colorAgua = new Color(0.55f, 0.8f, 0.95f, 0.6f);
    [SerializeField] private Color colorLimonada = new Color(0.85f, 0.6f, 0.2f, 0.8f);

    
    [SerializeField] private float tiempoMezcla = 3f;

    private readonly List<TipoItem> dentro = new List<TipoItem>();
    private ItemCocina[] vasos;
    private bool mezclando;
    private float timer;
    private MaterialPropertyBlock bloque;

    public bool Mezclando => mezclando;
    public float ProgresoMezcla => mezclando ? Mathf.Clamp01(timer / tiempoMezcla) : 0f; // para la barra


    //para la cucahra
    public revolver cuchara;

    private void Awake()
    {
        vasos = new ItemCocina[slotsVasos.Length];
    }

    private void Start()
    {
        bloque = new MaterialPropertyBlock();
        Reiniciar();
    }

    private void Update()
    {
        if (!mezclando) return;

        timer += Time.deltaTime;
        PintarAgua(Color.Lerp(colorAgua, colorLimonada, timer / tiempoMezcla));

        if (timer >= tiempoMezcla)
        {
            mezclando = false;
            if (cuchara) cuchara.Activar(false);
            CrearVasos();
            Reiniciar(); // la jarra vuelve a agua
            Debug.Log($"{name}: ¡limonada lista! ({vasos.Length} vasos)");
        }
    }

    
    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        // Devolver un vaso de limonada a un puesto libre
        if (item.tipo == TipoItem.Limonada)
        {
            int libre = SlotMasCercano(slotsVasos, posJugador, j => vasos[j] == null);
            if (libre < 0) return ResultadoEstacion.Rechazado;

            vasos[libre] = item;
            item.Grab.ColocarEn(slotsVasos[libre]);
            return ResultadoEstacion.ObjetoRecibido;
        }

        // Ingredientes
        if (mezclando) return ResultadoEstacion.Rechazado;
        if (!BandejaVacia())
        {
            
            return ResultadoEstacion.Rechazado;
        }
        if (System.Array.IndexOf(receta, item.tipo) < 0) return ResultadoEstacion.Rechazado; // no va en la limonada
        if (dentro.Contains(item.tipo)) return ResultadoEstacion.Rechazado;                 // ya está adentro

        dentro.Add(item.tipo);
        MostrarIngrediente(item.tipo, true);
        Destroy(item.gameObject);
        Debug.Log($"{name}: entró {item.tipo} ({dentro.Count}/{receta.Length})");

        if (dentro.Count >= receta.Length)
        {
            mezclando = true;
            timer = 0f;
            if (cuchara) cuchara.Activar(true);
            Debug.Log($"{name}: mezclando...");
        }
        return ResultadoEstacion.ObjetoRecibido;
    }

    
    public override ItemCocina Entregar(Vector3 posJugador)
    {
        int i = SlotMasCercano(slotsVasos, posJugador, j => vasos[j] != null);
        if (i < 0) return null;

        ItemCocina vaso = vasos[i];
        vasos[i] = null;
        return vaso;
    }

    private void CrearVasos()
    {
        for (int i = 0; i < slotsVasos.Length; i++)
        {
            if (vasos[i] != null) continue;
            vasos[i] = Instantiate(prefabVaso, slotsVasos[i].position, slotsVasos[i].rotation);
            vasos[i].Grab.ColocarEn(slotsVasos[i]);
        }
    }

    private bool BandejaVacia()
    {
        foreach (var v in vasos)
            if (v != null) return false;
        return true;
    }

    private void Reiniciar()
    {
        dentro.Clear();
        timer = 0f;
        if (cuchara) cuchara.Activar(false);
        MostrarIngrediente(TipoItem.Panela, false);
        MostrarIngrediente(TipoItem.Limon, false);
        PintarAgua(colorAgua);
    }

    private void MostrarIngrediente(TipoItem tipo, bool activo)
    {
        if (tipo == TipoItem.Panela && panelaDentro) panelaDentro.SetActive(activo);
        if (tipo == TipoItem.Limon && limonDentro) limonDentro.SetActive(activo);
    }

    private void PintarAgua(Color c)
    {
        if (agua == null) return;
        agua.GetPropertyBlock(bloque);
        bloque.SetColor("_BaseColor", c); // URP
        bloque.SetColor("_Color", c);     // Standard
        agua.SetPropertyBlock(bloque);
    }
}
