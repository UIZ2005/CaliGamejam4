using UnityEngine;

public class actionEvent : MonoBehaviour
{
    private FlujoTutorial flujo;
    public int Accion;
    void Start()
    {
        flujo = FindAnyObjectByType<FlujoTutorial>();
    }

    public void enviarAccion()
    {
        flujo.ActivarAccion(Accion);
    }
}
