using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Characters
{
public class CharacterAnimationMonitor : Singleton<CharacterAnimationMonitor>
{
    public enum CharacterAnimationState
    {
        Idle,
        Run,
        Hit,
        Staggered,
        Dead
    }

    // animator: 재생할 캐릭터의 Animator
    // characterData: 클립을 가져올 캐릭터 데이터
    // state: 재생하고자 하는 상태
    public void PlayAnimation(CharacterHandler characterHandler, CharacterState state)
    {
        CharacterAnimationState characterAnimationState = CharacterAnimationState.Idle;

        if(state == CharacterState.Normal) characterAnimationState = CharacterAnimationState.Idle;
        else if(state == CharacterState.Staggered) characterAnimationState = CharacterAnimationState.Staggered;
        else if(state == CharacterState.Dead) characterAnimationState = CharacterAnimationState.Dead;

        PlayAnimation(characterHandler, characterAnimationState);
    }
    
    public void PlayAnimation(CharacterHandler characterHandler, CharacterAnimationState state)
    {
        if (characterHandler.animator == null || characterHandler == null) return;

        AnimationClip clip = GetClip(characterHandler.GetCharacterStatus().CharacterData, state);
        if (clip == null)
        {
            Debug.Log(characterHandler.name + "의 " + state + " 애니메이션이 설정되지 않음");
            return;
        }

        AnimatorOverrideController overrideController = GetOrCreateOverrideController(characterHandler.animator);

        overrideController[state.ToString()] = clip;
        characterHandler.animator.CrossFade(state.ToString(), 0.1f);
    }

    private AnimatorOverrideController GetOrCreateOverrideController(Animator animator)
    {
        if (animator.runtimeAnimatorController is AnimatorOverrideController existing)
            return existing;

        // 최초 1회 Override Controller로 교체
        AnimatorOverrideController newOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = newOverride;

        return newOverride;
    }

    private AnimationClip GetClip(CharacterData characterData, CharacterAnimationState state)
    {
        return state switch
        {
            CharacterAnimationState.Idle      => characterData.idleAnimation,
            CharacterAnimationState.Run       => characterData.runAnimation,
            CharacterAnimationState.Hit       => characterData.hitAnimation,
            CharacterAnimationState.Staggered => characterData.staggeredAnimation,
            CharacterAnimationState.Dead      => characterData.deadAnimation,
            _ => null
        };
    }
}
}