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
        // object target이 prefab일수있고, hierarchy의 Object일 수 있다.
        // target이 prefab으로 들어온다면
        // return값이 false를 반환할수 있기때문에
        // 이름을 비교하여 같은지 확인한다.
    {
        var targetAsGameObject = target as GameObject;
        if (targetAsGameObject == null)
            return false;
        
        return targetAsGameObject.name.Contains(value.name);
        // prefab을 hierarchy에 여러번 넣으면 이름뒤에 "(n)"이 붙기 때문에 Contains함수를 사용한다.

    }
}
