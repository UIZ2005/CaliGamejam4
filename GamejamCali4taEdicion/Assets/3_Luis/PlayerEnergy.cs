
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energia")]
    [SerializeField] private float energiaMaxima = 100f;
    [SerializeField] private float energiaActual = 100f;
    [SerializeField] private float consumoPorSegundo = 8f;

    [Header("Interfaz")]
    [SerializeField] private Slider barraEnergia;
    [SerializeField] private Image rellenoBarra;

    [Header("Colores")]
    [SerializeField] private Color colorAlto = Color.green;
    [SerializeField] private Color colorMedio = Color.yellow;
    [SerializeField] private Color colorBajo = Color.red;

    private bool trabajando;

    public float EnergiaActual => energiaActual;
    public float EnergiaMaxima => energiaMaxima;
    public bool Agotado => energiaActual <= 0f;

    private void Start()
    {
        energiaActual = Mathf.Clamp(
            energiaActual, 0f, energiaMaxima
        );

        ActualizarInterfaz();
    }

    private void Update()
    {
        if (trabajando && !Agotado)
        {
            energiaActual -= consumoPorSegundo * Time.deltaTime;
            energiaActual = Mathf.Max(0f, energiaActual);

            ActualizarInterfaz();
        }
    }

    public void SetTrabajando(bool estado)
    {
        trabajando = estado && !Agotado;
    }

    public bool PuedeTrabajar()
    {
        return !Agotado;
    }

    public void RecuperarEnergia(float cantidad)
    {
        if (energiaActual <= 0)
        {
            gameObject.GetComponent<moveplayer>().recuperarVelocidad();
        }

        energiaActual = Mathf.Clamp(
            energiaActual + cantidad,
            0f,
            energiaMaxima
        );
        ActualizarInterfaz();
    }

    private void ActualizarInterfaz()
    {
        if (barraEnergia != null)
        {
            barraEnergia.maxValue = energiaMaxima;
            barraEnergia.value = energiaActual;
        }

        if (rellenoBarra != null)
        {
            float porcentaje = energiaActual / energiaMaxima;

            if (porcentaje > 0.5f)
                rellenoBarra.color = colorAlto;
            else if (porcentaje > 0.25f)
                rellenoBarra.color = colorMedio;
            else
                rellenoBarra.color = colorBajo;
        }
    }
}
