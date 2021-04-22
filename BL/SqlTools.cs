using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace WideFieldBL
{
    public enum PointClass
    {
        Srv = 0, Bs = 1, Or = 2, Gen = 3
    }

    public class DbPoint
    {
        public int ID;
        public int FieldID;
        public int LevelID;
        public int ClassID;
        public string Number;
        public string Info;
        public DateTime Time;
        public double X;
        public double Y;
        public double Z;
        public int StatusID;
        public DateTime Modified;
        public int WebID;

        public DbPoint(object[] values)
        {
            if (values[0] != null) this.ID = (int)values[0];
            this.LevelID = (int)values[1];
            this.FieldID = (int)values[2];
            this.ClassID = (int)values[3];
            this.Number = ((string)values[4]).Trim();
            this.Info = ((string)values[5]).Trim();
            this.Time = (DateTime)values[6];
            this.X = (double)values[7];
            this.Y = (double)values[8];
            this.Z = (double)values[9];
            this.StatusID = (int)values[10];
            this.Modified = (DateTime)values[11];
            try
            {
                if (values.Length > 12 && int.TryParse(values[12].ToString(), out int w)) this.WebID = w;
            }
            catch { }
        }

        public string GetAttribution()
        {
            return "ClassID: " + this.ClassID + ",  LevelID: " + this.LevelID + ",  FieldID: " + this.FieldID + ",  Number: " + this.Number;
        }
    }

    class SqlTools
    {
        internal SqlConnection sqlConn;

        public SqlTools()
        {
            this.sqlConn = new SqlConnection(GetLocalMeasureConnection());
        }

        private string GetLocalMeasureConnection()
        {
            string db_source = @".\SQLEXPRESS";
            string db_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Bedek\MeasureData";

            try
            {
                using (StreamReader sr = new StreamReader(db_folder + "\\db_source.dat"))
                {
                    db_source = sr.ReadLine();
                    if (db_source == "") db_source = @".\SQLEXPRESS";
                }
            }
            catch { }

            return @"Data Source=" + db_source +
                @";AttachDbFilename=" + db_folder +
                @"\MeasureDb.mdf;Integrated Security=True;Connect Timeout=30;";

        }
    }

    namespace SqlCommands
    {
        namespace Insert
        {
            class Point
            {
                internal SqlCommand Command;
                public Point(SqlConnection conn)
                {
                    this.Command = new SqlCommand("insert into Points (LevelID, FieldID, ClassID, Number, Info, Time, X,Y,Z, StatusID, WebID, Modified) output INSERTED.ID values (@levelId,@fieldID,@classId,@number,@info,@time,@x,@y,@z, @statusId, @webId, CURRENT_TIMESTAMP)", conn);
                    this.Command.Parameters.Add("levelId", SqlDbType.Int);
                    this.Command.Parameters.Add("fieldId", SqlDbType.Int);
                    this.Command.Parameters.Add("classId", SqlDbType.Int);
                    this.Command.Parameters.Add("number", SqlDbType.NChar);
                    this.Command.Parameters.Add("info", SqlDbType.NChar);
                    this.Command.Parameters.Add("time", SqlDbType.DateTime);
                    this.Command.Parameters.Add("x", SqlDbType.Float);
                    this.Command.Parameters.Add("y", SqlDbType.Float);
                    this.Command.Parameters.Add("z", SqlDbType.Float);
                    this.Command.Parameters.Add("statusId", SqlDbType.Int);
                    this.Command.Parameters.Add("webId", SqlDbType.Int);
                }
            }

            class Certificate
            {
                internal SqlCommand Command;
                public Certificate(SqlConnection conn)
                {
                    this.Command = new SqlCommand("insert into Updates (LastUpdate) values (@newTime)", conn);
                    this.Command.Parameters.Add("newTime", SqlDbType.DateTime);
                }
            }


        }

        namespace Select
        {

            class Point
            {
                internal SqlCommand Command;
                public Point(SqlConnection conn)
                {
                    this.Command = new SqlCommand("select * from Points where ID=@pointId", conn);
                    this.Command.Parameters.Add("pointId", SqlDbType.Int);
                }
            }

            class PointPosition
            {
                internal SqlCommand Command;
                public PointPosition(SqlConnection conn)
                {
                    this.Command = new SqlCommand("select X,Y,Z from Points where ID=@pointId", conn);
                    this.Command.Parameters.Add("pointId", SqlDbType.Int);
                }

                internal double[] GetPosition()
                {
                    SqlDataReader sdr = this.Command.ExecuteReader();
                    if (!sdr.Read()) throw new Exception("DataReader is empty!");

                    return new double[]
                    {
                        (double) sdr["X"],
                        (double) sdr["Y"],
                        (double) sdr["Z"]
                    };
                }
            }

            class News
            {
                internal SqlCommand Command;
                public News(SqlConnection conn)
                {
                    this.Command = new SqlCommand("select * from Points where StatusID=-1", conn);
                }

                internal DbPoint[] GetNews()
                {
                    List<DbPoint> points = new List<DbPoint>();
                    object[] vals = new object[13];

                    SqlDataReader sdr = this.Command.ExecuteReader();
                    while (sdr.Read())
                    {
                        sdr.GetValues(vals);
                        points.Add(new DbPoint(vals));
                    }
                    return points.ToArray();
                }
            }


            class PointByField
            {
                internal SqlCommand Command;
                public PointByField(SqlConnection conn)
                {
                    this.Command = new SqlCommand("select * from Points where FieldID=@fieldId", conn);
                    this.Command.Parameters.Add("fieldId", SqlDbType.Int);
                }

                internal DbPoint[] GetPoints()
                {
                    List<DbPoint> points = new List<DbPoint>();
                    object[] vals = new object[13];

                    SqlDataReader sdr = this.Command.ExecuteReader();
                    while (sdr.Read())
                    {
                        sdr.GetValues(vals);
                        points.Add(new DbPoint(vals));
                    }
                    return points.ToArray();
                }
            }

            class PointByLevelNoSRV
            {
                internal SqlCommand Command;
                public PointByLevelNoSRV(SqlConnection conn)
                {
                    this.Command = new SqlCommand("select * from Points where LevelID=@levelId and FieldID=-1", conn);
                    this.Command.Parameters.Add("levelId", SqlDbType.Int);
                }

                internal DbPoint[] GetPoints()
                {
                    List<DbPoint> points = new List<DbPoint>();
                    object[] vals = new object[13];

                    SqlDataReader sdr = this.Command.ExecuteReader();
                    while (sdr.Read())
                    {
                        sdr.GetValues(vals);
                        points.Add(new DbPoint(vals));
                    }
                    return points.ToArray();
                }
            }

            class PointByClass
            {
                internal SqlCommand Command;
                public PointByClass(SqlConnection conn)
                {
                    this.Command = new SqlCommand("select * from Points where LevelID=@levelId and ClassID=@classId", conn);
                    this.Command.Parameters.Add("levelId", SqlDbType.Int);
                    this.Command.Parameters.Add("classId", SqlDbType.Int);
                }

                internal DbPoint[] GetPoints()
                {
                    List<DbPoint> points = new List<DbPoint>();
                    object[] vals = new object[13];

                    SqlDataReader sdr = this.Command.ExecuteReader();
                    while (sdr.Read())
                    {
                        sdr.GetValues(vals);
                        points.Add(new DbPoint(vals));
                    }
                    return points.ToArray();
                }
            }

            class LastUpdate
            {
                internal SqlCommand Command;
                public LastUpdate(SqlConnection conn)
                {
                    this.Command = new SqlCommand("select LastUpdate from Updates", conn);
                }

                internal DateTime GetTime()
                {
                    return (DateTime)this.Command.ExecuteScalar();
                }
            }


        }

        namespace Delete
        {
            class Point
            {
                internal SqlCommand Command;
                public Point(SqlConnection conn)
                {
                    this.Command = new SqlCommand("delete from Points where ID=@pointId", conn);
                    this.Command.Parameters.Add("pointId", SqlDbType.Int);
                }
            }

        }

        namespace Update
        {
            class Uploaded
            {
                internal SqlCommand Command;
                public Uploaded(SqlConnection conn)
                {
                    this.Command = new SqlCommand("update Points set StatusID=0, WebID=@webId, Modified=CURRENT_TIMESTAMP where ID=@pointId", conn);
                    this.Command.Parameters.Add("pointId", SqlDbType.Int);
                    this.Command.Parameters.Add("webId", SqlDbType.Int);
                }
            }

            class PointName
            {
                internal SqlCommand Command;
                public PointName(SqlConnection conn)
                {
                    this.Command = new SqlCommand("update Points set Number=@newName, Modified=CURRENT_TIMESTAMP where ID=@pointId", conn);
                    this.Command.Parameters.Add("pointId", SqlDbType.Int);
                    this.Command.Parameters.Add("newName", SqlDbType.NChar);
                }
            }


            class Certificate
            {
                internal SqlCommand Command;
                public Certificate(SqlConnection conn)
                {
                    this.Command = new SqlCommand("update Updates set LastUpdate=@serverTime", conn);
                    this.Command.Parameters.Add("serverTime", SqlDbType.DateTime);
                }
            }

        }



    } //namespace SqlCommands
} //namespace BL
