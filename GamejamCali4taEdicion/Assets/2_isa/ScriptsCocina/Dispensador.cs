using UnityEngine;

public class Dispensador : Estacion
{
    [SerializeField] private ItemCocina prefab;
   
    [SerializeField] private bool aceptaDevolucion = true;
    
    [SerializeField] private GameObject tapa;

    private bool Tapada => tapa != null && tapa.activeInHierarchy;

    public override ItemCocina Entregar(Vector3 posJugador)
    {
        if (Tapada) return null;
        return Instantiate(prefab, transform.position + Vector3.up, Quaternion.identity);
    }

    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        if (!aceptaDevolucion || Tapada) return ResultadoEstacion.Rechazado;
        if (item.tipo != prefab.tipo) return ResultadoEstacion.Rechazado;
        if (item.Estado != prefab.EstadoInicial) return ResultadoEstacion.Rechazado; // una arepa asada no vuelve a la caja
        if (item.EsPlato && !item.PlatoVacio) return ResultadoEstacion.Rechazado;

        Destroy(item.gameObject);
        return ResultadoEstacion.ObjetoRecibido;
    }
}
