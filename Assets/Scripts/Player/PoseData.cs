using System;
using System.Collections.Generic;
using UnityEngine;

namespace Surexs.DanceOff.Player
{
    [Serializable]
    public sealed class PoseData
    {
        public PoseData(string id, string displayName, Color bodyColor, float leftArmAngle, float rightArmAngle)
        {
            this.id = id;
            this.displayName = displayName;
            this.bodyColor = bodyColor;
            this.leftArmAngle = leftArmAngle;
            this.rightArmAngle = rightArmAngle;
        }

        public string id;
        public string displayName;
        public Color bodyColor = Color.white;
        [Range(-180f, 180f)] public float leftArmAngle;
        [Range(-180f, 180f)] public float rightArmAngle;
    }

    public static class PoseResolver
    {
        public static bool TryResolve(IReadOnlyList<PoseData> poses, string poseId, out PoseData pose)
        {
            pose = null;
            if (poses == null || string.IsNullOrWhiteSpace(poseId))
            {
                return false;
            }

            for (var index = 0; index < poses.Count; index++)
            {
                var candidate = poses[index];
                if (candidate != null && string.Equals(candidate.id, poseId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    pose = candidate;
                    return true;
                }
            }

            return false;
        }

        public static PoseData ResolveOrFallback(IReadOnlyList<PoseData> poses, string poseId, PoseData fallback,
            out bool usedFallback)
        {
            if (TryResolve(poses, poseId, out var pose))
            {
                usedFallback = false;
                return pose;
            }

            usedFallback = true;
            return fallback;
        }
    }
}
