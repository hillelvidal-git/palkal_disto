using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
//using ActiviewManagerCore;
//using ActiviewManagerCore.Objects.Calibration;
//using ActiviewManagerCore.Objects;

namespace WideField
{
    public partial class MainForm : Form
    {
        private WideFieldBL.BL myBL;
        private int BatteryCriticPercentage = 15; //האחוז שמתחתיו תוצג הודעת אזהרה לגבי הסוללה
        private int CountForDuration = 0;
        private double MeanDurationSec = 0;
        int LastPointedRow;
        Process checkProcess;
        int checkProcId;
        private bool _tpsConnectionOK;
        int ptClassID = 0;
        double MaxTilt;
        //bool bCheckLevel = true;
        //int MinLevelValue = 15;

        bool Debbuging_Offline = false;//true;

        //זמני - נטרול בדיקת הקשר שבכל חצי דקה
        bool CheckConnection = false;

        private bool tpsConnectionOK
        {
            get { return _tpsConnectionOK; }
            set
            {
                _tpsConnectionOK = value;
                //עדכון ערכי המצב - סוללה, פילוס, חיבור
                GetTpsStatus(value);
            }
        }

        private bool bTpsTilted
        {
            get
            {
                return myBL.bTpsTilted; ;
            }
            set
            {
                lblRepairTilt.Visible = !value;
                myBL.bTpsTilted = value;

                if (value)
                {
                    //המכשיר מפולס
                    lblTilt.Text = "המכשיר מפולס";
                }
                else
                {
                    //המכשיר לא מפולס
                    //lblTilt.Text = "המכשיר איננו מפולס";
                    //TODO: !!!!
                }
            }
        }

        private short _TpsBattery;
        private short BatteryCapacity
        {
            get
            {
                return _TpsBattery;
            }
            set
            {
                _TpsBattery = value;
                tsBattery.Value = (int)value;
                tsBattery.ToolTipText = "מצב סוללה: " + value.ToString() + "%";
                statusStrip1.Refresh();
                if (value <= BatteryCriticPercentage)
                {
                    AlertTip.BackColor = Color.Violet;
                    AlertTip.Show("נותרו רק " + value.ToString() + "%, יש להחליף סוללה בהקדם", (IWin32Window)statusStrip1, 5500);
                }

            }
        }

        public MainForm()
        {
            if (this.Debbuging_Offline) MessageBox.Show("שים לב - אתה עובד במצב ניפוי שגיאות", "Offline", MessageBoxButtons.OK, MessageBoxIcon.Information);

            InitializeComponent();
            this.chkChekLevel.Checked = false;
            this.myBL = new WideFieldBL.BL(this.tsAction, this.tsProgress);
            TpsConnection_Changed(new object(), new EventArgs()); //מצב התחלתי - מכשיר מנותק

            SyncDataFromServer(); //טעינת עץ הפרויקטים העדכני וסינכרון הנקודות בין המקומי והשרת
            SelectLastField();
            CheckPointExists();
            SayHello();
        }

        private bool CheckPointExists()
        {
            bool flag = false;

            ListViewGroup grp;
            if (ptClassID == 0) grp = lvFieldPoints.Groups["Srv"];
            else grp = lvLevelPoints.Groups[ptClassID - 1];

            int wanted = (int)this.nudPointNumber.Value;
            int ptNum;
            string ptText;

            foreach (ListViewItem itm in grp.Items)
            {
                try
                {
                    ptText = itm.Text;
                    ptNum = Convert.ToInt32(ptText);
                    if (ptNum == wanted) { flag = true; break; }
                }
                catch { }
            }

            if (flag) this.nudPointNumber.BackColor = Color.FromArgb(255, 192, 192);
            else this.nudPointNumber.BackColor = Color.FromKnownColor(KnownColor.Control);

            return flag;
        }

        private int SetNextNumber()
        {
            ListViewGroup grp;
            if (ptClassID == 0) grp = lvFieldPoints.Groups["Srv"];
            else grp = lvLevelPoints.Groups[ptClassID - 1];

            int max = 0; int ptNum;
            string ptText;

            foreach (ListViewItem itm in grp.Items)
            {
                ptText = itm.Text;
                ptNum = Convert.ToInt32(ptText);
                if (ptNum > max) max = ptNum;
            }
            return ++max;
        }

        private void SelectLastField()
        {
            //טעינת השדה בו עבדו לאחרונה
            try
            {
                string[] att = myBL.LoadLastField();
                TreeNode n;
                n = tvProjects.Nodes[att[0]];
                if (att[1] != "-1")
                    n = tvProjects.Nodes[att[0]].Nodes[att[1]];
                if (att[2] != "-1")
                    n = tvProjects.Nodes[att[0]].Nodes[att[1]].Nodes[att[2]];
                if (att[3] != "-1")
                    n = tvProjects.Nodes[att[0]].Nodes[att[1]].Nodes[att[2]].Nodes[att[3]];

                tvProjects.SelectedNode = n;

                try
                {
                    n.Parent.Expand();
                    n.Parent.Parent.Expand();
                    n.Parent.Parent.Parent.Expand();
                }
                catch { }
            }
            catch { }
        }

        private void SayHello()
        {
            int NowHour = DateTime.Now.Hour;
            if ((NowHour < 5) || (NowHour >= 20))
                tsAction.Text = "לילה טוב. ";
            else if (NowHour >= 17)
                tsAction.Text = "ערב טוב. ";
            else if (NowHour >= 12)
                tsAction.Text = "צהריים טובים. ";
            else
                tsAction.Text = "בוקר טוב. ";

            tsAction.Text += "ברוך הבא!";
        }

        private void TpsConnection_Changed(object sender, EventArgs e)
        {
            //לפני ניתוק כבה את ציין הלייזר
            if (!chkConnect.Checked) chkRedLaser.Checked = false;

            myBL.ConnectTps(chkConnect.Checked, "COM" + nudPortName.Value.ToString());

            if (myBL.bTpsConnected)
            {
                TpsConnected(true); //עדכן מצב - מחובר
                //שמירת מספר היציאה הטורית, לשימוש עתידי
                try { myBL.SaveComPort((int)nudPortName.Value); }
                catch { }
            }
            else
            {
                TpsConnected(false); //עדכן מצב - מנותק
                tsAction.Text = "ההתחברות נכשלה. מנותק.";
            }
        }

        private void TpsConnected(bool connected)
        {
            this.chkConnect.CheckedChanged -= new System.EventHandler(this.TpsConnection_Changed);
            chkConnect.Checked = connected;
            this.chkConnect.CheckedChanged += new System.EventHandler(this.TpsConnection_Changed);

            //אפשר או מנע את השימוש בכפתורים השונים בהתאם למצב החיבור
            chkRedLaser.Enabled = connected;
            chkTrack.Enabled = connected;
            chkPrism.Enabled = connected;
            chkAutoTarget.Enabled = connected;
            chkReflectorLess.Enabled = connected;
            chkAccuracy.Enabled = connected;
            //btnDoMeasure.Enabled = connected;
            btnMeasureBs.Enabled = connected;
            btnSetStation.Enabled = connected;
            btnImportStation.Enabled = connected;
            btnImportStation.Enabled = connected;
            groupBox11.Visible = connected;

            //אנימציה של חיבור
            pctConnected.Visible = connected;

            if (connected)
            {
                StatusTimer_Tick(new object(), new EventArgs());
                chkRedLaser.Checked = true; //לאחר חיבור נסה להפעיל את ציין הלייזר
                chkReflectorLess.Checked = true;
                tsAction.Text = "מחובר לדיסטומט";

                GetTpsStatus(true); // בודק ומציג את מצב הפילוס והסוללה

                //StatusTimer.Start(); //מפעיל שעון שבודק כל פרק זמן האם החיבור עדיין תקין
            }
            else
            {
                //StatusTimer.Stop();

                chkTrack.Checked = false;
                chkPrism.Checked = false;
                chkAutoTarget.Checked = false;
                chkReflectorLess.Checked = false;
                tsAction.Text = "מנותק";
            }

            //הצג את סמל החיבור
            tsConnection.Visible = connected;
        }

        private void chkRedLaser_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRedLaser.Checked) //הפעלת הציין
            {
                if (myBL.RedLaserSwitch(true)) //נסה להפעיל את ציין הלייזר
                {
                    tsAction.Text = "ציין הלייזר נדלק בהצלחה"; //הציין הופעל
                    tsRedLaser.Visible = true;

                    CheckTps(); //מכיוון שקיבלנו תשובה, ניתן לבצע בדיקת סוללה ופילוס

                }
                else //הפעלה נכשלה
                    chkRedLaser.Checked = false;
            }
            else //כיבוי הציין
            {
                myBL.RedLaserSwitch(false);
                tsRedLaser.Visible = false;
                tsAction.Text = "ציין הלייזר כבוי";
            }
        }

        private void tsAction_TextChanged(object sender, EventArgs e)
        {
            //בכל פעם שהטקסט משתנה יש לרענן את הפקד
            statusStrip1.Refresh();
        }

        private void MeasurePoint(object sender, EventArgs e)
        {
            if (!ReadyToMeasure())
                return;

            if ((chkAutoTarget.Checked) && (!FindPrism()))
                return;

            tsAction.Text = "מבצע מדידה...";

            StatusTimer.Stop();

            double[] pt = new double[3];
            bool UsePrism = chkPrism.Checked || chkAutoTarget.Checked;

            if (TryToMeasure(UsePrism, out pt))
            {
                tsAction.Text = "הנקודה נמדדה בהצלחה";
                CheckTps(); //מכיוון שקיבלנו תשובה, ניתן לבצע בדיקת סוללה ופילוס

                string errorMsg;
                WideFieldBL.DbPoint newPoint = CreateNewPoint(pt);
                if (SavePointToDB(newPoint, out errorMsg))
                {
                    myBL.TpsBeepNormal();
                    this.nudPointNumber.Value++;
                    AddPointToList(newPoint);
                }
                else
                {
                    tsAction.Text = "שמירת הנקודה נכשלה";
                    MessageBox.Show(errorMsg, "שמירת הנקודה נכשלה");
                }
            }
            else
                tsAction.Text = "מדידה נכשלה";

            StatusTimer.Start();
        }

        private void AddPointToList(WideFieldBL.DbPoint pt)
        {
            ListView lv; ListViewGroup grp;
            if (pt.ClassID == 0)
            {
                lv = lvFieldPoints;
                grp = lv.Groups[0];
            }
            else
            {
                lv = lvLevelPoints;
                grp = lv.Groups[pt.ClassID - 1];
            }

            ListViewItem itm = new ListViewItem();
            itm.Name = pt.ID.ToString();
            itm.Text = pt.Number;

            itm.BackColor = Color.Yellow;

            itm.ToolTipText = pt.Info + "   [" + pt.Time.ToString() + "]";
            itm.Group = grp;
            itm.ImageIndex = pt.ClassID;
            lv.Items.Add(itm);
        }

        private WideFieldBL.DbPoint CreateNewPoint(double[] position)
        {
            //השתמש במאפייני הטופס כדי לקבוע את ערכי הנקודה החדשה
            object[] values = new object[]
            {
                null, //ID
                this.myBL.CurrentAttribution[1], //LevelID
                -1,  //FieldID is -1 unless it's an SRV point
                this.ptClassID, //ClassID
                this.nudPointNumber.Value.ToString(), //Number
                this.tbPtInfo.Text.Trim(), //Info
                DateTime.Now, //time
                position[0], position[1], position[2], //position
                -1, //statusId=-1 -> point is new and hasn't been exported yet
                DateTime.Now, //Modified
            };

            //if it's an SRV point - set field ID
            if (this.ptClassID == 0) values[2] = this.myBL.CurrentAttribution[3];

            return new WideFieldBL.DbPoint(values);
        }

        private bool TryToMeasure(bool UsePrism, out double[] pt)
        {
            if (this.Debbuging_Offline)
            {
                //Demo: when working in debbuging mode, return an arbitrary position
                pt = new double[] { DateTime.Now.Millisecond / 10, DateTime.Now.Second / 6, DateTime.Now.Minute / 3 };
                return true;
            }

            try
            {
                if ((this.ptClassID == 0) && (chkReflectorLess.Checked)) //מדידת נקודת סקר ללא פריזמה
                    return myBL.MeasureBore(0.04, out pt);
                else
                    return myBL.DoMeasure(true, UsePrism, out pt); //מדידת נקודה רגילה
            }
            catch (Exception em)
            {
                MessageBox.Show(em.Message, "המדידה נכשלה");
                pt = new double[0];
                return false;
            }
        }

        private bool FindPrism()
        {
            //יש למצוא את הפריזמה לפני המדידה
            Color previousColor = this.tpSurvey.BackColor;

            try
            {
                tsAction.Text = "מחפש מטרה...";
                this.tpSurvey.BackColor = Color.Yellow; this.tpSurvey.Refresh();

                //נסה למצוא את הפריזמה
                if (myBL.AutoTargetSwitch(true))
                {
                    tsAction.Text = "מטרה נמצאה. מבצע מדידה...";
                    return true;
                }
                else
                {
                    throw new Exception("Cannot find Prism");
                }
            }
            catch
            {
                tsAction.Text = "מטרה לא נמצאה. מדידה בוטלה.";
                return false;
            }
            finally
            {
                this.tpSurvey.BackColor = previousColor;
            }
        }

        private bool ReadyToMeasure()
        {
            string tmp;
            return ReadyToMeasure(out tmp);
        }

        private bool ReadyToMeasure(out string resp)
        {
            string msg = "";

            if (!myBL.bTpsConnected && !this.Debbuging_Offline)
                msg += "\nהמכשיר איננו מקושר -";

            if (!EnsureLeveled())
                msg += "\nהמכשיר איננו מפולס -";

            if ((this.ptClassID == 0) && (myBL.CurrentAttribution[3] == -1))
                msg += "\nיש לבחור את השדה בו מודדים -";

            if ((this.ptClassID != 0) && (myBL.CurrentAttribution[1] == -1))
                msg += "\nיש לבחור את המפלס בו מודדים -";

            try { int t = (int)nudPointNumber.Value; }
            catch { msg += "\nשם הנקודה איננו חוקי -"; }

            if (CheckPointExists())
                msg += "\nכבר קיימת נקודה בשם זה -";

            resp = msg;

            if (msg != "")
            {
                string caption = "לא ניתן עדיין לבצע את המדידה";
                MessageBox.Show
                    (msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
                tsAction.Text = "לא ניתן לבצע מדידה";
                nudPointNumber.Focus();
                return false; ;
            }
            return true;
        }

        private void CheckTps()
        {
            //בדוק את מצב הפילוס והסוללה
            this.CheckConnection = true;
            try
            {
                this.StatusTimer_Tick(new object(), new EventArgs());
            }
            catch
            {

            }
            finally
            {
                this.CheckConnection = false;
            }
        }

        private bool SavePointToDB(WideFieldBL.DbPoint newPt, out string error)
        {
            bool nameExists, webidExists;
            return myBL.SaveNewPoint(newPt, out webidExists, out nameExists, out error);
        }

        private void chkTrack_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrack.Checked) //הפעלת נעילה ועקיבה
            {
                StatusTimer.Stop();

                chkPrism.Checked = true; //אם מפעילים עקיבה צריך לעבור למצב פריזמה
                if (myBL.TrackingSwitch(true)) //אם הושגה נעילה
                {
                    tsTrack.Visible = true;
                    tsAction.Text = "עקיבה הופעלה";
                }
                else
                    chkTrack.Checked = false; //נעילה נכשלה

                StatusTimer.Start();
            }
            else //ביטול נעילה
            {
                myBL.TrackingSwitch(false);
                tsTrack.Visible = false;
                tsAction.Text = "עקיבה בוטלה";
            }
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (!this.CheckConnection) return;

            if (myBL.bTpsConnected)
                GetTpsStatus(true);

            //BeginStatusCheck();
            //EndCheckTimer.Start();
        }

        public void GetTpsStatus(bool connected)
        {

            bool tilted;
            short battery;
            double[] TiltVals;

            if (!connected)
            {
                //אבד הקשר עם המכשיר
                AlertTip.ToolTipTitle = "הקשר נותק";
                AlertTip.Show("הקשר עם הדיסטומט אבד. עובר למצב מנותק", (IWin32Window)statusStrip1, 5500);
                //התנתק
                chkConnect.Checked = false;

                return;
            }
            else
            {
                //החיבור תקין
                //ועל כן ניתן לבדוק כעת את מצב הפילוס והסוללה

                myBL.CheckStatus(out battery, out tilted, out TiltVals);

                if (tilted)
                {
                    //המכשיר יצא מפילוס
                    AlertTip.ToolTipTitle = "בעיית פילוס";
                    AlertTip.Show("המכשיר יצא מפילוס. יש לפלסו שוב לפני המשך המדידות", (IWin32Window)statusStrip1, 5500);

                    this.bTpsTilted = true;

                    //TODO: האם צריך להתנתק?

                }
                else
                {
                    this.bTpsTilted = false;
                }

                //בכל מקרה, הצג את ערכי הפלס
                double angT1 = TiltVals[0] / Math.PI * 180 * 10000;
                double angT2 = TiltVals[1] / Math.PI * 180 * 10000;

                DisplayTpsStatus(angT1, angT2, battery);
            }
        }

        private void DisplayTpsStatus(double angT1, double angT2, short battery)
        {
            //הצג ערכי הפילוס
            lblCrossTilt.Text = angT1.ToString("0.0") + " Deg.(e-4)";
            lblLengthTilt.Text = angT2.ToString("0.0") + " Deg.(e-4)";
            int ct = (int)Math.Abs(angT1 * 2);
            int lt = (int)Math.Abs(angT2 * 2);
            if (ct > pbCrossTilt.Maximum) ct = pbCrossTilt.Maximum;
            if (lt > pbLengthTilt.Maximum) lt = pbLengthTilt.Maximum;
            pbCrossTilt.Value = ct;
            pbLengthTilt.Value = lt;

            //הצג מצב סוללה
            this.BatteryCapacity = battery;
        }

        private void BeginStatusCheck()
        {
            //התחל תהליך מקביל שאחראי לבדוק את הקשר עם הדיסטומט ולכתוב את התוצאה בקובץ טקסט

            ClearStatusLog();
            ProcessStartInfo stInfo = new ProcessStartInfo();
            stInfo.FileName = "c:\\TpsStatusChecker.exe";
            stInfo.CreateNoWindow = true;
            stInfo.WindowStyle = ProcessWindowStyle.Hidden;

            checkProcess = new Process();
            checkProcess.StartInfo = stInfo;
            checkProcess.Start();
            checkProcId = checkProcess.Id;
        }



        private string[] GetElements(string line)
        {
            string[] str = new string[6];
            int s = 0;
            for (int i = 0; i < 6; i++)
            {
                int l = line.IndexOf(",", s);
                str[i] = line.Substring(s, l - s);
                s = l + 1;
            }

            return str;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //אם הדיסטומט לא מקוון
            if (!myBL.bTpsConnected)
            {
                tsAction.Text = "הדיסטומט איננו מקוון. לא ניתן להפעיל התוויה";
                return;
            }

            //אם לא נבחרה נקודה
            if (dgvStakoutPoints.SelectedRows.Count < 1)
            {
                tsAction.Text = "יש לבחור נקודה להתוויה";
                return;
            }

            myBL.StopPlay = false;
            DataGridViewRow row = dgvStakoutPoints.SelectedRows[0];
            DoStakeOut(row, chkSlope.Checked);

            UpdateStakoutCount();
        }

        private void UpdateStakoutCount()
        {
            int marked = 0;
            foreach (DataGridViewRow row in dgvStakoutPoints.Rows)
                if ((bool)row.Cells["dgcMarked"].Value)
                    marked++;
            toolTip1.SetToolTip(prgMarked, "סומנו: " + marked.ToString() + " מתוך " + dgvStakoutPoints.Rows.Count.ToString());
            double percentage;

            if (dgvStakoutPoints.Rows.Count == 0)
                percentage = 0;
            else
                percentage = ((double)marked / (dgvStakoutPoints.Rows.Count)) * 100;

            prgMarked.Value = (int)percentage;
        }

        private void DoStakeOut(DataGridViewRow row, bool MeasuStartZ)
        {
            if (chkStartZ.Checked)
                MeasuStartZ = true;

            tsAction.Text = (string)row.Cells["dgcName"].Value + ": מתכונן להתוויה...";
            double[] xyz = new double[3];
            //קבל את שיעורי הנקודה
            try
            {
                xyz[0] = Convert.ToDouble(row.Cells["dgcX"].Value);
                xyz[1] = Convert.ToDouble(row.Cells["dgcY"].Value);
                xyz[2] = Convert.ToDouble(row.Cells["dgcZ"].Value);
            }
            catch
            {
                tsAction.Text = "הפעולה נכשלה - נתונים שגויים";
                return;
            }

            double finalZ;
            TimeSpan duration;
            bool StakeOutSucceeded = myBL.FindPoint((string)row.Cells["dgcName"].Value, xyz, chkSlope.Checked, (double)nudPercisionMm.Value, out finalZ, 0, MeasuStartZ, out duration); //ביצוע ההתוויה

            if (StakeOutSucceeded)
            {
                //ההתוויה הצליחה
                //הדלק לייזר אדום
                myBL.RedLaserSwitch(true);

                tsAction.Text = "נקודה " + (string)row.Cells["dgcName"].Value + " סומנה בהצלחה";
                row.Cells["dgcZ"].Value = finalZ.ToString("0.000");
                row.Cells["dgcZ"].Style.ForeColor = Color.DeepSkyBlue;
                row.Cells["dgcFound"].Value = true;
                row.Cells["dgcMarked"].Value = true;
                row.Cells["dgcName"].Style.BackColor = Color.Green;

                chkStartZ.Checked = false;

                //חישוב משך הזמן
                double DurationSeconds = duration.TotalMilliseconds / 1000;
                this.textBox3.Text = DurationSeconds.ToString("0.00"); //הצג את הזמן האחרון

                //חשב מחדש את הממוצע
                double TotalDuration = DurationSeconds + (this.CountForDuration * this.MeanDurationSec);
                this.CountForDuration++;
                this.MeanDurationSec = TotalDuration / this.CountForDuration;
                this.textBox4.Text = this.MeanDurationSec.ToString("0.00"); //הצג את הממוצע המעודכן
            }
            else
            {
                //ההתוויה נכשלה
                //כבה לייזר אדום
                myBL.RedLaserSwitch(false);
                tsAction.Text = "נקודה " + (string)row.Cells["dgcName"].Value + " לא נמצאה!";
                row.Cells["dgcName"].Style.BackColor = Color.Red;
            }

            //שמור את מספר השורה כדי לאפשר אחר כך לסמן אותה כלא סומנה
            LastPointedRow = row.Index;

            row.Selected = false;
            //בכל מקרה, קדם את הבחירה לנקודה הבאה אם יש עוד נקודות כמובן
            if (row.Index + 1 < dgvStakoutPoints.Rows.Count)
                dgvStakoutPoints.Rows[row.Index + 1].Selected = true;

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            //if (dataGridView1.SelectedRows.Count == 1)
            //    tsAction.Text = "נקודה אחת נבחרה";
            //else
            //tsAction.Text = "נבחרו " + dataGridView1.SelectedRows.Count.ToString() + " נקודות";
        }

        private void LoadStakeoutPts()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                lblStakeoutFile.Text = openFileDialog1.FileName;
                //נקה את הנתונים הקיימים
                dgvStakoutPoints.Rows.Clear();

                int news = 0; bool marked;
                using (StreamReader StakeOutFile = new StreamReader(lblStakeoutFile.Text))
                {
                    for (int i = 0; i < 3; i++)
                        StakeOutFile.ReadLine(); //Skip file header
                    string line;
                    string[] LineElements;
                    object[] RowParams = new object[7];

                    do
                    {
                        try
                        {
                            line = StakeOutFile.ReadLine();
                            LineElements = GetElements(line);
                            marked = Convert.ToBoolean(LineElements[5]);
                            RowParams = new object[] 
                        { 
                            LineElements[0], //ID
                            LineElements[1], //שם הנקודה
                            LineElements[2], //X
                            LineElements[3], //Y
                            LineElements[4], //Z
                            false, //Found
                            marked, //Marked
                            "" 
                        };
                            dgvStakoutPoints.Rows.Add(RowParams);
                            if (LineElements[1].ToString().Contains("[Plata!]"))
                                dgvStakoutPoints.Rows[dgvStakoutPoints.Rows.Count - 1].Cells[1].Style.BackColor = Color.PaleVioletRed;
                            if (!marked) news++;
                        }
                        catch { }
                    } while (!StakeOutFile.EndOfStream);
                }
                LastPointedRow = -1; //אפס את המצביע לסימון הנקודות
                chkStartZ.Checked = true; //הגדר מדידה לגובה התחלתי

                tsAction.Text = "קובץ הנתונים נטען, והוא מכיל " + news.ToString() + " נקודות להתוויה";
                UpdateStakoutCount();
            }
        }


        private void UpdateStakeoutFile(bool ShowMassage)
        {

            string line;
            int remain = 0;
            using (StreamWriter sw = new StreamWriter(lblStakeoutFile.Text))
            {
                sw.WriteLine("Stakeout Points - WideField");
                sw.WriteLine("Date: " + DateTime.Now.ToString());
                sw.WriteLine("");

                foreach (DataGridViewRow row in dgvStakoutPoints.Rows)
                {
                    line =
                        row.Cells[0].Value.ToString() + "," + //ID
                        row.Cells[1].Value.ToString() + "," + //Name
                        row.Cells[2].Value.ToString() + "," + //X
                        row.Cells[3].Value.ToString() + "," + //Y
                        row.Cells[4].Value.ToString() + "," + //Z
                        row.Cells[6].Value.ToString() + ",";  //Marked?

                    sw.WriteLine(line);
                    if (!Convert.ToBoolean(row.Cells[6].Value))
                        remain++;
                }

            }
            if (ShowMassage)
                MessageBox.Show("הקובץ עודכן בהצלחה\n" + saveFileDialog1.FileName + "\nנקודות שנותרו לסימון: " + remain.ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            myBL.StopPlay = true;
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (!myBL.bTpsConnected)
            {
                tsAction.Text = "הדיסטומט איננו מקוון. לא ניתן להפעיל התוויה";
                return;
            }

            if (myBL.bTpsTilted)
            {
                tsAction.Text = "המכשיר איננו מפולס. יש לפלס אותו לפני המשך ההתוויה";
                return;
            }


            //פיקוד באמצעות שלט-רחוק, 4 כפתורים
            DataGridViewRow RowToPoint;
            int index;

            //אם אין נקודות כלל - בטל
            if (dgvStakoutPoints.Rows.Count == 0)
                return;

            //אם נבחרה שורה, הצבע אליה
            if (dgvStakoutPoints.SelectedRows.Count > 0)
                RowToPoint = dgvStakoutPoints.SelectedRows[0];
            //אם לא נבחרה - התחל בשורה הראשונה
            else
                RowToPoint = dgvStakoutPoints.Rows[0];

            switch (e.KeyCode.ToString())
            {
                case "B":
                case "b":
                    //סמן את הנקודה האחרונה שנמצאה כלא סומנה    
                    if (LastPointedRow == -1)
                        return;
                    dgvStakoutPoints["dgcMarked", LastPointedRow].Value = false;
                    break;

                case "Next":
                    //הצבעה לנקודה הנוכחית, ואז העברת הפוינטר לנקודה הבאה

                    //בחירת השורה הנוכחית
                    RowToPoint.Selected = true;
                    //הפעלת הצבעה
                    button3_Click(new object(), new EventArgs());
                    //שים לב - הפונטר יקודם לשורה הבאה בסוף ההצבעה

                    break;

                case "PageUp":
                    //עבור לנקודה הקודמת
                    index = RowToPoint.Index - 1;
                    if (index < 0)
                        return;
                    RowToPoint = dgvStakoutPoints.Rows[index];

                    //בחירת השורה
                    RowToPoint.Selected = true;
                    //הפעלת הצבעה
                    button3_Click(new object(), new EventArgs());
                    //שים לב - הפונטר יקודם לשורה הבאה בסוף ההצבעה - ולכן נחזיר אותו
                    RowToPoint.Selected = true;

                    break;
            }

        }

        private void textBox5_Enter(object sender, EventArgs e)
        {
            textBox5.BackColor = Color.PaleTurquoise;
            tpStakeOut.BackColor = Color.PaleTurquoise;
            tsAction.Text = "שליטה מרחוק הופעלה";
        }

        private void textBox5_Leave(object sender, EventArgs e)
        {
            textBox5.BackColor = Color.FromKnownColor(KnownColor.Control);
            tpStakeOut.BackColor = Color.FromKnownColor(KnownColor.Control);
            tsAction.Text = "שליטה מרחוק בוטלה";
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            myBL.UpdateStation();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            this.nudPortName.Value = myBL.GetComPort();
        }

        private void btnMeasureBs_Click(object sender, EventArgs e)
        {
            //מדידת נקודת בסיס והוספתה לרשימת המטרות

            if (!myBL.bTpsConnected)
            {
                tsAction.Text = "הדיסטומט איננו מקוון";
                return;
            }

            if (!EnsureLeveled())
            {
                tsAction.Text = "המכשיר איננו מפולס. המדידה בוטלה";
                return;
            }

            WideFieldBL.Target newTarget;
            int BsId;
            string bsName = cmbBsName.Text;
            if (!GetBsId(bsName, out BsId))
            {
                tsAction.Text = "המדידה בוטלה. נקודה לא נמצאת במאגר";
                return;
            }

            if (myBL.MeasureTarget(BsId, bsName, out newTarget, chkBsPrism.Checked))
            {
                object[] TargetData = new object[11];
                //העתקת נתוני המטרה שנמדדה לשורה בטבלה
                TargetData[0] = true; //בשימוש
                TargetData[1] = dgvTargets.Rows.Count + 1; //מספר המטרה
                TargetData[2] = newTarget.pointNumber; //שם המטרה
                TargetData[3] = newTarget.Position[0]; //X
                TargetData[4] = newTarget.Position[1]; //Y
                TargetData[5] = newTarget.Position[2]; //Z
                TargetData[6] = newTarget.SlopeDistance.ToString();
                TargetData[7] = newTarget.HzAngle.ToString();
                TargetData[8] = newTarget.VAngle.ToString();
                TargetData[9] = newTarget.HzDist.ToString();
                TargetData[10] = newTarget.VDist.ToString();

                dgvTargets.Rows.Add(TargetData);
                tsAction.Text = "מטרה מס' " + TargetData[1].ToString() + " נמדדה בהצלחה";
            }
            else
            {
                tsAction.Text = "מדידת הנקודה נכשלה";
            }
        }

        private bool GetBsId(string name, out int id)
        {
            ListViewGroup bs = lvLevelPoints.Groups[0];
            id = -1;
            string ptNum;
            foreach (ListViewItem itm in bs.Items)
            {
                ptNum = itm.Text.Trim();
                if (ptNum == name)
                {
                    id = Convert.ToInt32(itm.Name);
                    return true;
                }
            }
            return false;
        }

        private bool EnsureLeveled()
        {
            if (!this.chkChekLevel.Checked)
                return true;

            bool tilted;
            short battery;
            double[] TiltVals;
            myBL.CheckStatus(out battery, out tilted, out TiltVals);
            double angT1 = TiltVals[0] / Math.PI * 180 * 10000;
            double angT2 = TiltVals[1] / Math.PI * 180 * 10000;
            DisplayTpsStatus(angT1, angT2, battery);

            double th = (double)this.nudMinLevel.Value;
            if ((Math.Abs(angT1) < th) && (Math.Abs(angT2) < th))
                return true;
            else
                return false;
        }

        private void btnCalcStation_Click(object sender, EventArgs e)
        {
            tsAction.Text = "מחשב תחנה...";
            if (!myBL.bTpsConnected)
            {
                tsAction.Text = "הדיסטומט איננו מקוון";
                return;
            }

            //עבור על הטבלה ואסוף את המטרות הרלבנטיות לחישוב
            List<WideFieldBL.Target> Targets = new List<WideFieldBL.Target>();
            foreach (DataGridViewRow row in dgvTargets.Rows)
            {
                if ((Convert.ToInt16(row.Cells["dgsUse"].Value) == 0))
                    continue;

                //העתק את נתוני המטרה
                WideFieldBL.Target t = new WideFieldBL.Target();
                t.pointNumber = Convert.ToInt32(row.Cells["dgsName"].Value);
                t.Position = new double[]
                    {
                        Convert.ToDouble(row.Cells["dgsX"].Value),
                        Convert.ToDouble(row.Cells["dgsY"].Value),
                        Convert.ToDouble(row.Cells["dgsZ"].Value)
                    };
                t.SlopeDistance = Convert.ToDouble(row.Cells["dgsSlopeDist"].Value);
                t.HzAngle = Convert.ToDouble(row.Cells["dgsHzAng"].Value);
                t.VAngle = Convert.ToDouble(row.Cells["dgsVAng"].Value);
                t.HzDist = Convert.ToDouble(row.Cells["dgsHzDist"].Value);
                t.VDist = Convert.ToDouble(row.Cells["dgsVDist"].Value);

                //הוסף את המטרה לרשימה
                Targets.Add(t);
            }

            if (Targets.Count < 2)
            {
                tsAction.Text = "יש למדוד לפחות שתי מטרות על מנת להתמקם";
                return;
            }

            double[] newPos;
            if (myBL.SetStationByResection(Targets, out newPos))
            {
                tsAction.Text = "התחנה נקבעה בהצלחה";
                //הצג את מיקום התחנה
                //מחק את תצוגת המטרות שנמדדו
                dgvTargets.Rows.Clear();
            }
            else
                tsAction.Text = "חישוב התחנה לא הצליח";

        }


        private void StationTab_Enter(object sender, EventArgs e)
        {
            if (cmbBsName.Items.Count == 0)
            {
                tabControl2.SelectedIndex = 1;
                tabControl2.TabPages[1].Parent.Focus();
            }
            else
            {
                tabControl2.SelectedIndex = 0;
                tabControl2.TabPages[0].Parent.Focus();
            }
        }

        private void LoadBsList()
        {
            string ptNumber;
            this.cmbBsName.Items.Clear();
            foreach (ListViewItem bs in lvLevelPoints.Groups["Bs"].Items)
            {
                ptNumber = bs.Text;
                cmbBsName.Items.Add(ptNumber);
            }
        }

        private void btnRefreshTree_Click(object sender, EventArgs e)
        {
            SyncDataFromServer();
        }

        private void SyncDataFromServer()
        {
            SyncWizard sw = new SyncWizard(myBL, this.tvProjects.Nodes, this.lblTreeTime);
            sw.ShowDialog();

            LoadLevelPoints(myBL.CurrentAttribution[1]);
            LoadFieldPoints(myBL.CurrentAttribution[3]);

            SelectLastField();
        }

        private void AfterAreaSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode n = tvProjects.SelectedNode;
            if (n == null) return;

            int[] newAtt = new int[] { -1, -1, -1, -1 };

            string newProjectName, newLevelName, newBlocName, newFieldName;
            newProjectName = newLevelName = newBlocName = newFieldName = "";

            switch (n.Level)
            {
                case 0:
                    newAtt[0] = Convert.ToInt32(n.Name); newProjectName = n.Text;
                    break;

                case 1:
                    newAtt[1] = Convert.ToInt32(n.Name); newLevelName = n.Text;
                    newAtt[0] = Convert.ToInt32(n.Parent.Name); newProjectName = n.Parent.Text;
                    break;

                case 2:
                    newAtt[2] = Convert.ToInt32(n.Name); newBlocName = n.Text;
                    newAtt[1] = Convert.ToInt32(n.Parent.Name); newLevelName = n.Parent.Text;
                    newAtt[0] = Convert.ToInt32(n.Parent.Parent.Name); newProjectName = n.Parent.Parent.Text;
                    break;

                case 3:
                    newAtt[3] = Convert.ToInt32(n.Name); newFieldName = n.Text;
                    newAtt[2] = Convert.ToInt32(n.Parent.Name); newBlocName = n.Parent.Text;
                    newAtt[1] = Convert.ToInt32(n.Parent.Parent.Name); newLevelName = n.Parent.Parent.Text;
                    newAtt[0] = Convert.ToInt32(n.Parent.Parent.Parent.Name); newProjectName = n.Parent.Parent.Parent.Text;
                    break;
            }

            DisplayAttribution(newProjectName, newLevelName, newBlocName, newFieldName);

            //load field points
            myBL.CurrentAttribution[3] = newAtt[3];
            LoadFieldPoints(newAtt[3]);
            myBL.CurrentAttribution[2] = newAtt[2];

            if (myBL.CurrentAttribution[1] != newAtt[1])
            {
                //load level points
                myBL.CurrentAttribution[1] = newAtt[1];
                LoadLevelPoints(newAtt[1]);
                if (myBL.CurrentAttribution[0] != newAtt[0])
                {
                    myBL.CurrentAttribution[0] = newAtt[0];
                }
            }


            myBL.SaveLastField();
            this.nudPointNumber.Value = SetNextNumber();
        }

        private void LoadFieldPoints(int fieldId)
        {
            lvFieldPoints.Items.Clear();
            if (fieldId == -1)
            {
                this.pnlFieldPoints.Visible = false;
                return;
            }

            WideFieldBL.DbPoint[] SrvPoints;
            this.myBL.GetFieldPoints(fieldId, out SrvPoints);

            ListViewItem itm;

            foreach (WideFieldBL.DbPoint srv in SrvPoints)
            {
                itm = new ListViewItem();
                itm.Name = srv.ID.ToString();
                itm.Text = srv.Number;
                if (srv.StatusID == -1)
                {
                    itm.BackColor = Color.Yellow;
                }

                itm.ToolTipText = srv.Info + "   [" + srv.Time.ToString() + "]";
                itm.Group = lvFieldPoints.Groups["Srv"];
                itm.ImageIndex = 0;
                lvFieldPoints.Items.Add(itm);
            }

            this.pnlFieldPoints.Visible = true;
        }

        private void LoadLevelPoints(int levelId)
        {
            lvLevelPoints.Items.Clear();
            if (levelId == -1)
            {
                this.pnlLevelPoints.Visible = false;
                return;
            }

            WideFieldBL.DbPoint[] Points;
            this.myBL.GetLevelPoints(levelId, out Points);

            ListViewItem itm;

            foreach (WideFieldBL.DbPoint pt in Points)
            {
                if (pt.ClassID == 0) continue;
                itm = new ListViewItem();
                itm.Name = pt.ID.ToString();
                itm.Text = pt.Number;
                if (pt.StatusID == -1) itm.BackColor = Color.Yellow;

                itm.ToolTipText = pt.Info + "   [" + pt.Time.ToString() + "]";
                itm.Group = lvLevelPoints.Groups[pt.ClassID - 1];
                itm.ImageIndex = pt.ClassID;
                lvLevelPoints.Items.Add(itm);
            }

            this.pnlLevelPoints.Visible = true;

            //מלא את רשימת ה-BS בדף התחנה
            LoadBsList();
        }

        private void DisplayAttribution(string newProjectName, string newLevelName, string newBlocName, string newFieldName)
        {
            this.tbProject.Text = newProjectName;
            this.textBox10.Text = newProjectName;

            tbLevel.Text = newLevelName;
            textBox9.Text = newLevelName;
            tbLevel.Text = newLevelName;

            tbField.Text = newFieldName;
            if (newFieldName == "0")
                tbField.Text = "---";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            double[] XYZ;
            double orientaionAngle;
            try
            {
                XYZ = new double[]{ Convert.ToDouble(textBox15.Text.Trim()),
               Convert.ToDouble(textBox11.Text.Trim()),
                Convert.ToDouble(textBox16.Text.Trim())};

                orientaionAngle = Convert.ToDouble(textBox17.Text.Trim());

            }
            catch
            {
                tsAction.Text = "הנתונים שהוזנו אינם תקינים";
                return;
            }


            try
            {
                if (myBL.SetStationByValues(XYZ, orientaionAngle))
                {
                    MessageBox.Show("התחנה נקבעה בהצלחה.\nכעת יש למדוד מיד נקודות בסיס, ולסמנן בשטח\nעל מנת להתמקם בפעמים הבאות");
                    tsAction.Text = "התחנה נקבעה בהצלחה";
                }
                else
                {
                    tsAction.Text = "קביעת התחנה נכשלה";
                }
            }
            catch
            {
                tsAction.Text = "קביעת התחנה נכשלה";
            }
        }

        private void dataGridView3_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //רק נקודות חדשות
        }

        private void ClearStatusLog()
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TpsStatus.txt";
            using (StreamWriter sw = new StreamWriter(file))
                sw.WriteLine("Cleared : " + DateTime.Now.ToString());
        }

        private void EndCheckTimer_Tick(object sender, EventArgs e)
        {
            //עצירת תהליך המשנה, אם הוא עדין רץ, כי זה אומר שהוא נתקע
            foreach (Process runningProc in Process.GetProcesses())
                if (runningProc.Id == this.checkProcId)
                {
                    checkProcess.Kill();
                    break;
                }
            checkProcess = null;

            //עצירת הטיימר הקטן
            EndCheckTimer.Stop();

            //קריאת התוצאה של בדיקת המצב מתוך הקובץ
            this.tpsConnectionOK = ReadStatusLog();
        }

        private bool ReadStatusLog()
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TpsStatus.txt";
            string line;
            using (StreamReader sw = new StreamReader(file))
                line = sw.ReadLine();

            return (line.StartsWith("OK"));
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            GetTpsStatus(true);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ליתר בטחון, הפסק את תהליך בקידת המצב, למקרה שהוא עדין פעיל
            try
            {
                this.checkProcess?.Kill();
                //this.activiewClient.Stop();
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateStakeoutFile();
        }

        private void UpdateStakeoutFile()
        {
            UpdateStakeoutFile(true);
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            LoadStakeoutPts();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox1.BackColor = Color.PaleTurquoise;
            tpSurvey.BackColor = Color.PaleTurquoise;
            tsAction.Text = "שליטה מרחוק הופעלה";
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            textBox1.BackColor = Color.FromKnownColor(KnownColor.Control);
            tpSurvey.BackColor = Color.FromKnownColor(KnownColor.Control);
            tsAction.Text = "שליטה מרחוק בוטלה";
        }

        private void RemoteControl_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                tsAction.Text = "";
                switch (e.KeyCode.ToString())
                {
                    case "B":
                    case "b":
                    case "OemPeriod":
                        MeasurePoint(new object(), new EventArgs());
                        break;
                    case "F5":
                        chkRedLaser.Checked = !chkRedLaser.Checked;
                        break;

                    case "Next":
                        this.nudPointNumber.Value++;
                        break;

                    case "PageUp":
                        this.nudPointNumber.Value--;
                        break;

                }

            }

            catch
            {
                tsAction.Text = "הפעולה לא הצליחה";
            }
        }

        private void btnGoToBS_Click(object sender, EventArgs e)
        {
            if (!myBL.bTpsConnected) return;
            if (cmbBsName.SelectedIndex < 0) return;

            string bsName = cmbBsName.SelectedItem.ToString();
            int pointId;
            if (!GetBsId(bsName, out pointId))
            {
                tsAction.Text = "הנקודה BS " + bsName + "לא נמצאה";
                return;
            }

            double[] bsLocation;
            myBL.GetPointXYZ(pointId, out bsLocation);
            myBL.PointAt(bsLocation);
        }

        private void nudPointNumber_ValueChanged(object sender, EventArgs e)
        {
            CheckPointExists();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (dgvStakoutPoints.SelectedRows.Count != 1)
            {
                MessageBox.Show("יש לבחור נקודה אחת");
                return;
            }
            if (!myBL.bTpsConnected)
            {
                MessageBox.Show("המכשיר איננו מחובר");
                return;
            }

            double[] xyz = new double[]
            {
                Convert.ToInt32(dgvStakoutPoints.SelectedRows[0].Cells[7].Value),
                Convert.ToInt32(dgvStakoutPoints.SelectedRows[0].Cells[8].Value),
                Convert.ToInt32(dgvStakoutPoints.SelectedRows[0].Cells[9].Value)
            };
            myBL.PointAt(xyz);
        }

        private bool ParsePointLine(string p, out int[] att, out double[] pos)
        {
            att = new int[3];
            pos = new double[3];

            int e = 0;
            int s = 0;
            string[] parts = new string[6];
            //p += ",";

            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == ',')
                {
                    parts[e] = p.Substring(s, i - s);

                    e++;
                    if (e == 6)
                    {
                        att[0] = Convert.ToInt32(parts[0]); //סוג הנקודה
                        att[1] = Convert.ToInt32(parts[1]); //מספר השדה
                        att[2] = Convert.ToInt32(parts[2]); //מספר הנקודה

                        pos[0] = Convert.ToDouble(parts[3]); //XYZ...
                        pos[1] = Convert.ToDouble(parts[4]);
                        pos[2] = Convert.ToDouble(parts[5]);

                        return true;
                    }
                    s = i + 1;
                }
            }
            return false;
        }

        private void btnCalculateStation_Click(object sender, EventArgs e)
        {
            if (dgvTargets.Rows.Count < 2)
            {
                tsAction.Text = "טרם נמדדו מספיק מטרות";
                return;
            }

            List<global::WideFieldBL.TargetData> targets = new List<WideFieldBL.TargetData>();
            foreach (DataGridViewRow row in dgvTargets.Rows)
            {
                try
                {
                    if (!Convert.ToBoolean(row.Cells[0].Value)) continue; // Use = False

                    global::WideFieldBL.TargetData t = new WideFieldBL.TargetData();
                    t.Number = Convert.ToInt32(row.Cells[1].Value);
                    t.BsNumber = Convert.ToInt32(row.Cells[2].Value);
                    t.Position = new double[]
                        {  
                            Convert.ToDouble(row.Cells[3].Value),
                            Convert.ToDouble(row.Cells[4].Value),
                            Convert.ToDouble(row.Cells[5].Value)
                        };

                    t.HzAngle = Convert.ToDouble(row.Cells[7].Value);
                    t.VAngle = Convert.ToDouble(row.Cells[8].Value);
                    t.HzDist = Convert.ToDouble(row.Cells[9].Value);
                    t.VDist = Convert.ToDouble(row.Cells[10].Value);

                    targets.Add(t);
                }
                catch
                {
                    MessageBox.Show("Parse Error. Target Skipped: " + row.Index.ToString());
                }
            }

            if (myBL.Locate(targets))
            {
                dgvTargets.Rows.Clear();
                tsAction.Text = "תחנה נקבעה בהצלחה";
            }
        }

        private void chkBsPrism_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBsPrism.Checked)
            {
                chkBsPrism.Size = new Size(48, 48);
                tsAction.Text = "מדידת BS באמצעות פריזמה";
            }
            else
            {
                chkBsPrism.Size = new Size(38, 38);
                tsAction.Text = "מדידת BS באמצעות פריזמה בוטלה!";
            }
        }

        private void PtClass_CheckedChanged(object sender, EventArgs e)
        {
            switch (((Control)sender).Text)
            {
                case "SRV": this.ptClassID = 0; this.nudPointNumber.ForeColor = Color.Red; break;
                case "BS": this.ptClassID = 1; this.nudPointNumber.ForeColor = Color.FromArgb(0, 192, 0); break;
                case "OR": this.ptClassID = 2; this.nudPointNumber.ForeColor = Color.DodgerBlue; break;
                case "GEN": this.ptClassID = 3; this.nudPointNumber.ForeColor = Color.Gray; break;
            }

            tbPtInfo.Text = "";
            try
            {
                tbField.Visible = (this.ptClassID == 0);
                label11.Visible = (this.ptClassID == 0);
                if (this.ptClassID == 0)
                {
                    if (myBL.bTpsConnected)
                    {
                        chkAutoTarget.Checked = true;
                    }
                }
                else //אם זה לא SRV
                {
                    if (myBL.bTpsConnected)
                    {
                        chkRedLaser.Checked = true;
                    }
                }

                this.nudPointNumber.Value = SetNextNumber();
            }
            catch { }

        }

        private void cmsPoints_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip cm = (ContextMenuStrip)sender;
            ListView lv = (ListView)cm.SourceControl;
            if (lv.SelectedItems.Count != 1) e.Cancel = true;
            else if (lv.SelectedItems[0].BackColor != Color.Yellow) e.Cancel = true; //ניתן לבצע פעולות רק על נקודות חדשות
        }

        private void tsmiDelete_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            ContextMenuStrip cm = (ContextMenuStrip)tsmi.Owner;
            ListView lv = (ListView)cm.SourceControl;
            if (lv.SelectedItems.Count == 0) return;
            ListViewItem itm = lv.SelectedItems[0];
            //למחוק רק נקודות חדשות!!!

            if (MessageBox.Show("You're about to get rid of point No. " + itm.Text + " of type '" + itm.Group.Header + "'\nAre You Sure??", "Delete a Point", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                string msg;
                if (myBL.DeletePoint(Convert.ToInt32(itm.Name), out msg))
                    itm.Remove();
                //anyway...
                MessageBox.Show(msg, "Delete a point");
            }
        }

        private void שנהשםToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ListView lv = (ListView)cmsPoints.SourceControl;
                if (lv.SelectedItems.Count == 0) return;
                ListViewItem itm = lv.SelectedItems[0];
                lv.LabelEdit = true;
                itm.BeginEdit();
            }
            catch
            {

            }

        }

        private void lvLevelPoints_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string msg = "";
            ListView lv = (ListView)sender;
            try
            {
                string newname = e.Label;
                int id = Convert.ToInt32(lv.Items[e.Item].Name);
                if (!myBL.RenamePoint(id, newname)) throw new Exception("לא ניתן לשנות את שם הנקודה. נסה שם אחר");
                msg = "שם הנקודה השתנה בהצלחה";
            }
            catch (Exception ee)
            {
                msg = ee.Message;
                e.CancelEdit = true;
            }
            finally
            {
                lv.LabelEdit = false;
                MessageBox.Show(msg);
            }
        }

        private void btnUpdateSoFile_Click(object sender, EventArgs e)
        {
            UpdateStakeoutFile();
        }

        private void btnUpdateStakeout_Click(object sender, EventArgs e)
        {
            //שמור את הנתונים לקובץ הנקודות
            UpdateStakeoutFile(false);

            string msg;
            if (!myBL.CheckServerConnection(out msg))
            {
                MessageBox.Show("העדכון נכשל\n" + msg, "אין תשובה מהשרת");
                return;
            }

            //עדכן את השרת אודות הנקודות שסומנו
            List<int[]> pts = new List<int[]>();
            int id, status; bool marked;

            //קריאת הנתונים מתוך הרשימה
            foreach (DataGridViewRow row in this.dgvStakoutPoints.Rows)
            {
                id = Convert.ToInt32(row.Cells[0].Value);
                marked = Convert.ToBoolean(row.Cells[6].Value);
                if (marked) status = 1; else status = 0;
                pts.Add(new int[] { id, status });
            }

            //שליחת הנתונים לעדכון בשרת
            if (!myBL.UpdateStakout(pts, out msg))
            {
                MessageBox.Show(msg, "העדכון נכשל");
                return;
            }
            else
            {
                MessageBox.Show(msg, "תוצאות העדכון");
                return;
            }
        }

        private void btnGetPointFromSite_Click(object sender, EventArgs e)
        {
            Process.Start("http://google.com");
        }

        //ActiviewManagerCore.SocketClient activiewClient;
        private void btnActiview_Click(object sender, EventArgs e)
        {
            //string activiewServerAddress = "ws://" + this.tbActiviewServerAddress.Text + ":2013";
            //this.activiewClient = new ActiviewManagerCore.SocketClient(activiewServerAddress);
            //activiewClient.MessageReceived += ActiviewMessageReceived;
            //activiewClient.ClientOpened += ActiviewClientOpened;
            //activiewClient.Start();
        }

        private void ActiviewClientOpened()
        {
            //this.activiewClient.Send(Consts.IS_DISTOMAT_CLIENT);
        }

        //private void ActiviewMessageReceived(string incomeMsg)
        //{

        //    string[] TypeData;
        //    Serializer.GetTypeData(incomeMsg, out TypeData);
        //    if (Serializer.Is(typeof(DistomatRequestVector3), TypeData[0]))
        //    {
        //        double[] newPt;

        //        DistomatRequestVector3 request = Serializer.Deserialize<DistomatRequestVector3>(TypeData[1]);
        //        DistomatAnswerVector3 answer = new DistomatAnswerVector3();

        //        //Do Measurement
        //        if (request.RequestID.StartsWith("SELF_POSITION"))
        //        {
        //            answer.Success = GetSelfPositionForServer(out newPt, out answer.ErrorMsg);

        //        }
        //        else
        //        {
        //            answer.Success = MeasurePointForServer(out newPt, out answer.ErrorMsg);
        //        }

        //        answer.RequestID = request.RequestID;
        //        answer.Position = new Vector3();

        //        if (answer.Success)
        //        {
        //            answer.Position.X = (float)newPt[0];
        //            answer.Position.Y = (float)newPt[1];
        //            answer.Position.Z = (float)newPt[2];
        //        }
        //        else
        //        {
        //            answer.Position.X = -1;
        //            answer.Position.Y = -1;
        //            answer.Position.Z = -1;
        //        }

        //        this.activiewClient.Send(Serializer.SerializeTypeData(answer));

        //    }
        //}

        private bool GetSelfPositionForServer(out double[] newPt, out string p)
        {
            try
            {
                newPt = this.myBL.GetStation();
                p = "ok";
                return true;
            }
            catch (Exception e)
            {
                newPt = new double[] { -1, -1, -1 };
                p = "error: " + e.Message;
                return false;
            }
        }

        private bool MeasurePointForServer(out double[] pt, out string resp)
        {
            if (!ReadyToMeasure(out resp))
            {
                pt = new double[0];
                return false;
            }

            if ((chkAutoTarget.Checked) && (!FindPrism()))
            {
                pt = new double[0];
                resp = "הפריזמה לא נמצאה";
                return false;
            }

            tsAction.Text = "מבצע מדידה...";

            StatusTimer.Stop();

            bool UsePrism = chkPrism.Checked || chkAutoTarget.Checked;

            if (TryToMeasure(UsePrism, out pt))
            {
                tsAction.Text = "הנקודה נמדדה בהצלחה";
                CheckTps(); //מכיוון שקיבלנו תשובה, ניתן לבצע בדיקת סוללה ופילוס
                StatusTimer.Start();
                resp = "OK";
                return true;
            }
            else
            {
                tsAction.Text = "מדידה נכשלה";
                resp = "המדידה נכשלה";
                StatusTimer.Start();
                return false;
            }

        }



    } //Class
} //Namespace
