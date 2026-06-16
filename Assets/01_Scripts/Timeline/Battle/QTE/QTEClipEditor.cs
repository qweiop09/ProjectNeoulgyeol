#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace _01_Scripts.Timeline.Battle.QTE.Editor
{
    [CustomTimelineEditor(typeof(QTEClip))]
    public class QTEClipEditor : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip)
        {
            base.OnClipChanged(clip);

            var qteClip = clip.asset as QTEClip;
            if (qteClip == null) return;

            float targetDuration = qteClip.badTime * 2f;

            // badTime 기준으로 클립 길이를 강제 동기화
            if (!Mathf.Approximately((float)clip.duration, targetDuration) && targetDuration > 0f)
            {
                clip.duration = targetDuration;
                EditorUtility.SetDirty(clip.asset);
            }
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var qteClip = clip.asset as QTEClip;
            if (qteClip == null) return;

            float duration = (float)clip.duration;
            if (duration <= 0f) return;

            float center = 0.5f; // 클립 중앙 = 0.5 (비율 기준)

            float perfectRatio = qteClip.perfectTime / duration;
            float goodRatio = qteClip.goodTime / duration;
            float badRatio = qteClip.badTime / duration;

            float regionStart = (float)(region.startTime / clip.duration);
            float regionEnd = (float)(region.endTime / clip.duration);

            Color perfectColor = new Color(0.3f, 0.8f, 0.3f, 0.6f);
            Color goodColor = new Color(1f, 0.85f, 0.2f, 0.6f);
            Color badColor = new Color(0.8f, 0.3f, 0.3f, 0.6f);

            // 좌측: Bad - Good - Perfect (중심으로 갈수록 좁아짐)
            DrawSegment(region, regionStart, regionEnd, center - badRatio, center - goodRatio, badColor);
            DrawSegment(region, regionStart, regionEnd, center - goodRatio, center - perfectRatio, goodColor);
            DrawSegment(region, regionStart, regionEnd, center - perfectRatio, center + perfectRatio, perfectColor);

            // 우측: Perfect - Good - Bad (대칭)
            DrawSegment(region, regionStart, regionEnd, center + perfectRatio, center + goodRatio, goodColor);
            DrawSegment(region, regionStart, regionEnd, center + goodRatio, center + badRatio, badColor);
        }

        private void DrawSegment(ClipBackgroundRegion region, float regionStart, float regionEnd, float segStart, float segEnd, Color color)
        {
            float drawStart = Mathf.Max(regionStart, segStart);
            float drawEnd = Mathf.Min(regionEnd, segEnd);
            if (drawStart >= drawEnd) return;

            float regionWidth = region.position.width;
            float regionRange = regionEnd - regionStart;
            if (regionRange <= 0f) return;

            float x = region.position.x + (drawStart - regionStart) / regionRange * regionWidth;
            float width = (drawEnd - drawStart) / regionRange * regionWidth;

            var rect = new Rect(x, region.position.y, width, region.position.height);
            EditorGUI.DrawRect(rect, color);
        }
    }
}
#endif