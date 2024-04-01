using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    protected float speed = 10f;
    protected Vector3 velocity;

    protected Rigidbody player;

    protected TextMeshProUGUI debugInfo;
    protected Dictionary<GameObject, List<Vector3>> collisionNormal = new Dictionary<GameObject, List<Vector3>>();
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
        PlayerMove();
        debugInfo.text = " ";
        foreach (var value in collisionNormal)
        {
            debugInfo.text += value.Key.name + " ";
            foreach (var normal in value.Value)
            {
                debugInfo.text += normal + " ";
            }
            debugInfo.text += "\n";
        }
    }
    private void PlayerMove()
    {
        player.velocity = velocity * speed;
    }
    private void OnCollisionStay(Collision collision)
    {
        List<Vector3> normals = new List<Vector3>();
        foreach (var value in collision.contacts)
        {
            normals.Add(value.normal);
        }
        if (collisionNormal.ContainsKey(collision.gameObject))
        {
            collisionNormal[collision.gameObject] = normals;
        }
        else
        {
            collisionNormal.Add(collision.gameObject, normals);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        collisionNormal.Remove(collision.gameObject);
    }

}

