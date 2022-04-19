using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/GameObject", fileName = "Target_")]
public class GameObjectTarget : TaskTarget
{
    [SerializeField]
    private GameObject value;

    public override object Value => value;
    public override bool IsEqual(object target) 
        // object target�� prefab�ϼ��ְ�, hierarchy�� Object�� �� �ִ�.
        // target�� prefab���� ���´ٸ�
        // return���� false�� ��ȯ�Ҽ� �ֱ⶧����
        // �̸��� ���Ͽ� ������ Ȯ���Ѵ�.
    {
        var targetAsGameObject = target as GameObject;
        if (targetAsGameObject == null)
            return false;
        
        return targetAsGameObject.name.Contains(value.name);
        // prefab�� hierarchy�� ������ ������ �̸��ڿ� "(n)"�� �ٱ� ������ Contains�Լ��� ����Ѵ�.

    }
}
