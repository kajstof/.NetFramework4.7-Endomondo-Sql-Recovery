using System;

namespace EndomondoSqlRecovery
{
    class Track
    {
        public int Id { get { return id; } }
        public Guid WorkoutId { get { return workoutId; } }
        public DateTime Timestamp { get { return timestamp; } }
        public int Instruction { get { return instruction; } }
        public double Latitude { get { return latitude; } }
        public double Longitude { get { return longitude; } }
        public double Distance { get { return distance; } }
        public double Speed { get { return speed; } }
        public double Altitude { get { return altitude; } }
        public bool SentToServer { get { return sentToServer; } }

        private readonly int id;
        private readonly Guid workoutId;
        private readonly DateTime timestamp;
        private readonly int instruction;
        private readonly double latitude;
        private readonly double longitude;
        private readonly double distance;
        private readonly double speed;
        private readonly double altitude;
        private readonly bool sentToServer;

        public Track(int id, Guid workoutId, DateTime timestamp, int instruction, double latitude, double longitude, double distance, double speed, double altitude, bool sentToServer)
        {
            this.id = id;
            this.workoutId = workoutId;
            this.timestamp = timestamp;
            this.instruction = instruction;
            this.latitude = latitude;
            this.longitude = longitude;
            this.distance = distance;
            this.speed = speed;
            this.altitude = altitude;
            this.sentToServer = sentToServer;
        }
    }
}