using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Health : MonoBehaviour, IPlayerDamageable
{
    [SerializeField] int currentHealth;
    [SerializeField] int maxhealth;

    private KnockBack knockBack;
    private FlashEffect flashEffect;
    private HealthBar _healthBar;

    public int MaxHealth
    {
        get => maxhealth;
        set => maxhealth = Mathf.Max(0,value);
    }

    public int CurrentHealth { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        knockBack = GetComponent<KnockBack>();
        flashEffect = GetComponent<FlashEffect>();
        _healthBar = GetComponentInChildren<HealthBar>();

        currentHealth = maxhealth;
    }

   public void Damage(int damageAmount, Vector2 hitDirection)
    {
        currentHealth -= damageAmount;

        //KnockBack
        knockBack.CallKnockBackCoroutine(hitDirection, Vector2.up, Input.GetAxisRaw("Horizontal"));

        //Damage Flash
        flashEffect.CallDamageFlash();

        //Update Health Bar
        _healthBar.UpdateHealthBar(maxhealth, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        //Die
        gameObject.SetActive(false);
    }

}
