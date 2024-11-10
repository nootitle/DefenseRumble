using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager instacne = null;

    [Header("Player")]
    [SerializeField] Script_PlayerController _localPlayer = null;
    [SerializeField] List<Script_PlayerController> _remotePlayers = new();
    [SerializeField] Transform _aimTranform = null;

    public enum Player { local, remote }

    [Header("Weapon")]
    [SerializeField] List<Script_Weapon> _weapons = null;
    [SerializeField] List<Script_Fx_Tracker> _muzzleEffect = null;
    [SerializeField] List<Script_Fx_Tracker> _bulletHoleEffect = null;

    public enum Fx { normal, muzzleEffect, BulletHoleSmoke }

    [Header("Layer")]
    [SerializeField] List<layerSetting> _layerSettings = null;

    [Serializable]
    public class layerSetting
    {
        public Layer type;
        public int layerInt;
        public int layerShift;

        public void Init()
        {
            layerShift = 1 << layerInt;
        }
    }

    public enum Layer { player, weapon, staticObject }

    public Script_PlayerController GetPlayer(Player type, int index = 0)
    {
        switch (type)
        {
            case Player.local: return _localPlayer;
            case Player.remote: return _remotePlayers[index];
            default: return null;
        }
    }

    public void PlayEffect(Fx type, Transform destinationTransform)
    {
        switch (type)
        {
            case Fx.normal: break;
            case Fx.muzzleEffect:
                {
                    for(int i = 0; i < _muzzleEffect.Count; ++i)
                    {
                        if (!_muzzleEffect[i].IsPlay())
                        {
                            _muzzleEffect[i].StartTracking(destinationTransform, 0.2f);
                            break;
                        }
                    }

                    break;
                }
            case Fx.BulletHoleSmoke:
                {
                    for (int i = 0; i < _bulletHoleEffect.Count; ++i)
                    {
                        if (!_bulletHoleEffect[i].IsPlay())
                        {
                            _bulletHoleEffect[i].Play(destinationTransform.position, destinationTransform.forward);
                            break;
                        }
                    }

                    break;
                }
        }
    }

    public void PlayEffect(Fx type, Vector3 position, Vector3 forward)
    {
        switch (type)
        {
            case Fx.normal: break;
            case Fx.muzzleEffect:
                {
                    break;
                }
            case Fx.BulletHoleSmoke:
                {
                    for (int i = 0; i < _bulletHoleEffect.Count; ++i)
                    {
                        if (!_bulletHoleEffect[i].IsPlay())
                        {
                            _bulletHoleEffect[i].Play(position, forward);
                            break;
                        }
                    }

                    break;
                }
        }
    }

    public layerSetting GetLayer(int index)
    {
        return _layerSettings[index];
    }

    public layerSetting GetLayer(Layer type)
    {
        return _layerSettings[(int)type];
    }

    public Transform GetAim()
    {
        return _aimTranform;
    }

    void InitLayerSetting()
    {
        foreach (layerSetting l in _layerSettings)
            l.Init();
    }

    private void Awake()
    {
        if (instacne != null)
        {
            Destroy(this.gameObject);
            return;
        }
        else
            instacne = this;

        InitLayerSetting();
    }
}
