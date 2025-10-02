using UnityEngine;

public class VisualsFollow : MonoBehaviour
{
    public Transform playerTarget;
    // ayuda a que un objeto sigua la posicion de otro
    void LateUpdate()
    {
        transform.position = playerTarget.position;
    }
}
