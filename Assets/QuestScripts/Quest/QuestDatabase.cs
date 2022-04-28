using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName ="Quest/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField]
    private List<Quest> quests;
    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string codeName) =>quests.FirstOrDefault(x => x.CodeName == codeName);

    [ContextMenu("FindQuests")]
    private void FindQuests()
    {
        FindQuestsBy<Quest>();
    }
    [ContextMenu("FindAchievements")]
    private void FindAchievements()
    {
        FindQuestsBy<Achievement>();
    }
    private void FindQuestsBy<T>() where T : Quest
    {
        quests = new List<Quest>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
        // 중요!!
        // FindAssets함수는 Asset 폴더에서 filter에 맞는 asset의 GUID를 가져오는 함수이다.

        foreach(var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<Quest>(assetPath);

            if (quest.GetType() == typeof(T)) // Achievement 클래스가 Quest를 상속받기 때문에 타입확인을 해준다.
                quests.Add(quest);

            EditorUtility.SetDirty(this);
            // SetDirty는 QuestDatabase 객체가 가진 Serialize 변수의 변화가 생겼으므로
            //Asset을 저장할때 반영하는 함수이다.

            AssetDatabase.SaveAssets();
            // 저장하지 않으면 List에 quest를 채워넣어도 유니티editor를 껏다키면 사라진다.
        }
    }

}
