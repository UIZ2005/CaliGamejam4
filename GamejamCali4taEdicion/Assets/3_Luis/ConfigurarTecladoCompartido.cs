
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class ConfigurarTecladoCompartido : MonoBehaviour
{
    [SerializeField] private string mapaAcciones = "Player1";
    [SerializeField] private string esquemaControl = "Teclado";

    private PlayerInput playerInput;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        if (Keyboard.current == null)
        {
            Debug.LogError("No se detectó un teclado.");
            return;
        }

        // Emparejar el teclado con este jugador.
        if (Mouse.current != null &&
            esquemaControl == "Teclado")
        {
            playerInput.SwitchCurrentControlScheme(
                esquemaControl,
                Keyboard.current,
                Mouse.current
            );
        }
        else
        {
            playerInput.SwitchCurrentControlScheme(
                "Teclado",
                Keyboard.current
            );
        }

        // Activar el mapa correspondiente.
        playerInput.SwitchCurrentActionMap(mapaAcciones);
    }
}
