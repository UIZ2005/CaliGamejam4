using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManejoUI : MonoBehaviour
{
    private Vector2 interaccion;
    public GameObject[] skins;
    public GameObject[] skinsPanels;
    private int actindex=0;
    private bool gamestart=false;
    void Start()
    {
        gameObject.GetComponent<moveplayer>().activarquieto();
    }
    private void OnMove(InputValue context)
    {
        if (!gamestart) { 
        interaccion = context.Get<Vector2>();
        cambiarPersonaje(interaccion.x);
         }
    }

    public void cambiarPersonaje(float x)
    {
        actindex += ((int)x);
        if (x > 0)
        {
            skinsPanels[actindex - 1].SetActive(false);
            if (actindex > skinsPanels.Length - 1)
            {
                actindex = 0;
            }
            skinsPanels[actindex].SetActive(true);


        }
        else if (x < 0)
        {
            skinsPanels[actindex + 1].SetActive(false);
            if (actindex < 0)
            {
                actindex = skinsPanels.Length-1;
            }
            skinsPanels[actindex].SetActive(true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void Empezarjuego()
    {
        gamestart = true;
        gameObject.GetComponent<moveplayer>().enabled=true;
        gameObject.GetComponent<moveplayer>().desactivarquieto();

        skins[actindex].SetActive(true);
        gameObject.GetComponent<moveplayer>().animator = skins[actindex].GetComponent<Animator>();

        gameObject.GetComponent<ManejoUI>().enabled=false;
    }
}
