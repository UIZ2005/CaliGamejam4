using UnityEngine;

public class Parrilla : Estacion
{

    [SerializeField] private Transform[] slots;           // 3 empties sobre la parrilla
    [SerializeField] private float tiempoCoccion = 6f;
    [SerializeField] private float tiempoAntesDeQuemarse = 6f;
    [Tooltip("Cuánto se ve la arepa negra antes de desaparecer")]
    [SerializeField] private float tiempoVisibleQuemada = 1.5f;

    private ItemCocina[] arepas;
    private float[] tiempos;

    public HumoQuemado[] humos;

    public AudioSource audioSource;
    public AudioClip sonidoFuego;

    private void Awake()
    {
        arepas = new ItemCocina[slots.Length];
        tiempos = new float[slots.Length];
    }

    private void Update()
    {
        for (int i = 0; i < arepas.Length; i++)
        {
            ItemCocina a = arepas[i];
            if (a == null) 
            {
                if (humos != null &&i < humos.Length && humos[i] != null) humos[i].Apagar();
                continue; 
            }

            tiempos[i] += Time.deltaTime;

            if (a.Estado == EstadoCoccion.Crudo && tiempos[i] >= tiempoCoccion)
            {
                a.SetEstado(EstadoCoccion.Listo);
                Debug.Log($"Parrilla: arepa {i + 1} lista");
            }
            else if (a.Estado == EstadoCoccion.Listo && tiempos[i] >= tiempoCoccion + tiempoAntesDeQuemarse)
            {
                a.SetEstado(EstadoCoccion.Quemado);
                Debug.Log($"Parrilla: arepa {i + 1} se QUEMÓ");
            }
            else if (a.Estado == EstadoCoccion.Quemado &&
                     tiempos[i] >= tiempoCoccion + tiempoAntesDeQuemarse + tiempoVisibleQuemada)
            {
                Destroy(a.gameObject);
                arepas[i] = null;
                tiempos[i] = 0f;
            }
            if (humos != null && i < humos.Length && humos[i] != null)
            {
                if (arepas[i] == null || arepas[i].Estado == EstadoCoccion.Crudo)
                    humos[i].Apagar();
                else
                    humos[i].Mostrar((tiempos[i] - tiempoCoccion) / tiempoAntesDeQuemarse);
            }
        }
        //  suena mientras haya al menos una arepa en la parrilla
        if (audioSource)
        {
            bool hayArepas = false;
            for (int j = 0; j < arepas.Length; j++)
                if (arepas[j] != null) { hayArepas = true; break; }

            if (hayArepas && !audioSource.isPlaying)
            {
                audioSource.clip = sonidoFuego;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (!hayArepas && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    // Para la barra o reloj visual de después (0 a 1)
    public float Progreso(int slot)
    {
        if (arepas[slot] == null) return 0f;
        return Mathf.Clamp01(tiempos[slot] / tiempoCoccion);
    }

    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        // Plato vacío en la mano -> se sirve la arepa lista más cercana
        if (item.EsPlato)
        {
            int i = SlotMasCercano(slots, posJugador, j =>
                arepas[j] != null && item.PuedeRecibirComida(arepas[j]));

            if (i < 0) return ResultadoEstacion.Rechazado;

            item.RecibirComida(arepas[i]);
            arepas[i] = null;
            return ResultadoEstacion.SeQuedaEnMano;
        }

        // Masa cruda  a la parrilla
        if (item.tipo != TipoItem.Arepa || item.Estado != EstadoCoccion.Crudo)
            return ResultadoEstacion.Rechazado;

        int libre = SlotMasCercano(slots, posJugador, j => arepas[j] == null);
        if (libre < 0) return ResultadoEstacion.Rechazado;

        arepas[libre] = item;
        tiempos[libre] = 0f;
        item.Grab.ColocarEn(slots[libre]);
        return ResultadoEstacion.ObjetoRecibido;
    }

    // Manos vacías: solo se puede sacar una arepa lista
    public override ItemCocina Entregar(Vector3 posJugador)
    {
        int i = SlotMasCercano(slots, posJugador, j =>
            arepas[j] != null && arepas[j].Estado == EstadoCoccion.Listo);

        if (i < 0) return null;

        ItemCocina a = arepas[i];
        arepas[i] = null;
        return a;
    }
}
