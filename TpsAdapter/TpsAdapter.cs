using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GeoComTypes;

namespace TpsAdapter
{
    public partial class TpsAdapter
    {
        public bool GeocomInitialized;
        private short nLastResponse;
        private TMC_STATION CurrentStation;
        //משתנים ששומרים את גודל חלון החיפוש
        private double SearchRangeHz = 0.06;
        private double SearchRangeV = 0.06;

        bool IsRunning;
        bool IsConnected;
        Thread t;
        public List<Action> commandList;
        const int KEEPALIVE_INTERVAL_MS = 1700;

        public Action<short> actBattery;
        public Action actAlive;

        public TpsAdapter()
        {
            //אתחול התקשורת
            commandList = new List<Action>();
            Console.WriteLine("dll path: " + WrapperPath);
            nLastResponse = hv_COM_Init();
            short wait = 2; //2 seconds for communication timeout
            hv_COM_SetTimeOut(out wait);
            if (nLastResponse == 0) GeocomInitialized = true;

        }

        ~TpsAdapter()
        {
            IsRunning = false;
            //Turn pointer off
            hv_EDM_Laserpointer(ON_OFF_TYPE.OFF);
            //Close connection
            this.cmdDisconnectTps();
            //End communication
            hv_COM_End();
        }

        public void Run()
        {
            IsRunning = true;
            commandList = new List<Action>();
            t = new Thread(Loop);
            t.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            hv_COM_End();
        }

        public void Loop()
        {
            Stopwatch swKeepAlive = new Stopwatch();
            swKeepAlive.Start();

            while (IsRunning & IsConnected)
            {
                foreach (var command in commandList.ToList())
                {
                    try
                    {
                        command.Invoke();
                        Thread.Sleep(250);
                    }
                    catch { }
                    commandList.Remove(command);
                }

                if (swKeepAlive.ElapsedMilliseconds > KEEPALIVE_INTERVAL_MS)
                {
                    swKeepAlive.Reset(); swKeepAlive.Start();
                    KeepAlive();
                    swKeepAlive.Reset(); swKeepAlive.Start();
                }

                Thread.Sleep(1);
            }
            Disconnect();
        }


        private void Disconnect()
        {

        }

        private void KeepAlive()
        {
            GetBatteryPower(out short voltage); //מצב הסוללה)             
            string json = @"{cmd : 'battery', val: '" + voltage.ToString() + "', res: " + nLastResponse + ", point: [] }";            
            if (voltage > 0)
            {
                actReturned?.Invoke(json); 
                actAlive?.Invoke();
                cmdCheckTilt();
            }
        }

        public bool Connect(int Port, int Baudrate, int Retries)
        {
            //נסיון לפתיחת החיבור עם הדיסטומט
            short to = 1000;
            hv_COM_End();
            nLastResponse = hv_COM_Init();
            hv_COM_SetTimeOut(out to);
            COM_PORT ePort = (COM_PORT)(Port - 1);
            nLastResponse = hv_COM_OpenConnection(ePort, (COM_BAUD_RATE.COM_BAUD_115200), (short)Retries);

            IsConnected = (nLastResponse == 0);
            return IsConnected;
        }

        

        public bool RedLaserSwitch(bool on)
        {
            ON_OFF_TYPE state;
            if (on) state = ON_OFF_TYPE.ON;
            else state = ON_OFF_TYPE.OFF;

            nLastResponse = hv_EDM_Laserpointer(state);
            return (nLastResponse == 0);
        }

        public bool DoMeasure(bool HighAccuracy, bool UsePrism, out double[] Point3d, out DateTime PointTime)
        {

            double dummy1, dummy2, dummy3;

            //זמני - אין דרך טובה למדידה מדויקת יותר
            HighAccuracy = false; //TODO!...

            //set mode
            if (UsePrism)
            {
                //מדידה באמצעות פריזמה
                if (HighAccuracy)
                    nLastResponse = hv_TMC_SetEdmMode(EDM_MODE.EDM_AVERAGE_IR);
                else
                    nLastResponse = hv_TMC_SetEdmMode(EDM_MODE.EDM_SINGLE_STANDARD);
            }
            else
            {
                //מדידה באמצעות לייזר אדום
                if (HighAccuracy)
                    nLastResponse = hv_TMC_SetEdmMode(EDM_MODE.EDM_AVERAGE_SR);
                else
                    nLastResponse = hv_TMC_SetEdmMode(EDM_MODE.EDM_SINGLE_SRANGE);
            }


            //perform a single distance measurement
            nLastResponse = hv_TMC_DoMeasure(TMC_MEASURE_PRG.TMC_DEF_DIST, TMC_INCLINE_PRG.TMC_AUTO_INC);
            if (nLastResponse != 0) throw new Exception("DoMeasure Error: " + nLastResponse.ToString());
            nLastResponse = hv_TMC_GetSimpleCoord(5000, out dummy1, out dummy2, out dummy3, TMC_INCLINE_PRG.TMC_AUTO_INC);
            // to complete the measurement, and clear data
            hv_TMC_DoMeasure(TMC_MEASURE_PRG.TMC_CLEAR, TMC_INCLINE_PRG.TMC_AUTO_INC);
            //שמור את נתוני המדידה
            Point3d = new double[3];
            Point3d[0] = PASS_GetX();
            Point3d[1] = PASS_GetY();
            Point3d[2] = PASS_GetZ();

            PointTime = new DateTime(2000, 1, 1);

            return (nLastResponse == 0);
        }


        public bool AutoTargetSwitch(bool on)
        {
            int Retries = 1; //מספר פעמים שיש לחכות למציאת המטרה
            //כרגע האופציה מושבתת, ומנסים רק פעם אחת, ללא לולאה כלל

            if (on)
            { //הפעלת חיפוש הפריזמה
                nLastResponse = hv_AUS_SetUserLockState(ON_OFF_TYPE.ON);
                //for (int i = 0; i < Retries; i++)
                //{
                nLastResponse = hv_AUT_FineAdjust(this.SearchRangeHz, this.SearchRangeV, BOOLE.FALSE); //חיפוש הפריזמה והתמקדות במרכזה
                //אם אין בעיה, מסיימים
                //אם ישנה בעיה, גם אז מפסיקים מיד
                //הלולאה נמשכת רק במקרה הספציפי שנגמר הזמן
                //וגם אז כמובן יש מספר ניסיונות קבוע מראש שלאחריו מסיימים בכל מקרה
                //if (nLastResponse != 3077)
                //    break; 
                //}
            }
            else
                nLastResponse = hv_AUS_SetUserLockState(ON_OFF_TYPE.OFF);
            return (nLastResponse == 0);
        }

        public bool LockOnPrism(bool on)
        {
            if (on)
            {
                nLastResponse = hv_AUT_LockIn(); //נסה לנעול את המכשיר על הפריזמה
                if (nLastResponse == 0) hv_BMM_BeepAlarm(); //השמע פצפוף לאות שהמכשיר ננעל
            }
            else
                nLastResponse = hv_AUT_SetLockStatus(ON_OFF_TYPE.OFF);
            return (nLastResponse == 0);
        }

        /// <summary>
        /// Makes the Tps pointing at a given point.
        /// </summary>
        /// <param name="PointCoordinates"></param>
        /// <returns></returns>
        public bool PointAt(double Rel_HZ_Angle, double Rel_V_Angle)
        {
            //בצע הצבעה לנקודה
            short nTimeOutMs = 4500; //קביעת זמן תגובה ארוך על מנת לאפשר את הנסיעה
            hv_COM_SetTimeOut(out nTimeOutMs);
            short ret = hv_AUT_MakePositioning(Rel_HZ_Angle, Rel_V_Angle, AUT_POSMODE.AUT_NORMAL, AUT_ATRMODE.AUT_POSITION, BOOLE.FALSE); ;
            return (ret == 0);
        }

        /// <summary>
        /// Calculates the relative angles and distance from the current station to a given point
        /// </summary>
        /// <param name="RelHzAngle"></param>
        /// <param name="RelVAngle"></param>
        /// <param name="RelDistance"></param>
        public void GetRelativePosition(double[] Pt, out double RelHzAngle, out double RelVAngle)
        {
            //חשב את המרחקים בשלושת הצירים
            double dx, dy, dz;
            dx = Pt[0] - this.CurrentStation.dE0;
            dy = Pt[1] - this.CurrentStation.dN0;
            dz = Pt[2] - this.CurrentStation.dH0 - this.CurrentStation.dHi;

            double dxy = Math.Sqrt(Math.Pow(dx, 2) + (Math.Pow(dy, 2))); // זהו המרחק האופקי בלבד

            //חישוב המרחק התלת מימדי - כרגע אין צורך
            //double RelDistance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));

            //הזווית במישור האפקי
            RelHzAngle = Math.Atan2(dy, dx);
            RelHzAngle = Math.PI / 2 - RelHzAngle;
            if (RelHzAngle < 0)
                RelHzAngle += Math.PI * 2;

            //הזווית במישור האנכי
            RelVAngle = Math.Atan2(dz, dxy);
            RelVAngle = Math.PI / 2 - RelVAngle;
            if (RelVAngle < 0)
                RelVAngle += Math.PI * 2;

            return;
        }

        public bool CheckConnection()
        {
            //זמני
            return true;

            //short ret = -1;

            //try
            //{
            //    //בודק האם החיבור תקין
            //    ret = hv_COM_NullProc();
            //}
            //catch
            //{

            //}

            //return ((ret == 0) || (ret == 13) || (ret == 1293));
        }

        public short GetBatteryPower(out short capacity)
        {
            CSV_POWER_PATH powerSource = CSV_POWER_PATH.CSV_INTERNAL_POWER;
            CSV_POWER_PATH SuggestedSource = CSV_POWER_PATH.CSV_INTERNAL_POWER;

            return hv_CSV_CheckPower(out capacity, out powerSource, out SuggestedSource);
        }

        public void GetCurrentAngles(out double[] angles)
        {
            //שים לב כי הזוית המתקבלת היא ישירות מהדיסטומט ללא שום המרה
            //כלומר גדלה עם כיוון השעון וכו
            TMC_HZ_V_ANG dummy;
            hv_TMC_GetAngle(out dummy, TMC_INCLINE_PRG.TMC_AUTO_INC);
            angles = new double[2];
            angles[0] = PASS_GetHZ();
            angles[1] = PASS_GetV();
        }

        /// <summary>
        /// Returns the horizontal distance between two points
        /// </summary>
        /// <param name="originalPoint"></param>
        /// <param name="measuredPoint"></param>
        /// <returns></returns>
        public double GetHorizontalGap(double[] originalPoint, double[] measuredPoint)
        {
            double OriginalPointDist, MeasuredPointDist;

            //המרחק בין התחנה לנקודה המקורית
            double xgap = originalPoint[0] - this.CurrentStation.dE0;
            double ygap = originalPoint[1] - this.CurrentStation.dN0;
            OriginalPointDist = Math.Sqrt(Math.Pow(xgap, 2) + Math.Pow(ygap, 2));

            //המרחק בין התחנה לנקודה המדודה
            xgap = measuredPoint[0] - this.CurrentStation.dE0;
            ygap = measuredPoint[1] - this.CurrentStation.dN0;
            MeasuredPointDist = Math.Sqrt(Math.Pow(xgap, 2) + Math.Pow(ygap, 2));

            return (MeasuredPointDist - OriginalPointDist);
        }

        public bool GetDistAndAngles(bool UsePrism, bool MeasureFirst, out double SlopeDist, out double HzAngle, out double VAngle)
        {

            TMC_HZ_V_ANG dummyAng;
            double dummyDist;
            SlopeDist = HzAngle = VAngle = -1;

            if (MeasureFirst)
            {
                //set mode
                if (UsePrism)
                {
                    //מדידה באמצעות פריזמה
                    nLastResponse = hv_TMC_SetEdmMode(EDM_MODE.EDM_SINGLE_STANDARD);
                }
                else
                {
                    //מדידה באמצעות לייזר אדום
                    nLastResponse = hv_TMC_SetEdmMode(EDM_MODE.EDM_SINGLE_SRANGE);
                }

                //perform a single distance measurement
                nLastResponse = hv_TMC_DoMeasure(TMC_MEASURE_PRG.TMC_DEF_DIST, TMC_INCLINE_PRG.TMC_MEA_INC);
                if (nLastResponse != 0)
                    return false;
            }

            //wait for data
            nLastResponse = hv_TMC_GetSimpleMea(3000, out dummyAng, out dummyDist, TMC_INCLINE_PRG.TMC_MEA_INC);
            if (nLastResponse != 0)
                return false;

            //to complete the measurement, and clear data
            hv_TMC_DoMeasure(TMC_MEASURE_PRG.TMC_CLEAR, TMC_INCLINE_PRG.TMC_MEA_INC);

            //שמור את נתוני המדידה
            SlopeDist = PASS_GetSlopeDist();

            //זווית אופקית
            //==============
            HzAngle = PASS_GetHZ();
            //תקן את הזוית הנמדדת כך שתתאים למערכת הצירים שלנו
            //כלומר הזווית גדלה נגד כיוון השעון
            //ולא כמו בדיסטומט עצמו, שם הזוית גדלה עם כיוון השעון
            HzAngle = Math.PI / 2 - HzAngle;
            if (HzAngle < 0)
                HzAngle += Math.PI * 2;

            //זווית עלרוד
            //==============
            VAngle = PASS_GetV();
            //תקן את הזוית הנמדדת כך שתתאים למערכת הצירים שלנו
            //כלומר הזווית גדלה נגד כיוון השעון
            //ולא כמו בדיסטומט עצמו, שם הזוית גדלה עם כיוון השעון
            VAngle = Math.PI / 2 - VAngle;
            if (VAngle < 0)
                VAngle += Math.PI * 2;

            return true;
        }

        public bool SetStation(double[] Position)
        {
            TMC_STATION newStation = new TMC_STATION();
            newStation.dHi = this.CurrentStation.dHi;
            newStation.dE0 = Position[0];
            newStation.dN0 = Position[1];
            newStation.dH0 = Position[2] - newStation.dHi;

            nLastResponse = hv_TMC_SetStation(newStation); //קבע את מיקום התחנה
            cmdImportStation(); //קרא מחדש את המיקום מהמכשיר
            return (nLastResponse == 0);
        }

        public bool SetOrientation(double newAngle)
        {
            nLastResponse = hv_TMC_SetOrientation(newAngle);
            return (nLastResponse == 0);
        }

        public double GetInstrumentHeight()
        {
            return CurrentStation.dHi;
        }

        public double[] MeasureBore(double diameter)
        {
            double[] center = new double[3];
            double[] P1, P2, P3, radiuses;
            double l, P1_Ang, vAng, nextHAng, dAng;
            double d = 0.02;

            DateTime time;

            //מדידת נקודה ראשונה
            this.DoMeasure(true, false, out P1, out time);

            //חישוב הזוית והמרחק לנקודה שנמדדה
            this.GetRelativePosition(P1, out P1_Ang, out vAng);
            l = GetDistBetween(P1, GetStationPt());

            //חישוב הפרש הזוית לנקודות הבאות
            dAng = Math.Atan2(d, l);

            //תזוזה לנקודה השניה, ומדידה
            nextHAng = P1_Ang - dAng;
            this.PointAt(nextHAng, vAng);
            this.DoMeasure(true, false, out P2, out time);

            //תזוזה לנקודה השלישית, ומדידה
            nextHAng = P1_Ang + dAng;
            this.PointAt(nextHAng, vAng);
            this.DoMeasure(true, false, out P3, out time);

            CalcCircleCenter(P1, P2, P3, out center, out radiuses);
            double dev = Math.Abs(radiuses[1] - radiuses[0]) + Math.Abs(radiuses[2] - radiuses[1]) + Math.Abs(radiuses[2] - radiuses[0]) / 3;
            if (Math.Abs(diameter - dev) <= 0.0035)
            {
                //TODO
            }

            return center;

        }

        //Returns the station 3d point
        private double[] GetStationPt()
        {
            return new double[]
            {
                this.CurrentStation.dE0,
                this.CurrentStation.dN0,
                this.CurrentStation.dH0+this.CurrentStation.dHi
            };
        }


        //Returns the geometric distance between two 3d points
        private double GetDistBetween(double[] p1, double[] p2)
        {
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];
            return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));
        }

        private void CalcCircleCenter(double[] P1, double[] P2, double[] P3, out double[] P0, out double[] radius)
        {
            double dxA = P2[0] - P1[0];
            double dyA = P2[1] - P1[1];
            double mA = dyA / dxA;

            double dxB = P3[0] - P2[0];
            double dyB = P3[1] - P2[1];
            double mB = dyB / dxB;

            double x0 = mA * mB * (P1[1] - P3[1]) + mB * (P1[0] + P2[0]) - mA * (P2[0] + P3[0]);
            x0 /= 2 * (mB - mA);

            double y0 = (-1 / mA) * (x0 - (P1[0] + P2[0]) / 2) + (P1[1] + P2[1]) / 2;

            P0 = new double[] { x0, y0, P1[2] };

            double radius1 = Math.Sqrt(Math.Pow(P1[0] - x0, 2) + Math.Pow(P1[1] - y0, 2));
            double radius2 = Math.Sqrt(Math.Pow(P2[0] - x0, 2) + Math.Pow(P2[1] - y0, 2));
            double radius3 = Math.Sqrt(Math.Pow(P3[0] - x0, 2) + Math.Pow(P3[1] - y0, 2));
            radius = new double[] { radius1, radius2, radius3 };
        }

        public bool GetTiltStatus(out double[] Inclination)
        {
            Inclination = new double[3];

            //קריאת נתוני הפלס מהמכשיר לתוך העוטף
            nLastResponse = hv_TMC_GetTiltStatus();
            if ((nLastResponse != 0) && (nLastResponse != 1285))
                return false; //המדידה לא הצליחה
            else
            {
                //קריאת נתוני הפלס מהעוטף
                Inclination[0] = PASS_GetX();
                Inclination[1] = PASS_GetY();
                Inclination[2] = PASS_GetZ();
                return true;
            }
        }

        public bool TpsTilted(double maxTilt, out double[] TiltVals)
        {
            //אם המדידה לא הצליחה החזר ערך של לא מפולס
            if (!GetTiltStatus(out TiltVals)) return true;

            return (Math.Abs(TiltVals[2]) >= maxTilt);
        }

        

        public double[] GetStation()
        {
            return new double[]
            {
            this.CurrentStation.dE0,
            this.CurrentStation.dN0,
            this.CurrentStation.dH0
            };
        }


    } //End of Class
} //End of Namespace
