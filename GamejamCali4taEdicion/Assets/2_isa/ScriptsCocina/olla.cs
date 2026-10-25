using System.Collections.Generic;
using UnityEngine;

public class olla : Estacion
{

    public enum EstadoOlla { Agua, Cocinando, Listo, Quemado }


    [SerializeField] private TipoItem[] receta = { TipoItem.Pollo, TipoItem.Yuca, TipoItem.Platano };

    //ingrediente en nivevl 3
    [SerializeField] private TipoItem[] extrasNivel3 = { TipoItem.Mazorca };
    [SerializeField] private TipoItem comidaResultado = TipoItem.Sancocho;

    //ingredientes en la olla
    // NUEVO: en vez de la lista, 4 campos directos. Arrastra aquí los modelos que están DENTRO de la olla
    [SerializeField] private GameObject polloDentro;
    [SerializeField] private GameObject yucaDentro;
    [SerializeField] private GameObject platanoDentro;
    [SerializeField] private GameObject mazorcaDentro;



    [SerializeField] private Renderer caldo; // agua dell caldo
    [SerializeField] private Color colorAgua = new Color(0.55f, 0.8f, 0.95f);
    [SerializeField] private Color colorSancocho = new Color(0.9f, 0.7f, 0.25f);
    [SerializeField] private Color colorQuemado = new Color(0.08f, 0.06f, 0.05f);


    [SerializeField] private float tiempoCoccion = 20f;

    [SerializeField] private float tiempoAntesDeQuemarse = 12f;
    [SerializeField] private float tiempoVisibleQuemado = 2f;
    [SerializeField] private int porcionesPorOlla = 3;

    private EstadoOlla estado = EstadoOlla.Agua;
    private readonly List<TipoItem> dentro = new List<TipoItem>();
    private float timer;
    private int porciones;
    private MaterialPropertyBlock bloque;

    public EstadoOlla Estado => estado;
    public float ProgresoCoccion => estado == EstadoOlla.Cocinando ? Mathf.Clamp01(timer / tiempoCoccion) : 0f; // para la barra
    public int Porciones => porciones;


    //para la cuchara
    public revolver cuchara;

    //humo
    public HumoQuemado humo;

    private void Start()
    {
        bloque = new MaterialPropertyBlock();
        Reiniciar();
    }

    private void Update()
    {
        if (estado == EstadoOlla.Agua) return;

        timer += Time.deltaTime;

        switch (estado)
        {
            case EstadoOlla.Cocinando:
                // el color pasa poco a poco de agua a sancocho
                PintarCaldo(Color.Lerp(colorAgua, colorSancocho, timer / tiempoCoccion));
                if (timer >= tiempoCoccion)
                {
                    estado = EstadoOlla.Listo;
                    if (cuchara) cuchara.Activar(false);
                    porciones = porcionesPorOlla;
                    timer = 0f;
                    PintarCaldo(colorSancocho);
                    Debug.Log($"{name}: sancocho listo ({porciones} platos)");
                }
                break;

            case EstadoOlla.Listo:
                if (timer >= tiempoAntesDeQuemarse)
                {
                    estado = EstadoOlla.Quemado;
                    timer = 0f;
                    PintarCaldo(colorQuemado);
                    Debug.Log($"{name}: se QUEMÓ el sancocho");
                }
                break;

            case EstadoOlla.Quemado:
                if (timer >= tiempoVisibleQuemado)
                    Reiniciar();
                break;
        }

        //  humo se va oscureciendo; quemada negro
        if (humo != null)
        {
            if (estado == EstadoOlla.Listo) humo.Mostrar(timer / tiempoAntesDeQuemarse);
            else if (estado == EstadoOlla.Quemado) humo.Mostrar(1f);
        }
    }

    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {

        if (item.EsPlato)
        {
            if (estado != EstadoOlla.Listo) return ResultadoEstacion.Rechazado;
            if (!item.LlenarCon(comidaResultado)) return ResultadoEstacion.Rechazado; // plato pando o ya lleno

            porciones--;
            timer = 0f; // servir le da más tiempo antes de quemarse
            Debug.Log($"{name}: plato servido, quedan {porciones}");

            if (porciones <= 0) Reiniciar();
            return ResultadoEstacion.SeQuedaEnMano;
        }


        if (estado != EstadoOlla.Agua) return ResultadoEstacion.Rechazado; //

        TipoItem[] recetaActual = RecetaActual();
        if (System.Array.IndexOf(recetaActual, item.tipo) < 0) return ResultadoEstacion.Rechazado; // no va en el sancocho
        if (dentro.Contains(item.tipo)) return ResultadoEstacion.Rechazado;                        // ya está adentro

        dentro.Add(item.tipo);
        MostrarIngrediente(item.tipo, true);
        Destroy(item.gameObject);
        Debug.Log($"{name}: entró {item.tipo} ({dentro.Count}/{recetaActual.Length})");

        if (dentro.Count >= recetaActual.Length)
        {
            estado = EstadoOlla.Cocinando;
            timer = 0f;
            if (cuchara) cuchara.Activar(true);
            Debug.Log($"{name}: cocinando");
        }
        return ResultadoEstacion.ObjetoRecibido;
    }

    private TipoItem[] RecetaActual()
    {
        bool nivel3 = NivelManager.Instancia != null && NivelManager.Instancia.NivelActual >= 3;
        if (!nivel3 || extrasNivel3.Length == 0) return receta;

        var lista = new List<TipoItem>(receta);
        lista.AddRange(extrasNivel3);
        return lista.ToArray();
    }

    private void Reiniciar()
    {
        estado = EstadoOlla.Agua;
        dentro.Clear();
        timer = 0f;
        porciones = 0;
        if (humo != null) humo.Apagar();
        if (cuchara) cuchara.Activar(false);


        // NUEVO: apaga los 4 ingredientes de adentro
        MostrarIngrediente(TipoItem.Pollo, false);
        MostrarIngrediente(TipoItem.Yuca, false);
        MostrarIngrediente(TipoItem.Platano, false);
        MostrarIngrediente(TipoItem.Mazorca, false);

        PintarCaldo(colorAgua);
    }

    private void MostrarIngrediente(TipoItem tipo, bool activo)
    {
        // NUEVO: elige cuál de los 4 modelos prender o apagar
        GameObject visual = null;
        switch (tipo)
        {
            case TipoItem.Pollo: visual = polloDentro; break;
            case TipoItem.Yuca: visual = yucaDentro; break;
            case TipoItem.Platano: visual = platanoDentro; break;
            case TipoItem.Mazorca: visual = mazorcaDentro; break;
        }
        if (visual) visual.SetActive(activo);
    }

    private void PintarCaldo(Color c)
    {
        if (caldo == null) return;
        caldo.GetPropertyBlock(bloque);
        bloque.SetColor("_BaseColor", c); // URP
        bloque.SetColor("_Color", c);     // Standard
        caldo.SetPropertyBlock(bloque);
    }
}