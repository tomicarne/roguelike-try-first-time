using UnityEngine;

public class VisualsFollow : MonoBehaviour
{
    public Transform playerTarget;
    void LateUpdate()
    {
        transform.position = playerTarget.position;
    }
}
