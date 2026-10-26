using UnityEngine;
using System.Collections; // Necesario para la corrutina de tiempo

[RequireComponent(typeof(BoxCollider))]
public class SocketAgua : MonoBehaviour
{
    [Header("Configuración del Socket")]
    [Tooltip("El punto exacto donde se acomodará la cubeta al soltarla")]
    public Transform puntoSnap; 

    [Header("Sonido de Grifo")]
    public AudioSource audioGrifo;
    [Tooltip("Cuánto dura el desvanecimiento del sonido al final (en segundos)")]
    public float tiempoFadeOut = 0.2f;

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return; 

        if (other.TryGetComponent(out CubetaAgua cubeta))
        {
            if (!cubeta.estaLlena)
            {
                if (puntoSnap != null)
                {
                    cubeta.transform.position = puntoSnap.position;
                    cubeta.transform.rotation = puntoSnap.rotation;

                    if (cubeta.TryGetComponent(out Rigidbody rb))
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }

                // 1. Iniciamos la animación de llenado en la cubeta
                cubeta.IniciarLlenado();

                // 2. Reproducimos el sonido sincronizado con el tiempo de la cubeta
                if (audioGrifo != null)
                {
                    StartCoroutine(ReproducirSonidoGrifo(cubeta.tiempoLlenado));
                }
            }
        }
    }

    private IEnumerator ReproducirSonidoGrifo(float duracionTotal)
    {
        // Nos aseguramos de que el volumen empiece al máximo
        float volumenMaximo = 1f;
        audioGrifo.volume = volumenMaximo;
        audioGrifo.Play();

        // Calculamos cuánto tiempo debe sonar normal antes de empezar a apagarse
        float tiempoEsperaNormal = duracionTotal - tiempoFadeOut;
        
        // Si por error pusiste un fade out más largo que el llenado, lo ajustamos
        if (tiempoEsperaNormal > 0)
        {
            yield return new WaitForSeconds(tiempoEsperaNormal);
        }
        else
        {
            tiempoFadeOut = duracionTotal; 
        }

        // Inicia el efecto de Fade Out
        float tiempo = 0;
        while (tiempo < tiempoFadeOut)
        {
            tiempo += Time.deltaTime;
            // Interpola el volumen desde el máximo hasta 0 a medida que pasa el tiempo
            audioGrifo.volume = Mathf.Lerp(volumenMaximo, 0f, tiempo / tiempoFadeOut);
            yield return null;
        }

        // Detenemos el audio completamente y restauramos el volumen para la próxima vez
        audioGrifo.Stop();
        audioGrifo.volume = volumenMaximo;
    }
}