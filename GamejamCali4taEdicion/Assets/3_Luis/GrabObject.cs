using UnityEngine;

public class GrabObject : MonoBehaviour
{
    [Header("Agarre")]
    public Transform puntoAgarre;
    [SerializeField] private float radioInteraccion = 2f;
    [SerializeField] private LayerMask capaObjetos;

    [Header("Ajustes de Posición y Rotación")]
    [Tooltip("Ajusta la distancia/posición en la que se sostiene este objeto")]
    public Vector3 offsetPosicion = Vector3.zero;
    [Tooltip("Ajusta el ángulo/rotación en el que se sostiene este objeto")]
    public Vector3 offsetRotacion = Vector3.zero;

    private Rigidbody objetoAgarrado;
    private Transform padreOriginal;
    private bool kinematicOriginal;
    
    // Variable para guardar la escala y evitar que se deforme
    private Vector3 escalaOriginal;

    public void Interact()
    {
        if (objetoAgarrado != null)
        {
            SoltarObjeto();
        }
        else
        {
            AgarrarObjeto();
        }
    }

    private void AgarrarObjeto()
    {
        Collider[] objetos = Physics.OverlapSphere(
            transform.position,
            radioInteraccion,
            capaObjetos
        );

        Rigidbody objetoCercano = null;
        float distanciaMinima = float.MaxValue;

        foreach (Collider col in objetos)
        {
            Rigidbody candidato = col.attachedRigidbody;

            if (candidato == null)
                continue;

            float distancia = Vector3.Distance(
                transform.position,
                candidato.worldCenterOfMass
            );

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                objetoCercano = candidato;
            }
        }

        if (objetoCercano == null)
            return;

        objetoAgarrado = objetoCercano;

        padreOriginal = objetoAgarrado.transform.parent;
        kinematicOriginal = objetoAgarrado.isKinematic;

        objetoAgarrado.linearVelocity = Vector3.zero;
        objetoAgarrado.angularVelocity = Vector3.zero;
        objetoAgarrado.isKinematic = true;
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
        
        // GUARDAMOS LA ESCALA ORIGINAL ANTES DE EMPARENTAR
        escalaOriginal = objetoAgarrado.transform.localScale;
        
        objetoAgarrado.transform.SetParent(puntoAgarre);
        
        // Se aplican los valores personalizados en lugar de dejarlo en (0,0,0)
        objetoAgarrado.transform.localPosition = offsetPosicion;
        objetoAgarrado.transform.localEulerAngles = offsetRotacion;
    }

    private void SoltarObjeto()
    {
        objetoAgarrado.transform.SetParent(padreOriginal);
        
        // RESTAURAMOS LA ESCALA ORIGINAL AL SOLTARLO
        objetoAgarrado.transform.localScale = escalaOriginal;
        
        gameObject.GetComponent<BoxCollider>().isTrigger = false;
        objetoAgarrado.isKinematic = kinematicOriginal;
        objetoAgarrado.linearVelocity = Vector3.zero;
        objetoAgarrado.angularVelocity = Vector3.zero;

        objetoAgarrado = null;
        padreOriginal = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }

    //parte para lo de la cocina ---------------------------------------------
    private bool kinematicInicial;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) kinematicInicial = rb.isKinematic;
    }

    // Si este objeto está en la mano de alguien
    public bool EstaAgarrado => objetoAgarrado != null;

    public void AgarrarDirecto(Transform punto)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

        puntoAgarre = punto;
        objetoAgarrado = rb;
        padreOriginal = null;              // al soltarlo al piso no vuelve a la mesa
        kinematicOriginal = kinematicInicial;
        escalaOriginal = transform.localScale;

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        transform.SetParent(punto);
        transform.localPosition = offsetPosicion;
        transform.localEulerAngles = offsetRotacion;
    }

    // Acomoda eel objeto en u lugar especifico par auq eno quede mal acomodado
    
    public void ColocarEn(Transform slot)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

        Vector3 escala = objetoAgarrado != null ? escalaOriginal : transform.localScale;

        objetoAgarrado = null;
        padreOriginal = null;

        rb.isKinematic = true;
        if (col != null) col.enabled = false;

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = escala;     // que no se deforme
    }
}