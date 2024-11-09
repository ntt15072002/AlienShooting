using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    public GameObject viewFinderPb;

    private int m_bullet;
    private Camera m_cam;
    private GameObject m_viewFinderClone;

    public int Bullet { get => m_bullet; set => m_bullet = value; }

    public override void Awake()
    {
        MakeSingleton(false);
    }

    public override void Start()
    {
        if (!GameManager.Ins) return;

        m_cam = GameManager.Ins.cam;

        if (GameManager.Ins.isOnMobile || !viewFinderPb) return;

        m_viewFinderClone = Instantiate(viewFinderPb, new Vector3(10000, 10000, 0f), Quaternion.identity);
    }

    private void Update()
    {
        if (!m_cam) return;

        Vector3 mousePos = m_cam.ScreenToWorldPoint(Input.mousePosition);

        m_viewFinderClone.transform.position = new Vector3(
            mousePos.x, mousePos.y, 0f
            );

        if (Input.GetMouseButtonDown(0))
        {
            Shoot(mousePos);   
        }
    }

    private void Shoot(Vector3 mousePos)
    {
        if (m_bullet <= 0 || !GameManager.Ins || GameManager.Ins.state != GameState.Playing) return;

        m_bullet--;

        Vector3 shootingDir = m_cam.transform.position - mousePos;
        shootingDir.Normalize();
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, shootingDir, 0.1f);

        if (hits == null || hits.Length <= 0) return;
        for(int i = 0; i < hits.Length; i++)
        {
            var hitted = hits[i];

            if (!hitted.collider) continue;

            var enemy = hitted.collider.GetComponent<Enemy>();

            if (enemy)
            {
                enemy.Dead();
            }
        }

        if (GUIManager.Ins)
        {
            GUIManager.Ins.UpdateBulletsText(m_bullet);
        }

        if (AudioController.Ins)
        {
            AudioController.Ins.PlaySound(AudioController.Ins.shootingSound);
        }
    }
}
