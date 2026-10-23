using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class moveplayer : MonoBehaviour
{
    private Vector2 moveInput;
    private PlayerEnergy energia;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float slow = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    public Camera camara;
    private bool quieto = false;

    [Header("Agarre")]
    public Transform puntoAgarrePoder;
    public float radioInteraccion = 2f;
    public LayerMask capaAgarre;
    private GrabObject grabObject;

    private Rigidbody rb;
    public Animator animator;
    private bool slowapply=false;
    private audioManager audio;

    public void Awake()
    {
        audio = FindAnyObjectByType<audioManager>();
        energia = GetComponent<PlayerEnergy>();
        rb = GetComponent<Rigidbody>();
    }
    private void OnMove(InputValue context)
    {
        if (quieto) return;
        moveInput = context.Get<Vector2>();

        if (moveInput != Vector2.zero)
        {
            if (audio != null)
                audio.seleccionAudio(0);

            if (energia != null && energia.PuedeTrabajar())
            {
                energia.SetTrabajando(true);
            }
            if(!energia.PuedeTrabajar() && !slowapply)
            {
                animator.SetBool("walk", true);
                animator.SetBool("run", false);
                slowapply = true;
                speed -=slow;
            }
            if (speed > slow)
            {
                animator.SetBool("run", true);
            }
            else
            {
                animator.SetBool("walk", true);
            }
        }
        else
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
            energia.SetTrabajando(false);
        }

       
    }

    private bool EstaAlFrente(Collider objeto)
    {
        Vector3 direccion = objeto.bounds.center - transform.position;

        // Ignorar diferencias de altura.
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.001f)
            return true;

        direccion.Normalize();

        // Solo aceptar objetos dentro de los 180 grados frontales.
        return Vector3.Dot(transform.forward, direccion) > 0f;
    }


    private void OnEat(InputValue context)
    {
        Collider[] objetosCercanos = Physics.OverlapSphere(
                transform.position,
                radioInteraccion,
                capaAgarre
            );
        foreach (var objeto in objetosCercanos)
        {
            if (!EstaAlFrente(objeto))
                continue;

            if (objeto.TryGetComponent(out Eatobject comidaencontrada))
            {
                energia.RecuperarEnergia(comidaencontrada.comer());
            }
        }
    }
    private void OnInteract(InputValue context)
    {
        if (!context.isPressed) return;

        // 1. Si ya tiene un objeto en la mano, lo suelta y termina. (Cumple tu regla de no instanciar si ya tiene algo).
        if (grabObject != null)
        {
            grabObject.Interact();
            grabObject = null;
            return;
        }

        // 2. Si NO tiene nada en la mano, busca objetos o zonas alrededor.
        Collider[] objetosCercanos = Physics.OverlapSphere(
            transform.position,
            radioInteraccion,
            capaAgarre
        );
        
        foreach (var objeto in objetosCercanos)
        {
            if (!EstaAlFrente(objeto))
                continue;

            // CASO A: Es un objeto normal que ya existe tirado en el piso (Ladrillo/Cemento suelto)
            if (objeto.TryGetComponent(out GrabObject grabEncontrado))
            {
                grabObject = grabEncontrado;
                grabObject.puntoAgarre = puntoAgarrePoder;
                grabObject.Interact();
                return; // Terminamos aquí
            }

            // CASO B: Es la ZONA DE INSTANCIACIÓN (Caja de materiales infinita)
            if (objeto.TryGetComponent(out ZonaInstanciador zona))
            {
                // Instanciamos el prefab exactamente en la mano del jugador
                GameObject nuevoObjeto = zona.InstanciarObjeto(puntoAgarrePoder.position);

                // Como el objeto instanciado tiene el script GrabObject, lo agarramos automáticamente
                if (nuevoObjeto.TryGetComponent(out GrabObject nuevoGrab))
                {
                    grabObject = nuevoGrab;
                    grabObject.puntoAgarre = puntoAgarrePoder;
                    // Llama al interact del objeto nuevo para que se emparenté y se configure
                    grabObject.Interact(); 
                }
                return; // Terminamos aquí
            }
        }
    }

    private void FixedUpdate()
    {
        if (quieto) return;

        // Direcci�n horizontal de la c�mara.
        Vector3 direccionAdelante = camara.transform.forward;
        Vector3 direccionDerecha = camara.transform.right;

        direccionAdelante.y = 0f;
        direccionDerecha.y = 0f;

        direccionAdelante.Normalize();
        direccionDerecha.Normalize();

        // Movimiento relativo a la c�mara.
        Vector3 movimiento =
            direccionDerecha * moveInput.x +
            direccionAdelante * moveInput.y;

        movimiento = Vector3.ClampMagnitude(movimiento, 1f);

        // Rotaci�n hacia la direcci�n del movimiento.
        if (movimiento.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(movimiento);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

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
    public void recuperarVelocidad()
    {
        animator.SetBool("walk", false);
        animator.SetBool("run", true);
        slowapply = false;
        speed += slow;
    }
}
