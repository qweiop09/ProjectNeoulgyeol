using _01_Scripts.Runtime.Battles;
using UnityEngine;
using UnityEngine.Playables;

namespace _01_Scripts.Timeline.Battle.QTE
{
public class QTEMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {

        var listner = playerData as QTEListner;
        if (listner == null) return;

        int inputCount = playable.GetInputCount();
        bool anyActive = false;
        
        
        for (int i = 0; i < inputCount; i++)
        {
            if (playable.GetInputWeight(i) <= 0f) continue;

            var input = (ScriptPlayable<QTEBehaviour>)playable.GetInput(i);
            var behaviour = input.GetBehaviour();

            float localTime = (float)input.GetTime();
            float duration = (float)input.GetDuration();

            listner.UpdateQteState(localTime, duration, behaviour.perfectTime, behaviour.goodTime, behaviour.badTime,
                behaviour.hpDamageCoefficient, behaviour.staminaDamageCoefficient, behaviour.multipliers);
            anyActive = true;
        }

        if (!anyActive)
            listner.ClearQteState();
    }
}
}