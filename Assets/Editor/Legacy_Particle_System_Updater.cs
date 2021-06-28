/*
 * Legacy Particle System Updater
 * A tool that can be used to convert Legacy Particle Systems into new Particle System Components.
 * https://forum.unity.com/threads/release-legacy-particle-system-updater.510879/
 *
 * v1.0
 * Initial release
 *
 * v1.1
 * Fixed incorrect billboard mode
 * Added Undo support
 * Fixed emission using Min and Max state when Min and Max values were the same.
 * Fixed emission when using one shot, should be Burst.
 * Added support for sizeGrow.
 *
 * v1.2
 * Fixed incorrect shape when Ellipsoid emitter is (0,0,0).
 * Fixed emitterVelocityScale being used incorrectly. It should be inherit velocity.
 * Fixed velocity dampening. We need to apply this to the velocity curve.
 * Set duration to be max lifetime.
 * Fixed particles using transform scale. Legacy did not support this.
 * Fixed size grow. grow is not linear, we also did not handle min and max times.
 *
 * v1.3
 * Fixed compilation issues on 2017.1
 * Warning added for Unity version 2018.3 and newer. Legacy particles will be removed in 2018.3.
 *
 * v1.4
 * Fixed incorrect version detection and information in help message.
 *
 * v1.5
 * Added compilation message for 2018.3+ to inform that this script is not supported.
 *
 * v1.6
 * Fixed random force calculation when linear force was not zero.
 */

#pragma warning disable 618

#if UNITY_2018_3_OR_NEWER
#error "This script(LegacyParticleUpdater) is not supported in this version, please use a Unity version between 2017.4 and 2018.2.
#else

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LegacyParticleUpdater : ScriptableWizard
{
    const string kVersion = "1.5";

    public enum LegacyCleanupMode
    {
        PreserveLegacyComponents,
        DisableLegacyRenderer,
        DeleteLegacyComponents
    };

    enum LegacyParticleRenderMode
    {
        Billboard = 0,
        Stretch2D = 1,
        Stretch3D = 3,
        SortedBillboard = 2,
        BillboardFixedHorizontal = 4,
        BillboardFixedVertical = 5,
    };

    public LegacyCleanupMode cleanupMode = LegacyCleanupMode.DisableLegacyRenderer;
    private ParticleEmitter[] components;
    private ParticleEmitter[] prefabs;

    [MenuItem("Assets/Upgrade Legacy Particles")]
    public static void ShowWindow()
    {
        ScriptableWizard.DisplayWizard<LegacyParticleUpdater>("Upgrade Legacy Particles v" + kVersion, "Upgrade Selected", "Upgrade Everything");
    }

    void OnWizardUpdate()
    {
        helpString = @"This Script adds ParticleSystem and ParticleSystemRenderer Components to all GameObjects that contain Legacy Particle Components.
        Legacy Particle System Components can be deleted, disabled, or preserved for comparison.
        This script supports Unity versions between 2017.4 and 2018.2.";
    }

    // Find selected assets
    void OnWizardCreate()
    {
        components = Selection.GetFiltered<ParticleEmitter>(SelectionMode.Unfiltered);
        prefabs = null;

        UpgradeAll();
    }

    // Find all assets
    void OnWizardOtherButton()
    {
        List<ParticleEmitter> results = new List<ParticleEmitter>();
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                ParticleEmitter[] emitters = root.GetComponentsInChildren<ParticleEmitter>(true);
                results.AddRange(emitters);
            }
        }
        components = results.ToArray();

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:ParticleEmitter");
        prefabs = new ParticleEmitter[prefabGUIDs.Length];
        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGUIDs[i])).GetComponent<ParticleEmitter>();
        }

        UpgradeAll();
    }

    void UpgradeAll()
    {
        try
        {
            if (prefabs != null)
            {
                EditorUtility.DisplayProgressBar("Upgrading", "Upgrading Prefabs", 0);
                for (int i = 0; i < prefabs.Length; i++)
                {
                    Upgrade(prefabs[i]);
                    EditorUtility.DisplayProgressBar("Upgrading", "Upgrading Prefabs", (float)(i + 1) / prefabs.Length);
                }
                AssetDatabase.SaveAssets();
            }

            if (components != null)
            {
                EditorUtility.DisplayProgressBar("Upgrading", "Upgrading Objects", 0);
                for (int i = 0; i < components.Length; i++)
                {
                    Upgrade(components[i]);
                    EditorUtility.DisplayProgressBar("Upgrading", "Upgrading Prefabs", (float)(i + 1) / components.Length);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static ParticleSystem.MinMaxCurve ConvertToMinMaxCurve(float min, float max)
    {
        if (Mathf.Approximately(min, max))
            return new ParticleSystem.MinMaxCurve(min);
        else
            return new ParticleSystem.MinMaxCurve(min, max);
    }

    static ParticleSystem.MinMaxCurve ApplyDampen(ParticleSystem.MinMaxCurve velocityCurve, float dampen)
    {
        const float step = 0.2f;

        if (velocityCurve.mode == ParticleSystemCurveMode.Constant)
        {
            if (Mathf.Approximately(velocityCurve.constant, 0) || Mathf.Approximately(dampen, 0))
                return new ParticleSystem.MinMaxCurve(0, AnimationCurve.Linear(0, 1, 1, 1));

            // Create the curve shape
            var animCurve = new AnimationCurve();
            for (float time = 0; time <= 1.0f; time += step)
            {
                animCurve.AddKey(time, Mathf.Pow(dampen, time));
            }

            // Remove keys used to create the shape
            while (animCurve.keys.Length > 2)
                animCurve.RemoveKey(1);

            return new ParticleSystem.MinMaxCurve(velocityCurve.constant, animCurve);
        }
        else
        {
            Debug.Assert(velocityCurve.mode == ParticleSystemCurveMode.TwoConstants);

            var animCurveMin = new AnimationCurve();
            var animCurveMax = new AnimationCurve();

            if (Mathf.Approximately(velocityCurve.constantMin, velocityCurve.constantMax) || Mathf.Approximately(dampen, 0))
                return new ParticleSystem.MinMaxCurve(0, AnimationCurve.Linear(0, 1, 1, 1), AnimationCurve.Linear(0, 1, 1, 1));

            float minAbs = Math.Abs(velocityCurve.constantMin);
            float maxAbs = Math.Abs(velocityCurve.constantMax);
            float normalizedMin = minAbs / maxAbs;
            float minSign = velocityCurve.constantMin < 0 ? -1 : 1;
            float maxSign = velocityCurve.constantMax < 0 ? -1 : 1;
            for (float time = 0; time <= 1.0f; time += step)
            {
                animCurveMin.AddKey(time, minSign * normalizedMin * Mathf.Pow(dampen, time));
                animCurveMax.AddKey(time, maxSign * Mathf.Pow(dampen, time));
            }

            // Remove keys used to create the general shape
            while (animCurveMin.keys.Length > 2)
            {
                animCurveMin.RemoveKey(1);
                animCurveMax.RemoveKey(1);
            }

            return new ParticleSystem.MinMaxCurve(maxAbs, animCurveMin, animCurveMax);
        }
    }

    static ParticleSystem.MinMaxCurve GenerateSizeGrowCurve(float sizeGrow, float minTime, float maxTime)
    {
        const float step = 0.2f;

        if (sizeGrow > 0)
        {
            if (Mathf.Approximately(minTime, maxTime))
            {
                float finalSize = Mathf.Pow(1.0f + sizeGrow, minTime);
                var animCurve = new AnimationCurve();
                for (float time = 0; time <= 1.0f; time += step)
                {
                    animCurve.AddKey(time, Mathf.Pow(1 + sizeGrow, time * maxTime) / finalSize);
                }

                // Remove keys used to create the shape
                while (animCurve.keys.Length > 2)
                    animCurve.RemoveKey(1);

                return new ParticleSystem.MinMaxCurve(finalSize, animCurve);
            }
            else
            {
                float finalSizeMax = Mathf.Pow(1.0f + sizeGrow, maxTime);

                var animCurveMin = new AnimationCurve();
                var animCurveMax = new AnimationCurve();
                for (float time = 0; time <= 1.0f; time += step)
                {
                    animCurveMin.AddKey(time, Mathf.Pow(1 + sizeGrow, time * minTime) / finalSizeMax);
                    animCurveMax.AddKey(time, Mathf.Pow(1 + sizeGrow, time * maxTime) / finalSizeMax);
                }

                while (animCurveMin.keys.Length > 2)
                {
                    animCurveMin.RemoveKey(1);
                    animCurveMax.RemoveKey(1);
                }

                return new ParticleSystem.MinMaxCurve(finalSizeMax, animCurveMin, animCurveMax);
            }
        }
        else
        {
            if (Mathf.Approximately(minTime, maxTime))
            {
                var animCurve = new AnimationCurve();
                for (float time = 0; time <= 1.0f; time += step)
                {
                    animCurve.AddKey(time, Mathf.Pow(-sizeGrow, time));
                }

                // Remove keys used to create the shape
                while (animCurve.keys.Length > 2)
                    animCurve.RemoveKey(1);

                return new ParticleSystem.MinMaxCurve(1, animCurve);
            }
            else
            {
                var animCurveMin = new AnimationCurve();
                var animCurveMax = new AnimationCurve();
                float minTimeOverMaxTime = minTime / maxTime;
                for (float time = 0; time <= 1.0f; time += step)
                {
                    animCurveMin.AddKey(time, Mathf.Pow(-sizeGrow, time * minTimeOverMaxTime));
                    animCurveMax.AddKey(time, Mathf.Pow(-sizeGrow, time));
                }

                while (animCurveMin.keys.Length > 2)
                {
                    animCurveMin.RemoveKey(1);
                    animCurveMax.RemoveKey(1);
                }

                return new ParticleSystem.MinMaxCurve(1, animCurveMin, animCurveMax);
            }
        }
    }

    void Upgrade(ParticleEmitter emitter)
    {
        // Add component
        if (emitter.GetComponent<ParticleSystem>() == null)
            Undo.AddComponent<ParticleSystem>(emitter.gameObject);
        else
            Undo.RecordObject(emitter.GetComponent<ParticleSystem>(), "Upgrade Legacy System");

        ParticleSystem ps = emitter.GetComponent<ParticleSystem>();
        ParticleSystemRenderer psr = emitter.GetComponent<ParticleSystemRenderer>();
        SerializedObject serializedObject = new SerializedObject(emitter);

        // Upgrade
        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;
        var velocityOverLifetime = ps.velocityOverLifetime;
        var rotationOverLifetime = ps.rotationOverLifetime;
        var colorOverLifetime = ps.colorOverLifetime;
        var forceOverLifetime = ps.forceOverLifetime;
        var textureSheetAnimation = ps.textureSheetAnimation;
        var sizeOverLifetime = ps.sizeOverLifetime;
        var inheritVelocity = ps.inheritVelocity;

        // Main
        main.startSpeed = 0.0f;
        main.maxParticles = 10000;
        shape.enabled = false;
        ps.playOnAwake = serializedObject.FindProperty("m_Emit").boolValue;
        main.startSize = ConvertToMinMaxCurve(serializedObject.FindProperty("minSize").floatValue, serializedObject.FindProperty("maxSize").floatValue);
        main.startLifetime = ConvertToMinMaxCurve(serializedObject.FindProperty("minEnergy").floatValue, serializedObject.FindProperty("maxEnergy").floatValue);
        main.duration = serializedObject.FindProperty("maxEnergy").floatValue;
        main.startRotation = ConvertToMinMaxCurve(0.0f, serializedObject.FindProperty("rndRotation").boolValue ? Mathf.Deg2Rad * 360.0f : 0.0f);
        main.loop = true;
        main.simulationSpace = serializedObject.FindProperty("Simulate in Worldspace?").boolValue ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Shape;

        // Emission
        var minEmission = serializedObject.FindProperty("minEmission").floatValue;
        var maxEmission = serializedObject.FindProperty("maxEmission").floatValue;
        if (serializedObject.FindProperty("m_OneShot").boolValue)
        {
            main.loop = false;
            emission.rateOverTime = 0;

#if UNITY_2017_2_OR_NEWER
            emission.burstCount = 1;
#endif

            ParticleSystem.Burst burst = new ParticleSystem.Burst();
            burst.cycleCount = 1;
            burst.cycleCount = int.MaxValue;
            burst.repeatInterval = main.startLifetime.constantMax;

#if UNITY_2017_2_OR_NEWER
            burst.count = ConvertToMinMaxCurve(minEmission, maxEmission);
            emission.SetBurst(0, burst);
#else
            burst.maxCount = (short)maxEmission;
            burst.minCount = (short)minEmission;
            emission.SetBursts(new[] { burst });
#endif
        }
        else
        {
            emission.rateOverTime = ConvertToMinMaxCurve(minEmission, maxEmission);
        }

        // Inherit velocity
        float velocityScale = serializedObject.FindProperty("emitterVelocityScale").floatValue;
        if (!Mathf.Approximately(velocityScale, 0.0f))
        {
            inheritVelocity.enabled = true;
            inheritVelocity.curve = velocityScale;
        }

        // Velocity over lifetime
        Vector3 localVelocity = serializedObject.FindProperty("localVelocity").vector3Value;
        Vector3 worldVelocity = serializedObject.FindProperty("worldVelocity").vector3Value;
        Vector3 randomVelocity = serializedObject.FindProperty("rndVelocity").vector3Value;
        Vector3? minVel = null;
        Vector3? maxVel = null;

        if (localVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            minVel = localVelocity - randomVelocity;
            maxVel = localVelocity + randomVelocity;
        }
        else if (worldVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            minVel = worldVelocity - randomVelocity;
            maxVel = worldVelocity + randomVelocity;
        }
        else if (randomVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            minVel = -randomVelocity;
            maxVel = randomVelocity;
        }

        if (minVel != null)
        {
            var min = minVel.Value;
            var max = maxVel.Value;
            if (min == max)
            {
                velocityOverLifetime.x = min.x;
                velocityOverLifetime.y = min.y;
                velocityOverLifetime.z = min.z;
            }
            else
            {

                velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(min.x, max.x);
                velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(min.y, max.y);
                velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(min.z, max.z);
            }
        }

        // Rotation over lifetime
        float angularVelocity = serializedObject.FindProperty("rndAngularVelocity").floatValue;
        if (angularVelocity > Mathf.Epsilon)
        {
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * angularVelocity, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)));
        }

        // Shape
        if (emitter is EllipsoidParticleEmitter)
        {
            Vector3 ellipsoid = serializedObject.FindProperty("m_Ellipsoid").vector3Value;
            float maxDimension = Mathf.Max(ellipsoid.x, Mathf.Max(ellipsoid.y, ellipsoid.z));
            if (maxDimension > 0)
            {
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = maxDimension;
#if UNITY_2017_OR_NEWER
                shape.scale = ellipsoid / maxDimension;
#endif
                }
        }

        ParticleAnimator animator = emitter.GetComponent<ParticleAnimator>();
        if (animator != null)
        {
            // Color over lifetime
            SerializedObject serializedObjectAnimator = new SerializedObject(animator);
            if (serializedObjectAnimator.FindProperty("Does Animate Color?").boolValue)
            {
                Gradient gradient = new Gradient();

                gradient.colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(serializedObjectAnimator.FindProperty("colorAnimation[0]").colorValue, 0.0f),
                    new GradientColorKey(serializedObjectAnimator.FindProperty("colorAnimation[1]").colorValue, 0.25f),
                    new GradientColorKey(serializedObjectAnimator.FindProperty("colorAnimation[2]").colorValue, 0.5f),
                    new GradientColorKey(serializedObjectAnimator.FindProperty("colorAnimation[3]").colorValue, 0.75f),
                    new GradientColorKey(serializedObjectAnimator.FindProperty("colorAnimation[4]").colorValue, 1.0f)
                };

                gradient.alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(serializedObjectAnimator.FindProperty("colorAnimation[0]").colorValue.a, 0.0f),
                    new GradientAlphaKey(serializedObjectAnimator.FindProperty("colorAnimation[1]").colorValue.a, 0.25f),
                    new GradientAlphaKey(serializedObjectAnimator.FindProperty("colorAnimation[2]").colorValue.a, 0.5f),
                    new GradientAlphaKey(serializedObjectAnimator.FindProperty("colorAnimation[3]").colorValue.a, 0.75f),
                    new GradientAlphaKey(serializedObjectAnimator.FindProperty("colorAnimation[4]").colorValue.a, 1.0f)
                };

                colorOverLifetime.enabled = true;
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            }

            // Force over lifetime
            Vector3 linearForce = serializedObjectAnimator.FindProperty("force").vector3Value;
            Vector3 rndForce = serializedObjectAnimator.FindProperty("rndForce").vector3Value;
            if (linearForce.sqrMagnitude > Mathf.Epsilon || rndForce.sqrMagnitude > Mathf.Epsilon)
            {
                forceOverLifetime.enabled = true;
                forceOverLifetime.randomized = true;
                forceOverLifetime.x = ConvertToMinMaxCurve(-rndForce.x + linearForce.x, rndForce.x + linearForce.x);
                forceOverLifetime.y = ConvertToMinMaxCurve(-rndForce.y + linearForce.y, rndForce.y + linearForce.y);
                forceOverLifetime.z = ConvertToMinMaxCurve(-rndForce.z + linearForce.z, rndForce.z + linearForce.z);
            }

            // Size over lifetime
            var sizeGrow = serializedObjectAnimator.FindProperty("sizeGrow").floatValue;
            if (!Mathf.Approximately(sizeGrow, 0))
            {
                // If energy has min and max we will then need a min and max curve
                sizeOverLifetime.enabled = true;
                sizeOverLifetime.size = GenerateSizeGrowCurve(sizeGrow, main.startLifetime.mode == ParticleSystemCurveMode.Constant ? main.startLifetime.constantMax : main.startLifetime.constantMin, main.startLifetime.constantMax);
            }

#if UNITY_2017_2_OR_NEWER
            // Stop action
            if (serializedObjectAnimator.FindProperty("autodestruct").boolValue)
                main.stopAction = ParticleSystemStopAction.Destroy;
#else
            Debug.LogWarning("Can not replicate the autodestruct behavior in this version. Consider upgrading to 2017.2+ or writing a script to replicate this behavior.", emitter);
#endif

            // Dampen
            float damping = serializedObjectAnimator.FindProperty("damping").floatValue;
            if (damping < 1.0f)
            {
                // Dampening works different in legacy. We map the dampen value across to the velocity over lifetime curve instead of using the limit velocity dampen.
                velocityOverLifetime.x = ApplyDampen(velocityOverLifetime.x, damping);
                velocityOverLifetime.y = ApplyDampen(velocityOverLifetime.y, damping);
                velocityOverLifetime.z = ApplyDampen(velocityOverLifetime.z, damping);
            }
        }

        // Renderer
        ParticleRenderer renderer = emitter.GetComponent<ParticleRenderer>();
        if (renderer != null)
        {
            SerializedObject serializedObjectRenderer = new SerializedObject(renderer);
            var renderMode = (LegacyParticleRenderMode)serializedObjectRenderer.FindProperty("m_StretchParticles").intValue;
            switch (renderMode)
            {
                case LegacyParticleRenderMode.Billboard:
                    psr.renderMode = ParticleSystemRenderMode.Billboard;
                    break;

                case LegacyParticleRenderMode.Stretch2D:
                case LegacyParticleRenderMode.Stretch3D:
                    psr.renderMode = ParticleSystemRenderMode.Stretch;
                    break;

                case LegacyParticleRenderMode.SortedBillboard:
                    psr.renderMode = ParticleSystemRenderMode.Billboard;
                    psr.sortMode = ParticleSystemSortMode.Distance;
                    break;

                case LegacyParticleRenderMode.BillboardFixedHorizontal:
                    psr.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                    break;

                case LegacyParticleRenderMode.BillboardFixedVertical:
                    psr.renderMode = ParticleSystemRenderMode.VerticalBillboard;
                    break;
            }

            psr.cameraVelocityScale = serializedObjectRenderer.FindProperty("m_CameraVelocityScale").floatValue;
            psr.lengthScale = serializedObjectRenderer.FindProperty("m_LengthScale").floatValue;
            psr.velocityScale = serializedObjectRenderer.FindProperty("m_VelocityScale").floatValue;
            psr.maxParticleSize = serializedObjectRenderer.FindProperty("m_MaxParticleSize").floatValue;

            psr.shadowCastingMode = renderer.shadowCastingMode;
            psr.receiveShadows = renderer.receiveShadows;
            psr.sharedMaterial = renderer.sharedMaterial;

            int xTile = serializedObjectRenderer.FindProperty("UV Animation.x Tile").intValue;
            int yTile = serializedObjectRenderer.FindProperty("UV Animation.y Tile").intValue;
            float cycles = serializedObjectRenderer.FindProperty("UV Animation.cycles").floatValue;
            if (xTile > 1 || yTile > 1)
            {
                textureSheetAnimation.enabled = true;
                textureSheetAnimation.numTilesX = xTile;
                textureSheetAnimation.numTilesY = yTile;
                textureSheetAnimation.cycleCount = (int)cycles;
            }
        }

        // Cleanup
        if (cleanupMode == LegacyCleanupMode.DeleteLegacyComponents)
        {
            Undo.DestroyObjectImmediate(emitter.gameObject.GetComponent<ParticleAnimator>());
            Undo.DestroyObjectImmediate(emitter.gameObject.GetComponent<ParticleRenderer>());
            Undo.DestroyObjectImmediate(emitter);
        }
        else if (cleanupMode == LegacyCleanupMode.DisableLegacyRenderer)
        {
            Undo.RecordObject(emitter.gameObject.GetComponent<ParticleRenderer>(), "Disable Object");
            emitter.gameObject.GetComponent<ParticleRenderer>().enabled = false;
        }
    }
}
#endif

#pragma warning restore 618
