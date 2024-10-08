using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveController : MonoBehaviour, IMovable
{
    public float MovementSpeed => _movementSpeed;
    private float _movementSpeed;

    void Start()
    {
        _movementSpeed = GetComponent<Actor>().Stats.MovementSpeed;
    }

    public void SetSpeed (float speed)
    {
        _movementSpeed = speed;
    }

    public void Move(Vector2 direction)
    {
        transform.Translate(_movementSpeed * Time.deltaTime * direction);
    }
}
