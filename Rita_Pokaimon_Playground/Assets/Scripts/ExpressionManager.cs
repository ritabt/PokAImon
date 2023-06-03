using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionManager : MonoBehaviour
{

    public GameObject expDefault;
    public GameObject expHurt;

    private Rigidbody m_Rb;


    // Start is called before the first frame update
    void Start()
    {
        m_Rb = GetComponent<Rigidbody>();
        expDefault.SetActive(true);
        expHurt.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("floor"))
        {
            expDefault.SetActive(false);
            expHurt.SetActive(true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("floor"))
        {
            expDefault.SetActive(true);
            expHurt.SetActive(false);
        }
    }
}
