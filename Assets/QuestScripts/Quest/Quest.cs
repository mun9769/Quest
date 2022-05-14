using System.Collections;
using System.Collections.Generic;
using System.Diagnostics; // 
using UnityEngine;
using System.Linq;

using Debug = UnityEngine.Debug;

public enum QuestState
{
    Inactive,
    Running,
    Complete,
    Cancel,
    WaitingForCompletion,
}

[CreateAssetMenu(menuName ="Quest/Quest", fileName = "Quest_")]
public class Quest : ScriptableObject
{
    #region Events
    // 1 보고 받았을 때 실행할 이벤트
    // 2 quest 완료할때 실행할 이벤트
    // 3 quest를 취소할 때 실행할 이벤트
    // 4 새로운 TaskGroup이 시작될때 실행할 이벤트
    
    public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    public delegate void CompletedHandler(Quest quest);
    public delegate void CancelHandler(Quest quest);
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);
    #endregion
    [SerializeField]
    private Category category;
    [SerializeField]
    private Sprite icon;

    [Header("Text")]
    [SerializeField]
    private string codeName;
    [SerializeField]
    private string displayName;
    [SerializeField, TextArea] // textArea?
    private string description;

    [Header("Task")]
    [SerializeField]
    private TaskGroup[] taskGroups;

    [Header("Reward")]
    [SerializeField]
    private Reward[] rewards;

    [Header("Option")]
    [SerializeField]
    private bool useAutoComplete;
    [SerializeField]
    private bool isCancelable;
    [SerializeField]
    private bool isSaveable;

    [Header("Condition")]
    [SerializeField]
    private Condition[] acceptionConditions;
    [SerializeField]
    private Condition[] cancelConditions;

    private int currentTaskGroupIndex;

    public Category Category => category;
    public Sprite Icon => icon;
    public string CodeName => codeName;
    public string DisplayName => displayName;
    public string Description => description;
    public QuestState State { get; private set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
    public IReadOnlyList<Reward> Rewards => rewards;
    public bool IsRegistered => State != QuestState.Inactive;
    public bool IsCompletable => State == QuestState.WaitingForCompletion;
    public bool IsComplete => State == QuestState.Complete; 
    public bool IsCancel => State == QuestState.Cancel;
    public virtual bool IsCancelable => isCancelable && cancelConditions.All(x => x.IsPass(this));
    public bool IsAcceptable => acceptionConditions.All(x => x.IsPass(this));
    public virtual bool IsSavable => isSaveable;

    public event TaskSuccessChangedHandler onTaskSuccessChanged;
    public event CompletedHandler onCompleted;
    public event CancelHandler onCanceled;
    public event NewTaskGroupHandler onNewTaskGroup;

    public void OnRegister() // awake 역할 // quest가 등록됨
    {
        Debug.Assert(!IsRegistered, "This quest has already been registerd.");
        // Assert함수는 false면 문장을 띄어준다.

        foreach(var taskGroup in taskGroups)
        {
            taskGroup.Setup(this); // 굳이 왜 setup을 해? Quest_.asset을 보면 드래그앤드롭으로 Task와 TaskGroup을 만들어 줬잖아.
            // 나중에 task만 보고 그 quest를 찾기 위해 미리 등록해준건가봐
            foreach (var task in taskGroup.Tasks)
                task.onSuccessChanged += OnSuccessChanged;
        }

        State = QuestState.Running;
        CurrentTaskGroup.Start();
    }
    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(!IsRegistered, "This quest has already been registerd.");
        Debug.Assert(!IsCancel, "This quest has been canceled");

        if (IsComplete)
            return;

        CurrentTaskGroup.ReceiveReport(category, target, successCount);

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WaitingForCompletion;
                if (useAutoComplete)
                    Complete();
            }
            else
            {
                var prevTaskGroup = taskGroups[currentTaskGroupIndex++];
                prevTaskGroup.End();
                CurrentTaskGroup.Start();
                onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);

            }
        }
        else
            State = QuestState.Running;

    }
    public void Complete()
    {
        CheckIsRunning();
        foreach (var taskGroup in taskGroups)
            taskGroup.Complete();
        State = QuestState.Complete;

        onCompleted?.Invoke(this);

        foreach (var reward in rewards)
            reward.Give(this);

        onTaskSuccessChanged = null;
        onCompleted = null;
        onCanceled = null;
        onNewTaskGroup = null;
    }
    public virtual void Cancel()
    {
        CheckIsRunning();
        Debug.Assert(IsCancelable, "This quest can't be canceled");

        State = QuestState.Cancel;
        onCanceled?.Invoke(this);
    }

    public Quest Clone()
    {
        var clone = Instantiate(this); // 왜 복사한다 했지?
        clone.taskGroups = taskGroups.Select(x => new TaskGroup(x)).ToArray();
        return clone;
    }

    public QuestSaveData ToSaveData()
    {
        return new QuestSaveData
        {
            codeName = codeName,
            state = State,
            taskGroupIndex = currentTaskGroupIndex,
            taskSuccessCounts = CurrentTaskGroup.Tasks.Select(x => x.CurrentSuccess).ToArray(),
        };
    }

    public void LoadFrom(QuestSaveData saveData)
    {
        State = saveData.state;
        currentTaskGroupIndex = saveData.taskGroupIndex;

        for(int i=0; i<currentTaskGroupIndex; i++)
        {
            var taskGroup = taskGroups[i];
            taskGroup.Start();
            taskGroup.Complete();
        }
        for(int i=0; i<saveData.taskSuccessCounts.Length; i++)
        {
            CurrentTaskGroup.Start();
            CurrentTaskGroup.Tasks[i].CurrentSuccess = saveData.taskSuccessCounts[i];
        }
    }

    private void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
        => onTaskSuccessChanged?.Invoke(this, task, currentSuccess, prevSuccess);

#if UNITY_EDITER
#endif
    //conditional ATTIRIBUTE는 UNITY_EDITER 심볼값이 선언되어 있으면 함수를 실행하고
    // 안선언되었다면 함수를 무시한다.
    [Conditional("UNITY_EDITER")] 
    private void CheckIsRunning()
    {
        Debug.Assert(!IsRegistered, "This quest has already been registerd.");
        Debug.Assert(!IsCancel, "This quest has been canceled");
        Debug.Assert(!IsComplete, "This quest has already been completed");
    }

}
