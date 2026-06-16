using _01_Scripts.Runtime.Battles;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _01_Scripts.Timeline.Battle.QTE
{
[TrackColor(0.8f, 0.2f, 0.2f)]
[TrackClipType(typeof(QTEClip))]
[TrackBindingType(typeof(QTEListner))]
public class QTETrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<QTEMixerBehaviour>.Create(graph, inputCount);
    }
}
}