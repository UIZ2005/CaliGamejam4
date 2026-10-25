using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.PlayerInput;

public class Diaologo : MonoBehaviour
{
    [Header("Sistema de dialogos")]
    public string nombreNpc;
    public GameObject dialogopanel;
    public Animator viejito;

    public TextMeshProUGUI texto;

    [SerializeField, TextArea(4, 6)]
    private string[] lineasDialogo;

    public float typingtext = 0.05f;

    private bool didDialagoStart;
    private int LineIndex;
    private moveplayer[] player;
    private FlujoTutorial flujo;

    private audioManager audioManager;

    private void Start()
    {
        flujo = FindAnyObjectByType<FlujoTutorial>();
        audioManager = FindAnyObjectByType<audioManager>();
        player = FindObjectsByType<moveplayer>(FindObjectsSortMode.None);
    }


    public void interact()
    {

            if (!didDialagoStart)
            {

                startdialogo();
                viejito.SetInteger("talk", 1);
            }
            else if (texto.text == ObtenerLineaActual())
            {
                nextdialogoLine();
                viejito.SetInteger("talk", 2);
            }
            else
            {
                StopAllCoroutines();
                texto.text = ObtenerLineaActual();
                viejito.SetInteger("talk", 3);
            }
    }

    public void startdialogo()
    {
        foreach (moveplayer pj in player)
        {
            pj.activarquieto();
        }
        
        didDialagoStart = true;

        dialogopanel.SetActive(true);


        LineIndex = 0;

        StartCoroutine(ShowLine());
    }

    IEnumerator ShowLine()
    {
        texto.text = string.Empty;

        foreach (char ch in ObtenerLineaActual())
        {
            texto.text += ch;
            yield return new WaitForSecondsRealtime(typingtext);
        }

        yield return null;
    }

    private string ObtenerLineaActual()
    {

        return lineasDialogo[LineIndex];
    }

    private string[] ObtenerDialogoActual()
    {

        return lineasDialogo;
    }

    public void nextdialogoLine()
    {
        LineIndex++;

        string[] dialogoActual = ObtenerDialogoActual();

        if (LineIndex < dialogoActual.Length)
        {
            StartCoroutine(ShowLine());
        }
        else
        {
            didDialagoStart = false;

            dialogopanel.SetActive(false);
            flujo.aumentarDialogo();
            viejito.SetInteger("talk",0);

            texto.text = "";
            gameObject.SetActive(false);
        }

    }
}