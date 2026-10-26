using UnityEngine;

public class Eatobject : MonoBehaviour
{
    public float energiarecuperada;

    public float comer()
    {
        float energia = energiarecuperada;

        // Busca el GrabObject en este objeto o en sus padres.
        GrabObject objetoAgarrable = GetComponentInParent<GrabObject>();

        if (objetoAgarrable != null)
        {
            // Destruye el objeto que contiene GrabObject.
            Destroy(objetoAgarrable.gameObject);
        }
        else
        {
            // Si no hay GrabObject en los padres,
            // destruye solamente este objeto.
            Destroy(gameObject);
        }

        return energia;
    }
}