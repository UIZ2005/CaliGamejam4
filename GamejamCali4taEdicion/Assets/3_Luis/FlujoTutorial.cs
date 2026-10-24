using System.Collections;
using UnityEngine;

public class FlujoTutorial : MonoBehaviour
{
    public int DiaReprod;

    public GameObject camaraviejo;
    public GameObject camaraanim;

    public GameObject[] circulos;
    public GameObject[] Botones;
    public Diaologo[] Dialogos;


    public ManejoUI[] manejouis;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void aumentarDialogo()
    {
        DiaReprod += 1;

        foreach(ManejoUI pj in manejouis)
        {
            pj.dialogoActivo = false;
        }

        accionDialogo(DiaReprod);
    }
    public void accionDialogo(int Dialog)
    {
        switch (Dialog)
        {
            case 1:
                //Camara alejandose, circulo de donde deben acercarse activaddo,activar botones que se deben oprimir en ese punto
                break;
            case 2:
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
}
