using UnityEngine;

public class EntregaComedor : Estacion
{
    
    [SerializeField] private TipoItem[] pedidosAceptados = { TipoItem.Arepa, TipoItem.Cafe, TipoItem.Sancocho };

    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        if (!item.TryGetTipoServido(out TipoItem servido))
        {
            // DEBUG: dice por qué no lo recibió
            Debug.Log($"[Entrega] RECHAZADO: '{item.name}' no tiene comida lista (tipo={item.tipo}, plato vacío={item.PlatoVacio})");
            return ResultadoEstacion.Rechazado;
        }
        if(ManagerNPCs.Instancia != null)
        {
            if (!ManagerNPCs.Instancia.Entregar(servido))
            {
                Debug.Log("No hay nadie en la fila con ese pedido");
                return ResultadoEstacion.SeQuedaEnMano;
            }
        }

        else if (System.Array.IndexOf(pedidosAceptados, servido) < 0)
        {
            // DEBUG: la comida está bien, pero no está en la lista del Inspector
            Debug.Log($"[Entrega] RECHAZADO: '{servido}' no está en Pedidos Aceptados de {name}");
            return ResultadoEstacion.Rechazado;
        }

        Debug.Log($"[Entrega] Entregado: {servido}"); // DEBUG

        Destroy(item.gameObject); // se va el plato con la comida (o el café)
        return ResultadoEstacion.ObjetoRecibido;
    }
}
