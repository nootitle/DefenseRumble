using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Script_Fx_Tracker : MonoBehaviour
{
    [SerializeField] AlignAxis _allignAxis = AlignAxis.Z;
    [SerializeField] ParticleSystem _particleSystem = null;

    public enum AlignAxis { X, Y, Z, XR, YR, ZR }

    Transform _trackingTarget = null;
    float _trackingTime = 0f;

    Coroutine _trackingCo = null;

    public bool IsPlay()
    {
        return _particleSystem.isPlaying;
    }
    
    public void StartTracking(Transform target, float time)
    {
        _trackingTarget = target;
        _trackingTime = time;

        _particleSystem.Play();

        if (_trackingCo != null) StopCoroutine(_trackingCo);
        _trackingCo = StartCoroutine(TrackingCorutine());
    }

    IEnumerator TrackingCorutine()
    {
        while (_trackingTime > 0f)
        {
            this.transform.position = _trackingTarget.position;

            switch(_allignAxis)
            {
                case AlignAxis.X: this.transform.right = _trackingTarget.forward; break;
                case AlignAxis.Y: this.transform.up = _trackingTarget.forward; break;
                case AlignAxis.Z: this.transform.forward = _trackingTarget.forward; break;
                case AlignAxis.XR: this.transform.right = -_trackingTarget.forward; break;
                case AlignAxis.YR: this.transform.up = -_trackingTarget.forward; break;
                case AlignAxis.ZR: this.transform.forward = -_trackingTarget.forward; break;
                default: this.transform.forward = _trackingTarget.forward; break;
            }

            yield return null;

            _trackingTime += -Time.deltaTime;
        }
    }
}
