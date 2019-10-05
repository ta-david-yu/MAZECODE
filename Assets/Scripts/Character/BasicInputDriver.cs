using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentController))]
public class BasicInputDriver : MonoBehaviour
{
    [SerializeField]
    private AgentController m_Agent;

    // Update is called once per frame
    void Update()
    {
        float horiztonal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 moveVec = new Vector2(horiztonal, vertical);
        m_Agent.Move(moveVec);

        if (Mathf.Abs(horiztonal) > 0 || Mathf.Abs(vertical) > 0)
        {
            m_Agent.Face(moveVec);
        }
    }
}
