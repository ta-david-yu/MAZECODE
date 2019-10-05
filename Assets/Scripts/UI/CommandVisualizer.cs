using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandVisualizer : MonoBehaviour
{
    enum SignalSequenceUIAnchor
    {
        Center,
        Right
    }

    [Header("Prefab")]
    [SerializeField]
    private RectTransform m_SignalSequenceAnchorPrefab;

    [Space]
    [SerializeField]
    private RectTransform m_ShortSignalPrefab;

    [SerializeField]
    private RectTransform m_LongSignalPrefab;

    [Header("Reference")]
    [SerializeField]
    private BinarySignalReader m_CommandReader;

    [Space]
    [SerializeField]
    private Image m_IndicatorImg;

    [SerializeField]
    private RectTransform m_IndicatorRoot;

    [SerializeField]
    private CanvasGroup m_IndicatorShadowCanvasGroup;



    [Space]
    [SerializeField]
    private RectTransform m_SignalSequenceRoot;

    [Header("Settings")]
    [SerializeField]
    private SignalSequenceUIAnchor m_Anchor = SignalSequenceUIAnchor.Center;

    [Space]
    [SerializeField]
    private Color m_ShortColor;

    [SerializeField]
    private Color m_LongColor;

    [SerializeField]
    private Color m_LowColor;

    [SerializeField]
    private Color m_FilledLongColor;

    [Space]
    [SerializeField]
    private float m_MinWidth = 10;

    [SerializeField]
    private float m_MaxWidth = 120;


    private RectTransform m_SignalSequenceAnchor;

    private List<RectTransform> m_SignalBars = new List<RectTransform>();
    private float m_TotalBarWidth = 0;


    private float m_LerpToLowDuration = 0.2f;
    private Color m_ToLowOriginalColor;
    private float m_LerpToLowTimer = 0.2f;

    private Tweener m_IndicatorShadowTwner = null;
    private Tweener m_SignalBarTwner = null;
    private Tweener m_IndicatorRootTwner = null;

    private float m_BarSequenceTargetPosX = 0;

    // pool
    //private List<GameObject> m_ShortSignalPools = new List<GameObject>();
    //private List<GameObject> m_LongSignalPools = new List<GameObject>();

    private void OnEnable()
    {
        m_CommandReader.OnBinaryStateChanged += handleOnBinaryStateChanged;
        m_CommandReader.OnBeginSignalSequence += handleOnBeginSignalSequence;
        m_CommandReader.OnEndSignalSequence += handleOnEndSignalSequence;
        m_CommandReader.OnToLongSignalProgressChanged += handleOnToLongSignalProgressChanged;
        m_CommandReader.OnBecomeLongSignal += handleOnBecomeLongSignal;
        m_CommandReader.OnPushNewSignal += handleOnPushNewSignal;

        m_CommandReader.OnHighToLow += handleOnHighToLow;
        m_CommandReader.OnLowToHigh += handleOnLowToHigh;
    }

    private void OnDisable()
    {
        m_CommandReader.OnBinaryStateChanged -= handleOnBinaryStateChanged;
        m_CommandReader.OnBeginSignalSequence -= handleOnBeginSignalSequence;
        m_CommandReader.OnEndSignalSequence -= handleOnEndSignalSequence;
        m_CommandReader.OnToLongSignalProgressChanged -= handleOnToLongSignalProgressChanged;
        m_CommandReader.OnBecomeLongSignal -= handleOnBecomeLongSignal;
        m_CommandReader.OnPushNewSignal -= handleOnPushNewSignal;

        m_CommandReader.OnHighToLow -= handleOnHighToLow;
        m_CommandReader.OnLowToHigh -= handleOnLowToHigh;
    }

    private void Update()
    {
        if (m_LerpToLowTimer < m_LerpToLowDuration)
        {
            m_LerpToLowTimer += Time.deltaTime;
            m_IndicatorImg.color = Color.Lerp(m_ToLowOriginalColor, m_LowColor, m_LerpToLowTimer / m_LerpToLowDuration);
        }

        if (m_SignalSequenceAnchor != null)
        {
            if (Mathf.Abs(m_BarSequenceTargetPosX - m_SignalSequenceAnchor.anchoredPosition.x) > 0.001f)
            {
                var y = m_SignalSequenceAnchor.anchoredPosition.y;
                m_SignalSequenceAnchor.anchoredPosition = new Vector2(Mathf.Lerp(m_SignalSequenceAnchor.anchoredPosition.x, m_BarSequenceTargetPosX, Time.deltaTime * 5.0f), y);
            }
        }
    }

    private void handleOnHighToLow(BinarySignalReader.SignalType signalType)
    {
        if (m_IndicatorRootTwner != null)
        {
            m_IndicatorRootTwner.Abort();
            m_IndicatorRootTwner = null;
        }

        m_IndicatorRootTwner = TweenManager.Instance.Tween((float progress) =>
        {
            m_IndicatorRoot.anchoredPosition = new Vector2(0, - (1 - progress) * 5);
        }).SetEase(EasingFunction.Ease.EaseOutCubic).SetTime(0.2f);

        m_IndicatorRootTwner.OnTerminateCallback += () =>
        {
            m_IndicatorRootTwner = null;
        };
    }

    private void handleOnLowToHigh()
    {
        if (m_IndicatorRootTwner != null)
        {
            m_IndicatorRootTwner.Abort();
            m_IndicatorRootTwner = null;
        }

        m_IndicatorRootTwner = TweenManager.Instance.Tween((float progress) =>
        {
            m_IndicatorRoot.anchoredPosition = new Vector2(0, -progress * 5);
        }).SetEase(EasingFunction.Ease.EaseOutCubic).SetTime(0.2f);

        m_IndicatorRootTwner.OnTerminateCallback += () =>
        {
            m_IndicatorRootTwner = null;
        };
    }

    private void handleOnBinaryStateChanged(BinarySignalReader.BinaryState state)
    {
        if (state == BinarySignalReader.BinaryState.Low)
        {
            m_ToLowOriginalColor = m_IndicatorImg.color;
            m_LerpToLowTimer = 0.0f;
        }
    }

    private void handleOnBeginSignalSequence()
    {
        m_SignalSequenceAnchor = Instantiate(m_SignalSequenceAnchorPrefab, m_SignalSequenceRoot);
    }

    private void handleOnEndSignalSequence(List<BinarySignalReader.SignalType> sequence)
    {
        var oldAnchor = m_SignalSequenceAnchor;
        var canvasGroup = oldAnchor.GetComponent<CanvasGroup>();
        var originPosY = oldAnchor.anchoredPosition.y;
        var originPosX = oldAnchor.anchoredPosition.x;
        TweenManager.Instance.Tween((float progress) =>
        {
            //oldAnchor.localScale = Vector3.one * EasingFunction.EaseOutCubic(1, 1.2f, progress);
            //oldAnchor.anchoredPosition = new Vector2(originPosX, EasingFunction.EaseOutCubic(originPosY, originPosY + 15, progress));
            //canvasGroup.alpha = (progress > 0.2f)?  (1 - (progress - 0.2f) / 0.8f) : 1.0f;
            canvasGroup.alpha = 1 - progress;
        }).SetEase(EasingFunction.Ease.Linear).SetTime(0.4f).SetEndCallback(() =>
        {
            Destroy(oldAnchor.gameObject);
        });

        m_SignalSequenceAnchor = null;
        m_BarSequenceTargetPosX = 0;
        m_TotalBarWidth = 0;
        m_SignalBars.Clear();
    }

    private void handleOnToLongSignalProgressChanged(float prev, float progress)
    {
        var rectTrans = m_IndicatorImg.transform as RectTransform;
        var size = rectTrans.sizeDelta;
        var scale = Mathf.Lerp(m_MinWidth, m_MaxWidth, progress);
        size.x = scale;
        size.y = scale;
        rectTrans.sizeDelta = size;

        (m_IndicatorShadowCanvasGroup.transform as RectTransform).sizeDelta = size;

        if (progress < 1)
        {
            //m_IndicatorImg.color = m_ShortColor;
            m_IndicatorImg.color = Color.Lerp(m_ShortColor, m_LongColor, progress);
        }
    }

    private void handleOnBecomeLongSignal()
    {
        // play frame effect
        if (m_IndicatorShadowTwner != null)
        {
            m_IndicatorShadowTwner.Abort();
            m_IndicatorShadowTwner = null;
        }

        m_IndicatorShadowTwner = TweenManager.Instance.Tween((float progress) =>
        {
            m_IndicatorShadowCanvasGroup.alpha = 1 - progress;
            m_IndicatorShadowCanvasGroup.transform.localScale = Vector3.one * EasingFunction.EaseOutSine(1.1f, 1.8f, progress);
        }).SetEase(EasingFunction.Ease.Linear).SetTime(0.35f);

        m_IndicatorShadowTwner.OnTerminateCallback += () =>
        {
            m_IndicatorShadowTwner = null;
        };
    }

    private void handleOnPushNewSignal(BinarySignalReader.SignalType signalType)
    {
        // press frame


        // bar anim
        var rectTransform = (signalType == BinarySignalReader.SignalType.Short)? 
            Instantiate(m_ShortSignalPrefab, m_SignalSequenceAnchor) as RectTransform :
            Instantiate(m_LongSignalPrefab, m_SignalSequenceAnchor) as RectTransform;

        var image = rectTransform.GetComponent<Image>();

        float width = (m_SignalBars.Count > 0) ? 15 : 0;    // bar offset
        width += rectTransform.sizeDelta.x;                 // bar width
        m_TotalBarWidth += width;
        rectTransform.anchoredPosition = new Vector2(m_TotalBarWidth - rectTransform.sizeDelta.x / 2, 0);

        m_SignalBars.Add(rectTransform);

        /*
        if (m_SignalBarTwner != null)
        {
            m_SignalBarTwner.Abort();
            m_SignalBarTwner = null;
        }
        */
        switch (m_Anchor)
        {
            case SignalSequenceUIAnchor.Center:
                m_BarSequenceTargetPosX = -m_TotalBarWidth / 2;
                break;
            case SignalSequenceUIAnchor.Right:
                m_BarSequenceTargetPosX = -m_TotalBarWidth;
                break;
        }

        m_SignalBarTwner = TweenManager.Instance.Tween((float progress) =>
        {
            rectTransform.localScale = Vector3.one * EasingFunction.EaseOutExpo(2.0f, 1.0f, progress);

            var col = image.color;
            col.a = progress;
            image.color = col;
        }).SetEase(EasingFunction.Ease.Linear).SetTime(0.4f);

        /*
        m_SignalBarTwner.OnTerminateCallback += () =>
        {
            rectTransform.localScale = Vector3.one;

            var col = image.color;
            col.a = 1;
            image.color = col;

            //m_SignalBarTwner = null;
        };
        */
    }

}
