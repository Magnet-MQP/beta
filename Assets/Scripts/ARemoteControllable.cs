using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface implemented by objects that are controllable by remote action
/// </summary>

public abstract class ARemoteControllable : MonoBehaviour
{
    public abstract void RemoteActivate(IRemoteController controller);
}
