using UnityEngine;


public class ItemCocina : MonoBehaviour
{
    public TipoItem tipo;
    [SerializeField] private EstadoCoccion estadoInicial = EstadoCoccion.Listo;

    [Header("Solo para platos")]
    [SerializeField] private Transform puntoComida;
    [SerializeField] private TipoItem[] comidasAceptadas = { TipoItem.Arepa };

    
    // esta es la arepa en bolita
    [SerializeField] private GameObject visualCrudo;
    
    //arepa lista
    [SerializeField] private GameObject visualListo;

    //
    [SerializeField] private GameObject visualQuemado;
    [SerializeField] private Color colorQuemado = new Color(0.08f, 0.06f, 0.05f);

    public EstadoCoccion Estado { get; private set; }
    public EstadoCoccion EstadoInicial => estadoInicial;
    public GrabObject Grab { get; private set; }

    // ---- Plato ----
    public bool EsPlato => tipo == TipoItem.Plato;
    public ItemCocina Comida { get; private set; }

    private void Awake()
    {
        Grab = GetComponent<GrabObject>();
        SetEstado(estadoInicial);
    }

    


    public void SetEstado(EstadoCoccion nuevo)
    {
        Estado = nuevo;
        if (visualCrudo) visualCrudo.SetActive(nuevo == EstadoCoccion.Crudo);
        if (visualListo) visualListo.SetActive(nuevo == EstadoCoccion.Listo);
        if (visualQuemado) visualQuemado.SetActive(nuevo == EstadoCoccion.Quemado);

        if (nuevo == EstadoCoccion.Quemado && visualQuemado == null)
        {
            if (visualListo) visualListo.SetActive(true);
            PintarDeNegro();
        }
    }

    private void PintarDeNegro()
    {
        var bloque = new MaterialPropertyBlock();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.GetPropertyBlock(bloque);
            bloque.SetColor("_BaseColor", colorQuemado); 
            bloque.SetColor("_Color", colorQuemado);     
            r.SetPropertyBlock(bloque);
        }
    }

    //  PLATO 

    public bool PuedeRecibirComida(ItemCocina comida)
    {
        if (!EsPlato || Comida != null || comida == null) return false;
        if (comida.Estado != EstadoCoccion.Listo) return false;

        foreach (var t in comidasAceptadas)
            if (comida.tipo == t) return true;

        return false;
    }

    public bool RecibirComida(ItemCocina comida)
    {
        if (!PuedeRecibirComida(comida)) return false;

        Comida = comida;
        comida.Grab.ColocarEn(puntoComida); // encima del plato
        return true;
    }

    

   // depdnde de que pedido se esta requerimiendo
    public bool TryGetTipoServido(out TipoItem servido)
    {
        if (EsPlato)
        {
            if (Comida != null && Comida.Estado == EstadoCoccion.Listo)
            {
                servido = Comida.tipo;
                return true;
            }
            servido = default;
            return false;
        }

        if (tipo == TipoItem.Cafe || tipo == TipoItem.Limonada)
        {
            servido = tipo;
            return true;
        }

        servido = default;
        return false;
    }
}
