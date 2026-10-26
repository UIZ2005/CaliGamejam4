using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscenaFInal : MonoBehaviour
{
    public GameObject dialogo;
    public Animator camara;
    public moveplayer Player1;
    public moveplayer Player2;
    private Animator pj1;
    private Animator pj2;

    private bool inicioCinematica = false;
    void Start()
    {
        pj1 = Player1.animator;
        pj2 = Player2.animator;
    }

    // Update is called once per fram
    public void iniciarfin()
    {
        Destroy(dialogo);
        StartCoroutine(cinematica());
    }

    IEnumerator cinematica()
    {
        pj1.SetBool("lose", true);
        pj2.SetBool("lose", true);
        camara.SetInteger("step", 1);
        yield return new WaitForSeconds(7.5f);
        SceneManager.LoadScene("3D Main Menu");
        yield return null;
    }
}
