using UnityEngine;

public class HitTimeTest : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log(gameObject.name + ": Time of the animation Start- " + Time.time);
    }
}
