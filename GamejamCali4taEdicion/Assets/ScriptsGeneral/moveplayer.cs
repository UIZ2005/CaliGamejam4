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
    public bool quieto = false;

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
    
    //"Distancia máxima desde el jugador hasta el borde de la estación
    public float alcanceEstacion = 1f;
    [Range(0f, 1f)] public float precisionFrente = 0.5f;


    //SONIDOS
    public AudioSource audioSource;
    public AudioClip pasosbaldosa;
    public LayerMask baldosa;
    public float distanciaRayoPiso = 1.5f;

    public AudioSource audioAgarre;   // AudioSource propio 
    public AudioClip sonidoAgarre;

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
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.001f)
            return true;

        direccion.Normalize();
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

            if (objeto.TryGetComponent(out actionEvent actionevent))
            {
                actionevent.enviarAccion();
            }

            if (objeto.TryGetComponent(out Eatobject comidaencontrada))
            {
                energia.RecuperarEnergia(comidaencontrada.comer());
            }
        }
    }
    private void OnInteract(InputValue context)
    {
        if (!context.isPressed) return;
        Collider[] objetosCercanos = Physics.OverlapSphere(
            transform.position,
            radioInteraccion,
            capaAgarre
        );
        foreach (var objeto in objetosCercanos)
        {
            if (!EstaAlFrente(objeto))
                continue;
            if (objeto.TryGetComponent(out actionEvent actionevent))
            {
                actionevent.enviarAccion();
            }
        }

            Vector3 puntoCocina = transform.position + transform.forward * distanciaFrente;
        Estacion estacion = BuscarEstacionCocina(puntoCocina);

        if (grabObject != null)
        {
            if (estacion != null && grabObject.TryGetComponent(out ItemCocina itemEnMano))
            {
                ResultadoEstacion r = estacion.Recibir(itemEnMano, puntoCocina);
                if (r == ResultadoEstacion.ObjetoRecibido)
                {
                    grabObject = null;
                    return;
                }
                //if (r == ResultadoEstacion.SeQuedaEnMano)
                    return;
                
               
            }

            grabObject.Interact();
            grabObject = null;
            return;
        }

        // --- PRIORIDAD 1: Buscar objetos sueltos en el piso ---
        foreach (var objeto in objetosCercanos)
        {
            if (!EstaAlFrente(objeto))
                continue;

            if (objeto.TryGetComponent(out GrabObject grabEncontrado))
            {
                grabObject = grabEncontrado;
                //correcion del bug que habua en la cocina
                if (grabEncontrado.TryGetComponent(out ItemCocina _))
                {
                    grabObject.AgarrarDirecto(puntoAgarrePoder);
                    SonarAgarre();
                    return;
                }


                grabObject.puntoAgarre = puntoAgarrePoder;
                grabObject.Interact();
                return;
            }

        }

        // --- COCINA ---
        if (estacion != null)
        {
            ItemCocina dado = estacion.Entregar(puntoCocina);
            if (dado != null)
            {
                grabObject = dado.Grab;
                grabObject.AgarrarDirecto(puntoAgarrePoder);
                SonarAgarre();
                return;
            }
        }

        // --- PRIORIDAD 2: Zonas de instanciación ---
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
                return;
            }
        }

        // --- PRIORIDAD 3: Mesa de Madera ---
        foreach (var objeto in objetosCercanos)
        {
            if (!EstaAlFrente(objeto))
                continue;

            if (objeto.TryGetComponent(out MesaMadera mesa))
            {
                if (!TieneObjetoAgarrado())
                {
                    mesa.IniciarCorte(this);
                }
                return;
            }
        }
    }

    private Estacion BuscarEstacionCocina(Vector3 punto)
    {
        Collider[] cols = Physics.OverlapSphere(punto, radioEstaciones, capaEstaciones, QueryTriggerInteraction.Collide);

        Estacion mejor = null;
        float min = float.MaxValue;

        foreach (var c in cols)
        {
            Estacion e = c.GetComponentInParent<Estacion>();
            if (e == null) continue;

            // punto del borde de la estación más cercano al juga
            Vector3 borde = c.bounds.ClosestPoint(transform.position);
            Vector3 haciaEstacion = borde - transform.position;
            haciaEstacion.y = 0f;

            //  descartar estaciones lejanas
            if (haciaEstacion.magnitude > alcanceEstacion) continue;

            //  descartar estaciones que no están al frente
            if (haciaEstacion.sqrMagnitude > 0.0001f &&
                Vector3.Dot(transform.forward, haciaEstacion.normalized) < precisionFrente) continue;

            
            float d = (c.bounds.ClosestPoint(punto) - punto).sqrMagnitude;
            if (d < min)
            {
                min = d;
                mejor = e;
            }
        }
        return mejor;
    }

    private void Update()
    {
        if (audioSource == null) return;

        bool caminando = !quieto && moveInput != Vector2.zero;

        Vector3 origen = transform.position + Vector3.up * 0.5f;
        bool sobreBaldosa = Physics.Raycast(origen, Vector3.down, distanciaRayoPiso, baldosa, QueryTriggerInteraction.Ignore);

        if (caminando && sobreBaldosa)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = pasosbaldosa;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    private void FixedUpdate()
    {
        if (quieto) return;

        Vector3 direccionAdelante = camara.transform.forward;
        Vector3 direccionDerecha = camara.transform.right;

        direccionAdelante.y = 0f;
        direccionDerecha.y = 0f;

        direccionAdelante.Normalize();
        direccionDerecha.Normalize();

        Vector3 movimiento =
            direccionDerecha * moveInput.x +
            direccionAdelante * moveInput.y;

        movimiento = Vector3.ClampMagnitude(movimiento, 1f);

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * distanciaFrente, radioEstaciones);

        // líneas amarillas = hasta dónde y en qué ángulo alcanza una estación
        Gizmos.color = Color.yellow;
        float angulo = Mathf.Acos(precisionFrente) * Mathf.Rad2Deg;
        Vector3 izq = Quaternion.Euler(0, -angulo, 0) * transform.forward;
        Vector3 der = Quaternion.Euler(0, angulo, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + izq * alcanceEstacion);
        Gizmos.DrawLine(transform.position, transform.position + der * alcanceEstacion);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * alcanceEstacion);
    }

    // --- FUNCIONES NUEVAS PARA LA MESA DE MADERA ---
    public bool TieneObjetoAgarrado()
    {
        return grabObject != null;
    }

    public void ForzarAgarre(GrabObject nuevoGrab)
    {
        if (grabObject != null)
        {
            grabObject.Interact(); // Suelta si por error tenía algo
        }
        
        grabObject = nuevoGrab;
        grabObject.puntoAgarre = puntoAgarrePoder;
        grabObject.Interact();
    }

    private void SonarAgarre()
    {
        if (audioAgarre && sonidoAgarre) audioAgarre.PlayOneShot(sonidoAgarre);
    }
}