using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum TaskState
{
    Inactive,
    Running,
    Complete,
}


[CreateAssetMenu(menuName = "Quest/Task/Task", fileName = "Task_")]
public class Task : ScriptableObject
{
    // update문으로 상태를 계속 확인할 필요없이
    // event를 등록하고, 특정행위에 도달하면 TaskState를 갱신할 수 있다.
    #region Events
    public delegate void StateChangedHandler(Task task, TaskState currentState, TaskState prevState); //prevState를 간혹쓴단다
    public delegate void SuccessChangedHandler(Task task, int currentSuccess, int prevSuccess);
    #endregion


    [SerializeField] private            Category category;

    [Header("Text")]
    [SerializeField] private            string codeName;
    [SerializeField] private            string description;

    [Header("Action")]
    [SerializeField] private            TaskAction action;

    [Header("Target")]
    [SerializeField] private            TaskTarget[] targets;

    [Header("Setting")]
    [SerializeField] private            InitialSuccessValue initialSuccessValue;
    [SerializeField] private            int needSuccessToComplete;
    [SerializeField] private            bool canReceiveReportsDuringCompletion;
    // task가 완료되어도 계속 성공횟수를 보고받을것인가?

    private TaskState state;
    private int currentSuccess;

    public event StateChangedHandler onStateChanged;
    public event SuccessChangedHandler onSuccessChanged;

    public int CurrentSuccess
    {
        get => currentSuccess;
        set
        {
            int prevSuccess = currentSuccess;
            currentSuccess = Mathf.Clamp(value, 0, needSuccessToComplete);
            if(currentSuccess != prevSuccess)
            {
                State = currentSuccess == needSuccessToComplete ? TaskState.Complete : TaskState.Running;
                onSuccessChanged?.Invoke(this, currentSuccess, prevSuccess);
            }
        }
    }
    public Category Category => category;
    public string CodeName => codeName;
    public string Description => description;
    public int NeedSuccessToComplete => needSuccessToComplete;
    public TaskState State
    {
        get => state;
        set 
        {
            var prevState = state;
            state = value;
            onStateChanged?.Invoke(this, state, prevState);
        }
    }

    public bool IsComplete => State == TaskState.Complete;
    public Quest Owner { get; private set; }

    public void Setup(Quest owner)
    {
        Owner = owner;
    }
    public void Start()
    {
        State = TaskState.Running;
        if (initialSuccessValue)
            CurrentSuccess = initialSuccessValue.GetValue(this);
    }
    public void End()
    {
        onStateChanged = null;
        onSuccessChanged = null;
    }


    public void ReceiveReport(int successCount)
    {
        CurrentSuccess = action.Run(this, CurrentSuccess, successCount);
    }

    public void Complete()
    {
        CurrentSuccess = NeedSuccessToComplete;
    }

    public bool IsTarget(string category, object target)
        => Category == category && 
        targets.Any(x => x.IsEqual(target)) &&
        (!IsComplete || (IsComplete && canReceiveReportsDuringCompletion));

}
