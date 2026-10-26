using UnityEngine;

public class actionEvent : MonoBehaviour
{
    private FlujoTutorial flujo;
    public int Accion;
    public bool concolision = false;
    public bool limpiar = false;
    public GameObject boton;
    public GameObject circulo;
    void Start()
    {
        flujo = FindAnyObjectByType<FlujoTutorial>();
    }

    public void enviarAccion()
    {
        flujo.ActivarAccion(Accion);

        if (limpiar && Accion==0)
        {
            Destroy(circulo);
            Destroy(boton);
            Destroy(gameObject);
        }
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
