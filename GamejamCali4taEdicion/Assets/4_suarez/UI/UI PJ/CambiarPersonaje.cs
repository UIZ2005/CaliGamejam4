
using UnityEngine;

public class CambiaPersonaje : MonoBehaviour
{
    [Header("Avatares de esta posición")]
    [SerializeField] private GameObject[] avatars = new GameObject[4];

    [Header("Configuración")]
    [SerializeField] private int startingIndex = 0;
    public bool pj1 = true;
    private GameManager gameManager;

    private int currentAvatar = 0;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (avatars == null || avatars.Length == 0)
        {
            Debug.LogWarning(
                "CharacterSwitcher: no hay avatares asignados.",
                this
            );
            return;
        }

        currentAvatar = Mathf.Clamp(
            startingIndex, 0, avatars.Length - 1
        );

        ShowAvatar(currentAvatar);
    }

    public void NextAvatar()
    {
        if (!HasValidAvatars()) return;

        currentAvatar = (currentAvatar + 1) % avatars.Length;
        ShowAvatar(currentAvatar);
    }

    public void PreviousAvatar()
    {
        if (!HasValidAvatars()) return;

        currentAvatar =
            (currentAvatar - 1 + avatars.Length) % avatars.Length;

        ShowAvatar(currentAvatar);
    }

    private void ShowAvatar(int index)
    {
        for (int i = 0; i < avatars.Length; i++)
        {
            if (avatars[i] != null)
            {
                avatars[i].SetActive(i == index);
            }
        }
    }

    private bool HasValidAvatars()
    {
        if (avatars == null || avatars.Length == 0)
            return false;

        foreach (GameObject avatar in avatars)
        {
            if (avatar == null)
            {
                Debug.LogWarning(
                    "CharacterSwitcher: falta asignar un avatar.",
                    this
                );
                return false;
            }
        }

        return true;
    }

    public void EnviarAlManager()
    {
        if (pj1)
        {
            gameManager.skinSeleccionadaP1 = currentAvatar;
        }
        else
        {
            gameManager.skinSeleccionadaP2 = currentAvatar;
        }
    }
}
