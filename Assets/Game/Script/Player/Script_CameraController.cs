using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_CameraController : MonoBehaviour
{
    [SerializeField] Vector3 _zoomInLocalPosition = Vector3.zero;
    [SerializeField] Vector3 _zoomOutLocalPosition = Vector3.zero;
    [SerializeField] float _zoomTime = 1f;

    bool _zoomin = false;

    Coroutine _zoomCo = null;

    public void Zoom(bool zoomin)
    {
        if (_zoomin == zoomin) return;

        _zoomin = zoomin;

            if (_zoomCo != null) StopCoroutine(_zoomCo);
        _zoomCo = StartCoroutine(ZoomCorutine(zoomin));
    }

    IEnumerator ZoomCorutine(bool zoomIn)
    {
        float timeCount = 0f;

        if(zoomIn)
        {
            while(timeCount < _zoomTime)
            {
                this.transform.localPosition = Vector3.Lerp(_zoomOutLocalPosition, _zoomInLocalPosition, timeCount / _zoomTime);

                yield return null;

                timeCount += Time.deltaTime;
            }
        }
        else
        {
            while (timeCount < _zoomTime)
            {
                this.transform.localPosition = Vector3.Lerp(_zoomInLocalPosition, _zoomOutLocalPosition, timeCount / _zoomTime);

                yield return null;

                timeCount += Time.deltaTime;
            }
        }
    }
}
