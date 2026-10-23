using UnityEngine;

public abstract class Estacion : MonoBehaviour
{
    // El jugador trae algo en la mano
    public virtual ResultadoEstacion Recibir(ItemCocina item, Vector3 posJugador)
    {
        return ResultadoEstacion.Rechazado;
    }

    // El jugador tiene las manos vacías: la estación le da algo 
    public virtual ItemCocina Entregar(Vector3 posJugador)
    {
        return null;
    }

    
    protected static int SlotMasCercano(Transform[] slots, Vector3 pos, System.Func<int, bool> condicion)
    {
        int mejor = -1;
        float min = float.MaxValue;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!condicion(i)) continue;

            float d = (slots[i].position - pos).sqrMagnitude;
            if (d < min)
            {
                min = d;
                mejor = i;
            }
        }
        return mejor;
    }
}