using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManejoUI : MonoBehaviour
{
    private Vector2 interaccion;
    public GameObject[] skins;
    private GameObject[] skinsPanels;
    public bool Pj1 = true;
    private int actindex=0;
    public bool gamestart=false;
    private Diaologo[] dialogos;
    private FlujoTutorial flujo;
    public bool dialogoActivo=true;
    void Start()
    {
        flujo = FindAnyObjectByType<FlujoTutorial>();

        if (FindAnyObjectByType<GameManager>() != null)
        {
            if (Pj1)
            {
                actindex = FindAnyObjectByType<GameManager>().skinSeleccionadaP1;
            }
            else
            {
                actindex = FindAnyObjectByType<GameManager>().skinSeleccionadaP2;
            }

        }
        

        Debug.Log(actindex);

        if (gamestart)
        {
            gameObject.GetComponent<moveplayer>().activarquieto();
            Empezarjuego();
        }
    }
    private void OnInteract(InputValue context)
    {
        if (gamestart && dialogoActivo)
        {
            dialogos[flujo.DiaReprod].interact();
        }
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
