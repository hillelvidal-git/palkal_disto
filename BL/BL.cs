using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Timers;

namespace WideFieldBL
{
    public struct Target
    {
        public int pointNumber;
        public double[] Position;
        public double SlopeDistance;
        public double HzAngle;
        public double VAngle;
        public double HzDist;
        public double VDist;
    }

    public class BL
    {
        private TpsAdapter.TpsAdapter tps;
        public double StakeoutStartZ = -1;
        public bool StopPlay = false;
        private string lastDsitomatCOM;
        public bool bTpsTilted;
        FilesAdapter filesTool;

        public int[] CurrentAttribution;

        bool tpsLastAlive;
        DateTime tpsLastHeard;
        double TpsAliveMaxIntervalMs = 8000;

        public Action actTpsAlive;
        public Action<string> actTpsReturned;
        public Action<bool, bool> actTpsConnectionStatus;
        public Action<string> actPrompt;

        //דגל שמציין האם המכשיר מחובר
        public bool bTpsConnected;

        short tpsBatteryVoltage;
        string tps_port;

        System.Timers.Timer timerCheckTps;

        public BL(string tpsport)
        {
            tps_port = tpsport;

            // Create a timer with a two second interval.
            timerCheckTps = new System.Timers.Timer(TpsAliveMaxIntervalMs);
            timerCheckTps.Elapsed += checkTpsConnection;
            timerCheckTps.AutoReset = true;
            timerCheckTps.Enabled = true;

            StartTps();

            this.CurrentAttribution = new int[] { -1, -1, -1, -1 };

            this.filesTool = new FilesAdapter();
        }

        private void checkTpsConnection(object sender, ElapsedEventArgs e)
        {
            //Console.WriteLine("check tps connection > alive? " + tpsLastAlive + " / connected? " + bTpsConnected);

            timerCheckTps.Stop();
            //bool alive = tpsLastAlive;
            Double gap = (DateTime.Now - tpsLastHeard).TotalMilliseconds;
            bool alive = gap <= TpsAliveMaxIntervalMs;

            actTpsConnectionStatus?.Invoke(alive, false);

            if (bTpsConnected) //should be connected
            {

                if (alive)
                {
                    //Console.WriteLine("should be connected, and is actually alive, so nothing to do");
                    Console.WriteLine("------> Tps Connection OK. ("+gap+")");
                }
                else
                {
                    //dispose tps object and thread  
                    //Console.WriteLine("should be connected, but it's not alive, so STOP");
                    Console.WriteLine("------> Tps Connection Broken! Stopping connection. (" + gap + ")");
                    //DialogResult dr = MessageBox.Show("נראה שחיבור ה BLUETOOTH אבד. האם תרצו להתחבר שוב?", "בעיית תקשורת", MessageBoxButtons.YesNoCancel);
                    
                    //if (dr== DialogResult.Yes) 
                    StopTps();
                }                
            }
            else //sould be disconnected
            {
                //try to reconnect tps                
                if (this.try_to_connect)
                {
                    Console.WriteLine("------> Trying Recoonect Tps...");
                    StopTps();
                    ConnectTps(true, tps_port);
                }
            }

            tpsLastAlive = false; //reset flag. next time, timer will check if we got a new alive msg.
            timerCheckTps.Start();
        }

        private void StartTps()
        {
            tps = new TpsAdapter.TpsAdapter();
            tps.actAlive += tpsAlive;
            tps.actBattery += tpsBattery;
            tps.actReturned += tpsReturned;


        }

        private void tpsReturned(string obj)
        {
            actTpsReturned?.Invoke(obj);
        }

        ~BL()
        {
            StopTps();
        }

        private void tpsBattery(short v)
        {
            tpsBatteryVoltage = v;
        }

        private void tpsAlive()
        {
            tpsLastAlive = true;
            tpsLastHeard = DateTime.Now;
            Console.WriteLine(">> ALIVE <<");
            timerCheckTps.Stop(); timerCheckTps.Start();
            actTpsAlive?.Invoke();
        }

        public void ConnectTps(bool conncet, string PortName)
        {
            //Console.WriteLine("func. connect > " + conncet);

            timerCheckTps.Stop();

            if (!conncet) //אם זו בקשה להתנתק
            {
                Console.WriteLine("DISCONNECTING by request and setting [try = false]");
                StopTps(); //stop and dispose tps object    
                //PortName = "";
                this.try_to_connect = false;
            }
            else
            {
                Console.WriteLine("trying to connect: " + PortName);

                //אם זו בקשה להתחבר
                this.try_to_connect = true;
                StartTps();

                bool success = false;

                string DistomatPort = "";
                string strPortName;
                //List<string> Ports = GetPortsNames(this.lastDsitomatCOM);
                //Ports.Insert(0, PortName);
                List<string> Ports = new List<string>() { PortName };

                foreach (string port in Ports)
                {
                    strPortName = FixPortName(port);
                    actPrompt?.Invoke("מנסה להתחבר: COM" + strPortName + "...");

                    try
                    {
                        if (tps.Connect(Convert.ToInt16(strPortName), 115200, 1))
                        {
                            Console.WriteLine("CONNECTION SUCCEDDED");
                            this.bTpsConnected = true;
                            //החיבור עם היציאה הצליח
                            //tpsLastAlive = true;
                            tpsAlive();
                            DistomatPort = strPortName;
                            success = true;
                            tps.Run();
                            //System.Threading.Thread.Sleep(200);
                            tps.cmdImportStation();
                            break;
                        }
                    }
                    catch (Exception er)
                    {
                        MessageBox.Show(strPortName + "\t" + er.Message);
                    }
                }

                if (success)
                {
                    PortName = DistomatPort;
                    tps_port = "COM" + PortName;
                    this.lastDsitomatCOM = DistomatPort;
                    actPrompt?.Invoke("החיבור הצליח");
                }
                else
                {
                    Console.WriteLine("CONNECTION FAILED");
                    PortName = "";
                    actPrompt?.Invoke("דיסטומט לא נמצא");
                    this.bTpsConnected = false; //save new status
                    actTpsConnectionStatus?.Invoke(false, true); //inform user
                }
            }
            timerCheckTps.Start();
        }

        private List<string> GetPortsNames(string firstPort)
        {
            List<string> ports = new List<string>();
            ports.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            ports.Remove(firstPort);
            ports.Insert(0, firstPort);
            return ports;
        }

        private string FixPortName(string port)
        {
            int temp;
            string fix = port;
            for (int i = 3; i < fix.Length; i++)
            {
                if (!int.TryParse(fix.Substring(i, 1), out temp))
                    return fix.Substring(3, i - 3);
            }
            return fix.Substring(3);
        }

        public bool DoMeasure(bool HighAccruacy, bool UsePrism, out double[] pt)
        {
            pt = new double[3];

            if (!this.bTpsConnected)
            {
                actPrompt?.Invoke("המכשיר מנותק. מדידה נכשלה.");
                return false;
            }
            if (this.bTpsTilted)
            {
                actPrompt?.Invoke("המכשיר איננו מפולס. מדידה נכשלה.");
                return false;
            }

            DateTime PointTime;
            return tps.DoMeasure(HighAccruacy, UsePrism, out pt, out PointTime);
        }

        public bool AutoTargetSwitch(bool on)
        {
            actPrompt?.Invoke("מחפש מטרה...");
            return tps.AutoTargetSwitch(on);
        }

        public bool FindPoint(string ptName, double[] XYZ, bool IsSlope, double dPercision, out double finalZ, int ExclusiveMethod, bool MeasureStartZ, out TimeSpan duration)
        {
            if ((this.bTpsTilted) || (!this.bTpsConnected))
            {
                finalZ = 0; duration = new TimeSpan();
                return false;
            }

            //קבע את רמת הדיוק על פי בחירת המשתמש
            double percision = dPercision / 1000;

            double HorizontalAngle, VerticalAngle;
            double[] actualXYZ = new double[] { 0, 0, 0 }; //הנקודה הנמדדת הנוכחית
            double[] lastXYZ = new double[] { 0, 0, 0 }; //הנקודה הקודמת שנמדדה
            double[] beforeLastXYZ = new double[] { 0, 0, 0 }; //הנקודה הלפני-קודמת שנמדדה
            double[] Plane = new double[] { -1, -1, -1, -1 };

            double XYgap = 0;
            double lastXYgap = 0;
            double M = 0;
            DateTime ptime;
            finalZ = -1;
            duration = new TimeSpan();
            string M_Vals = "";
            string Z_Vals = "";
            string Gap_Vals = "";

            if (StakeoutStartZ == -1)
                MeasureStartZ = true;

            //אם יש צורך - ביצוע מדידת גובה התחלתי
            if (MeasureStartZ)
            {
                double[] startZpoint;
                actPrompt?.Invoke("מודד גובה ראשוני...");
                if (tps.DoMeasure(false, false, out startZpoint, out ptime))
                    XYZ[2] = startZpoint[2];
                else
                    return false; //מדידת נקודת גובה לא הצליחה
            }
            else
            {
                XYZ[2] = this.StakeoutStartZ; //התחל מהגובה השמור מפעם קודמת
            }

            #region TheLoop
            string proxMethod = "ByHeight";
            int MaxTries = 7;

            bool PointFound = false; //דגל שישמש לערך המוחזר
            DateTime startTime = DateTime.Now;
            int triesNum = 0;

            for (int tries = 1; tries < MaxTries; tries++)
            {
                Application.DoEvents();
                if (this.StopPlay)
                    break;

                actPrompt?.Invoke("נקודה  " + ptName + ": >ניסיון " + tries.ToString() + "< מחשב...");
                //חישוב הזוויות לנקודה
                tps.GetRelativePosition(XYZ, out HorizontalAngle, out VerticalAngle);

                //ביצוע הצבעה
                actPrompt?.Invoke("נקודה  " + ptName + ": ניסיון " + tries.ToString() + ": נוסע לנקודה...");
                if (!tps.PointAt(HorizontalAngle, VerticalAngle))
                    return false; //ההצבעה נכשלה

                //מדידת הנקודה בפועל
                actPrompt?.Invoke("נקודה  " + ptName + ": ניסיון " + tries.ToString() + ": מודד נקודה...");
                if (!tps.DoMeasure(false, false, out actualXYZ, out ptime))
                    return false; //המדידה נכשלה

                Z_Vals += "\n" + actualXYZ[2].ToString("0.0000");

                //חישוב המרחק האופקי מהנקודה המקורית
                XYgap = tps.GetHorizontalGap(XYZ, actualXYZ);
                Gap_Vals += "\n" + XYgap.ToString("0.0000");

                //בדיקה האם הדיוק הושג
                if (Math.Abs(XYgap) <= percision)
                {
                    triesNum = tries;
                    PointFound = true;
                    actPrompt?.Invoke("נקודה  " + ptName + " נמצאה");
                    goto EndLoop;
                }

                //החלטה איך יתבצע התיקון
                /*
                 * האם יש שתי נק
                 * האם הפרש הגבהים...
                 * האם גודל השיפוע...
                 * האם נוסה בעבר...
                 */

                proxMethod = "ByHeight";
                if (IsSlope) //אם מדובר באיזור משופע
                {
                    if (tries > 1) proxMethod = "ByTangens";
                }

                //בדיקה האם עברנו צד של הנקודה
                if (Math.Sign(XYgap) != Math.Sign(lastXYgap))
                {
                    //הנקודה הנוכחית כבר מעבר לנקודה המבוקשת
                }

                switch (proxMethod)
                {
                    case "ByHeight":
                        //תיקון על פי גובה
                        XYZ[2] = actualXYZ[2];
                        break;
                    case "ByPlane": //כרגע לא בשימוש
                        //חישוב המישור
                        Plane = GetPlaneBy3Points(actualXYZ, lastXYZ, beforeLastXYZ);
                        //תיקון על פי חישוב המישור
                        XYZ[2] = GetZByPlane(XYZ, Plane);
                        break;
                    case "ByTangens":
                        //תיקון על פי שיפוע
                        XYZ[2] = GetZByTangens(XYZ, actualXYZ, lastXYZ, XYgap, lastXYgap, out M);
                        M_Vals += "\n" + M.ToString("0.0000");

                        //אמנם אם השיפוע הוא קטן מאוד, כנראה שמדובר במכשול (כגון קיר) ולא נחשב לפי השיפוע
                        if (Math.Abs(M) < 0.15)
                            XYZ[2] = actualXYZ[2] + 0.3; //עלה 30 סמ בניסיון לעקוף את המכשול
                        break;
                    case "BySplit":
                        XYZ[2] = (lastXYZ[2] + actualXYZ[2]) / 2; //חזרה לגובה הממוצע בין שתי הדגימות האחרונות
                        break;
                    case "Impossible":
                        tries = MaxTries;
                        break;
                }

                //שמירת הנקודה הקודמת שנמדדה
                beforeLastXYZ = new double[] { lastXYZ[0], lastXYZ[1], lastXYZ[2] };
                lastXYZ = new double[] { actualXYZ[0], actualXYZ[1], actualXYZ[2] };
                lastXYgap = XYgap;

            }
        #endregion

        EndLoop:

            DateTime endTime = DateTime.Now;
            duration = endTime - startTime;

            //שמירת הגובה על מנת לעדכן את המשתמש
            finalZ = actualXYZ[2];
            //שמירת הגובה האמיתי על מנת להתחיל ממנו בחיפוש הנקודות הבאות
            this.StakeoutStartZ = finalZ;

            //החזרת ערך בהתאם למציאת הנקודה
            return PointFound;
        }

        public void cmdRedLaser(bool on)
        {
            tps?.cmdRedLaser(on);
        }

        private double GetXyDist(double[] XYZ, double[] p)
        {
            return Math.Sqrt(Math.Pow((XYZ[0] - p[0]), 2) + Math.Pow((XYZ[1] - p[1]), 2));
        }

        private double[] GetPlaneBy3Points(double[] P1, double[] P2, double[] P3)
        {
            double[] V1, V2, VP, PLANE;
            double d;
            V1 = new double[3] { P2[0] - P1[0], P2[1] - P1[1], P2[2] - P1[2] }; //חישוב וקטור ראשון במישור
            V2 = new double[3] { P3[0] - P2[0], P3[1] - P2[1], P3[2] - P2[2] }; //חישוב וקטור שני במישור
            VP = CrossProduct(V1, V2); //חישוב הוקטור המאונך למישור
            d = 0 - ScalarProduct(P1, VP); //חישוב המקדם הסקלר במשוואת המישור

            PLANE = new double[] { VP[0], VP[1], VP[2], d }; //וקטור מקדמי המישור
            return PLANE;
        }

        public void cmdMeasure(bool usePrism)
        {
            tps?.cmdMeasure(usePrism);
        }

        private double GetZByPlane(double[] P0, double[] PLANE)
        {
            double z;
            z = (P0[0] * PLANE[0] + P0[1] * PLANE[1] + PLANE[3]) / PLANE[2]; //חישוב ערך זד בנקודה המבוקשת

            return z;
        }

        private double ScalarProduct(double[] V1, double[] V2)
        {
            return (V1[0] * V2[0] + V1[1] * V2[1] + V1[2] * V2[2]);
        }

        private double[] CrossProduct(double[] V1, double[] V2)
        {
            return new double[3] { V1[1] * V2[2] - V1[2] * V2[1], V1[2] * V2[0] - V1[0] * V2[2], V1[0] * V2[1] - V1[1] * V2[0] };
        }

        private double GetZByTangens(double[] XYZ, double[] actualXYZ, double[] lastXYZ, double XYgap, double lastXYgap, out double M)
        {
            M = (lastXYgap - XYgap) / (lastXYZ[2] - actualXYZ[2]); //חישוב השיפוע
            //TODO: בדיקה שהשיפוע אמין
            double z = actualXYZ[2] - (XYgap / M); //חיזוי הגובה שיתן הפרש אפס
            return z;
        }

        public void CheckStatus(out short voltage, out bool tilted, out double[] TiltVals)
        {
            //GeoComServer.GetBatteryPower(out capacity); //מצב הסוללה
            voltage = tpsBatteryVoltage;
            tilted = tps.TpsTilted(0.0001, out TiltVals); //מצב הפלס
            return;
        }

        public void GetAngles(out double[] angles)
        {
            tps.GetCurrentAngles(out angles);
        }

        public void UpdateStation()
        {
            tps.cmdImportStation();
        }

        /// <summary>
        /// Performs a measurment of BasePoint and stores the relevant data
        /// </summary>
        /// <param name="pointNumber"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool MeasureTarget(int ptId, string ptName, out Target target, bool UsePrism)
        {
            target = new Target();
            try
            {
                target.pointNumber = Convert.ToInt32(ptName);
            }
            catch { target.pointNumber = 0; }

            //קבל את שיעורי הנקודה שנמדדה ממסד הנתונים על פי שמה
            if (!GetPointXYZ(ptId, out target.Position))
                return false;

            actPrompt?.Invoke("מודד מטרה...");

            // במקרה של פריזמה, יש למצוא את הפריזמה לפני המדידה
            if (UsePrism)
            {
                actPrompt?.Invoke("מחפש מטרה...");
                if (AutoTargetSwitch(true))
                {
                    actPrompt?.Invoke("מטרה נמצאה. מבצע מדידה...");
                }
                else
                {
                    actPrompt?.Invoke("מטרה לא נמצאה. מדידה בוטלה.");
                    return false;
                }
            }

            //בצע מדידת מרחק וזויות
            if (!tps.GetDistAndAngles(UsePrism, true, out target.SlopeDistance, out target.HzAngle, out target.VAngle))
                return false;
            //הערה חשובה: הזוית הנשמרת היא כבר לאחר המרה למערכת הצירים הרגילה שלנו ולא במערכת הדיסטומט, שם הזויות הפוכות

            //חשב את המרחקים בשני הצירים
            target.HzDist = target.SlopeDistance * Math.Cos(target.VAngle);
            target.VDist = target.SlopeDistance * Math.Sin(target.VAngle);

            return true;
        }

        public bool CalcStationBy2Targets(Target t1, Target t2, double MaxGap, out double[] Station)
        {
            Station = new double[3];

            bool ShortWayCCW; //מציין האם הדרך הקצרה להגיע מ1 ל2 היא להסתובב נגד כיוון השעון. נועד לאפשר הכרעה בין שתי התחנות האפשריות
            double minAngle; //שומר את ערך הזוית הקטנה בין 1 ו 2. נועד לאפשר בדיקת דיוק של התחנה המחושבת

            //חשב  את הזוית בין שתי המטרות וזאת על ידי הפרש הזוית המוחלטת אל כל אחת
            //הזויות המוחלטות מתקבלות כנגד כיוון השעון
            //ולכן ההפרש הוא הזוית הנמדדת כאשר הולכים מאחת לשתיים כנגד כיוון השעון
            minAngle = t2.HzAngle - t1.HzAngle;

            //מניעת מינוס
            if (minAngle < 0)
                minAngle += Math.PI * 2;

            //המבוקש הוא הזוית הקטנה ממאה ושמונים
            //וכן לשמור את הנתון האם זוית זו נמדדת נגד כיוון השעון

            if (minAngle > Math.PI)
            {
                minAngle = Math.PI * 2 - minAngle;
                ShortWayCCW = false;
            }
            else
            {
                ShortWayCCW = true;
            }

            //מצא את נקודת התחנה על ידי חיתוך שני מעגלים, במישור בלבד
            if (!FindIntersecOf2Circles(t1.Position, t1.HzDist, t2.Position, t2.HzDist, minAngle, ShortWayCCW, MaxGap, out Station[0], out Station[1]))
                return false;

            //מצא את גובה התחנה, על ידי ממוצע מדידות המטרות
            Station[2] = ((t1.Position[2] + t2.Position[2]) - (t1.VDist + t2.VDist)) / 2;

            return true;
        }               

        public void cmdSetPrismTrack(bool on)
        {
            tps?.cmdSetPrismTrack(on);
        }

        public bool FindIntersecOf2Circles(double[] P0, double r0, double[] P1, double r1, double ang, bool ShortWayCCW, double MaxGap, out double StationX, out double StationY)
        {
            StationX = StationY = 0;

            double d, a, h;
            double[] P2;
            double[] P3_1, P3_2;

            //חישוב המרחק בין הנקודות ובירור האם יש פתרון
            d = Math.Sqrt(Math.Pow(P1[0] - P0[0], 2) + Math.Pow(P1[1] - P0[1], 2));
            if
                (
                (d > r0 + r1)  //המעגלים אינם נחתכים
                ||
                (d < Math.Abs(r0 - r1))  //המעגלים מוכלים
                ||
                ((d == 0) && (r0 == r1))  //המעגלים חופפים
                )

                return false;

            //חישוב ערכי ביניים לצורך הפתרון
            a = (Math.Pow(r0, 2) - Math.Pow(r1, 2) + Math.Pow(d, 2)) / (2 * d);
            h = Math.Sqrt(Math.Pow(r0, 2) - Math.Pow(a, 2));

            P2 = new double[]
            {
                P0[0] + (a / d) * (P1[0] - P0[0]),
                P0[1] + (a / d) * (P1[1] - P0[1])
            };

            //חישוב נקודות החיתוך
            P3_1 = new double[]
            {
                P2[0] + (h / d) * (P1[1] - P0[1]),
                P2[1] - (h / d) * (P1[0] - P0[0])
            };

            P3_2 = new double[]
            {
                P2[0] - (h / d) * (P1[1] - P0[1]),
                P2[1] + (h / d) * (P1[0] - P0[0])
            };

            //חישוב הזויות מנקודת מבט הפתרונות
            double angSol1 = GetAngleCCW(P3_1, P0, P1);
            double angSol2 = GetAngleCCW(P3_2, P0, P1);
            double angSelectedSol;

            //בחירת הפתרון המתאים למטרות שנמדדו על פי הזווית
            if ((angSol1 < Math.PI) == ShortWayCCW)
            {
                //בחר בפתרון מס' 1
                StationX = P3_1[0];
                StationY = P3_1[1];
                angSelectedSol = angSol1;
            }
            else
            {
                //בחר בפתרון מס' 2
                StationX = P3_1[0];
                StationY = P3_1[1];
                angSelectedSol = angSol2;
            }

            //שמירת הזוית הקטנה
            //כי אנו הולכים להשוות אותה עם הזווית הקטנה המקורית
            if (angSelectedSol > Math.PI)
                angSelectedSol = Math.PI * 2 - angSelectedSol;

            //חישוב הדיוק שהושג
            double accuracy = angSelectedSol / ang;
            if (Math.Abs(accuracy - 1) >= MaxGap)
            {
                MessageBox.Show("הדיוק הנדרש לא הושג");
                return false;
            }
            else
                MessageBox.Show("דיוק הזוית שהושג: " + Math.Abs(1 - accuracy).ToString());

            return true;
        }

        private double GetAngleCCW(double[] C, double[] A, double[] B)
        {
            double a = Math.Atan2(A[1] - C[1], A[0] - C[0]);
            double b = Math.Atan2(B[1] - C[1], B[0] - C[0]);
            //הערה חשובה: הזויות המחושבות כאן הן במערכת הצירים הרגילה, נגד כיוון השעון, ומתחילים מציר האיקס

            double angACB = b - a;
            if (angACB < 0)
                angACB += Math.PI * 2;

            return angACB;
        }

        DbAdapter lda = new DbAdapter();
        public bool try_to_connect;

        public bool GetPointXYZ(int ptId, out double[] XYZ)
        {
            XYZ = new double[3];

            if (!lda.GetPointPosition(ptId, out XYZ))
                return false;
            else return true;
        }

        public bool SetStationByResection(List<Target> Targets, out double[] StationPosition)
        {
            StationPosition = new double[3];

            //מעבר על כל הזוגות האפשריים של מטרות וחישוב תחנה על פי כל זוג
            List<double[]> Stations = new List<double[]>();
            double MaxGap = 0.001; //הפרש הזיויות המקסימלי בין הזוית הנמדדת לזוית המחושבת

            for (int i = 0; i < Targets.Count; i++)
                for (int j = 0; j < i; j++)
                {
                    double[] newStation = new double[3];
                    if (CalcStationBy2Targets(Targets[j], Targets[i], MaxGap, out newStation))
                        Stations.Add(newStation);
                }

            if (Stations.Count == 0)
                return false;

            double gap;
            //בדיקה שהתחנות שחושבו לא רחוקות זו מזו
            for (int i = 0; i < Stations.Count; i++)
                for (int j = 0; j < i; j++)
                {
                    gap = GetDistBetween(Stations[i], Stations[j]);
                    if (gap >= 0.02)
                    {
                        MessageBox.Show("האופציות השונות רחוקות מדי: " + gap.ToString());
                        return false;
                    }
                }

            //ולבסוף - חישוב הממוצע המרחבי של כל התחנות שחושבו
            double[] MeanStation = new double[3];
            foreach (double[] Suggestion in Stations)
            {
                MeanStation[0] += Suggestion[0];
                MeanStation[1] += Suggestion[1];
                MeanStation[2] += Suggestion[2];
            }

            MeanStation[0] /= Stations.Count;
            MeanStation[1] /= Stations.Count;
            MeanStation[2] /= Stations.Count;

            //בקש אישור מהמשתמש לכיוונון התחנה
            if (MessageBox.Show
                ("התחנה חושבה בהצלחה.\nהאם ברצונך לקבוע את התחנה?",
                "קביעת תחנה",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RtlReading
                ) != DialogResult.Yes)
                return false;

            //אם נתקבל אישור - קבע את התחנה
            if (!tps.SetStation(MeanStation))
                return false;

            //קבע את הכיוון
            double[] angs;
            double dx = Targets[0].Position[0] - MeanStation[0];
            double dy = Targets[0].Position[1] - MeanStation[1];

            double NewTargetAngle = Math.Atan2(dy, dx); //הזוית המחושבת כעת לכיוון מטרה מסוימת
            double OldTargetAngle = Targets[0].HzAngle; //הזוית הנ"ל כפי שנמדדה קודם לכן
            double AnglesDifference = NewTargetAngle - OldTargetAngle; //ההפרש בין הזוית הקיימת במכשיר לבין הזוית המחושבת

            //חשוב מאוד - תיקון הפרש הזיויות המחושב, מכיון שבדיסטומט הכיוון הוא הפוך כלומר עם כיוון השעון
            AnglesDifference = 0 - AnglesDifference;

            tps.GetCurrentAngles(out angs);
            //הזוית הנוכחית כפי שנמדדת במכשיר
            double OldCurrentAngle = angs[0];
            //תיקון הזוית הנוכחית כך שתתאים לחישוב התחנה החדשה
            double NewCurrentAngle = OldCurrentAngle + AnglesDifference;
            if (NewCurrentAngle < 0)
                NewCurrentAngle += Math.PI * 2;

            if (!tps.SetOrientation(NewCurrentAngle))
                return false;

            //שמור את ערכי התחנה להצגה בממשק
            StationPosition = new double[]
            {
                MeanStation[0],
                MeanStation[1],
                MeanStation[2]
            };

            return true;
        }

        //Returns the geometric distance between two 3d points
        private double GetDistBetween(double[] p1, double[] p2)
        {
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];
            return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2) + Math.Pow(dz, 2));
        }

        private void RelaeaseConnection(System.Data.SqlClient.SqlConnection connection)
        {
            System.Data.SqlClient.SqlConnection.ClearPool(connection);
        }

        public bool GetLevelClass(int levelId, int classId, out DbPoint[] points)
        {
            //מחזיר את רשימת השמות של נקודות הבסיס במפלס הנתון
            return lda.GetLevelPointByClass(levelId, classId, out points);
        }

        public bool SaveNewPoint(DbPoint pt, out bool webidExists, out bool nameExists, out string errorMsg)
        {
            if (lda.InsertPoint(pt, out webidExists, out nameExists, out errorMsg))
            {
                actPrompt?.Invoke("נקודה מס' " + pt.Number + " נשמרה בהצלחה");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool MeasureBore(double diameter, out double[] pt)
        {
            try
            {
                pt = tps.MeasureBore(diameter);
                return true;
            }
            catch
            {
                pt = new double[3];
                return false;
            }
        }

        public void PointAt(double[] XYZ)
        {
            //חישוב הזוויות לנקודה
            double HorizontalAngle, VerticalAngle;
            tps.GetRelativePosition(XYZ, out HorizontalAngle, out VerticalAngle);

            //ביצוע הצבעה
            tps.PointAt(HorizontalAngle, VerticalAngle);
        }

        public bool SetStationByValues(double[] XYZ, double orientaionAngle)
        {
            return tps.SetStation(XYZ) && tps.SetOrientation(orientaionAngle);
        }

        public bool GetLevelPoints(int levelId, out DbPoint[] points)
        {
            return lda.GetLevelPoints(levelId, out points);
        }

        public bool GetFieldPoints(int fieldId, out DbPoint[] points)
        {
            return lda.GetFieldPoints(fieldId, out points);
        }

        public bool DeletePoint(int id, out string msg)
        {
            if (!lda.DeletePoint(id))
            {
                msg = lda.GetErrorMessage();
                return false;
            }
            else
            {
                msg = "הנקודה נמחקה בהצלחה";
                return true;
            }

        }

        public void cmdTpsBeepNormal()
        {
            tps.cmdBeepAlarm();
        }

        public double[] GetStation()
        {
            return tps.GetStation();
        }

        public bool Locate(List<TargetData> targets)
        {
            try
            {

                if (targets.Count < 2)
                {
                    MessageBox.Show("Not enough Targets");
                    return false;
                }

                LocationCalculator lc = new LocationCalculator();
                Station newSt = lc.CalculateBestStation(targets);

                double angCorrection = lc.GetAngleCorrection(newSt, targets);

                //קבע את התחנה
                if (!tps.SetStation(newSt.Position))
                {
                    MessageBox.Show("Station Not Set!");
                    return false;
                }

                //קבע את הכיוון
                //---------------

                double[] angs;
                //הזוית הנוכחית כפי שנמדדת במכשיר
                tps.GetCurrentAngles(out angs);
                double OldCurrentAngle = angs[0];

                lc.log.Add("");
                lc.log.Add("Angle Setting:");
                lc.log.Add("==============");
                lc.log.Add("Distomat Current Angle: " + OldCurrentAngle.ToString("0.00000"));
                OldCurrentAngle = Math.PI / 2 - OldCurrentAngle;
                lc.log.Add("Converted To Drawing System: " + OldCurrentAngle.ToString("0.00000"));

                if (OldCurrentAngle < 0)
                {
                    OldCurrentAngle += Math.PI * 2;
                    lc.log.Add("---> After Adjusting: " + OldCurrentAngle.ToString("0.00000"));
                }

                //תיקון הזוית הנוכחית כך שתתאים לחישוב התחנה החדשה
                double NewCurrentAngle = OldCurrentAngle + angCorrection;
                lc.log.Add("Distomat New Angle: " + NewCurrentAngle.ToString("0.00000"));

                //תיקון זוית
                while (NewCurrentAngle < 0)
                    NewCurrentAngle += Math.PI * 2;
                while (NewCurrentAngle > Math.PI * 2)
                    NewCurrentAngle -= Math.PI * 2;

                lc.log.Add("---> After Adjusting: " + NewCurrentAngle.ToString("0.00000"));

                //התאמת הזוית למערכת הצירים של הדיסטומט, כלומר גדלה עם כיוון השעון וכו
                NewCurrentAngle = Math.PI / 2 - NewCurrentAngle;
                //תיקון זוית
                while (NewCurrentAngle < 0)
                    NewCurrentAngle += Math.PI * 2;
                while (NewCurrentAngle > Math.PI * 2)
                    NewCurrentAngle -= Math.PI * 2;
                lc.log.Add("Converted To Distomat System: " + NewCurrentAngle.ToString("0.00000"));

                if (!tps.SetOrientation(NewCurrentAngle))
                {
                    MessageBox.Show("Angle Not Set!");
                    return false;
                }
                lc.log.Add("Angle Has Been Set");

                string filename = DateTime.Now.ToString();
                filename = filename.Replace(":", ".").Replace("/", ".").Replace(" ", ".");
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\BedekLogs\\WFLocating";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                filename = folder + "\\" + filename + ".DAT";
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    foreach (string line in lc.log)
                        sw.WriteLine(line);
                }


                MessageBox.Show("התחנה נקבעה בהצלחה");
                //System.Diagnostics.Process.Start(filename);
                return true;
            }
            catch (Exception es)
            {
                MessageBox.Show("Error: " + es.Message);
                return false;
            }
        }

        public void SaveLastField()
        {
            this.filesTool.SaveLastField(this.CurrentAttribution);
        }

        public string[] LoadLastField()
        {
            return this.filesTool.LoadLastField();
        }


        public bool InsertPointsToLocal(DbPoint[] newPoints, out string[] log, out int saved, out int errors, out List<int> existing)
        {
            List<string> logLines = new List<string>();
            bool webidExists; bool nameExists;
            saved = 0; errors = 0;
            string msg;
            existing = new List<int>();

            try
            {
                foreach (DbPoint pt in newPoints)
                {
                    logLines.Add("Inserting: " + pt.GetAttribution());
                    if (lda.InsertPoint(pt, out webidExists, out nameExists, out msg) | nameExists | webidExists)
                    {
                        if (nameExists) existing.Add(pt.ID);
                        logLines.Add("[OK] " + msg);
                        saved++;
                    }
                    else
                    {
                        logLines.Add("[Error] " + msg);
                        errors++;
                    }
                }

                log = logLines.ToArray();
                return (errors == 0);
            }
            catch (Exception e)
            {
                logLines.Add("");
                logLines.Add("Error:");
                logLines.Add(e.Message);
                log = logLines.ToArray();
                return false;
            }
        }

        public bool GetLocalNewsList(out DbPoint[] news, out string msg)
        {
            try
            {
                return lda.GetNews(out news, out msg); //Get all point where StatusID=-1
            }
            catch (Exception e)
            {
                news = new DbPoint[0];
                msg = e.Message;
                return false;
            }
        }

        public void UploadPoints(DbPoint[] news, out string[] outMsg, out int uploaded, out int errors, out List<int> conflict)
        {
            int webId;
            List<string> msg = new List<string>();
            msg.Add("Uploading Points:");
            uploaded = 0; errors = 0;
            string ptMsg;
            conflict = new List<int>();

            try
            {
                foreach (DbPoint newOne in news)
                {
                    msg.Add(newOne.GetAttribution());
                    if (filesTool.UploadNew(newOne, out webId, out ptMsg))
                    {
                        uploaded++;
                        msg.Add("   [Upload OK]");

                        if (lda.UpdateUploaded(newOne.ID, webId, out ptMsg))
                        {
                            msg.Add("   [Update OK]");
                        }
                        else
                        {
                            msg.Add("   [Update ERROR] " + ptMsg);
                        }
                    }
                    else
                    {
                        errors++;
                        msg.Add("   [Upload ERROR] " + ptMsg);
                        if (ptMsg.Contains("IX_Measurements")) conflict.Add(newOne.ID);
                    }
                }

            }
            catch (Exception e)
            {
                msg.Add("   Error! >> " + e.Message);
            }
            finally
            {
                outMsg = msg.ToArray();
            }

        }

        public bool CheckServerConnection(out string msg)
        {
            return filesTool.CheckConnection(out msg);
        }

        public int GetComPort()
        {
            return filesTool.GetComPort();
        }

        public void SaveComPort(int comport)
        {
            filesTool.SaveComPort(comport);
        }

        public bool GetUpdateCertificate(out DateTime time, out string msg)
        {
            try
            {
                if (lda.GetLastUpdateCertificate(out time))
                {
                    msg = "OK";
                    return true;
                }
                else
                {
                    //מכיון שאין אף אישור עדכניות, נוסיף אחד ראשוני
                    if (lda.InsertInitialCertificate(out time))
                    {
                        msg = "New certificate has been created";
                        return true;
                    }
                    else
                    {
                        msg = "Cannot create certificate";
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                time = new DateTime(2000, 1, 1);
                return false;
            }
        }

        public bool DownloadPoints(DateTime fromDate, out DbPoint[] serverNews, out string msg)
        {
            return filesTool.GetNewMeasurements(fromDate, out serverNews, out msg);
        }

        public bool GetServerTime(out DateTime serverTime)
        {
            return filesTool.GetServerTime(out serverTime);
        }

        public bool InsertNewCertificate(DateTime newTime, out string msg)
        {
            return lda.CertifyUpdate(newTime, out msg);
        }

        public string[] GetLocalPointDetails(int id)
        {
            object[] vals = lda.GetPoint(id);
            return new string[] { vals[0].ToString(), vals[1].ToString(), vals[2].ToString(), vals[3].ToString(), vals[4].ToString().Trim() };
        }

        public bool RenamePoint(int ptid, string newName)
        {
            return lda.RenamePoint(ptid, newName);
        }

        public bool UpdateStakout(List<int[]> pts, out string msg)
        {
            return filesTool.UpdateStakout(pts, out msg);
        }

        public void StopTps()
        {
            Console.WriteLine("------> Stopping Tps Connection...");
            try
            {
                actPrompt?.Invoke("מתנתק...");
                this.bTpsConnected = false; //save new status
                actTpsConnectionStatus?.Invoke(false, true); //inform user
                tpsLastAlive = false;

                if (!(tps is null))
                {
                    tps.cmdDisconnectTps();
                    System.Threading.Thread.Sleep(150);
                    tps.Stop();
                    tps.actAlive -= tpsAlive;
                    tps.actBattery -= tpsBattery;
                    tps = null;
                }
            }
            catch { }
        }

        public void cmdPointAt(double[] xyz)
        {
            //חישוב הזוויות לנקודה
            double HorizontalAngle, VerticalAngle;
            tps.GetRelativePosition(xyz, out HorizontalAngle, out VerticalAngle);

            //ביצוע הצבעה
            tps.cmdPointAt(HorizontalAngle, VerticalAngle);
        }
    }
}