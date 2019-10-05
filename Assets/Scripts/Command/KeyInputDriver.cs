using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BinarySignalReader))]
public class KeyInputDriver : MonoBehaviour
{
    [SerializeField]
    private BinarySignalReader m_Reader;

    private bool m_IsPressed = false;

    // Update is called once per frame
    void Update()
    {
        if (m_IsPressed)
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                m_Reader.ToLowState();
                m_IsPressed = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Reader.ToHighState();
                m_IsPressed = true;
            }
        }
    }
}
