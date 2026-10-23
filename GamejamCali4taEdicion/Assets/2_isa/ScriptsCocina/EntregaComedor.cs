using UnityEngine;

public class EntregaComedor : Estacion
{
    
    [SerializeField] private TipoItem[] pedidosAceptados = { TipoItem.Arepa, TipoItem.Cafe };

    public override ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        if (!item.TryGetTipoServido(out TipoItem servido))
            return ResultadoEstacion.Rechazado;

        if (System.Array.IndexOf(pedidosAceptados, servido) < 0)
            return ResultadoEstacion.Rechazado;

        if (NivelManager.Instancia != null)
            NivelManager.Instancia.RegistrarComida(servido);

        Destroy(item.gameObject); // se va el plato con la comida (o el café)
        return ResultadoEstacion.ObjetoRecibido;
    }
}
