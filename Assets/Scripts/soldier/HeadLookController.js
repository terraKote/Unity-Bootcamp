#pragma strict
#pragma implicit
#pragma downcast

class BendingSegment 
{
    var firstTransform : Transform;
    var lastTransform : Transform ;
    var thresholdAngleDifference = 0.0;
    var bendingMultiplier = 0.6;
    var maxAngleDifference = 30.0;
    var maxBendingAngle = 80.0;
    var responsiveness = 5.0;
    internal var angleH : float;
    internal var angleV : float;
    internal var dirUp : Vector3;
    internal var referenceLookDir : Vector3;
    internal var referenceUpDir : Vector3;
    internal var chainLength : int;
    internal var origRotations : Quaternion[];
}

class NonAffectedJoints 
{
    var joint : Transform;
    var effect = 0.0;
}

public var rootNode : Transform;
public var segments : BendingSegment[];
public var nonAffectedJoints : NonAffectedJoints[];
public var headLookVector = Vector3.forward;
public var headUpVector = Vector3.up;
public var target = Vector3.zero;
public var targetTransform : Transform;
public var effect = 1.0;
public var overrideAnimation = false;
   
function Start ()
{
    if (rootNode == null) {
        rootNode = transform;
    }
       
    // Setup segments
    for (var segment : BendingSegment in segments) 
    {
        var parentRot : Quaternion = segment.firstTransform.parent.rotation;
        var parentRotInv : Quaternion = Quaternion.Inverse(parentRot);
        segment.referenceLookDir =
            parentRotInv * rootNode.rotation * headLookVector.normalized;
        segment.referenceUpDir =
            parentRotInv * rootNode.rotation * headUpVector.normalized;
        segment.angleH = 0.0;
        segment.angleV = 0.0;
        segment.dirUp = segment.referenceUpDir;
       
        segment.chainLength = 1;
        var t : Transform = segment.lastTransform;
        while (t != segment.firstTransform && t != t.root) {
            segment.chainLength++;
            t = t.parent;
        }
       
        segment.origRotations = new Quaternion[segment.chainLength];
        t = segment.lastTransform;
        for (var i=segment.chainLength-1; i>=0; i--) {
            segment.origRotations[i] = t.localRotation;
            t = t.parent;
        }
    }
}

function LateUpdate ()
{
    if (Time.deltaTime == 0)
        return;
   	
   	target = targetTransform.position;
   	
    // Remember initial directions of joints that should not be affected
    var jointDirections : Vector3[] = new Vector3[nonAffectedJoints.Length];
    for (var i=0; i<nonAffectedJoints.Length; i++) {
        for (var child : Transform in nonAffectedJoints[i].joint) {
            jointDirections[i] = child.position - nonAffectedJoints[i].joint.position;
            break;
        }
    }
   
    // Handle each segment
    for (var segment : BendingSegment in segments) {
        var t : Transform = segment.lastTransform;
        if (overrideAnimation) {
            for (i=segment.chainLength-1; i>=0; i--) {
                t.localRotation = segment.origRotations[i];
                t = t.parent;
            }
        }

        var parentRot : Quaternion = segment.firstTransform.parent.rotation;
        var parentRotInv : Quaternion = Quaternion.Inverse(parentRot);

        // Desired look direction in world space
        var lookDirWorld : Vector3 = (target - segment.lastTransform.position).normalized;
       
        // Desired look directions in neck parent space
        var lookDirGoal : Vector3 = (parentRotInv * lookDirWorld);
       
        // Get the horizontal and vertical rotation angle to look at the target
        var hAngle : float = AngleAroundAxis(
            segment.referenceLookDir, lookDirGoal, segment.referenceUpDir
        );
       
        var rightOfTarget : Vector3 = Vector3.Cross(segment.referenceUpDir, lookDirGoal);
       
        var lookDirGoalinHPlane : Vector3 =
            lookDirGoal - Vector3.Project(lookDirGoal, segment.referenceUpDir);
       
        var vAngle : float  = AngleAroundAxis(
            lookDirGoalinHPlane, lookDirGoal, rightOfTarget
        );
       
        // Handle threshold angle difference, bending multiplier,
        // and max angle difference here
        var hAngleThr : float = Mathf.Max(
            0, Mathf.Abs(hAngle) - segment.thresholdAngleDifference
        ) * Mathf.Sign(hAngle);
       
        var vAngleThr : float = Mathf.Max(
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
       
        var referenceRightDir : Vector3 =
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
        var upDirGoal : Vector3 = segment.referenceUpDir;
        Vector3.OrthoNormalize(lookDirGoal, upDirGoal);
       
        // Interpolated look and up directions in neck parent space
        var lookDir : Vector3 = lookDirGoal;
        segment.dirUp = Vector3.Slerp(segment.dirUp, upDirGoal, Time.deltaTime*5);
        Vector3.OrthoNormalize(lookDir, segment.dirUp);
       
        // Look rotation in world space
        var lookRot : Quaternion = (
            (parentRot * Quaternion.LookRotation(lookDir, segment.dirUp))
            * Quaternion.Inverse(
                parentRot * Quaternion.LookRotation(
                    segment.referenceLookDir, segment.referenceUpDir
                )
            )
        );
       
        // Distribute rotation over all joints in segment
        var dividedRotation : Quaternion =
            Quaternion.Slerp(Quaternion.identity, lookRot, effect / segment.chainLength);
        t = segment.lastTransform;
        for (i=0; i<segment.chainLength; i++) {
            t.rotation = dividedRotation * t.rotation;
            t = t.parent;
        }
    }
   
    // Handle non affected joints
    for (i=0; i<nonAffectedJoints.Length; i++) {
        var newJointDirection : Vector3 = Vector3.zero;
       
        for (var child : Transform in nonAffectedJoints[i].joint) {
            newJointDirection = child.position - nonAffectedJoints[i].joint.position;
            break;
        }
       
        var combinedJointDirection : Vector3 = Vector3.Slerp(
            jointDirections[i], newJointDirection, nonAffectedJoints[i].effect
        );
       
        nonAffectedJoints[i].joint.rotation = Quaternion.FromToRotation(
            newJointDirection, combinedJointDirection
        ) * nonAffectedJoints[i].joint.rotation;
    }
}
   
// The angle between dirA and dirB around axis
static function AngleAroundAxis (dirA : Vector3, dirB : Vector3, axis : Vector3)
{
    // Project A and B onto the plane orthogonal target axis
    dirA = dirA - Vector3.Project(dirA, axis);
    dirB = dirB - Vector3.Project(dirB, axis);
   
    // Find (positive) angle between A and B
    var angle : float = Vector3.Angle(dirA, dirB);
   
    // Return angle multiplied with 1 or -1
    return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
}