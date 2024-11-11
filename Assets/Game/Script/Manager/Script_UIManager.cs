using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Script_UIManager : MonoBehaviour
{
    [SerializeField] List<Image> _totalImage = null;
    [SerializeField] float _UIShowSpeed = 1f;

    Color _colorBuffer = Color.white;
    Coroutine _showUICo = null;
    WaitForSeconds _autoHideWfs = new(5f);

    [Header("Weapon")]
    [SerializeField] Image _weaponImage = null;
    [SerializeField] List<Sprite> _weaponSprites_M4 = null;
    [SerializeField] Sprite _emptySprite = null;

    public enum WeaponType { none, M4 }

    public enum WeaponImage { black, yellow, white }

    public void SetWeaponSprite(WeaponType type, WeaponImage imageType)
    {
        switch(type)
        {
            case WeaponType.none: _weaponImage.sprite = _emptySprite; break;
            case WeaponType.M4: _weaponImage.sprite = _weaponSprites_M4[(int)imageType]; break;
        }

        ShowUI(true);
    }

    void ShowUI(bool show)
    {
        if (_showUICo != null) StopCoroutine(_showUICo);
        _showUICo = StartCoroutine(ShowUICorutine(show));
    }

    IEnumerator ShowUICorutine(bool show)
    {
        if (show)
        {
            while (_totalImage[0].color.a < 0.9f)
            {
                for (int i = 0; i < _totalImage.Count; i++)
                {
                    _colorBuffer = _totalImage[i].color;
                    _colorBuffer.a = Mathf.Lerp(_colorBuffer.a, 1f, _UIShowSpeed * 10f * Time.deltaTime);
                    _totalImage[i].color = _colorBuffer;
                }

                yield return null;
            }

            for (int i = 0; i < _totalImage.Count; i++)
            {
                _colorBuffer = _totalImage[i].color;
                _colorBuffer.a = 1f;
                _totalImage[i].color = _colorBuffer;
            }
        }
        else
        {
            while (_totalImage[0].color.a > 0.1f)
            {
                for (int i = 0; i < _totalImage.Count; i++)
                {
                    _colorBuffer = _totalImage[i].color;
                    _colorBuffer.a = Mathf.Lerp(_colorBuffer.a, 0f, _UIShowSpeed * Time.deltaTime);
                    _totalImage[i].color = _colorBuffer;
                }

                yield return null;
            }

            for (int i = 0; i < _totalImage.Count; i++)
            {
                _colorBuffer = _totalImage[i].color;
                _colorBuffer.a = 0f;
                _totalImage[i].color = _colorBuffer;
            }
        }

        yield return _autoHideWfs;

        if (show)
        {
            while (_totalImage[0].color.a > 0.1f)
            {
                for (int i = 0; i < _totalImage.Count; i++)
                {
                    _colorBuffer = _totalImage[i].color;
                    _colorBuffer.a = Mathf.Lerp(_colorBuffer.a, 0f, _UIShowSpeed * Time.deltaTime);
                    _totalImage[i].color = _colorBuffer;
                }

                yield return null;
            }

            for (int i = 0; i < _totalImage.Count; i++)
            {
                _colorBuffer = _totalImage[i].color;
                _colorBuffer.a = 0f;
                _totalImage[i].color = _colorBuffer;
            }
        }
    }

    private void Start()
    {
        ShowUI(false);
    }
}
