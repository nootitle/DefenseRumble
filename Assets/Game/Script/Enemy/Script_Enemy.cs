using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Script_Enemy : MonoBehaviour
{
    public bool spawned = false;

    protected virtual void CheckCollision(Collision collision)
    {

    }

    public virtual void StartMove(Vector3 startPoint, Vector3 direction)
    {

    }

    protected virtual void Move()
    {

    }

    public virtual void StopMove()
    {

    }

    public virtual void Hit()
    {

    }

    protected virtual IEnumerator LifeTime()
    {
        yield return null;
    }
}
