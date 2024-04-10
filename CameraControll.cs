using System;
using UnityEngine;
public class CameraControll : MonoBehaviour
{
    public float sensitiveX = 1f;
    public float sensitiveY = 1f;

    [SerializeField]
    private Vector3 cameraOffset = new Vector3(0, 0, 0);
    [SerializeField]
    private GameObject Player;

    private float _xRotate;
    private float _yRotate;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        transform.position = Player.transform.position + cameraOffset;

        _xRotate += Input.GetAxis("Mouse Y") * sensitiveY;
        _yRotate -= Input.GetAxis("Mouse X") * sensitiveX;

        _xRotate = Math.Clamp(_xRotate, -90, 90);
    }
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(_xRotate, _yRotate, 0);
        Player.transform.rotation = Quaternion.Euler(0, _yRotate, 0);
    }
}
