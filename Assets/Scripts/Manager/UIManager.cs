using System;
using System.Collections;
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

    //Wave Title
    [SerializeField] private TextMeshProUGUI _wavetitle;

    public static event Action<int> OnDoubleJumpUpdate;
    public static event Action<float, float, int> OnDashCoolDownUpdate;
    public static event Action<int> OnGrappleUpdate;
    public static event Action<int> OnLivesUpdate;
    public static event Action<int, int> OnScoreUpdate;
    public static event Action<int, int> OnWaveAndEnemyCountUpdate;
    public static event Action<int, string> OnBulletAndFireModeUpdate;
    public static event Action<string, int> OnWaveTitleUpdate;

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
        OnWaveTitleUpdate += UpdateWaveTitle;
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
        OnWaveTitleUpdate -= UpdateWaveTitle;
    }

    public static void InvokeDoubleJumpUpdate(int count) => OnDoubleJumpUpdate?.Invoke(count);
    public static void InvokeDashCoolDownUpdate(float elapsed, float totalCoolDown, int count) => OnDashCoolDownUpdate?.Invoke(elapsed, totalCoolDown, count);
    public static void InvokeGrappleUpdate(int count) => OnGrappleUpdate?.Invoke(count);
    public static void InvokeLivesUpdate(int count) => OnLivesUpdate?.Invoke(count);
    public static void InvokeScoreUpdate(int highScoreCount, int scoreCount) => OnScoreUpdate?.Invoke(highScoreCount, scoreCount);
    public static void InvokeWaveAndEnemyCountUpdate(int waveCount, int enemyCount) => OnWaveAndEnemyCountUpdate?.Invoke(waveCount, enemyCount);
    public static void InvokeBulletAndFireModeUpdate(int bulletCount, string fireMode) => OnBulletAndFireModeUpdate?.Invoke(bulletCount, fireMode);
    public static void InvokeWaveTilteUpdate(string word, int count) => OnWaveTitleUpdate?.Invoke(word, count);


    public void UpdateDoubleJumpUI(int count)
    {
        _doubleJump.SetActive(count > 0);

        if(count == int.MaxValue)
        {
            _doubleJumpCount.text = "\u221E";
        }
        else
        {
            _doubleJumpCount.text = count.ToString();
        }
    }

    public void UpdateDashUI(float elapsed, float totalCoolDown, int count)
    {
        //fill goes from 0 to 1 as cooldown completes
        dashUI.fillAmount = Mathf.Clamp01(elapsed / totalCoolDown);
        _dash.SetActive(count > 0);

        if (count == int.MaxValue)
        {
            _dashCount.text = "\u221E";
        }
        else
        {
            _dashCount.text = count.ToString();
        }
    }

    public void UpdateGrappleUI(int count)
    {
        _grapple.SetActive(count > 0);

        if (count == int.MaxValue)
        {
            _grappleAmmoCount.text = "\u221E";
        }
        else
        {
            _grappleAmmoCount.text = count.ToString();
        }
    }

    public void UpdateLivesUI(int count)
    {
        _lives.SetActive(count > 0);

        if (count == int.MaxValue)
        {
            _livesCount.text = "\u221E";
        }
        else
        {
            _livesCount.text = count.ToString();
        }
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

    #region Wave Title

    public void UpdateWaveTitle(string word, int count)
    {
        StartCoroutine(TypeAWord(word, count));
    }

    private IEnumerator TypeAWord(string word, int count)
    {
        _wavetitle.text = " ";

        foreach(char letter in word)
        {
            _wavetitle.text += letter;

            yield return new WaitForSeconds(0.2f);
        }

        //Add the count at last
        _wavetitle.text += count.ToString();

        yield return new WaitForSeconds(2f);

        _wavetitle.text = " ";
    }

    #endregion
}
