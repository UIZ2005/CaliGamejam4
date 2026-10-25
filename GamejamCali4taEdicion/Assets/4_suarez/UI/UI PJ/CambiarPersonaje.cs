
using UnityEngine;

public class CambiarPersonaje : MonoBehaviour
{
    [Header("Avatares disponibles")]
    [SerializeField] private GameObject avatarA;
    [SerializeField] private GameObject avatarB;

    // 0 = Avatar A, 1 = Avatar B
    private int currentAvatar = 0;

    private void Start()
    {
        // Mostrar únicamente el avatar inicial
        ShowAvatar(0);
    }

    public void NextAvatar()
    {
        currentAvatar = (currentAvatar + 1) % 2;
        ShowAvatar(currentAvatar);
    }

    public void PreviousAvatar()
    {
        currentAvatar = (currentAvatar - 1 + 2) % 2;
        ShowAvatar(currentAvatar);
    }

    private void ShowAvatar(int index)
    {
        currentAvatar = index;

        if (avatarA != null)
            avatarA.SetActive(index == 0);

        if (avatarB != null)
            avatarB.SetActive(index == 1);
    }
}
