
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Escena de carga")]
    [SerializeField] private string escenaCarga = "LoadingScreen";

    public void Jugar()
    {
        SceneManager.LoadSceneAsync(escenaCarga);
    }
}
