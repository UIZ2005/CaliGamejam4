using UnityEngine;

public class Eatobject : MonoBehaviour
{
    public float energiarecuperada;
    public bool istutorial = false;
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
            if (istutorial)
            {
                FlujoTutorial flujo = FindAnyObjectByType<FlujoTutorial>();
                flujo.ActivarAccion(4);
            }
            Destroy(gameObject);
        }

        return energia;
    }
}