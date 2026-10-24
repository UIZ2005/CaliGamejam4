using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Renombramos la clase para no tener errores con el script del Nivel 1
[System.Serializable]
public class RecetaConstruccionN2
{
    public string nombreFase;
    public int reqConcreto;
    public int reqLadrillos;
    public int reqCemento;
    public int reqMadera;
    public int reqAgua; 
}

[RequireComponent(typeof(BoxCollider))]
public class ZonaConstruccionNivel2 : MonoBehaviour
{
    [Header("Configuración de Nivel 2")]
    [Tooltip("Añade aquí las 8 recetas en orden")]
    public List<RecetaConstruccionN2> listaRecetas;
    
    [Header("Modelos 3D de la Construcción")]
    [Tooltip("El modelo de pre-construcción (Aparece en la receta 4)")]
    public GameObject modeloFaseMedia;
    [Tooltip("El modelo de la barbería final (Aparece al terminar)")]
    public GameObject modeloFaseFinal;

    [Header("Interfaz UI")]
    public Text textoRecetaUI;

    private int indiceRecetaActual = 0;

    // Contadores internos
    private int faltaConcreto;
    private int faltaLadrillos;
    private int faltaCemento;
    private int faltaMadera;
    private int faltaAgua;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        
        // Nos aseguramos de apagar los modelos al iniciar el nivel
        if (modeloFaseMedia != null) modeloFaseMedia.SetActive(false);
        if (modeloFaseFinal != null) modeloFaseFinal.SetActive(false);

        CargarReceta(0); 
    }

    void CargarReceta(int indice)
    {
        // LÓGICA DE APARICIÓN DE MODELOS 3D (NIVEL 2)
        // Si llegamos al índice 4 (es decir, completamos 0, 1, 2 y 3)
        if (indice == 4)
        {
            if (modeloFaseMedia != null) modeloFaseMedia.SetActive(true);
        }

        // Si ya completamos todas las recetas (Índice 8)
        if (indice >= listaRecetas.Count)
        {
            // Apagamos la pre-construcción y encendemos la Barbería terminada
            if (modeloFaseMedia != null) modeloFaseMedia.SetActive(false);
            if (modeloFaseFinal != null) modeloFaseFinal.SetActive(true);

            indiceRecetaActual = indice;
            textoRecetaUI.text = "¡BARBERÍA COMPLETADA!";
            return;
        }

        // Cargar datos de la nueva receta
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
        // Ignorar si lo tiene en la mano o ya acabo el nivel
        if (other.isTrigger || indiceRecetaActual >= listaRecetas.Count) return;

        bool recibioMaterial = false;

        // 1. Validar Agua
        if (faltaAgua > 0 && other.TryGetComponent(out CubetaAgua cubeta))
        {
            if (cubeta.estaLlena)
            {
                cubeta.Vaciar(); 
                faltaAgua--;
                recibioMaterial = true;
            }
        }
        // 2. Validar Concreto
        else if (faltaConcreto > 0 && other.TryGetComponent(out ItemConcreto concreto))
        {
            Destroy(concreto.gameObject);
            faltaConcreto--;
            recibioMaterial = true;
        }
        // 3. Validar Ladrillo
        else if (faltaLadrillos > 0 && other.TryGetComponent(out ItemLadrillo ladrillo))
        {
            Destroy(ladrillo.gameObject);
            faltaLadrillos--;
            recibioMaterial = true;
        }
        // 4. Validar Cemento
        else if (faltaCemento > 0 && other.TryGetComponent(out ItemCemento cemento))
        {
            Destroy(cemento.gameObject);
            faltaCemento--;
            recibioMaterial = true;
        }
        // 5. Validar Madera
        else if (faltaMadera > 0 && other.TryGetComponent(out ItemMadera madera))
        {
            Destroy(madera.gameObject);
            faltaMadera--;
            recibioMaterial = true;
        }

        if (recibioMaterial)
        {
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
}