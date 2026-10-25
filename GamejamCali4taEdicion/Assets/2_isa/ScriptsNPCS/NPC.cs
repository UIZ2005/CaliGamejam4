using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float velocidadGiro = 10f;

    
    [SerializeField] private Animator animator;
    [SerializeField] private string parametroCaminar = "walk";

    [Header("Globo del pedido")]
    [SerializeField] private GameObject canvasPedido;
    [SerializeField] private Image[] iconos;

    private readonly List<TipoItem> pendientes = new List<TipoItem>();
    private readonly List<TipoItem> tiposEnIconos = new List<TipoItem>();
    private Vector3 destino;
    private Vector3 mirarAlLlegar;
    private bool saliendo;
    private Camera cam;

    public bool YaLlego { get; private set; }     
    public GameObject PrefabOrigen { get; set; }  

    
    public void Iniciar(List<TipoItem> pedido, ManagerNPCs fila)
    {
        cam = Camera.main;
        pendientes.Clear();
        pendientes.AddRange(pedido);
        tiposEnIconos.Clear();

        for (int i = 0; i < iconos.Length; i++)
        {
            bool hay = i < pedido.Count;
            iconos[i].gameObject.SetActive(hay);
            if (hay)
            {
                iconos[i].sprite = fila.IconoDe(pedido[i]);
                tiposEnIconos.Add(pedido[i]);
            }
        }

        if (canvasPedido) canvasPedido.SetActive(false); 
        Debug.Log($"{name} pide: {string.Join(" + ", pedido)}");
    }

    public void IrA(Vector3 posicion, Vector3 mirarHacia)
    {
        destino = posicion;
        mirarAlLlegar = mirarHacia;
    }

    
    public bool Quiere(TipoItem tipo) => YaLlego && !saliendo && pendientes.Contains(tipo);

    
    public bool Recibir(TipoItem tipo)
    {
        pendientes.Remove(tipo);

        // apaga el ícono de lo que ya recibió
        for (int i = 0; i < tiposEnIconos.Count; i++)
        {
            if (tiposEnIconos[i] == tipo && iconos[i].gameObject.activeSelf)
            {
                iconos[i].gameObject.SetActive(false);
                break;
            }
        }
        return pendientes.Count == 0;
    }

    public void Salir(Vector3 puntoSalida)
    {
        saliendo = true;
        destino = puntoSalida;
        if (canvasPedido) canvasPedido.SetActive(false);
    }

    private void Update()
    {
        Vector3 haciaDestino = destino - transform.position;
        haciaDestino.y = 0f;
        bool caminando = haciaDestino.magnitude > 0.05f;

        if (caminando)
        {
            Vector3 paso = Vector3.MoveTowards(transform.position,
                new Vector3(destino.x, transform.position.y, destino.z),
                velocidad * Time.deltaTime);
            transform.position = paso;
            Girar(haciaDestino);
        }
        else
        {
            if (saliendo)
            {
                Destroy(gameObject); // llegó a la salida
                return;
            }

            if (!YaLlego)
            {
                YaLlego = true;
                if (canvasPedido) canvasPedido.SetActive(true);
            }
            Girar(mirarAlLlegar);
        }

        if (animator && !string.IsNullOrEmpty(parametroCaminar))
            animator.SetBool(parametroCaminar, caminando);
    }

    private void Girar(Vector3 direccion)
    {
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.0001f) return;
        Quaternion objetivo = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, objetivo, velocidadGiro * Time.deltaTime);
    }

    // El globo siempre mira a la cámara
    private void LateUpdate()
    {
        if (canvasPedido && canvasPedido.activeSelf && cam)
            canvasPedido.transform.rotation = cam.transform.rotation;
    }
}
