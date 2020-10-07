using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WideFieldBL
{
    class FilesAdapter
    {
        string bedekFolder;
        string LastFieldFile;
        string ComPortFile;

        public FilesAdapter()
        {
            this.bedekFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Bedek";
            this.LastFieldFile = bedekFolder + "\\Logs\\Measurement_Field.txt";
            this.ComPortFile = bedekFolder + "\\Logs\\Measure_ComPort.txt";
        }

        internal string[] LoadLastField()
        {
            string file = this.LastFieldFile;
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(file))
                    return new string[]{
                        sr.ReadLine(),
                        sr.ReadLine(),
                        sr.ReadLine(),
                        sr.ReadLine()};
            }
            return new string[1];
        }

        internal void SaveLastField(int[] att)
        {
            string file = this.LastFieldFile;
            string path = file.Substring(0, file.LastIndexOf("\\"));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (StreamWriter sw = new StreamWriter(file))
            {

                sw.WriteLine(att[0].ToString());
                sw.WriteLine(att[1].ToString());
                sw.WriteLine(att[2].ToString());
                sw.WriteLine(att[3].ToString());
            }
        }

        internal bool UploadNew(DbPoint newOne, out int webId, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfAnyType values = new BedekSurveyWebService.ArrayOfAnyType();

                values.Add(newOne.LevelID);
                values.Add(newOne.FieldID);
                values.Add(newOne.ClassID);
                values.Add(newOne.Number);
                values.Add(newOne.Info);
                values.Add(newOne.Time);
                values.Add(newOne.X);
                values.Add(newOne.Y);
                values.Add(newOne.Z);
                return client.InsertNewMeasurements(values, out webId, out msg);
            }
            catch (Exception e)
            {
                msg = e.Message;
                webId = -1;
                return false;
            }
        }

        internal bool CheckConnection(out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                msg = client.TryIt();
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool GetNewMeasurements(DateTime fromDate, out DbPoint[] news, out string msg)
        {
            string errorMsg = "";
            List<DbPoint> points = new List<DbPoint>();

            try
            {
                BedekSurveyWebService.ArrayOfInt ids;
                BedekSurveyWebService.ArrayOfAnyType s_new;
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                
                //קבל את רשימת החדשות
                if (!client.GetAllNewMeasurements(fromDate, out ids, out msg))
                    throw new Exception("WebService Error Getting IDs:\n" + msg);

                //עבור כל נקודה ברשימה, קבל את פרטיה מהשרת
                foreach (int id in ids)
                {
                    if (!client.GetMeasurement(id, out s_new))
                    {
                        errorMsg += id.ToString() + "; ";
                    }
                    points.Add(new DbPoint(s_new.ToArray()));
                }

                news = points.ToArray();

                if (errorMsg != "")
                {
                    msg = "These points failed to be downloaded: IDs " + errorMsg;
                    return false;
                }
                msg = "";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                news = new DbPoint[0];
                return false;
            }
        }

        internal int GetComPort()
        {
            try
            {
                string line;
                using (StreamReader sr = new StreamReader(this.ComPortFile))
                {
                    line = sr.ReadLine();
                }

                return Convert.ToInt16(line);
            }
            catch
            {
                return 1;
            }
        }

        internal void SaveComPort(int comport)
        {
            string path = this.ComPortFile.Substring(0, this.ComPortFile.LastIndexOf("\\"));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (StreamWriter sw = new StreamWriter(this.ComPortFile))
            {
                sw.WriteLine(comport.ToString());
            }
        }

        internal bool GetServerTime(out DateTime serverTime)
        {
            string msg;
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                serverTime = client.GetServerTime();
                msg = "";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                serverTime = new DateTime(2000, 1, 1);
                return false;
            }
        }

        public bool GetBox(int id, out object[] attributes, out double[][] samples, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfAnyType vals;
                BedekSurveyWebService.ArrayOfDouble[] samps;
                if (!client.GetSurvey(id, out vals, out samps, out msg))
                    throw new Exception(msg);
                attributes = vals.ToArray();
                samples = new double[samps.Length][];
                for (int i = 0; i < samps.Length; i++)
                    samples[i] = samps[i].ToArray();

                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                attributes = new string[0];
                samples = new double[0][];
                return false;
            }
        }

        internal bool UpdateStakout(List<int[]> pts, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfInt[] pts_toserver = new BedekSurveyWebService.ArrayOfInt[pts.Count];
                
                //שמירת הנקודות במערך שיכול להשלח לשרת
                for (int i = 0; i < pts.Count; i++)
                    pts_toserver[i] = new BedekSurveyWebService.ArrayOfInt { pts[i][0], pts[i][1] };

                if (!client.UpdateStakeout(pts_toserver, out msg))
                    throw new Exception(msg);
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }
    }
}
