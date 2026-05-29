using UnityEngine;

public abstract class BaseState<T> where T : BaseManagerState<T>
{
    public abstract void EnterState(T bms);
    public abstract void UpdateState(T bms);
    public abstract void ExitState(T bms);
}



