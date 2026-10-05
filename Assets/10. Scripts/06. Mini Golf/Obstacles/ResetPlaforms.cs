using System;
using UnityEngine;

namespace Study3D.MiniGolf.Obstacles
{
    public class ResetPlaforms : MonoBehaviour
    {
        private GolfBall ball;
        
        [SerializeField] private GolfEnding golfEnding;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Ball")
            {
                ball = other.gameObject.GetComponent<GolfBall>();
                ball.ReturnToLasShotPosition();
                if (golfEnding != null)
                {
                    golfEnding.RecordMove();
                }
            }
        }
    }
}