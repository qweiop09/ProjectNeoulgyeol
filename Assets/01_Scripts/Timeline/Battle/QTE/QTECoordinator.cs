using System;
using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
public class QTECoordinator : Singleton<QTECoordinator>
{
    public Action<QTEHitInfo> OnQTEMarkerReceived;
}
}
