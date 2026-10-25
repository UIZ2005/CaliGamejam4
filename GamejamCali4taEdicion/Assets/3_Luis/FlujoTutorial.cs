using System.Collections;
using UnityEngine;

public class FlujoTutorial : MonoBehaviour
{
    public int DiaReprod;

    public GameObject camaraviejo;
    public GameObject camaraanim;
    private Animator camarator;
    public Animator viejito;

    public GameObject[] acciones;
    public GameObject[] circulos;
    public GameObject[] Botones;
    public Diaologo[] Dialogos;


    public ManejoUI[] manejouis;

    void Start()
    {
        camarator = camaraanim.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ActivarAccion(int x)
    {
        switch (x)
        {
            case 1:
                StartCoroutine(accion1());
                //Se activa cuando coje la arepa, lo que hace es desactivar los botones de la arepa y activar el circulo y los botones de la parrilla 
                break;
            case 2:
                StartCoroutine(accion2());
                break;
            case 3:
                StartCoroutine(accion3());
                break;
        }
    }
    public void activardialogoAccion()
    {
        foreach (ManejoUI pj in manejouis)
        {
            pj.dialogoActivo = true;
        }
    }

    IEnumerator accion1()
    {
        camarator.SetInteger("Steps", 1);
        Dialogos[1].gameObject.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        Botones[0].SetActive(false);
        circulos[1].SetActive(true);
        yield return new WaitForSeconds(1f);
        Botones[1].SetActive(true);
        circulos[1].SetActive(false);
        acciones[1].SetActive(true);
        acciones[0].SetActive(false);
        yield return null;
    }
    IEnumerator accion2()
    {
        viejito.SetInteger("talk", 2);
        camaraviejo.SetActive(true);
        Dialogos[1].interact();
        activardialogoAccion();
        Botones[1].SetActive(false);
        acciones[2].SetActive(true);
        acciones[1].SetActive(false);
        Dialogos[2].gameObject.SetActive(true);
        yield return null;
    }
    IEnumerator accion3()
    {
        viejito.SetInteger("talk", 3);
        camaraviejo.SetActive(true);
        Dialogos[2].interact();
        activardialogoAccion();
        Botones[2].SetActive(false);
        acciones[2].SetActive(false);
        acciones[3].SetActive(true);
        yield return null;
    }

        public void aumentarDialogo()
    {
        DiaReprod += 1;

        foreach (ManejoUI pj in manejouis)
        {
            pj.dialogoActivo = false;
        }
        viejito.SetInteger("talk", 0);
        accionDialogo(DiaReprod);
    }
    public void accionDialogo(int Dialog)
    {
        switch (Dialog)
        {
            case 1:
                StartCoroutine(dialogo1finaliza());
                //Camara alejandose, circulo de donde deben acercarse activaddo,activar botones que se deben oprimir en ese punto
                break;
            case 2:
                StartCoroutine(dialogo2finaliza());
                break;
            case 3:
                StartCoroutine(dialogo3finaliza());
                break;
        }
    }
    public void activardialogos()
    {
        foreach (ManejoUI pj in manejouis)
        {
            pj.dialogoActivo = true;
        }
    }

    IEnumerator dialogo1finaliza()
    {
        camaraviejo.SetActive(false);
        camaraanim.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        circulos[0].SetActive(true);
        yield return new WaitForSeconds(1f);
        Botones[0].SetActive(true);
        circulos[0].SetActive(false);
        activarmovimiento();


        yield return null;
    }
    IEnumerator dialogo2finaliza()
    {
        camarator.SetInteger("Steps",2);
        camaraviejo.SetActive(false);
        yield return new WaitForSeconds(0.8f);
        circulos[2].SetActive(true);
        yield return new WaitForSeconds(1f);
        Botones[2].SetActive(true);
        circulos[2].SetActive(false);
        yield return null;
    }
    IEnumerator dialogo3finaliza()
    {
        camarator.SetInteger("Steps", 3);
        camaraviejo.SetActive(false);
        yield return new WaitForSeconds(0.8f);
        circulos[3].SetActive(true);
        yield return new WaitForSeconds(1f);
        Botones[3].SetActive(true);
        circulos[3].SetActive(false);
        yield return null;
    }


    private void activarmovimiento()
    {
        foreach (ManejoUI pj in manejouis)
        {
            pj.gameObject.GetComponent<moveplayer>().desactivarquieto();
        }
    }
}
