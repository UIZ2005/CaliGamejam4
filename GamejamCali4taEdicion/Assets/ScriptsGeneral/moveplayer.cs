using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class moveplayer : MonoBehaviour
{
    private Vector2 moveInput;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    public Camera camara;
    private bool quieto = false;

    [Header("Agarre")]
    public Transform puntoAgarrePoder;
    public float radioInteraccion = 2f;
    public LayerMask capaAgarre;
    private GrabObject grabObject;

    private Rigidbody rb;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void OnMove(InputValue context)
    {

        moveInput = context.Get<Vector2>();
    }
    private void OnInteract(InputValue context)
    {
        if (!context.isPressed) return;
        if (grabObject != null)
        {
            grabObject.Interact();
            grabObject = null;
            return;
        }

            Collider[] objetosCercanos = Physics.OverlapSphere(
                transform.position,
                radioInteraccion,
                capaAgarre
            );
        foreach (var objeto in objetosCercanos)
        {
            if (objeto.TryGetComponent(out GrabObject grabEncontrado))
            {
                grabObject = grabEncontrado;
                grabObject.puntoAgarre = puntoAgarrePoder;
                grabObject.Interact();
                return;
            }
        }
    }

    private void FixedUpdate()
    {
        if (quieto) return;


        // Dirección horizontal de la cámara.
        Vector3 direccionAdelante = camara.transform.forward;
        Vector3 direccionDerecha = camara.transform.right;

        direccionAdelante.y = 0f;
        direccionDerecha.y = 0f;

        direccionAdelante.Normalize();
        direccionDerecha.Normalize();

        // Movimiento relativo a la cámara.
        Vector3 movimiento =
            direccionDerecha * moveInput.x +
            direccionAdelante * moveInput.y;

        movimiento = Vector3.ClampMagnitude(movimiento, 1f);

        // Mantener la velocidad horizontal del jugador.
        Vector3 velocidad = rb.linearVelocity;
        velocidad.x = movimiento.x * speed;
        velocidad.z = movimiento.z * speed;

        rb.linearVelocity = velocidad;

    }
    public void activarquieto()
    {
        quieto = true;
        moveInput = Vector2.zero;

        rb.linearVelocity = Vector3.zero;
    }
    public void desactivarquieto()
    {
        quieto = false;
        moveInput = Vector2.zero;
    }

}
