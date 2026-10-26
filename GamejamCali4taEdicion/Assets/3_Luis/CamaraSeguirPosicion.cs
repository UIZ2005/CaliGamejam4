
using UnityEngine;

public class CamaraSeguirPosicion : MonoBehaviour
{
    public Transform jugador;
    public Vector3 offset;

    [Header("Altura de cámara")]
    public float alturaNormal = 4f;
    public float alturaBaldosa = 6f;
    public float velocidadCambio = 5f;

    [Header("Detección del suelo")]
    public float distanciaSuelo = 2f;
    public LayerMask layerBaldosa;

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

        bool sobreBaldosa = Physics.Raycast(
            jugador.position + Vector3.up * 0.2f,
            Vector3.down,
            out RaycastHit hit,
            distanciaSuelo,
            layerBaldosa
        );

        float alturaObjetivo = sobreBaldosa ? alturaBaldosa : alturaNormal;

        offset.y = Mathf.Lerp(
            offset.y,
            alturaObjetivo,
            velocidadCambio * Time.deltaTime
        );

        transform.position = jugador.position + offset;
    }
}
