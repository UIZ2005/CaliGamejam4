using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ZonaInstanciador : MonoBehaviour
{
    [Header("Configuración de la Zona")]
    [Tooltip("El prefab del ladrillo o cemento que se va a crear")]
    public GameObject prefabAInstanciar;
    
    [Tooltip("El tamaño del cuadrado (área de agarre)")]
    public Vector3 tamanoZona = new Vector3(2, 2, 2);
    
    [Tooltip("Color del cuadrado para diferenciar cemento de ladrillo")]
    public Color colorGizmo = Color.cyan;

    private void OnValidate()
    {
        // Esto ajusta automáticamente el BoxCollider para que tenga exactamente
        // el mismo tamaño que el cuadrado dibujado por los Gizmos.
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.size = tamanoZona;
        }
    }

    public GameObject InstanciarObjeto(Vector3 posicion)
    {
        // Crea el objeto y lo devuelve para que el jugador lo agarre inmediatamente
        return Instantiate(prefabAInstanciar, posicion, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        // Dibuja el borde del cuadrado
        Gizmos.color = colorGizmo;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, tamanoZona);
        
        // Dibuja un relleno semitransparente para ver bien el área
        Gizmos.color = new Color(colorGizmo.r, colorGizmo.g, colorGizmo.b, 0.3f);
        Gizmos.DrawCube(Vector3.zero, tamanoZona);
    }
}