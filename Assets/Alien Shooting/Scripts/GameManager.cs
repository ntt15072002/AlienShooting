using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameState state;
    public Camera cam;
    public bool isOnMobile;
    public Enemy[] enemyPbs;
    public int wavePerLevel;
    public int enemyPerLevel;
    public int enemyUpPerLevel;
    public int bulletExtra;
    public float timeDownPerLevel;

    private List<Enemy> m_enemySpawneds;
    private int m_killed;
    private int m_level = 1;
    private int m_waveCounting = 1;
    private int m_score;

    private bool m_isSlowed;
    private bool m_isBeginSlow;

    public int Killed { get => m_killed; set => m_killed = value; }

    public bool IsSlowed { get => m_isSlowed; set => m_isSlowed = value; }

    public int Score { get => m_score;}

    public override void Awake()
    {
        MakeSingleton(false);
        m_enemySpawneds = new List<Enemy>();
    }

    public override void Start()
    {
        state = GameState.Starting;

        if (AudioController.Ins)
        {
            AudioController.Ins.PlayBackgroundMusic();
        }
    }

    private void Update()
    {
        if (state != GameState.Playing) return;

        if(CanSlow() && !m_isBeginSlow)
        {
            m_isBeginSlow = true;
            float delay = Random.Range(0.01f, 0.05f);
            Timer.Schedule(this, delay, () =>
            {
                SlowController.Ins.DoSlowmotion();
            }, true);
        }

        if(Time.timeScale < 1 && m_killed >= m_enemySpawneds.Count && state != GameState.WaveComplete)
        {
            if(m_waveCounting % wavePerLevel == 0 )
            {
                state = GameState.WaveComplete;
                enemyPerLevel += enemyUpPerLevel;
                m_level++;

                if (GUIManager.Ins)
                {
                    GUIManager.Ins.UpdateLevelText(m_level);
                    if (GUIManager.Ins.waveCompleteDialog)
                    {
                        GUIManager.Ins.waveCompleteDialog.Show(true);
                    }
                }
            }
            else
            {
                ResetData();
                Spawn();
                m_waveCounting++;
                state = GameState.Playing;
            }
        }

        if (Time.timeScale >= 1 && !CanSlow() && m_killed < m_enemySpawneds.Count && m_isSlowed && state != GameState.Gameover)
        {
            state = GameState.Gameover;
        

            if (GUIManager.Ins && GUIManager.Ins.gameoverDialog)
            {
                GUIManager.Ins.gameoverDialog.Show(true);
            }
            m_score = 0;
        }
    }

    public void Spawn()
    {
        if (!SlowController.Ins || !Player.Ins) return;

        SlowController.Ins.slowdownLength = (enemyPerLevel / 2 + 1.5f) - timeDownPerLevel * m_waveCounting;

        Player.Ins.Bullet = enemyPerLevel + bulletExtra;

        if (enemyPbs == null || enemyPbs.Length <= 0) return;

        for (int i= 0 ; i < enemyPerLevel; i++)
        {
            int randIdx = Random.Range(0, enemyPbs.Length);

            var enemyPb = enemyPbs[randIdx];

            float spawnPosX = Random.Range(-8, 8);
            float spawnPosY = Random.Range(7f, 8.5f);
            Vector3 spawnPos = new Vector3(spawnPosX, spawnPosY, 0f);

            if (enemyPb)
            {
                var enemyClone = Instantiate(enemyPb, spawnPos, Quaternion.identity);
                m_enemySpawneds.Add(enemyClone);
            }
        }

        if (GUIManager.Ins)
        {
            GUIManager.Ins.UpdateBulletsText(Player.Ins.Bullet);
        }
    }

    public void ResetData()
    {
        m_isSlowed = false;
        m_isBeginSlow = false;

        m_killed = 0;
        if (state == GameState.Gameover)
        {
            m_waveCounting = 1;
            m_level = 1;
        }

        state = GameState.Starting;

        if (m_enemySpawneds == null || m_enemySpawneds.Count <= 0) return;

        for (int i = 0; i < m_enemySpawneds.Count; i++)
        {
            var enemy = m_enemySpawneds[i];
            if (enemy)
            {
                Destroy(enemy.gameObject);
            }
        }

        m_enemySpawneds.Clear();
    }

    public void NextLevel()
    {
        if (state == GameState.WaveComplete)
        {
            Timer.Schedule(this, 1f, () =>
            {
                m_waveCounting = 1;
                ResetData();
                Spawn();
                state = GameState.Playing;
            });
        }

        if (AudioController.Ins)
        {
            AudioController.Ins.PlayBackgroundMusic();
        }
    }

    public void StartGame()
    {
        ResetData();
        Timer.Schedule(this, 1f, () =>
        {
            Spawn();
            state = GameState.Playing;
        });

        if (GUIManager.Ins)
        {
            GUIManager.Ins.UpdateLevelText(m_level);

            GUIManager.Ins.UpdateBulletsText(Player.Ins.Bullet);

            GUIManager.Ins.ShowGamePlay(true);
        }

        if (AudioController.Ins)
        {
            AudioController.Ins.PlayBackgroundMusic();
        }
    }

    public void AddScore()
    {
        m_score++;
        Pref.bestScore = m_score;
    }

    private bool CanSlow()
    {
        if (m_enemySpawneds == null || m_enemySpawneds.Count <= 0) return false;

        int check = 0;

        for (int i = 0; i < m_enemySpawneds.Count; i++)
        {
            var enemy = m_enemySpawneds[i];
            if(enemy && enemy.CanSlow)
            {
                check++;
            }
        }

        if(check == m_enemySpawneds.Count)
        {
            return true;
        }

        return false;
    }
}
