using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BinarySignalReader;

public class GameFlow : MonoBehaviour
{
    private const string c_CompleteCmd = "Complete";

    private const string c_ProceedCmd = "Proceed";

    private const string c_ConfirmCmd = "Confirm";

    private const string c_CancelCmd = "Cancel";

    private static readonly SignalType[] c_TapSequence = new SignalType[] 
    { SignalType.Short };

    private static readonly SignalType[] c_TapThreeSequence = new SignalType[]
    { SignalType.Short, SignalType.Short, SignalType.Short };

    private static readonly SignalType[] c_HoldSequence = new SignalType[]
    { SignalType.Long };

    private static readonly SignalType[] c_HoldThreeSequence = new SignalType[]
    { SignalType.Long, SignalType.Long, SignalType.Long };

    private static readonly SignalType[] c_TapHoldHoldTapHoldSequence = new SignalType[]
    { SignalType.Short, SignalType.Long, SignalType.Long, SignalType.Short, SignalType.Long };

    enum GameState
    {
        Initial,

        Tap,
        TapThree,
        Hold,
        HoldThree,

        TapHoldHoldTapHold,

        DefineWalk,
        DefineLeft,
        DefineUp,
        DefineRight,
        DefineDown,

        ConfirmWalk,
        ConfirmLeft,
        ConfirmUp,
        ConfirmRight,
        ConfirmDown,

        Free
    }

    [Header("Reference")]

    [SerializeField]
    private BinarySignalReader m_CmdReader;

    [SerializeField]
    private CommandController m_CommandController;

    [Header("UI Reference")]
    [SerializeField]
    private TMPro.TextMeshProUGUI m_HintText;

    [SerializeField]
    private RectTransform m_MainUI;

    [SerializeField]
    private RectTransform m_CommandUI;

    [SerializeField]
    private RectTransform m_SequenceUIRoot;

    private DUCK.FSM.FiniteStateMachine<GameState> m_StateMachine;
    private CommandController.Command m_CurrentDefineCommand = CommandController.Command.NumOfCmd;

    private List<SignalType> m_TargetSequence = new List<BinarySignalReader.SignalType> { };

    private List<SignalType> m_CacheSequence;

    private void OnEnable()
    {
        m_CmdReader.OnEndSignalSequence += handleOnEndSignalSequence;
    }

    private void OnDisable()
    {
        m_CmdReader.OnEndSignalSequence -= handleOnEndSignalSequence;
    }

    private void Awake()
    {
        TweenManager.Instance.Init();

        m_StateMachine = DUCK.FSM.FiniteStateMachine<GameState>.FromEnum();

        m_StateMachine.AddTransition(GameState.Initial, GameState.Tap, c_CompleteCmd)
                      .AddTransition(GameState.Tap, GameState.TapThree, c_CompleteCmd)
                      .AddTransition(GameState.TapThree, GameState.Hold, c_CompleteCmd)
                      .AddTransition(GameState.Hold, GameState.HoldThree, c_CompleteCmd)
                      .AddTransition(GameState.HoldThree, GameState.TapHoldHoldTapHold, c_CompleteCmd)
                      .AddTransition(GameState.TapHoldHoldTapHold, GameState.DefineWalk, c_CompleteCmd)

                      .AddTransition(GameState.DefineWalk, GameState.ConfirmWalk, c_ProceedCmd)
                      .AddTransition(GameState.ConfirmWalk, GameState.DefineWalk, c_CancelCmd)
                      .AddTransition(GameState.ConfirmWalk, GameState.DefineLeft, c_ConfirmCmd)

                      .AddTransition(GameState.DefineLeft, GameState.ConfirmLeft, c_ProceedCmd)
                      .AddTransition(GameState.ConfirmLeft, GameState.DefineLeft, c_CancelCmd)
                      .AddTransition(GameState.ConfirmLeft, GameState.DefineUp, c_ConfirmCmd)
        
                      .AddTransition(GameState.DefineUp, GameState.ConfirmUp, c_ProceedCmd)
                      .AddTransition(GameState.ConfirmUp, GameState.DefineUp, c_CancelCmd)
                      .AddTransition(GameState.ConfirmUp, GameState.DefineRight, c_ConfirmCmd)
        
                      .AddTransition(GameState.DefineRight, GameState.ConfirmRight, c_ProceedCmd)
                      .AddTransition(GameState.ConfirmRight, GameState.DefineRight, c_CancelCmd)
                      .AddTransition(GameState.ConfirmRight, GameState.DefineDown, c_ConfirmCmd)
        
                      .AddTransition(GameState.DefineDown, GameState.ConfirmDown, c_ProceedCmd)
                      .AddTransition(GameState.ConfirmDown, GameState.DefineDown, c_CancelCmd)
                      .AddTransition(GameState.ConfirmDown, GameState.Free, c_ConfirmCmd);

        m_StateMachine.OnEnter(GameState.Tap, () =>
        {
            m_TargetSequence = new List<SignalType>(c_TapSequence);
            changeHintText("TAP");
        });

        m_StateMachine.OnEnter(GameState.TapThree, () =>
        {
            m_TargetSequence = new List<SignalType>(c_TapThreeSequence);
            changeHintText("TAP, TAP, TAP");
        });

        m_StateMachine.OnEnter(GameState.Hold, () =>
        {
            m_TargetSequence = new List<SignalType>(c_HoldSequence);
            changeHintText("HOLD");
        });

        m_StateMachine.OnEnter(GameState.HoldThree, () =>
        {
            m_TargetSequence = new List<SignalType>(c_HoldThreeSequence);
            changeHintText("HOLD, HOLD, HOLD");
        });

        m_StateMachine.OnEnter(GameState.TapHoldHoldTapHold, () =>
        {
            m_TargetSequence = new List<SignalType>(c_TapHoldHoldTapHoldSequence);
            changeHintText("TAP, HOLD, HOLD, TAP, HOLD");
        });

        m_StateMachine.OnChange(GameState.TapHoldHoldTapHold, GameState.DefineWalk, () =>
        {
            m_CmdReader.StopRecording();

            m_TargetSequence = new List<SignalType>();
            TweenManager.Instance.Tween((float progress) =>
            {
                // hide text
                var col = m_HintText.color;
                col.a = 1 - progress;
                m_HintText.color = col;
            }, delay: 0.15f).SetTime(0.25f).SetEndCallback(
                () =>
                {
                    var sequenceUIOriginal = m_SequenceUIRoot.anchoredPosition;
                    TweenManager.Instance.Tween((float progress) =>
                    {
                        // move ui, shrink ui
                        m_MainUI.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, new Vector2(0, -420), EasingFunction.EaseOutExpo(0, 1, progress));
                        m_CommandUI.localScale = Vector2.one * EasingFunction.EaseOutExpo(1.5f, 1.0f, progress);
                    }, delay: 0.1f).SetEase(EasingFunction.Ease.Linear).SetTime(2f).SetEndCallback(
                        () =>
                        {
                            // show player
                            m_CommandController.transform.localScale = Vector3.zero;
                            m_CommandController.gameObject.SetActive(true);
                            TweenManager.Instance.Tween((float progress) =>
                            {
                                m_CommandController.transform.localScale = Vector3.one * progress;
                            }, delay: 0.1f).SetTime(0.8f).SetEase(EasingFunction.Ease.EaseOutBack).SetEndCallback(
                                    () =>
                                    {
                                        // show next mission
                                        m_CmdReader.StartRecording();
                                        changeHintText("DEFINE [WALK/STOP]");
                                    }
                                );
                        });
                });
        });

        m_StateMachine.OnChange(GameState.ConfirmWalk, GameState.DefineWalk, () =>
        {
            changeHintText("DEFINE [WALK/STOP]");
        });

        m_StateMachine.OnEnter(GameState.ConfirmWalk, () =>
        {
            changeHintText("TAP TO [CONFIRM], HOLD TO [CANCEL]");
        });

        m_StateMachine.OnEnter(GameState.DefineLeft, () =>
        {
            changeHintText("DEFINE [TURN LEFT]");
        });

        m_StateMachine.OnEnter(GameState.ConfirmLeft, () =>
        {
            changeHintText("TAP TO [CONFIRM], HOLD TO [CANCEL]");
        });

        m_StateMachine.OnEnter(GameState.DefineUp, () =>
        {
            changeHintText("DEFINE [TURN UP]");
        });

        m_StateMachine.OnEnter(GameState.ConfirmUp, () =>
        {
            changeHintText("TAP TO [CONFIRM], HOLD TO [CANCEL]");
        });

        m_StateMachine.OnEnter(GameState.DefineRight, () =>
        {
            changeHintText("DEFINE [TURN RIGHT]");
        });

        m_StateMachine.OnEnter(GameState.ConfirmRight, () =>
        {
            changeHintText("TAP TO [CONFIRM], HOLD TO [CANCEL]");
        });

        m_StateMachine.OnEnter(GameState.DefineDown, () =>
        {
            changeHintText("DEFINE [TURN DOWN]");
        });

        m_StateMachine.OnEnter(GameState.ConfirmDown, () =>
        {
            changeHintText("TAP TO [CONFIRM], HOLD TO [CANCEL]");
        });
    }

    private void Start()
    {
        start();
    }

    // Update is called once per frame
    void Update()
    {
        TweenManager.Instance.Update(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private void start()
    {
        m_StateMachine.IssueCommand(c_CompleteCmd);
    }

    private void handleOnEndSignalSequence(List<SignalType> sequence)
    {
        if (m_StateMachine.CurrentState >= GameState.DefineWalk)
        {
            if (m_StateMachine.CurrentState >= GameState.DefineWalk &&
                m_StateMachine.CurrentState <= GameState.DefineDown)
            {
                var cmd = m_CommandController.SeqTree.GetCommand(sequence);

                if (cmd == CommandController.Command.Empty)
                {
                    m_CacheSequence = sequence;
                    m_StateMachine.IssueCommand(c_ProceedCmd);
                }
                else
                {
                    // re-display hint text
                    changeHintText(m_HintText.text);
                }
            }
            else if (m_StateMachine.CurrentState >= GameState.ConfirmWalk &&
                m_StateMachine.CurrentState <= GameState.ConfirmDown)
            {
                if (sequence.Count == 1)
                {
                    if (sequence[0] == SignalType.Short)
                    {
                        int index = m_StateMachine.CurrentState - GameState.ConfirmWalk;

                        m_StateMachine.IssueCommand(c_ConfirmCmd);

                        m_CommandController.SeqTree.PushNewCommand(m_CacheSequence, (CommandController.Command)index);
                    }
                    else if (sequence[0] == SignalType.Long)
                    {
                        m_StateMachine.IssueCommand(c_CancelCmd);
                    }
                }
            }
        }
        else
        {
            if (matchSequences(sequence, m_TargetSequence))
            {
                m_StateMachine.IssueCommand(c_CompleteCmd);
            }
        }
    }

    private void changeHintText(string text)
    {
        m_HintText.text = text;

        TweenManager.Instance.Tween((float progress) =>
        {
            m_HintText.transform.localScale = Vector3.one * Mathf.LerpUnclamped(1.5f, 1.0f, EasingFunction.EaseOutSine(0, 1, progress));
            var col = m_HintText.color;
            col.a = progress;
            m_HintText.color = col;
        }).SetTime(0.45f).SetEase(EasingFunction.Ease.Linear);
    }

    private bool matchSequences(List<SignalType> a, List<SignalType> b)
    {
        if (a.Count == b.Count)
        {
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
