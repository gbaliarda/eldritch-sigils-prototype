using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Actor
{
    [SerializeField] protected LayerMask _targetLayer;
    [SerializeField] protected float _sightRange, _attackRange;

    [SerializeField] private GameObject _drop;
    [SerializeField] private Sprite _dropSprite;

    protected MoveController moveController;
    protected AttackController attackController;
    private bool _targetInSight, _targetInAttack;

    private Animator _animator;
    [SerializeField] private float _disappearDelay = 2f; // Time to wait before destroying after disappearing

    public float SightRange => _sightRange;

    [SerializeField] private string onHitSound = "EnemyHit";

    protected override void Awake()
    {
        moveController = GetComponent<MoveController>();
        _animator = GetComponent<Animator>();

        if (moveController != null) moveController.SetSpeed(stats.MovementSpeed);
    }

    protected override void Update()
    {
        if (isDead) return;

        base.Update();

        _targetInSight = Physics2D.OverlapCircle(transform.position, _sightRange, _targetLayer);
        _targetInAttack = Physics2D.OverlapCircle(transform.position, _attackRange, _targetLayer);

        
        if (!_targetInSight && !_targetInAttack) Patrol();
        if (_targetInSight && !_targetInAttack) Chase();
        if (_targetInSight && _targetInAttack) Attack();
    }

    protected virtual void Patrol()
    {
    }

    protected virtual void Chase()
    {
        if (Player.Instance.GetComponent<IDamageable>() != null && Player.Instance.GetComponent<IDamageable>().IsDead == true) return;
        if (moveController != null)
        {
            Vector2 direction = (Player.Instance.transform.position - transform.position).normalized;
            moveController.Move(direction);
            /*GetComponent<Rigidbody2D>().MovePosition(transform.position + Stats.MovementSpeed * 5 * Time.deltaTime * (Vector3)direction);*/
            /*moveController.Move(Player.Instance.transform.position);*/
        }
    }

    protected virtual void Attack()
    {
        /*moveController.Move(transform.position);*/
        if (attackController == null) return;
        if (Player.Instance.GetComponent<IDamageable>() != null && Player.Instance.GetComponent<IDamageable>().IsDead == true) return;

        /*attackController.Attack(Player.Instance.transform.position);*/
    }

    public override int TakeDamage(DamageStats damage)
    {
        if (life > 0)
        {
            life -= damage.TotalDamage;
            AudioManager.Instance.PlaySFX(onHitSound);
            if (_animator != null)
            {
                _animator.SetTrigger("Hit");
            }
        }

        if (life <= 0)
            Die();

        return life;
    }


    public override void Die()
    {
        if (isDead) return;

        isDead = true;

        // Trigger death animation
        if (_animator != null)
        {
            _animator.SetBool("Dead", true);
        }

        // Disable components
        if (moveController != null) moveController.enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // Start coroutine to handle disappearing and item dropping
        StartCoroutine(DisappearAndDrop());
    }

    private IEnumerator DisappearAndDrop()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(deathAnimationDuration);

        SetVisibility(false);
        DropItem();

        yield return new WaitForSeconds(_disappearDelay);

        Destroy(gameObject);
    }

    private void SetVisibility(bool isVisible)
    {
        // Disable all renderers on this object and its children
        SetRendererVisibility(this.gameObject, isVisible);
    }

    private void SetRendererVisibility(GameObject obj, bool isVisible)
    {
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isVisible;
        }

        foreach (Transform child in obj.transform)
        {
            SetRendererVisibility(child.gameObject, isVisible);
        }
    }

    private void DropItem()
    {
        int randomNumber = Random.Range(1, 3);
        if (randomNumber == 1 && _drop != null)
        {
            var bullet = Instantiate(_drop, transform.position, Quaternion.identity, transform.parent);
        }
    }
}
