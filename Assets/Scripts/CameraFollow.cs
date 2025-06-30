using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    float followSpeed = 5f;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, target.position.y, -10), followSpeed * Time.deltaTime);
    }
}