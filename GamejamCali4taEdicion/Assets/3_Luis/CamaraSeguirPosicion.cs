
using UnityEngine;

public class CamaraSeguirPosicion : MonoBehaviour
{
    public Transform jugador;
    public Vector3 offset;

    void Start()
    {
        if (jugador != null)
        {
            offset = transform.position - jugador.position;
        }
    }

    void LateUpdate()
    {
        if (jugador == null) return;

        transform.position = jugador.position + offset;
    }
}
