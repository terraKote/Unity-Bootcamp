using UnityEngine;

[System.Serializable]
public struct TerrainQualitySettingsData
{
    public Terrain terrain;
}

public class TerrainQualitySettingsProcessor : IGameQualitySettingsProcessor
{
    private TerrainQualitySettingsData terrainQualitySettingsData;

    public TerrainQualitySettingsProcessor(TerrainQualitySettingsData terrainQualitySettingsData)
    {
        this.terrainQualitySettingsData = terrainQualitySettingsData;
	}

    public void ApplySettings(GameQualityService gameQualityService)
    {
		var cur = gameQualityService.CurrentSceneSettings;
		var nearTerrain = terrainQualitySettingsData.terrain;

        nearTerrain.detailObjectDistance = cur.detailObjectDistance;
        nearTerrain.detailObjectDensity = cur.detailObjectDensity;
        nearTerrain.treeDistance = cur.treeDistance;
        nearTerrain.heightmapPixelError = cur.nearTerrainPixelError;
        nearTerrain.treeBillboardDistance = cur.terrainTreesBillboardStart;
        nearTerrain.treeMaximumFullLODCount = cur.maxMeshTrees;
        nearTerrain.heightmapMaximumLOD = cur.heightmapMaximumLOD;
    }
}