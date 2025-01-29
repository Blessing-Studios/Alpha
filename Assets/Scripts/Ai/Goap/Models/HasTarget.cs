using Blessing.Ai.Goap.Sensors;
using UnityEngine;

namespace Blessing.Ai.Goap.Models
{
    public class HasTarget : BaseModel
    {
        public override string Name 
        { 
            get { return "HasTarget"; } 
        }
        public override string SensorType 
        { 
            get { return "ProximitySensor"; } 
        }
        public override void UpdateValue(AiAgent aiAgent)
        {
            ProximitySensor sensor = (ProximitySensor) aiAgent.GetSensor(SensorType);

            sensor.UpdateSensor();

            if (sensor.ClosestEnemy)
            {
                aiAgent.SetTarget(sensor.ClosestEnemy);
                Value = true;
                return;
            }

            aiAgent.SetTarget(null);
            Value = false;
        }
    }
}

