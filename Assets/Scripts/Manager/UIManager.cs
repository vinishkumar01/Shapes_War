using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _doubleJump;
    [SerializeField] private GameObject _dash;
    private Image dashUI;
    [SerializeField] private GameObject _grapple;
    [SerializeField] private GameObject _lives;

    [SerializeField] private TextMeshProUGUI _doubleJumpCount;
    [SerializeField] private TextMeshProUGUI _dashCount;
    [SerializeField] private TextMeshProUGUI _grappleAmmoCount;
    [SerializeField] private TextMeshProUGUI _livesCount;

    //Score UI
    [SerializeField] private TextMeshProUGUI _highScoreText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    //Wave Count and Enemy Count
    [SerializeField] private TextMeshProUGUI _waveCount;
    [SerializeField] private TextMeshProUGUI _enemyCount;

    //Bullets
    [SerializeField] private TextMeshProUGUI _bulletCount;
    [SerializeField] private TextMeshProUGUI _fireMode;

    public static event Action<int> OnDoubleJumpUpdate;
    public static event Action<float, float, int> OnDashCoolDownUpdate;
    public static event Action<int> OnGrappleUpdate;
    public static event Action<int> OnLivesUpdate;
    public static event Action<int, int> OnScoreUpdate;
    public static event Action<int, int> OnWaveAndEnemyCountUpdate;
    public static event Action<int, string> OnBulletAndFireModeUpdate;

    private void Awake()
    {
        dashUI = _dash.GetComponent<Image>();
    }

    private void OnEnable()
    {
        OnDoubleJumpUpdate += UpdateDoubleJumpUI;
        OnDashCoolDownUpdate += UpdateDashUI;
        OnGrappleUpdate += UpdateGrappleUI;
        OnLivesUpdate += UpdateLivesUI;
        OnScoreUpdate += UpdateHighScoreUI;
        OnWaveAndEnemyCountUpdate += UpdateWaveAndEnemyCount;
        OnBulletAndFireModeUpdate += UpdateBulletAndFireMode;
    }

    private void OnDisable()
    {
        OnDoubleJumpUpdate -= UpdateDoubleJumpUI;
        OnDashCoolDownUpdate -= UpdateDashUI;
        OnGrappleUpdate -= UpdateGrappleUI;
        OnLivesUpdate -= UpdateLivesUI;
        OnScoreUpdate -= UpdateHighScoreUI;
        OnWaveAndEnemyCountUpdate -= UpdateWaveAndEnemyCount;
        OnBulletAndFireModeUpdate -= UpdateBulletAndFireMode;
    }

    public static void InvokeDoubleJumpUpdate(int count) => OnDoubleJumpUpdate?.Invoke(count);
    public static void InvokeDashCoolDownUpdate(float elapsed, float totalCoolDown, int count) => OnDashCoolDownUpdate?.Invoke(elapsed, totalCoolDown, count);
    public static void InvokeGrappleUpdate(int count) => OnGrappleUpdate?.Invoke(count);
    public static void InvokeLivesUpdate(int count) => OnLivesUpdate?.Invoke(count);
    public static void InvokeScoreUpdate(int highScoreCount, int scoreCount) => OnScoreUpdate?.Invoke(highScoreCount, scoreCount);
    public static void InvokeWaveAndEnemyCountUpdate(int waveCount, int enemyCount) => OnWaveAndEnemyCountUpdate?.Invoke(waveCount, enemyCount);
    public static void InvokeBulletAndFireModeUpdate(int bulletCount, string fireMode) => OnBulletAndFireModeUpdate?.Invoke(bulletCount, fireMode);


    public void UpdateDoubleJumpUI(int count)
    {
        _doubleJumpCount.text = count.ToString();
        _doubleJump.SetActive(count > 0);
    }

    public void UpdateDashUI(float elapsed, float totalCoolDown, int count)
    {
        //fill goes from 0 to 1 as cooldown completes
        dashUI.fillAmount = Mathf.Clamp01(elapsed / totalCoolDown);
        _dashCount.text = count.ToString();
        _dash.SetActive(count > 0);
    }

    public void UpdateGrappleUI(int count)
    {
       _grappleAmmoCount.text = count.ToString();
        _grapple.SetActive(count > 0);
    }

    public void UpdateLivesUI(int count)
    {
       _livesCount.text = count.ToString();
        _lives.SetActive(count > 0);
    }

    public void UpdateHighScoreUI(int highScoreCount, int scoreCount)
    {
        _highScoreText.text = highScoreCount.ToString();
        _scoreText.text = scoreCount.ToString();
    }

    public void UpdateWaveAndEnemyCount(int waveCount, int enemyCount)
    {
        _waveCount.text = waveCount.ToString();
        _enemyCount.text = enemyCount.ToString();
    }

    public void UpdateBulletAndFireMode(int bulletCount, string fireMode)
    {
        _bulletCount.text = bulletCount.ToString();
        _fireMode.text = fireMode.ToString();
    }
}
