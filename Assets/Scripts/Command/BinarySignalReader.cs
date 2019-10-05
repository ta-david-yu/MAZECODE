using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BinarySignalReader : MonoBehaviour
{
    public enum BinaryState
    {
        Low,
        High
    }

    public enum SignalType
    {
        Short,
        Long
    }

    [SerializeField]
    private float m_MinimumTimeForLongSignal = 0.8f;
    public float MinimumTimeForLongSignal { get => m_MinimumTimeForLongSignal; private set => m_MinimumTimeForLongSignal = value; }

    [SerializeField]
    private float m_MinimumTimeForTerminateSignal = 1.8f;
    public float MinimumTimeForTerminateSignal { get => m_MinimumTimeForTerminateSignal; private set => m_MinimumTimeForTerminateSignal = value; }

    private BinaryState m_State = BinaryState.Low;
    public BinaryState State
    {
        get => m_State;
        private set
        {
            var prev = m_State;
            m_State = value;
            if (prev != value)
            {
                OnBinaryStateChanged.Invoke(value);
            }
        }
    }

    private List<SignalType> m_SignalSequence = new List<SignalType>();
    public ReadOnlyCollection<SignalType> SignalSequence { get => m_SignalSequence.AsReadOnly(); }

    private bool m_IsRecording = true;
    public bool IsRecording { get => m_IsRecording; private set => m_IsRecording = value; }

    private bool m_IsReadingSequence = false;
    public bool IsReadingSequence
    { 
        get => m_IsReadingSequence;
        private set => m_IsReadingSequence = value;
    }

    private float m_SequenceReaderCounter = 0;
    public float SequenceReaderCounter 
    { 
        get => m_SequenceReaderCounter;
        private set
        {
            var prev = m_SequenceReaderCounter;
            m_SequenceReaderCounter = value;

            if (State == BinaryState.High)
            {
                var prevProgress = prev / m_MinimumTimeForLongSignal;
                var progress = m_SequenceReaderCounter / m_MinimumTimeForLongSignal;
                OnToLongSignalProgressChanged.Invoke(prevProgress, progress);

                if (prevProgress < 1 && progress >= 1)
                {
                    OnBecomeLongSignal.Invoke();
                }
            }
        }
    }

    public event Action<BinaryState> OnBinaryStateChanged = delegate { };
    public event Action<SignalType> OnHighToLow = delegate { };
    public event Action OnLowToHigh = delegate { };
    public event Action<float, float> OnToLongSignalProgressChanged = delegate { };
    public event Action OnBecomeLongSignal = delegate { };

    public event Action<float, float> OnSequenceReaderCounterChanged = delegate { };

    public event Action<SignalType> OnPushNewSignal = delegate { };

    public event Action OnBeginSignalSequence = delegate { };
    public event Action<List<SignalType>> OnEndSignalSequence = delegate { };

    private void Update()
    {
        if (IsReadingSequence)
        {
            if (State == BinaryState.High)
            {
                SequenceReaderCounter += Time.deltaTime;
            }
            else if (State == BinaryState.Low)
            {
                SequenceReaderCounter += Time.deltaTime;

                if (SequenceReaderCounter > MinimumTimeForTerminateSignal)
                {
                    /*
                    System.Text.StringBuilder builder = new System.Text.StringBuilder("SEQ: ");
                    foreach (var type in SignalSequence)
                    {
                        builder.Append(type == SignalType.Short ? "0" : "1");
                    }
                    Debug.Log(builder);
                    */

                    IsReadingSequence = false;
                    SequenceReaderCounter = 0;

                    OnEndSignalSequence.Invoke(new List<SignalType>(m_SignalSequence));
                    m_SignalSequence.Clear();
                }
            }
        }
    }

    public void StartRecording()
    {
        IsRecording = true;
    }

    public void StopRecording()
    {
        IsRecording = false;

        if (State == BinaryState.High)
        {
            ToLowState();
        }
    }

    public void ToHighState()
    {
        if (!IsReadingSequence)
        {
            IsReadingSequence = true;
            OnBeginSignalSequence.Invoke();
        }

        var prevSignal = State;
        State = BinaryState.High;

        if (State != prevSignal)
        {
            OnLowToHigh.Invoke();
        }

        SequenceReaderCounter = 0;
    }

    public void ToLowState()
    {
        var prevSignal = State;
        State = BinaryState.Low;

        // high to low
        if (State != prevSignal)
        {
            var signalType = SignalType.Short;
            if (SequenceReaderCounter > MinimumTimeForLongSignal)
            {
                signalType = SignalType.Long;
            }
            m_SignalSequence.Add(signalType);

            OnHighToLow.Invoke(signalType);
            OnPushNewSignal.Invoke(signalType);
        }

        SequenceReaderCounter = 0;
    }
}
