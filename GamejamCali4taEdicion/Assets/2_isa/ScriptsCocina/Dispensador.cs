using UnityEngine;

public class Dispensador : Estacion
{
    [SerializeField] private ItemCocina prefab;
    [SerializeField] private bool aceptaDevolucion = true;
    [SerializeField] private GameObject tapa;

    //  espacio encima de la caja para dejar UN objeto (como una mesa de 1 puesto)
    private ItemCocina objetoEncima;
    [SerializeField] private Transform puntoEncima;

    private bool Tapada => tapa != null && tapa.activeInHierarchy;

    public override ItemCocina Entregar(Vector3 posJugador)
    {
        //  si hay algo encima, primero te da eso
        if (objetoEncima != null)
        {
            ItemCocina item = objetoEncima;
            objetoEncima = null;
            return item;
        }

        if (Tapada) return null;
        return Instantiate(prefab, transform.position + Vector3.up, Quaternion.identity);
    }

    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        // Devolver el mismo ingrediente a su caja (solo si está abierta)
        if (aceptaDevolucion && !Tapada &&
            item.tipo == prefab.tipo &&
            item.Estado == prefab.EstadoInicial &&
            !(item.EsPlato && !item.PlatoVacio))
        {
            Destroy(item.gameObject);
            return ResultadoEstacion.ObjetoRecibido;
        }

        //  si no, se deja encima de la caja (máximo 1 objeto)
        if (puntoEncima != null && objetoEncima == null)
        {
            objetoEncima = item;
            item.Grab.ColocarEn(puntoEncima);
            return ResultadoEstacion.ObjetoRecibido;
        }

        return ResultadoEstacion.Rechazado; // ya hay algo encima: sigue en la mano
    }
}
