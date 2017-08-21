using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Xml;

namespace EndomondoSqlRecovery
{
    class Program
    {
        private const string document = "test.xml";

        private static void Main(string[] args)
        {
            SqlCeConnection sqlCeCon = new SqlCeConnection("Data Source=\\endo.sdf");

            try
            {
                sqlCeCon.Open();
                Console.WriteLine("Connection is open");

                SqlCeCommand cmd = new SqlCeCommand();
                cmd.Connection = sqlCeCon;
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.CommandText = "SELECT * FROM Workout;";
                SqlCeResultSet rs = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);

                List<Workout> workoutList = new List<Workout>();
                if (rs.HasRows)
                {
                    int ordId = rs.GetOrdinal("Id");
                    int ordWorkoutId = rs.GetOrdinal("WorkoutId");
                    int ordSport = rs.GetOrdinal("Sport");
                    int ordDuration = rs.GetOrdinal("Duration");

                    while (rs.Read())
                    {
                        Guid id = rs.GetGuid(ordId);
                        string workoutId = rs.GetString(ordWorkoutId);
                        int sport = rs.GetInt32(ordSport);
                        double duration = rs.GetDouble(ordDuration);

                        workoutList.Add(new Workout(id, workoutId, sport, duration));
                    }
                }

                int counter = 1;
                foreach (Workout workout in workoutList)
                {
                    cmd.CommandText = $"SELECT * FROM Track WHERE Track.WorkoutId='{workout.Id}';";
                    //Console.WriteLine(cmd.CommandText);

                    rs = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);

                    List<Track> trackList = new List<Track>();
                    if (rs.HasRows)
                    {
                        int ordId = rs.GetOrdinal("Id");
                        int ordWorkoutId = rs.GetOrdinal("WorkoutId");
                        int ordTimestamp = rs.GetOrdinal("Timestamp");
                        int ordInstruction = rs.GetOrdinal("Instruction");
                        int ordLatitude = rs.GetOrdinal("Latitude");
                        int ordLongitude = rs.GetOrdinal("Longitude");
                        int ordDistance = rs.GetOrdinal("Distance");
                        int ordSpeed = rs.GetOrdinal("Speed");
                        int ordAltitude = rs.GetOrdinal("Altitude");
                        int ordSentToServer = rs.GetOrdinal("SentToServer");

                        while (rs.Read())
                        {
                            int id = rs.GetInt32(ordId);
                            Guid workoutId = rs.GetGuid(ordWorkoutId);
                            DateTime timestamp = rs.GetDateTime(ordTimestamp);
                            timestamp = timestamp.Subtract(new TimeSpan(2, 0, 0));

                            int instruction = rs.IsDBNull(ordInstruction) ? -1 : rs.GetInt32(ordInstruction);
                            double latitude = rs.GetDouble(ordLatitude);
                            double longitude = rs.GetDouble(ordLongitude);
                            double distance = rs.GetDouble(ordDistance);
                            double speed = rs.GetDouble(ordSpeed);
                            double altitude = rs.GetDouble(ordAltitude);
                            bool sentToServer = rs.GetBoolean(ordSentToServer);

                            trackList.Add(new Track(id, workoutId, timestamp, instruction, latitude, longitude, distance, speed, altitude, sentToServer));
                        }

                        string fileName;

                        fileName = String.Format("Endo_{0}_tcx.tcx", counter);
                        CreateXmlTcx(fileName, workout, trackList);
                        fileName = String.Format("Endo_{0}_gpx.gpx", counter);
                        CreateXmlGpx(fileName, workout, trackList);
                    }

                    counter++;
                }

                sqlCeCon.Close();
                Console.WriteLine("Connection is closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Source + " - " + ex.Message);
            }

            //Console.ReadKey();
        }

        private static void CreateXmlTcx(string fileName, Workout workout, List<Track> trackList)
        {
            string sportString;

            switch (workout.Sport)
            {
                case 0:
                    sportString = "Running";
                    break;

                case 1:
                    sportString = "Biking";
                    break;

                case 2:
                    goto case 1;
                default:
                    sportString = "Other";
                    break;
            }

            XmlWriter writer;
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            writer = XmlWriter.Create(fileName, xmlSettings);

            writer.WriteComment("Recovered data from Microsoft SQL CE database file: endo.sdf");

            // Write an element (root).
            writer.WriteStartElement("TrainingCenterDatabase");

            //writer.WriteAttributeString("xmlns", "bk", null, "urn:samples");
            //writer.WriteAttributeString("xmlns", "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2");
            //XmlDocument x = new XmlDocument();
            //XmlAttribute xa = x.CreateAttribute( ??? );

            // Write the namespace declaration.
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd");

            // Write the title.
            writer.WriteStartElement("Activities");
            writer.WriteStartElement("Activity");
            writer.WriteAttributeString("Sport", sportString);

            writer.WriteElementString("Id", trackList[trackList.Count - 1].Timestamp.ToString("u").Replace(" ", "T"));
            writer.WriteStartElement("Lap");
            writer.WriteAttributeString("StartTime", trackList[0].Timestamp.ToString("u").Replace(" ", "T"));
            writer.WriteElementString("TotalTimeSecond", workout.Duration.ToString());
            writer.WriteElementString("DistanceMeters", (trackList[trackList.Count - 1].Distance * 1000).ToString());
            writer.WriteElementString("Intensity", "Active");
            writer.WriteElementString("TriggerMethod", "Manual");
            writer.WriteStartElement("Track");
            foreach (Track track in trackList)
            {
                writer.WriteStartElement("Trackpoint");
                writer.WriteElementString("Time", track.Timestamp.ToString("u").Replace(" ", "T"));
                writer.WriteStartElement("Position");
                writer.WriteElementString("LatitudeDegrees", track.Latitude.ToString());
                writer.WriteElementString("LongitudeDegrees", track.Longitude.ToString());
                writer.WriteEndElement();
                writer.WriteElementString("AltitudeMeters", track.Altitude.ToString("0.0"));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Write the close tag for the root element.
            writer.WriteEndElement();

            // Write the XML to file and close the writer.
            writer.Flush();
            writer.Close();
        }

        private static void CreateXmlGpx(string fileName, Workout workout, List<Track> trackList)
        {
            string sportString;

            switch (workout.Sport)
            {
                case 0:
                    sportString = "RUNNING";
                    break;

                case 1:
                    sportString = "CYCLING_SPORT";
                    break;

                case 2:
                    sportString = "CYCLING_TRANSPORTATION";
                    break;

                default:
                    sportString = "OTHER";
                    break;
            }

            XmlWriter writer;
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            writer = XmlWriter.Create(fileName, xmlSettings);

            writer.WriteComment("Recovered data from Microsoft SQL CE database file: endo.sdf");

            // Write an element (root).
            writer.WriteStartElement("gpx");
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("creator", "Endomondo.com");
            writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd");
            writer.WriteAttributeString("xmlns", "gpxtpx", null, "http://www.garmin.com/xmlschemas/TrackPointExtension/v1");
            writer.WriteAttributeString("xmlns", "gpxx", null, "http://www.garmin.com/xmlschemas/GpxExtensions/v3");
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

            // Write the title.
            writer.WriteStartElement("metadata");
            writer.WriteStartElement("author");
            writer.WriteElementString("name", "Krzysztof Krysiak");
            writer.WriteStartElement("email");
            writer.WriteAttributeString("id", "krysiak86");
            writer.WriteAttributeString("domain", "gmail.com");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteStartElement("link");
            writer.WriteAttributeString("href", "http://www.endomondo.com");
            writer.WriteElementString("text", "Endomondo");
            writer.WriteEndElement();
            writer.WriteElementString("time", DateTime.Now.ToString("u").Replace(" ", "T"));
            writer.WriteStartElement("bounds");
            writer.WriteAttributeString("minlat", getMinLatitude(trackList).ToString());
            writer.WriteAttributeString("minlon", getMinLongitude(trackList).ToString());
            writer.WriteAttributeString("maxlat", getMaxLatitude(trackList).ToString());
            writer.WriteAttributeString("maxlon", getMaxLongitude(trackList).ToString());
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteStartElement("trk");
            writer.WriteElementString("src", "http://www.endomondo.com/");
            writer.WriteElementString("type", sportString);
            // trkseg

            // /trkseg
            writer.WriteEndElement();

            // Write the close tag for the root element.
            writer.WriteEndElement();

            // Write the XML to file and close the writer.
            writer.Flush();
            writer.Close();
        }

        private static double getMinLatitude(List<Track> trackList)
        {
            double minLatitude = double.MaxValue;

            foreach (Track track in trackList)
            {
                if (minLatitude > track.Latitude)
                {
                    minLatitude = track.Latitude;
                }
            }

            return minLatitude;
        }

        private static double getMaxLatitude(List<Track> trackList)
        {
            double maxLatitude = double.MinValue;

            foreach (Track track in trackList)
            {
                if (maxLatitude < track.Latitude)
                {
                    maxLatitude = track.Latitude;
                }
            }

            return maxLatitude;
        }

        private static double getMinLongitude(List<Track> trackList)
        {
            double minLongitude = double.MaxValue;

            foreach (Track track in trackList)
            {
                if (minLongitude > track.Longitude)
                {
                    minLongitude = track.Longitude;
                }
            }

            return minLongitude;
        }

        private static double getMaxLongitude(List<Track> trackList)
        {
            double maxLongitude = double.MinValue;

            foreach (Track track in trackList)
            {
                if (maxLongitude < track.Longitude)
                {
                    maxLongitude = track.Longitude;
                }
            }

            return maxLongitude;
        }
    }
}