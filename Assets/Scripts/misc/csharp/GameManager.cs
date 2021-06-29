using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    public GameObject gamePlaySoldier;
    public ParticleEmitter soldierSmoke;
    public SargeManager sarge;

    public bool receiveDamage;
    public bool scores;
    public float time;
    public bool running;

    void Start()
    {
        TrainingStatistics.ResetStatistics();

        running = false;
        scores = false;
        time = 0.0f;

        Transform auxT;
        bool hasCutscene = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            auxT = transform.GetChild(i);

            if (auxT.name == "Cutscene")
            {
                if (auxT.gameObject.activeSelf)
                {
                    hasCutscene = true;
                    break;
                }
            }
        }

        if (!hasCutscene)
        {
            StartGame();
        }
    }

    private void Update()
    {
        if (!PlayerInputService.GetInstance().IsPausing && running)
        {
            time += Time.deltaTime;
        }
    }

    void CutsceneStart()
    {
        gamePlaySoldier.SetActive(false);
    }

    void StartGame()
    {
        running = true;

        if (gamePlaySoldier != null)
        {
            if (!gamePlaySoldier.activeSelf)
            {
                gamePlaySoldier.SetActive(true);
            }
        }

        if (soldierSmoke != null)
        {
            if (GameQualitySettings.ambientParticles)
            {
                soldierSmoke.emit = true;
            }
        }

        if (sarge != null && SceneManager.GetActiveScene().name == "demo_forest")
        {
            sarge.ShowInstruction("instructions");
            sarge.ShowInstruction("instructions2");
            sarge.ShowInstruction("instructions3");
            sarge.ShowInstruction("instructions4");
            sarge.ShowInstruction("instructions5");
            sarge.ShowInstruction("additional_instructions");
        }
    }
}