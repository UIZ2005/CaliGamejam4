using UnityEngine;

public class HumoQuemado : MonoBehaviour
{
    
    
    public Color colorInicial = new Color(1f, 1f, 1f, 0.1f);
    
    public Color colorFinal = new Color(0.05f, 0.05f, 0.05f, 0.9f);

    [Header("Cantidad de humo (partículas por segundo)")]
    public float emisionInicial = 5f;
    public float emisionFinal = 25f;

    private ParticleSystem ps;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emision;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        main = ps.main;
        emision = ps.emission;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    
    public void Mostrar(float progreso)
    {
        progreso = Mathf.Clamp01(progreso);

        main.startColor = Color.Lerp(colorInicial, colorFinal, progreso);
        emision.rateOverTime = Mathf.Lerp(emisionInicial, emisionFinal, progreso);

        if (!ps.isPlaying) ps.Play();
    }

    public void Apagar()
    {
        if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
