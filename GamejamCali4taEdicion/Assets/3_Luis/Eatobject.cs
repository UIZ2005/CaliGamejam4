using UnityEngine;

public class Eatobject : MonoBehaviour
{
    public float energiarecuperada;

    public float comer()
    {
        Destroy(gameObject,0.1f);
        return energiarecuperada;
    }
}
