using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Script_Weapon : MonoBehaviour
{
    public virtual void Get()
    {

    }

    public virtual void Discard()
    {

    }

    public virtual void Use()
    {

    }

    public virtual bool CanReload()
    {
        return true;
    }

    public virtual void Reload()
    {

    }

    public virtual void CancelReload()
    {

    }
}
