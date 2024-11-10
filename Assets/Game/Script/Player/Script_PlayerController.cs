using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Script_PlayerController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Rigidbody _rigidBody = null;
    [SerializeField] Animator _animator = null;

    state _state = state.normal;

    public enum state { normal, run, fire, reload }

    Coroutine _stateChangeCo = null;

    [Header("Movement")]
    [SerializeField] float _landCheckRadius = 0.25f;
    [SerializeField] float _moveSpeed = 1f;
    [SerializeField] float _maxSpeed = 2f;
    [SerializeField] float _jumpSpeed = 5f;
    [SerializeField] float _jumpCoolDown = 0.25f;
    [SerializeField] float _rotateSpeed = 1f;

    Vector3 _velocity = Vector3.zero;
    Vector3 _velocityBuffer = Vector3.zero;
    Vector3 _eulerBuffer= Vector3.zero;
    Vector2 _axis = Vector2.zero;

    float _jumpCoolDownCount = 0f;

    bool _isGround = false;

    [Header("Weapon")]
    [SerializeField] Transform _rightHand = null;
    [SerializeField] Transform _leftHand = null;
    [SerializeField] float _grabRadius_Weapon = 2f;
    [SerializeField] Script_Weapon _weapon = null;

    [Header("Sound")]
    [SerializeField] AudioSource _audioSource = null;
    [SerializeField] List<AudioClip> _footStepSE = null;

    void Start()
    {
        Init();
    }

    void Update()
    {
        Move();
        MouseButton();
    }

    void Init()
    {
        if(_rigidBody ==  null)
            _rigidBody = GetComponent<Rigidbody>();

        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    void Move()
    {
        _isGround = Physics.CheckSphere(this.transform.position, _landCheckRadius, ReferenceManager.instacne.GetLayer(ReferenceManager.Layer.staticObject).layerShift);

        if (_isGround)
        {
            _velocityBuffer = this.transform.forward;
            _velocityBuffer.y = 0f;
            _velocityBuffer.Normalize();

            _axis.x = Input.GetAxis("Horizontal");
            _axis.y = Input.GetAxis("Vertical");

            if (_axis != Vector2.zero)
            {
                _velocity = Vector3.ClampMagnitude(_velocity + _axis.y * _moveSpeed * _velocityBuffer, _maxSpeed);
                _eulerBuffer.y = _axis.x * Mathf.Rad2Deg * _rotateSpeed;

                _animator.SetBool("IsMove", true);
                _animator.SetFloat("JumpState", 0.33f);

                FootStepSE();
            }
            else
            {
                _velocity.x = 0f;
                _velocity.z = 0f;
                _eulerBuffer.y = 0f;

                _animator.SetBool("IsMove", false);
                _animator.SetFloat("JumpState", 0f);
            }

            CoolDown_Jump();

            _velocity.y = 0f;
        }
        else
        {
            _velocity += Physics.gravity * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && _jumpCoolDownCount >= _jumpCoolDown)
        {
            this.transform.position += Vector3.up * _landCheckRadius;
            _velocity.y = _jumpSpeed;
            _jumpCoolDownCount = 0f;

            _animator.SetTrigger("Jump");
        }

        transform.Rotate(_eulerBuffer);
        transform.position += Time.deltaTime * _velocity;
    }

    void CoolDown_Jump()
    {
        if(_jumpCoolDownCount < _jumpCoolDown)
        {
            _jumpCoolDownCount += Time.deltaTime;
        }
    }

    void MouseButton()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            if (_weapon == null)
                Weapon_Get();
            else
                Weapon_Use();
        }
        else if (Input.GetButton("Fire1"))
        {
            if (_weapon != null)
                Weapon_Use();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Weapon_Discard();
        }
    }

    void Weapon_Get()
    {
        Collider[] weapons = Physics.OverlapSphere(RightHand().position, _grabRadius_Weapon, ReferenceManager.instacne.GetLayer(ReferenceManager.Layer.weapon).layerShift);

        foreach (Collider w in weapons)
        {
            Script_Weapon SC = w.GetComponent<Script_Weapon>();

            if (SC != null)
            {
                _weapon = SC;
                SC.Get();

                StateUpdate(state.run);
            }
        }
    }

    void Weapon_Discard()
    {
        if (_weapon == null) return;

        _weapon.Discard();
        _weapon = null;

        StateUpdate(state.normal);
    }

    void Weapon_Use()
    {
        StateUpdate(state.fire);

        _weapon.Use();
    }

    void StateUpdate(state newState)
    {
        switch(newState)
        {
            case state.normal:
                {
                    CorutineForStateChange(0.15f, 0f, 0f);
                    break;
                }
            case state.run:
                {
                    CorutineForStateChange(1f, 0.33f, 0f);
                    break;
                }
            case state.fire:
                {
                    _animator.SetFloat("IdleState", 0.66f);
                    _animator.SetFloat("MoveState", 0.66f);
                    CorutineForStateChange(1f, 0.33f, 0.15f);
                    break;
                }
            case state.reload:
                {
                    _animator.SetFloat("IdleState", 1f);
                    _animator.SetFloat("MoveState", 1f);
                    CorutineForStateChange(1f, 0.33f, 1f);
                    break;
                }
        }

        _state = newState;
    }

    void CorutineForStateChange(float time, float stateGoal, float delay)
    {
        if (_stateChangeCo != null) StopCoroutine(_stateChangeCo);
        _stateChangeCo = StartCoroutine(GoToIdle(time, stateGoal, delay));
    }

    IEnumerator GoToIdle(float time, float stateGoal, float delay)
    {
        float timeCount = delay;

        while(timeCount > 0)
        {
            yield return null;
            timeCount += -Time.deltaTime;
        }

        timeCount = 0f;

        while (timeCount < time)
        {
            float timeRatio = timeCount / time;

            _animator.SetFloat("IdleState", Mathf.Lerp(_animator.GetFloat("IdleState"), stateGoal, timeRatio));
            _animator.SetFloat("MoveState", Mathf.Lerp(_animator.GetFloat("MoveState"), stateGoal, timeRatio));

            yield return null;

            timeCount += Time.deltaTime;
        }

        _animator.SetFloat("IdleState", stateGoal);
        _animator.SetFloat("MoveState", stateGoal);
    }

    public Transform LeftHand()
    {
        return _leftHand;
    }

    public Transform RightHand()
    {
        return _rightHand;
    }

    void FootStepSE()
    {
        if (_audioSource.isPlaying) return;

        _audioSource.clip = _footStepSE[UnityEngine.Random.Range(0, _footStepSE.Count)];
        _audioSource.Play();
    }
}
