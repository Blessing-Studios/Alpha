using Blessing.Core;
using UnityEngine;

namespace Blessing.services
{
    
    public class CameraHelper : MonoBehaviour
    {
        public GameObject CameraTarget;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GameManager.Singleton.VirtualCamera.LookAt = CameraTarget.transform;
            GameManager.Singleton.VirtualCamera.Target.TrackingTarget = CameraTarget.transform;
        }
    }
}

