using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponStats _stats;
    [SerializeField] private GameObject _prefabToInstantiate;
    [SerializeField] private Transform _bulletOrigin;

    private Actor _owner;
    protected float cooldownLeft;

    public GameObject PrefabToInstantiate => _prefabToInstantiate;
    public WeaponStats Stats => _stats;
    public Transform BulletOrigin => _bulletOrigin;
    public Actor Owner => _owner;
    public float CooldownLeft => cooldownLeft;

    public void Attack(Vector2 origin, Vector2 direction)
    {
        if (Owner.IsDead) return;
        if (cooldownLeft > 0) return;
        
        if (TryGetComponent<BulletSoundController>(out var controller)) controller.Shoot();

        int numberOfProjectiles = _stats.Projectiles + _owner.Stats.ProjectileIncrease;
        float angleBetweenProjectiles = 5f;

        if (360 / angleBetweenProjectiles < numberOfProjectiles)
            numberOfProjectiles = Mathf.RoundToInt(360 / angleBetweenProjectiles);

        Vector2 mainDirection = direction.normalized;
        float mainAngle = Mathf.Atan2(mainDirection.y, mainDirection.x) * Mathf.Rad2Deg;

        float maxRotationAngle = angleBetweenProjectiles * (numberOfProjectiles - 1) / 2;

        for (float rotationAngle = -maxRotationAngle; rotationAngle <= maxRotationAngle; rotationAngle += angleBetweenProjectiles)
        {
            float currentAngle = mainAngle + rotationAngle;

            Quaternion projectileRotation = Quaternion.Euler(0, 0, currentAngle);

            GameObject projectile = Instantiate(_prefabToInstantiate, origin, projectileRotation);
            projectile.GetComponent<Projectile>().SetOwner(this);
        }

        if (_owner.Stats.BonusAttackSpeed > 0)
            cooldownLeft = _stats.Cooldown / _owner.Stats.BonusAttackSpeed;
        else
            cooldownLeft = _stats.Cooldown;
    }

    public void Attack(Vector2 direction)
    {
        Attack(_bulletOrigin.position, direction);
    }

    public void Attack()
    {
        Attack(transform.right);
    }

    protected void Start()
    {
        _owner = GetComponentInParent<Actor>();
        cooldownLeft = 0;
    }

    protected void Update()
    {
        if (cooldownLeft > 0) 
            cooldownLeft -= Time.deltaTime;
    }
}