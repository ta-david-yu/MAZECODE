using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAnimationController : MonoBehaviour
{
    [SerializeField]
    private Transform m_Appearance;

    [SerializeField]
    private CommandInputDriver m_InputDriver;

    private Tweener m_Twner;

    private void OnEnable()
    {
        m_InputDriver.OnReceivedCommand += handleOnReceviedCommand;
    }

    private void OnDisable()
    {
        m_InputDriver.OnReceivedCommand -= handleOnReceviedCommand;
    }

    private void handleOnReceviedCommand()
    {
        if (m_Twner != null)
        {
            m_Twner.Abort();
            m_Twner = null;
        }

        m_Twner = TweenManager.Instance.Tween((float progress) =>
        {
            m_Appearance.localScale = Vector3.one * (1 + 0.3f * (1 - progress));
        }).SetEase(EasingFunction.Ease.EaseOutSine).SetTime(0.45f);

        m_Twner.OnTerminateCallback += () =>
        {
            m_Twner = null;
        };
    }
}
