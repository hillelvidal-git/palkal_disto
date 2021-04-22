using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace WideFieldBL
{
    class DbAdapter
    {
        SqlTools sql = new SqlTools();
        string msg;

        internal bool GetPointPosition(int ptId, out double[] XYZ)
        {
            try
            {
                SqlCommands.Select.PointPosition select = new SqlCommands.Select.PointPosition(this.sql.sqlConn);
                select.Command.Parameters[0].Value = ptId;

                this.sql.sqlConn.Open();
                XYZ = select.GetPosition();
                return true;
            }
            catch (Exception e)
            {
                this.msg = e.Message;
                XYZ = new double[0];
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool GetLevelPointByClass(int levelId, int classId, out DbPoint[] points)
        {
            try
            {
                SqlCommands.Select.PointByClass select = new SqlCommands.Select.PointByClass(this.sql.sqlConn);
                select.Command.Parameters[0].Value = levelId;
                select.Command.Parameters[0].Value = classId;

                this.sql.sqlConn.Open();
                points = select.GetPoints();
                return true;
            }
            catch (Exception e)
            {
                points = new DbPoint[0];
                this.msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool InsertPoint(DbPoint pt, out bool webidExists, out bool nameExists, out string msg)
        {
            webidExists = false; nameExists = false;
            try
            {
                SqlCommands.Insert.Point insert = new SqlCommands.Insert.Point(this.sql.sqlConn);
                insert.Command.Parameters[0].Value = pt.LevelID;
                insert.Command.Parameters[1].Value = pt.FieldID;
                insert.Command.Parameters[2].Value = pt.ClassID;
                insert.Command.Parameters[3].Value = pt.Number;
                insert.Command.Parameters[4].Value = pt.Info;
                insert.Command.Parameters[5].Value = pt.Time;
                insert.Command.Parameters[6].Value = pt.X;
                insert.Command.Parameters[7].Value = pt.Y;
                insert.Command.Parameters[8].Value = pt.Z;
                insert.Command.Parameters[9].Value = pt.StatusID;
                insert.Command.Parameters[10].Value = pt.WebID;

                this.sql.sqlConn.Open();
                pt.ID = (int)insert.Command.ExecuteScalar();
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Unique_WebID")) webidExists = true;
                if (e.Message.Contains("Unique_Name")) nameExists = true;
                msg = e.Message;
                Console.WriteLine("error saving point: " + e.Message);
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool GetLevelPoints(int levelId, out DbPoint[] points)
        {
            try
            {
                SqlCommands.Select.PointByLevelNoSRV select = new SqlCommands.Select.PointByLevelNoSRV(this.sql.sqlConn);
                select.Command.Parameters[0].Value = levelId;

                this.sql.sqlConn.Open();
                points = select.GetPoints();
                return true;
            }
            catch (Exception e)
            {
                points = new DbPoint[0];
                this.msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool GetFieldPoints(int fieldId, out DbPoint[] points)
        {
            try
            {
                SqlCommands.Select.PointByField select = new SqlCommands.Select.PointByField(this.sql.sqlConn);
                select.Command.Parameters[0].Value = fieldId;

                this.sql.sqlConn.Open();
                points = select.GetPoints();
                return true;
            }
            catch (Exception e)
            {
                points = new DbPoint[0];
                this.msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool DeletePoint(int id)
        {
            try
            {
                SqlCommands.Delete.Point delete = new SqlCommands.Delete.Point(this.sql.sqlConn);
                delete.Command.Parameters[0].Value = id;

                this.sql.sqlConn.Open();
                return (delete.Command.ExecuteNonQuery() == 1);
            }
            catch (Exception e)
            {
                this.msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool GetNews(out DbPoint[] news, out string msg)
        {
            try
            {
                SqlCommands.Select.News select = new SqlCommands.Select.News(this.sql.sqlConn);

                this.sql.sqlConn.Open();
                news = select.GetNews();
                msg = "";
                return true;
            }
            catch (Exception e)
            {
                news = new DbPoint[0];
                msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool UpdateUploaded(int pointId, int webId, out string msg)
        {
            try
            {
                SqlCommands.Update.Uploaded update = new SqlCommands.Update.Uploaded(this.sql.sqlConn);
                update.Command.Parameters[0].Value = pointId;
                update.Command.Parameters[1].Value = webId;

                this.sql.sqlConn.Open();
                msg = "";
                return ((int)update.Command.ExecuteNonQuery() == 1);
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool GetLastUpdateCertificate(out DateTime time)
        {
            try
            {
                SqlCommands.Select.LastUpdate select = new SqlCommands.Select.LastUpdate(this.sql.sqlConn);
                this.sql.sqlConn.Open();
                time = select.GetTime();
                return true;
            }
            catch
            {
                time = new DateTime(2000, 1, 1);
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool CertifyUpdate(DateTime newUpdate, out string msg)
        {
            try
            {
                this.sql.sqlConn.Open();
                SqlCommands.Update.Certificate update = new SqlCommands.Update.Certificate(this.sql.sqlConn);
                update.Command.Parameters[0].Value = newUpdate;
                msg = "";
                return (update.Command.ExecuteNonQuery() > 0);
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal string GetErrorMessage()
        {
            return this.msg;
        }

        internal bool InsertInitialCertificate(out DateTime time)
        {
            try
            {
                this.sql.sqlConn.Open();
                SqlCommands.Insert.Certificate insert = new SqlCommands.Insert.Certificate(this.sql.sqlConn);
                insert.Command.Parameters[0].Value = new DateTime(2000, 1, 1);
                time = new DateTime(2000, 1, 1);
                return (insert.Command.ExecuteNonQuery() == 1);
            }
            catch
            {
                time = new DateTime(2000, 1, 1);
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal object[] GetPoint(int id)
        {
            try
            {
                SqlCommands.Select.Point select = new SqlCommands.Select.Point(this.sql.sqlConn);
                select.Command.Parameters[0].Value = id;

                this.sql.sqlConn.Open();
                SqlDataReader sdr = select.Command.ExecuteReader();
                sdr.Read();
                object[] vals = new object[13];
                sdr.GetValues(vals);
                return vals;
            }
            catch (Exception e)
            {
                this.msg = e.Message;
                return new object[0];
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }

        internal bool RenamePoint(int ptid, string newName)
        {
            try
            {
                SqlCommands.Update.PointName update = new SqlCommands.Update.PointName(this.sql.sqlConn);
                update.Command.Parameters[0].Value = ptid;
                update.Command.Parameters[1].Value = newName;

                this.sql.sqlConn.Open();
                return (update.Command.ExecuteNonQuery() == 1);
            }
            catch (Exception e)
            {
                this.msg = e.Message;
                return false;
            }
            finally
            {
                this.sql.sqlConn.Close();
            }
        }
    }
}
