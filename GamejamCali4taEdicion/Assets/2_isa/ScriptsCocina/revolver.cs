using UnityEngine;

public class revolver : MonoBehaviour
{
    public float velocidad = 270f;
    [Tooltip("Si está marcado, la cuchara solo se ve mientras gira")]
    public bool ocultarCuandoNoGira = false;

    private Quaternion rotacionInicial;

    public bool Girando { get; private set; }

    private void Awake()
    {
        rotacionInicial = transform.localRotation;
        Activar(false);
    }

    public void Activar(bool girar)
    {
        Girando = girar;

        // al parar vuelve a su posición original
        if (!girar) transform.localRotation = rotacionInicial;

        if (ocultarCuandoNoGira)
            foreach (Transform hijo in transform)
                hijo.gameObject.SetActive(girar);
    }

    private void Update()
    {
        if (Girando)
            transform.Rotate(0f, velocidad * Time.deltaTime, 0f, Space.Self);
    }
}
