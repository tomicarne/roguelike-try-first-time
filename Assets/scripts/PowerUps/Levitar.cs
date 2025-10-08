using UnityEngine;

public class Levitar : MonoBehaviour
{
    private Vector3 posicionInicial;
    [SerializeField] private float alturaLevitacion;
    [SerializeField] private float velocidadLevitacion;
    void Start()
    {
        posicionInicial = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float offsetVertical = Mathf.Sin(Time.time * velocidadLevitacion) * alturaLevitacion;
        transform.position = new Vector3(posicionInicial.x, posicionInicial.y + offsetVertical, posicionInicial.z);
    }
}
