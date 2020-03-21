using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LidarProcessor;

namespace RobotPWF2
{
    public class Robot
    {
        public bool jack { get; set; } = false;
        public byte vitesseDroite { get; set; } = 0;
        public byte vitesseGauche { get; set; } = 0;
        public double timestamp { get; set; } = 0;
        public double xPos { get; set; }
        public double yPos { get; set; }
        public double angle { get; set; }
        public double vitesseLineaire { get; set; }
        public double vitesseAngulaire { get; set; }
        public double vitesseLineaireConsigne { get; set; }
        public double vitesseAngulaireConsigne { get; set; }
        public double Kp { get; set; } = 2.0;
        public double Ki { get; set; } = 0;//6.0;
        public double Kd { get; set; } = 0;//5; 
        public double[] xRawArray { get; set; }
        public double[] yRawArray { get; set; }
        public double[] xProcessedArray { get; set; }
        public double[] yProcessedArray { get; set; }
        public List<CoherentObject> coherentObjectList { get; set; }
        public List<CoherentObject> ballList { get; set; }
        public CoherentObject closestBall { get; set; }
    }
}
