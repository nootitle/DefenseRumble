using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_Enemy_Box : Script_Enemy
{
    [SerializeField] Rigidbody _rigidBody = null;

    [SerializeField] float _moveSpeed = 1f;
    [SerializeField] float _rotateSpeed = 5f;
    [SerializeField] float _lifeTime = 5f;

    Coroutine _lifeTimeCorutine = null;

    protected override void CheckCollision(Collision collision)
    {
        if(collision.gameObject.layer == Script_ReferenceHub.instacne.GetLayer(Script_ReferenceHub.Layer.player).layerInt)
        {
            Script_ReferenceHub.instacne.GetPlayer(Script_ReferenceHub.Player.local).Hit();
            Hit();
        }
    }

    public override void StartMove(Vector3 startPoint, Vector3 direction)
    {
        _rigidBody.isKinematic = false;

        _rigidBody.MovePosition(startPoint);
        _rigidBody.AddForce(_moveSpeed * direction, ForceMode.Impulse);
        _rigidBody.AddTorque(_rotateSpeed * direction, ForceMode.Impulse);

        if (_lifeTimeCorutine != null) StopCoroutine(_lifeTimeCorutine);
        _lifeTimeCorutine = StartCoroutine(LifeTime());

        spawned = true;
    }

    public override void StopMove()
    {
        _rigidBody.isKinematic = true;

    }

    public override void Hit()
    {
        Script_ReferenceHub.instacne.PlayEffect(Script_ReferenceHub.Fx.hit, this.transform);
        StopMove();

        spawned = false;

        this.gameObject.SetActive(false);
    }

    protected override IEnumerator LifeTime()
    {
        float timeCount = 0f;

        while(timeCount < _lifeTime)
        {
            yield return null;
            timeCount += Time.deltaTime;
        }

        Hit();
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision);
    }
}
