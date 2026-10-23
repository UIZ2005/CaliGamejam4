
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class LoadingManager : MonoBehaviour
{
    [Header("Video de carga")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Escena del videojuego")]
    [SerializeField] private string escenaJuego = "GameScene";

    private bool videoTerminado;

    private void Start()
    {
        StartCoroutine(CargarJuego());
    }

    private IEnumerator CargarJuego()
    {
        if (videoPlayer == null)
        {
            Debug.LogError(
                "LoadingManager: falta asignar el VideoPlayer."
            );
            yield break;
        }

        // Preparar el video antes de reproducirlo.
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += AlTerminarVideo;

        videoPlayer.Prepare();

        // Esperar hasta que el video este preparado.
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // Reproducir la animacion de carga.
        videoPlayer.Play();

        // Comenzar la carga del videojuego en segundo plano.
        AsyncOperation carga = SceneManager.LoadSceneAsync(
            escenaJuego,
            LoadSceneMode.Single
        );

        if (carga == null)
        {
            Debug.LogError(
                "No se pudo iniciar la carga de la escena: "
                + escenaJuego
            );

            videoPlayer.loopPointReached -= AlTerminarVideo;
            yield break;
        }

        // Evitar que el juego aparezca antes de tiempo.
        carga.allowSceneActivation = false;

        // Esperar a que el juego este cargado y el video termine.
        while (carga.progress < 0.9f || !videoTerminado)
        {
            yield return null;
        }

        // Permitir el cambio a la escena del videojuego.
        carga.allowSceneActivation = true;

        while (!carga.isDone)
        {
            yield return null;
        }

        videoPlayer.loopPointReached -= AlTerminarVideo;
    }

    private void AlTerminarVideo(VideoPlayer source)
    {
        videoTerminado = true;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= AlTerminarVideo;
        }
    }
}
