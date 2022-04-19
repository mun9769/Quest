using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Quest/Task/Action/NegativeCount",fileName ="Negative Count")]
public class NegativeCount : TaskAction
{
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        if (successCount < 0)
            return currentSuccess + successCount;
        else
            return currentSuccess;
    }
}
