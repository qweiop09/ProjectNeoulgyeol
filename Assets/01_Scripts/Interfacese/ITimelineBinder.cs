// 인터페이스

using UnityEngine;
using UnityEngine.Playables;

public abstract class ITimelineBinder : ScriptableObject
{
    public abstract void Bind(PlayableDirector director, SkillActData data);
}