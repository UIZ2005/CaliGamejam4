
using UnityEngine;

public class BarraEnergiaBillboard : MonoBehaviour
{
    public Camera camara;

    private void LateUpdate()
    {
        if (camara == null) return;

        transform.rotation = camara.transform.rotation;
    }
}
