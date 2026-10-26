using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManejoUI : MonoBehaviour
{
    private Vector2 interaccion;
    public GameObject[] skins;
    public GameObject[] skinsPanels;
    public bool Pj1 = true;
    private int actindex=0;
    public bool gamestart=false;
    private Diaologo[] dialogos;
    private FlujoTutorial flujo;
    public bool dialogoActivo=true;
    void Start()
    {
        gameObject.GetComponent<moveplayer>().activarquieto();
        flujo = FindAnyObjectByType<FlujoTutorial>();
        if (Pj1)
        {
            actindex = FindAnyObjectByType<GameManager>().skinSeleccionadaP1;
        }
        else
        {
            actindex = FindAnyObjectByType<GameManager>().skinSeleccionadaP1;
        }
        Empezarjuego();
    }
    private void OnMove(InputValue context)
    {
        
        if (!gamestart) { 
        interaccion = context.Get<Vector2>();
        cambiarPersonaje(interaccion.x);
         }
    }
    private void OnInteract(InputValue context)
    {
        if (gamestart && dialogoActivo)
        {
            dialogos[flujo.DiaReprod].interact();
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
        dialogos = flujo.Dialogos;
        dialogos[0].interact();
        gameObject.GetComponent<moveplayer>().enabled=true;
        skins[actindex].SetActive(true);
        gameObject.GetComponent<moveplayer>().animator = skins[actindex].GetComponent<Animator>();
    }
}
