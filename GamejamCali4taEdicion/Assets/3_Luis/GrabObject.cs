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
        
        objetoAgarrado.transform.SetParent(puntoAgarre);
        
        // Se aplican los valores personalizados en lugar de dejarlo en (0,0,0)
        objetoAgarrado.transform.localPosition = offsetPosicion;
        objetoAgarrado.transform.localEulerAngles = offsetRotacion;
    }

    private void SoltarObjeto()
    {
        objetoAgarrado.transform.SetParent(padreOriginal);
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
}