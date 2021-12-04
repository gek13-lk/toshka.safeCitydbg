using System.Drawing;

namespace Toshka.dbgSave.Model
{
    public class Camera : BaseEntity
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsCamera { get; set; } //camera or people send notification

        public string Deg { get; set; } //угол
        public string Type { get; set; }// events on camera red/yellow/green
        public string PeopleDeg { get; set; } 
        public string Subtype { get; set; }// Тип обнаруженного
        public string Settings { get; set; }// Настройки камеры: исключить типы обнаружаемого
        public bool Markup { get; set; } // отображать разметку на камере или нет

        public double focalLength { get; set; } // фокусное расстояние

        public double imageSensorSize { get; set; } // фокусное расстояние
    }
}