using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.Player
{
    public sealed class PoseController : MonoBehaviour
    {
        private const string NeutralPoseId = "neutral";

        private readonly HashSet<string> warnedMissingPoses = new HashSet<string>();
        private IReadOnlyList<PoseData> poses;
        private RectTransform body;
        private RectTransform leftArm;
        private RectTransform rightArm;
        private Image bodyImage;
        private Text poseLabel;
        private PoseData neutralPose;

        public string CurrentPoseId { get; private set; } = NeutralPoseId;

        public void Configure(IReadOnlyList<PoseData> configuredPoses, RectTransform bodyTransform,
            RectTransform leftArmTransform, RectTransform rightArmTransform, Image playerBodyImage, Text label)
        {
            poses = configuredPoses;
            body = bodyTransform;
            leftArm = leftArmTransform;
            rightArm = rightArmTransform;
            bodyImage = playerBodyImage;
            poseLabel = label;

            if (!PoseResolver.TryResolve(poses, NeutralPoseId, out neutralPose))
            {
                neutralPose = new PoseData(NeutralPoseId, "NEUTRAL", new Color(0.22f, 0.67f, 0.92f), 15f, -15f);
                Debug.LogWarning("[PoseController] No se configuró la pose 'neutral'; se utilizará un fallback interno.", this);
            }

            ShowNeutral();
        }

        public bool ShowPose(string poseId)
        {
            var pose = PoseResolver.ResolveOrFallback(poses, poseId, neutralPose, out var usedFallback);
            if (usedFallback)
            {
                WarnMissingPose(poseId);
                ApplyPose(neutralPose);
                return false;
            }

            ApplyPose(pose);
            return true;
        }

        public void ShowNeutral()
        {
            ApplyPose(neutralPose);
        }

        private void ApplyPose(PoseData pose)
        {
            CurrentPoseId = pose.id;
            bodyImage.color = pose.bodyColor;
            body.localScale = string.Equals(pose.id, NeutralPoseId, System.StringComparison.OrdinalIgnoreCase)
                ? Vector3.one
                : new Vector3(1.06f, 1.06f, 1f);
            leftArm.localRotation = Quaternion.Euler(0f, 0f, pose.leftArmAngle);
            rightArm.localRotation = Quaternion.Euler(0f, 0f, pose.rightArmAngle);
            poseLabel.text = $"POSE: {pose.displayName.ToUpperInvariant()}";
        }

        private void WarnMissingPose(string poseId)
        {
            var safeId = string.IsNullOrWhiteSpace(poseId) ? "<vacía>" : poseId;
            if (warnedMissingPoses.Add(safeId))
            {
                Debug.LogWarning($"[PoseController] La pose '{safeId}' no existe. Se utilizará NEUTRAL.", this);
            }
        }
    }
}
