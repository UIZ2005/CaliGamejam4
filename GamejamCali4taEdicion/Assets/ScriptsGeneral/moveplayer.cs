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

    // COCINA
    public LayerMask capaEstaciones;
    public float distanciaFrente = 0.8f;
    public float radioEstaciones = 1f;

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

        // hay una estación de cocina al frente?
        Vector3 puntoCocina = transform.position + transform.forward * distanciaFrente;
        Estacion estacion = BuscarEstacionCocina(puntoCocina);

        // 1. Si ya tiene un objeto en la mano, lo suelta y termina.
        if (grabObject != null)
        {
            //  si es algo de cocina y hay estación al frente, la estación lo recibe
            if (estacion != null && grabObject.TryGetComponent(out ItemCocina itemEnMano))
            {
                ResultadoEstacion r = estacion.Recibir(itemEnMano, puntoCocina);
                if (r == ResultadoEstacion.ObjetoRecibido)
                    grabObject = null;
                return; 
            }

            grabObject.Interact();
            grabObject = null;
            return;
        }

        // 2. Busca objetos o zonas alrededor.
        Collider[] objetosCercanos = Physics.OverlapSphere(
            transform.position,
            radioInteraccion,
            capaAgarre
        );

        // --- PRIORIDAD 1: Buscar objetos sueltos en el piso primero ---
        foreach (var objeto in objetosCercanos)
        {
            if (!EstaAlFrente(objeto))
                continue;

            if (objeto.TryGetComponent(out GrabObject grabEncontrado))
            {
                grabObject = grabEncontrado;
                grabObject.puntoAgarre = puntoAgarrePoder;
                grabObject.Interact();
                return; // Se detiene aquí porque ya recogió el objeto del piso
            }
        }

        // --- COCINA: la estación de cocina al frente entrega algo ---
        if (estacion != null)
        {
            ItemCocina dado = estacion.Entregar(puntoCocina);
            if (dado != null)
            {
                grabObject = dado.Grab;
                grabObject.AgarrarDirecto(puntoAgarrePoder);
                return;
            }
        }

        // --- PRIORIDAD 2: Si no había objetos sueltos, busca la zona para instanciar ---
        foreach (var objeto in objetosCercanos)
        {
            if (!EstaAlFrente(objeto))
                continue;

            if (objeto.TryGetComponent(out ZonaInstanciador zona))
            {
                GameObject nuevoObjeto = zona.InstanciarObjeto(puntoAgarrePoder.position);

                if (nuevoObjeto.TryGetComponent(out GrabObject nuevoGrab))
                {
                    grabObject = nuevoGrab;
                    grabObject.puntoAgarre = puntoAgarrePoder;
                    grabObject.Interact();
                }
                return; // Se detiene aquí tras crear un objeto nuevo
            }
        }
    }


    // estacion de cocinas para identificar si esta frente a una
    private Estacion BuscarEstacionCocina(Vector3 punto)
    {
        Collider[] cols = Physics.OverlapSphere(punto, radioEstaciones, capaEstaciones, QueryTriggerInteraction.Collide);

        Estacion mejor = null;
        float min = float.MaxValue;

        foreach (var c in cols)
        {
            Estacion e = c.GetComponentInParent<Estacion>();
            if (e == null) continue;

            float d = (c.ClosestPoint(punto) - punto).sqrMagnitude;
            if (d < min)
            {
                min = d;
                mejor = e;
            }
        }
        return mejor;
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

    // COCINA: ver en la escena dónde busca estaciones
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * distanciaFrente, radioEstaciones);
    }
}
