using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SocketAgua : MonoBehaviour
{
    [Header("Configuración del Socket")]
    [Tooltip("El punto exacto donde se acomodará la cubeta al soltarla")]
    public Transform puntoSnap; 

    private void OnTriggerStay(Collider other)
    {
        // TRUCO: Gracias a tu script GrabObject, sabemos que si es un trigger, 
        // el jugador lo tiene en la mano. Si NO es trigger, está soltado en el piso.
        if (other.isTrigger) return; 

        // Si el objeto soltado tiene el script de la Cubeta
        if (other.TryGetComponent(out CubetaAgua cubeta))
        {
            // Y si NO está llena
            if (!cubeta.estaLlena)
            {
                // 1. Acomodamos la cubeta perfectamente en el centro del socket (opcional pero recomendado)
                if (puntoSnap != null)
                {
                    cubeta.transform.position = puntoSnap.position;
                    cubeta.transform.rotation = puntoSnap.rotation;

                    // Frenamos sus físicas para que no salga volando
                    if (cubeta.TryGetComponent(out Rigidbody rb))
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }

                // 2. Iniciamos la animación
                cubeta.IniciarLlenado();
            }
        }
    }
}