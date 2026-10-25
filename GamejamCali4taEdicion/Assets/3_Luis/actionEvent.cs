using UnityEngine;

public class actionEvent : MonoBehaviour
{
    private FlujoTutorial flujo;
    public int Accion;
    public bool concolision = false;
    void Start()
    {
        flujo = FindAnyObjectByType<FlujoTutorial>();
    }

    public void enviarAccion()
    {
        flujo.ActivarAccion(Accion);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && concolision)
        {
            flujo.ActivarAccion(Accion);

            gameObject.SetActive(false);
        }
    }
}
