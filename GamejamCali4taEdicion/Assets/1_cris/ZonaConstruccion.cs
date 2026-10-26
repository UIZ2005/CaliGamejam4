using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class RecetaConstruccion
{
    public string nombreFase;
    public int reqConcreto;
    public int reqLadrillos;
    public int reqCemento;
    public int reqMadera;
    public int reqAgua; 
}

[RequireComponent(typeof(BoxCollider))]
public class ZonaConstruccion : MonoBehaviour
{
    [Header("Configuración de Nivel")]
    public List<RecetaConstruccion> listaRecetas;
    
    [Header("Modelos 3D de la Construcción")]
    public GameObject modeloFaseMedia;
    public GameObject modeloFaseFinal;

    [Header("Interfaz UI")]
    public Text textoRecetaUI;

    [Header("Sonidos")]
    public AudioSource audioSource; // <-- NUEVO
    public AudioClip sonidoEntregaMaterial; // <-- NUEVO

    private int indiceRecetaActual = 0;
    private int faltaConcreto;
    private int faltaLadrillos;
    private int faltaCemento;
    private int faltaMadera;
    private int faltaAgua;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        
        if (modeloFaseMedia != null) modeloFaseMedia.SetActive(false);
        if (modeloFaseFinal != null) modeloFaseFinal.SetActive(false);

        CargarReceta(0); 
    }

    void CargarReceta(int indice)
    {
        if (indice == 2)
            if (modeloFaseMedia != null) modeloFaseMedia.SetActive(true);

        if (indice >= listaRecetas.Count)
        {
            if (modeloFaseMedia != null) modeloFaseMedia.SetActive(false);
            if (modeloFaseFinal != null) modeloFaseFinal.SetActive(true);

            indiceRecetaActual = indice;
            textoRecetaUI.text = "¡CONSTRUCCIÓN COMPLETADA!";
            
            if (NivelManager.Instancia != null)
                NivelManager.Instancia.RegistrarConstruccionCompletada();
                
            return;
        }

        indiceRecetaActual = indice;
        faltaConcreto = listaRecetas[indice].reqConcreto;
        faltaLadrillos = listaRecetas[indice].reqLadrillos;
        faltaCemento = listaRecetas[indice].reqCemento;
        faltaMadera = listaRecetas[indice].reqMadera;
        faltaAgua = listaRecetas[indice].reqAgua; 

        ActualizarUI();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger || indiceRecetaActual >= listaRecetas.Count) return;

        bool recibioMaterial = false;

        if (faltaAgua > 0 && other.TryGetComponent(out CubetaAgua cubeta))
        {
            if (cubeta.estaLlena)
            {
                cubeta.Vaciar(); 
                faltaAgua--;
                recibioMaterial = true;
            }
        }
        else if (faltaConcreto > 0 && other.TryGetComponent(out ItemConcreto concreto))
        {
            Destroy(concreto.gameObject);
            faltaConcreto--;
            recibioMaterial = true;
        }
        else if (faltaLadrillos > 0 && other.TryGetComponent(out ItemLadrillo ladrillo))
        {
            Destroy(ladrillo.gameObject);
            faltaLadrillos--;
            recibioMaterial = true;
        }
        else if (faltaCemento > 0 && other.TryGetComponent(out ItemCemento cemento))
        {
            Destroy(cemento.gameObject);
            faltaCemento--;
            recibioMaterial = true;
        }
        else if (faltaMadera > 0 && other.TryGetComponent(out ItemMadera madera))
        {
            Destroy(madera.gameObject);
            faltaMadera--;
            recibioMaterial = true;
        }

        // --- DISPARA EL SONIDO SI RECIBIÓ ALGO ---
        if (recibioMaterial) 
        {
            if (audioSource != null && sonidoEntregaMaterial != null)
            {
                audioSource.PlayOneShot(sonidoEntregaMaterial);
            }
            ComprobarProgreso();
        }
    }

    void ComprobarProgreso()
    {
        if (faltaConcreto <= 0 && faltaLadrillos <= 0 && faltaCemento <= 0 && faltaMadera <= 0 && faltaAgua <= 0)
        {
            CargarReceta(indiceRecetaActual + 1);
        }
        else
        {
            ActualizarUI();
        }
    }

    void ActualizarUI()
    {
        if (indiceRecetaActual >= listaRecetas.Count) return;

        string texto = "<b>" + listaRecetas[indiceRecetaActual].nombreFase + "</b>\n";
        if (faltaConcreto > 0) texto += "- Concreto: " + faltaConcreto + "\n";
        if (faltaLadrillos > 0) texto += "- Ladrillos: " + faltaLadrillos + "\n";
        if (faltaCemento > 0) texto += "- Cemento: " + faltaCemento + "\n";
        if (faltaMadera > 0) texto += "- Madera: " + faltaMadera + "\n";
        if (faltaAgua > 0) texto += "- Agua: " + faltaAgua + "\n"; 
        textoRecetaUI.text = texto;
    }

    private void OnDisable()
    {
        if (modeloFaseMedia != null) modeloFaseMedia.SetActive(false);
        if (modeloFaseFinal != null) modeloFaseFinal.SetActive(false);
    }
}