using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdOnItems : MonoBehaviour
{
    public int id = 0;
    public float groundDistance = 3f;
    public LayerMask groundMask;
    public bool isGrounded;
    Rigidbody m_Rigidbody;
    public int ammo = 15;
    void Start()
    {
        if (id == 3)
        {
            ammo = 15;
        }
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundDistance, groundMask);
        if (isGrounded)
        {
            m_Rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        }
        else
        {
            m_Rigidbody.constraints = RigidbodyConstraints.None;
        }
        if (gameObject.transform.parent == null)
        {
            m_Rigidbody.isKinematic = false;
            if (id == 1)
            {
                transform.rotation = Quaternion.Euler(90f, 0, 90f);
            }
            else if (id == 2)
            {
                transform.rotation = Quaternion.Euler(0, 0, 90f);
            }
        }
    }
}
