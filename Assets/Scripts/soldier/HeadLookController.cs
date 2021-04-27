using UnityEngine;
using System.Collections;

namespace Bootcamp.Soldier
{
    [AddComponentMenu("Bootcamp/Soldier/Head Look Controller")]
    public class HeadLookController : MonoBehaviour
    {
        [System.Serializable]
        private struct BendingSegment
        {
            public Transform firstTransform;
            public Transform lastTransform;
            public float thresholdAngleDifference;
            public float bendingMultiplier;
            public float maxAngleDifference;
            public float maxBendingAngle;
            public float responsiveness;
            [HideInInspector] public float angleH;
            [HideInInspector] public float angleV;
            [HideInInspector] public Vector3 dirUp;
            [HideInInspector] public Vector3 referenceLookDir;
            [HideInInspector] public Vector3 referenceUpDir;
            [HideInInspector] public int chainLength;
            [HideInInspector] public Quaternion[] origRotations;
        }

        [System.Serializable]
        private struct NonAffectedJoints
        {
            public Transform joint;
            public float effect;
        }

        [SerializeField] Transform rootNode;
        [SerializeField] BendingSegment[] segments;
        [SerializeField] NonAffectedJoints[] nonAffectedJoints;
        [SerializeField] Vector3 headLookVector = Vector3.forward;
        [SerializeField] Vector3 headUpVector = Vector3.up;
        [SerializeField] Vector3 target = Vector3.zero;
        [SerializeField] Transform targetTransform;
        public float effect = 1.0f;
        [SerializeField] bool overrideAnimation = false;

        private void Start()
        {
            if (!rootNode)
                rootNode = transform;

            // Setup segments
            for (int i = 0; i < segments.Length; i++)
            {
                BendingSegment segment = segments[i];
                Quaternion parentRot = segment.firstTransform.parent.rotation;
                Quaternion parentRotInv = Quaternion.Inverse(parentRot);
                segment.referenceLookDir =
                  parentRotInv * rootNode.rotation * headLookVector.normalized;
                segment.referenceUpDir =
                    parentRotInv * rootNode.rotation * headUpVector.normalized;
                segment.angleH = 0.0f;
                segment.angleV = 0.0f;
                segment.dirUp = segment.referenceUpDir;

                segment.chainLength = 1;
                Transform t = segment.lastTransform;
                while (t != segment.firstTransform && t != t.root)
                {
                    segment.chainLength++;
                    t = t.parent;
                }

                segment.origRotations = new Quaternion[segment.chainLength];
                t = segment.lastTransform;
                for (var ii = segment.chainLength - 1; ii >= 0; ii--)
                {
                    segment.origRotations[ii] = t.localRotation;
                    t = t.parent;
                }

                segments[i] = segment;
            }
        }

        void LateUpdate()
        {
            if (Time.deltaTime == 0)
                return;

            target = targetTransform.position;

            // Remember initial directions of joints that should not be affected
            var jointDirections = new Vector3[nonAffectedJoints.Length];
            for (var i = 0; i < nonAffectedJoints.Length; i++)
            {
                foreach (Transform child in nonAffectedJoints[i].joint)
                {
                    jointDirections[i] = child.position - nonAffectedJoints[i].joint.position;
                    break;
                }
            }

            // Handle each segment
            for (int j = 0; j < segments.Length; j++)
            {
                BendingSegment segment = segments[j];

                Transform t = segment.lastTransform;
                if (overrideAnimation)
                {
                    for (int i = segment.chainLength - 1; i >= 0; i--)
                    {
                        t.localRotation = segment.origRotations[i];
                        t = t.parent;
                    }
                }

                Quaternion parentRot = segment.firstTransform.parent.rotation;
                Quaternion parentRotInv = Quaternion.Inverse(parentRot);

                // Desired look direction in world space
                var lookDirWorld = (target - segment.lastTransform.position).normalized;

                // Desired look directions in neck parent space
                var lookDirGoal = (parentRotInv * lookDirWorld);

                // Get the horizontal and vertical rotation angle to look at the target
                var hAngle = AngleAroundAxis(
                    segment.referenceLookDir, lookDirGoal, segment.referenceUpDir
                );

                var rightOfTarget = Vector3.Cross(segment.referenceUpDir, lookDirGoal);

                var lookDirGoalinHPlane =
                    lookDirGoal - Vector3.Project(lookDirGoal, segment.referenceUpDir);

                var vAngle = AngleAroundAxis(
                    lookDirGoalinHPlane, lookDirGoal, rightOfTarget
                );

                // Handle threshold angle difference, bending multiplier,
                // and max angle difference here
                var hAngleThr = Mathf.Max(
                    0, Mathf.Abs(hAngle) - segment.thresholdAngleDifference
                ) * Mathf.Sign(hAngle);

                var vAngleThr = Mathf.Max(
                    0, Mathf.Abs(vAngle) - segment.thresholdAngleDifference
                ) * Mathf.Sign(vAngle);

                hAngle = Mathf.Max(
                    Mathf.Abs(hAngleThr) * Mathf.Abs(segment.bendingMultiplier),
                    Mathf.Abs(hAngle) - segment.maxAngleDifference
                ) * Mathf.Sign(hAngle) * Mathf.Sign(segment.bendingMultiplier);

                vAngle = Mathf.Max(
                    Mathf.Abs(vAngleThr) * Mathf.Abs(segment.bendingMultiplier),
                    Mathf.Abs(vAngle) - segment.maxAngleDifference
                ) * Mathf.Sign(vAngle) * Mathf.Sign(segment.bendingMultiplier);

                // Handle max bending angle here
                hAngle = Mathf.Clamp(hAngle, -segment.maxBendingAngle, segment.maxBendingAngle);
                vAngle = Mathf.Clamp(vAngle, -segment.maxBendingAngle, segment.maxBendingAngle);

                var referenceRightDir =
                    Vector3.Cross(segment.referenceUpDir, segment.referenceLookDir);

                // Lerp angles
                segment.angleH = Mathf.Lerp(
                    segment.angleH, hAngle, Time.deltaTime * segment.responsiveness
                );
                segment.angleV = Mathf.Lerp(
                    segment.angleV, vAngle, Time.deltaTime * segment.responsiveness
                );

                // Get direction
                lookDirGoal = Quaternion.AngleAxis(segment.angleH, segment.referenceUpDir)
                    * Quaternion.AngleAxis(segment.angleV, referenceRightDir)
                    * segment.referenceLookDir;

                // Make look and up perpendicular
                var upDirGoal = segment.referenceUpDir;
                Vector3.OrthoNormalize(ref lookDirGoal, ref upDirGoal);

                // Interpolated look and up directions in neck parent space
                var lookDir = lookDirGoal;
                segment.dirUp = Vector3.Slerp(segment.dirUp, upDirGoal, Time.deltaTime * 5);
                Vector3.OrthoNormalize(ref lookDir, ref segment.dirUp);

                // Look rotation in world space
                var lookRot = (
                    (parentRot * Quaternion.LookRotation(lookDir, segment.dirUp))
                    * Quaternion.Inverse(
                        parentRot * Quaternion.LookRotation(
                            segment.referenceLookDir, segment.referenceUpDir
                        )
                    )
                );

                // Distribute rotation over all joints in segment
                var dividedRotation =
                    Quaternion.Slerp(Quaternion.identity, lookRot, effect / segment.chainLength);
                t = segment.lastTransform;
                for (int i = 0; i < segment.chainLength; i++)
                {
                    t.rotation = dividedRotation * t.rotation;
                    t = t.parent;
                }

                segments[j] = segment;
            }

            // Handle non affected joints
            for (int i = 0; i < nonAffectedJoints.Length; i++)
            {
                var newJointDirection = Vector3.zero;

                foreach (Transform child in nonAffectedJoints[i].joint)
                {
                    newJointDirection = child.position - nonAffectedJoints[i].joint.position;
                    break;
                }

                var combinedJointDirection = Vector3.Slerp(
                    jointDirections[i], newJointDirection, nonAffectedJoints[i].effect
                );

                nonAffectedJoints[i].joint.rotation = Quaternion.FromToRotation(
                    newJointDirection, combinedJointDirection
                ) * nonAffectedJoints[i].joint.rotation;
            }
        }

        /// <summary>
        /// The angle between dirA and dirB around axis
        /// </summary>
        /// <param name="dirA"></param>
        /// <param name="dirB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
        {
            // Project A and B onto the plane orthogonal target axis
            dirA = dirA - Vector3.Project(dirA, axis);
            dirB = dirB - Vector3.Project(dirB, axis);

            // Find (positive) angle between A and B
            float angle = Vector3.Angle(dirA, dirB);

            // Return angle multiplied with 1 or -1
            return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
        }
    }
}