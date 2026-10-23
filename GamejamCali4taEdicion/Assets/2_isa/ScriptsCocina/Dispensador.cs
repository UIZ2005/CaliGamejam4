using UnityEngine;

public class Dispensador : Estacion
{
    public ItemCocina prefab;
    [SerializeField] private bool aceptaDevolucion = true;

    public override ItemCocina Entregar(Vector3 posJugador)
    {
        return Instantiate(prefab, transform.position + Vector3.up, Quaternion.identity);
    }

    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        if (!aceptaDevolucion) return ResultadoEstacion.Rechazado;
        if (item.tipo != prefab.tipo) return ResultadoEstacion.Rechazado;
        if (item.Estado != prefab.EstadoInicial) return ResultadoEstacion.Rechazado; // una arepa asada no vuelve a la caja
        if (item.EsPlato && item.Comida != null) return ResultadoEstacion.Rechazado;

        Destroy(item.gameObject);
        return ResultadoEstacion.ObjetoRecibido;
    }
}
