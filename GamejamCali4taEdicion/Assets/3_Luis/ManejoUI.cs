using UnityEngine;
using UnityEngine.InputSystem;

public class ManejoUI : MonoBehaviour
{
    private Vector2 interaccion;
    public GameObject skin1;
    public GameObject panelskin1;
    public GameObject skin2;
    public GameObject panelskin2;
    private bool skinH = true;
    void Start()
    {
        
    }
    private void OnMove(InputValue context)
    {
        interaccion = context.Get<Vector2>();
        cambiarPersonaje(interaccion.x);
    }

    public void cambiarPersonaje(float x)
    {
        if (x > 0)
        {
            panelskin1.SetActive(true);
            panelskin2.SetActive(false);
            skinH = true;

        }
        else if (x < 0)
        {
            panelskin1.SetActive(false);
            panelskin2.SetActive(true);
            skinH = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void Empezarjuego()
    {
        gameObject.GetComponent<moveplayer>().enabled=true;
        
        if (skinH)
        {
            skin1.SetActive(true);
        }
        else
        {
            skin2.SetActive(true);
        }
        gameObject.GetComponent<ManejoUI>().enabled=false;
    }
}
