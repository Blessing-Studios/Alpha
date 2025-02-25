using UnityEngine;

public class ApplicationQuitHandler : MonoBehaviour
{
    void OnApplicationQuit()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}