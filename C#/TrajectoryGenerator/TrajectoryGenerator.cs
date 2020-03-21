using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace TrajectoryGenerator
{
    public class TrajectoryGenerator
    {
        //L'expression de la distance de freinage est V(t)^2 / 2a
        private double accelerationLineaire;
        private double decelerationLineaire;
        private double vitesseMaxLineaire;
        private double vitesseLineaireConsigne;
        private double accelerationAngulaire;
        private double decelerationAngulaire;
        private double vitesseMaxAngulaire;
        private double vitesseAngulaireConsigne;
        private double sampleRate;
        public bool trajectoireEnCours = false;
        private TrajectoryState state;
        private PointD destination;

        public TrajectoryGenerator(double accelLin, double decelLin, double vMaxLin, double accelAng, double decelAng, double vMaxAng, double fe)
        {
            accelerationLineaire = accelLin;
            decelerationLineaire = decelLin;
            vitesseMaxLineaire = vMaxLin;
            accelerationAngulaire = accelAng;
            decelerationAngulaire = decelAng;
            vitesseMaxAngulaire = vMaxAng;
            sampleRate = fe;
        }

        public void InitTrajectory(PointD coordonnees)
        {
            destination = coordonnees;
            state = TrajectoryState.Tourne;
            trajectoireEnCours = true;
        }

        /// <summary>
        /// Retourne un tableau contenant dans l'ordre : les consignes angulaires et lineaires
        /// </summary>
        /// <param name="vAngCourant">Vitesse Angulaire actuelle</param>
        /// <param name="vLinCourant">Vitesse Lineaire actuelle</param>
        /// <param name="position">Position Courante</param>
        /// <param name="sampleFrequency">Frequence d'echantillonnage du timer</param>
        /// <returns></returns>
        public float[] GetSpeed(float vAngCourant, float vLinCourant, PointD position)
        {
            //Dans ce tableau, on place dans l'ordre : La vitesse angulaire puis la vitesse lineaire
            double dFreinAng = vAngCourant * vAngCourant / (2 * decelerationAngulaire);
            double dFreinLin = vLinCourant * vLinCourant / (2 * decelerationLineaire);
            double distanceSrcToDest = Math.Sqrt(Math.Pow(destination.X - position.X, 2) + Math.Pow(destination.Y - position.Y, 2));

            switch(state)
            {
                case TrajectoryState.Attente:
                    vitesseAngulaireConsigne = 0;
                    vitesseLineaireConsigne = 0;
                    break;

                case TrajectoryState.Tourne:
                    if (dFreinAng > distanceSrcToDest)
                    {
                        if (vitesseAngulaireConsigne < vitesseMaxAngulaire) 
                            vitesseAngulaireConsigne += accelerationAngulaire / sampleRate;
                        else
                            vitesseAngulaireConsigne = vitesseMaxAngulaire;
                    }
                    else
                    {
                        if (vitesseAngulaireConsigne > decelerationAngulaire / sampleRate)
                            vitesseAngulaireConsigne -= decelerationAngulaire / sampleRate;
                        else
                        {
                            vitesseAngulaireConsigne = 0;
                            state = TrajectoryState.Avance;
                        }
                    }
                    break;

                case TrajectoryState.Avance:
                    if (dFreinLin > distanceSrcToDest)
                    {
                        if (vitesseLineaireConsigne < vitesseMaxLineaire)
                            vitesseLineaireConsigne += accelerationLineaire / sampleRate;
                        else
                            vitesseLineaireConsigne = vitesseMaxLineaire;
                    }
                    else
                    {
                        if (vitesseLineaireConsigne > decelerationLineaire / sampleRate)
                            vitesseLineaireConsigne -= decelerationLineaire / sampleRate;
                        else
                        {
                            vitesseLineaireConsigne = 0;
                            state = TrajectoryState.Attente;
                            trajectoireEnCours = false;
                        }
                    }
                    break;
            }

            float[] vitesseTab = new float[2];
            vitesseTab[0] = (float)vitesseAngulaireConsigne;
            vitesseTab[1] = (float)vitesseLineaireConsigne;
            return vitesseTab;
        }
    }

    public enum TrajectoryState
    {
        Attente,
        Tourne,
        Avance    
    }
}
