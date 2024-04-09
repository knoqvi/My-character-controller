using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;

    private Vector3 velocity;
    private Vector3 movement;
    private Vector3 planeVector;
    private Dictionary<GameObject, List<Vector3>> GameObjectAndNormals = new Dictionary<GameObject, List<Vector3>>();
    private List<Vector3> normals = new List<Vector3>();
    
    private TextMeshProUGUI debugInfo;
    private Rigidbody player;
    
    private void Awake()
    {
        player = GetComponent<Rigidbody>();
        debugInfo = GameObject.FindGameObjectWithTag("debug_text").GetComponent<TextMeshProUGUI>();
    }
    private void Update()
    {
        velocity = Vector3.Normalize(transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical"));
    }
    private void FixedUpdate()
    {
        DebugInfo();
        PlaneProjection();
        PlayerMove();
    }
    private void PlayerMove()
    {
        player.velocity = movement * speed;
    }
    private void PlaneProjection()
    {
        NormalsTransfer();
        Vector3 normal = FindMinNormal();
        movement = Vector3.ProjectOnPlane(velocity, normal).normalized;
    }
    private Vector3 FindMinNormal()
    {
        Vector3 minNormal = normals.Count > 0 ? normals[0] : Vector3.zero;
        foreach (var normal in normals)
        {
            if (normal.y < minNormal.y)
            {
                minNormal = normal;
            }
        }
        normals.Clear();
        return minNormal;
    }
    private void NormalsTransfer()
    {
        foreach (var values in GameObjectAndNormals.Values)
        {
            foreach (var normal in values)
            {
                normals.Add(normal);
            }
        }
    }
    private void DebugInfo()
    {

        debugInfo.text = " ";
        foreach (var value in GameObjectAndNormals)
        {
            debugInfo.text += value.Key.name + " ";
            foreach (var normal in value.Value)
            {
                debugInfo.text += normal + " ";
            }
            debugInfo.text += "\n";
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        List<Vector3> getNormals = new List<Vector3>();
        foreach (var value in collision.contacts)
        {
            getNormals.Add(value.normal);
        }
        if (GameObjectAndNormals.ContainsKey(collision.gameObject))
        {
            GameObjectAndNormals[collision.gameObject] = getNormals;
        }
        else
        {
            GameObjectAndNormals.Add(collision.gameObject, getNormals);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        GameObjectAndNormals.Remove(collision.gameObject);
    }
}
