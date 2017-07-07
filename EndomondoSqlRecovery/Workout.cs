using System;

namespace EndomondoSqlRecovery
{
    class Workout
    {
        public Guid Id { get { return id; } }
        public string WorkoutId { get { return workoutId; } }
        public int Sport { get { return sport; } }
        public double Duration { get { return duration; } }

        private readonly Guid id;
        private readonly string workoutId;
        private readonly int sport;
        private readonly double duration;

        public Workout(Guid id, string workoutId, int sport, double duration)
        {
            this.id = id;
            this.workoutId = workoutId;
            this.sport = sport;
            this.duration = duration;
        }
    }
}