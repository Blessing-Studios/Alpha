using Blessing.Services;
using TMPro;
using UnityEngine;

namespace Blessing.UI
{
    public class SessionNameDisplay : MonoBehaviour
    {
        public TextMeshProUGUI sessionNameDisplay;
        private float pollingTime = 0.5f;
        private float time;
        
        void Awake()
        {
            if (sessionNameDisplay == null)
                sessionNameDisplay = GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            time += Time.deltaTime;

            if (ServicesHelper.Singleton.CurrentSession != null && time >= pollingTime)
            {
                sessionNameDisplay.text = "Session Name: " + ServicesHelper.Singleton.CurrentSession.Name;

                time = 0;
            }
        }
    }
}
