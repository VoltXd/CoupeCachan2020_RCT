using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtendedSerialPort;
using Utilities;

namespace RobotPWF2
{
    public static class Message
    {
        public static event EventHandler<MessageReadyToSendEventArgs> msgReady;

        /// <summary>
        /// Retourne le checksum d'une trame
        /// </summary>
        /// <param name="msgFunction">Code de la commande</param>
        /// <param name="msgPayloadLength">Taille des données utiles</param>
        /// <param name="msgPayload">Données</param>
        /// <returns></returns>
        public static byte CalculateChecksum(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte checksum;
            checksum = 0xFE;
            checksum ^= (byte)msgFunction;
            checksum ^= (byte)(msgFunction >> 8);
            checksum ^= (byte)msgPayloadLength;
            checksum ^= (byte)(msgPayloadLength >> 8);
            for (int i = 0; i < msgPayloadLength; i++)
                checksum ^= msgPayload[i];
            return checksum;
        }

        /// <summary>
        /// Retourne une trame encodé avec un checksum
        /// </summary>
        /// <param name="msgFunction">Code de la commande</param>
        /// <param name="msgPayloadLength">Taille des données utiles</param>
        /// <param name="msgPayload">Donnéesutiles</param>
        /// <returns></returns>
        public static void UartEncodeMessage(int msgFunction, int msgPayloadLength, byte [] msgPayload)
        {
            byte[] message = new byte[msgPayloadLength + 6];
            int index = 0;
            message[index++] = 0xFE;
            message[index++] = (byte)(msgFunction >> 8);
            message[index++] = (byte)msgFunction;
            message[index++] = (byte)(msgPayloadLength >> 8);
            message[index++] = (byte)msgPayloadLength;
            for (int i = 0; i < msgPayloadLength; i++)
                message[index++] = msgPayload[i];
            message[index++] = CalculateChecksum(msgFunction, msgPayloadLength, msgPayload);

            if (msgReady != null)
                msgReady(new object(), new MessageReadyToSendEventArgs { msg = message });
        }

        /// <summary>
        /// Encode une trame dedié aux consignes PWM, avec prise en compte des vitesses négatives
        /// </summary>
        /// <param name="consigneGauche">Pwm gauche</param>
        /// <param name="consigneDroite">Pwm droite</param>
        /// <returns></returns>
        public static void UartEncodeSpeedMsg(double consigneGauche, double consigneDroite)
        {
            //byte cGauche = consigneGauche >= 0 ? (byte)consigneGauche : (byte)(-consigneGauche + 0x80);
            //byte cDroite = consigneDroite >= 0 ? (byte)consigneDroite : (byte)(-consigneDroite + 0x80);

            byte[] msg = new byte[8];

            for (int i = 0; i < 4; i++)
            {
                msg[3 - i] = (byte)((int)(consigneGauche * 1000) >> (i * 8));
                msg[7 - i] = (byte)((int)(consigneDroite * 1000) >> (i * 8));
            }

            UartEncodeMessage((int)CommandID.SetPWMSpeed, msg.Length, msg);            
        }

        public static void UartEncodeSpeedPolarMsg(float vitesseAngulaire, float vitesseLineaire)
        {
            byte[] arrayLinear = vitesseLineaire.GetBytes();
            byte[] arrayAngular = vitesseAngulaire.GetBytes();

            byte[] msg = new byte[8];

            for(int i = 0; i < 4; i++)
            {
                msg[i] = arrayLinear[i];
                msg[i + 4] = arrayAngular[i];
            }

            UartEncodeMessage((int)CommandID.ConsigneData, msg.Length, msg);
        }
    }

    public class MessageReadyToSendEventArgs : EventArgs
    {
        public byte[] msg { get; set; }
    }

    public enum CommandID
    {
        Text = 0x0080,
        Jack = 0x0010,
        Led = 0x0020,
        Ir = 0x0030,
        PWM = 0x0040,
        State = 0x0050,
        SetRobotState = 0x0051,
        SetRobotAutoControl = 0x0052,
        SetPWMSpeed = 0x0053,
        PositionData = 0x0061,
        ConsigneData = 0x0062
    }
}
