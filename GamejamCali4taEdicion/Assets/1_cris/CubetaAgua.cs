using UnityEngine;
using System.Collections;

public class CubetaAgua : MonoBehaviour
{
    [Header("Configuración del Agua")]
    public GameObject objetoAgua; // Arrastra aquí el cilindro/plano azul del agua
    public float tiempoLlenado = 1f;
    
    public bool estaLlena = false;
    private Vector3 escalaOriginalAgua;

    private void Start()
    {
        // Guardamos el tamaño original del agua cuando está llena
        escalaOriginalAgua = objetoAgua.transform.localScale;
        
        // Al iniciar, la vaciamos
        Vaciar();
    }

    public void Vaciar()
    {
        estaLlena = false;
        // Ponemos la escala Y en 0 (invisible)
        objetoAgua.transform.localScale = new Vector3(escalaOriginalAgua.x, 0, escalaOriginalAgua.z);
        objetoAgua.SetActive(false);
    }

    public void IniciarLlenado()
    {
        if (!estaLlena)
        {
            StartCoroutine(RutinaLlenar());
        }
    }

    private IEnumerator RutinaLlenar()
    {
        estaLlena = true; // Se marca llena inmediatamente para no repetir el proceso
        objetoAgua.SetActive(true);
        
        float tiempo = 0f;

        while (tiempo < tiempoLlenado)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / tiempoLlenado;

            // Interpolamos la escala en Y de 0 hasta su tamaño original
            objetoAgua.transform.localScale = new Vector3(
                escalaOriginalAgua.x, 
                Mathf.Lerp(0, escalaOriginalAgua.y, progreso), 
                escalaOriginalAgua.z
            );

            yield return null;
        }

        // Aseguramos que quede exactamente en su tamaño final
        objetoAgua.transform.localScale = escalaOriginalAgua;
    }
}