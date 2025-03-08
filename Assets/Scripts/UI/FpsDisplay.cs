using TMPro;
using UnityEngine;

namespace Blessing.UI
{
    public class FpsDisplay : MonoBehaviour
    {
        public TextMeshProUGUI fpsDisplay;
        private float pollingTime = 0.5f;
        private float time;
        private int frameCount;
        
        void Awake()
        {
            if (fpsDisplay == null)
                fpsDisplay = GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
            frameCount++;

            if (time >= pollingTime)
            {
                fpsDisplay.text = "FPS " + Mathf.RoundToInt(frameCount / time);

                time = 0;
                frameCount = 0;
            }
        }
    }
}
