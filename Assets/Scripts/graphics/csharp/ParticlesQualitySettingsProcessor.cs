using UnityEngine;

public struct ParticlesQualitySettingsData
{
    public ParticleSystem[] particleSystems;
    public float particleQualityMultiplier;
    public bool displayAmbientParticles;
    public AmbientParticleSettings[] ambientParticleSettings;
}

public class ParticlesQualitySettingsProcessor : IGameQualitySettingsProcessor
{
    private ParticlesQualitySettingsData particlesQualitySettingsData;

    public ParticlesQualitySettingsProcessor(ParticlesQualitySettingsData particlesQualitySettingsData)
    {
        this.particlesQualitySettingsData = particlesQualitySettingsData;

        // ApplyCustomQualityLevel(qualityLevel);

        ParticleSystem[] ambientParticleObjects = particlesQualitySettingsData.particleSystems;

        if (ambientParticleObjects == null)
        {
            Debug.LogError("No particles were assigned!");
            return;
        }

        for (int k = 0; k < ambientParticleObjects.Length; k++)
        {
            if (ambientParticleObjects[k] == null) continue;

            // TODO: Implement particle marker class
            if (ambientParticleObjects[k].name == "dust" || ambientParticleObjects[k].name == "leaves") continue;

            ambientParticleObjects[k].gameObject.SetActive(particlesQualitySettingsData.displayAmbientParticles);
        }

        //if (_particleQualityMultiplier != particleQualityMultiplier)
        //{
        //    UpdateAmbientParticleQuality();
        //}
    }

    public void ApplySettings(GameQualityService gameQualityService)
    {
        float particleQualityMultiplier = particlesQualitySettingsData.particleQualityMultiplier;
        var ambientParticleObjects = particlesQualitySettingsData.particleSystems;

        for (int k = 0; k < ambientParticleObjects.Length; k++)
        {
            var setting = particlesQualitySettingsData.ambientParticleSettings[k];

            if (ambientParticleObjects[k] == null) continue;
            if (!ambientParticleObjects[k].gameObject.activeSelf) continue;

            ParticleSystem particle = ambientParticleObjects[k];
            // TODO: Replace properties from emitter to system
            //particle.minSize = setting.minSize * particleQualityMultiplier;
            //particle.maxSize = setting.maxSize * particleQualityMultiplier;
            //particle.minEmission = setting.minEmission * particleQualityMultiplier;
            //particle.maxEmission = setting.maxEmission * particleQualityMultiplier;
        }
    }
}