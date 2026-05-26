// 인터페이스

using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Playables;

public abstract class ITimelineBinder : ScriptableObject
{
    public abstract void Bind(PlayableDirector director, ActData data);
}