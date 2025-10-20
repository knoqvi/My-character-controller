using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private Transform camera;

    private void Update()
    {

        Vector3 newPosition = camera.position;
        newPosition.x = player.position.x;
        camera.position = newPosition;

    }
}
