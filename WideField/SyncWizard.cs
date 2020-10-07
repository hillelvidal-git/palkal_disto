using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WideField
{
    public partial class SyncWizard : Form
    {
        PictureBox[] pbs;
        Label[] lbls;
        List<string> log;

        WideFieldBL.BL myBL;
        TreeNodeCollection nodes;
        Label treeLbl;

        public SyncWizard(WideFieldBL.BL bl, TreeNodeCollection TreeNodes, Label lblTreeTime)
        {
            InitializeComponent();
            this.myBL = bl;
            this.nodes = TreeNodes;
            this.treeLbl = lblTreeTime;

            pbs = new PictureBox[] { pb1, pb2, pb3, pb4, pb5, pb6, pb0, pb7 };
            lbls = new Label[] { lbl1, lbl2, lbl3, lbl4, lbl5, lbl6, lbl0, lbl7 };
            log = new List<string>();
        }

        private void SyncWizard_Load(object sender, EventArgs e)
        {
        }

        public void DoSync()
        {
            WideFieldBL.DbPoint[] localNews;
            WideFieldBL.DbPoint[] serverNews;
            DateTime serverTime, certificateTime;
            string msg;
            string[] msgLong;
            int uploaded, errors, downloaded;
            List<int> downloadConflict = new List<int>();
            List<int> uploadConflict = new List<int>();
            bool allOk = true;

            log.Add(DateTime.Now + "  Starting Synchronization...");

            log.Add("Checking connection to server");
            pbs[0].Image = imageList1.Images[1]; //web
            pbs[0].Refresh();

            lbls[0].Enabled = true;
            if (!myBL.CheckServerConnection(out msg))
            {
                log.Add("Connection failed: " + msg);
                pbs[0].Image = imageList1.Images[3]; //error
                DownloadTree(out msg, false); //Download old tree
                return;
            }
            pbs[0].Image = imageList1.Images[0]; //done
            pbs[0].Refresh();
            log.Add("[OK]");

            log.Add("");
            log.Add("Reading Update certificate from local Db...");
            pbs[6].Image = imageList1.Images[4]; pbs[1].Refresh(); //working
            lbls[6].Enabled = true;
            if (!myBL.GetUpdateCertificate(out certificateTime, out msg))
            {
                log.Add("Reading failed: " + msg);
                pbs[6].Image = imageList1.Images[3];
                return;
            }
            pbs[6].Image = imageList1.Images[0]; //done
            pbs[6].Refresh();
            log.Add("[OK] " + certificateTime);
            res0.Text = certificateTime.ToString();

            log.Add("");
            log.Add("Downloading projects tree...");
            pbs[1].Image = imageList1.Images[4]; pbs[1].Refresh(); //working
            lbls[1].Enabled = true;
            if (!DownloadTree(out msg, true))
            {
                log.Add("Downloading Tree failed: " + msg);
                pbs[1].Image = imageList1.Images[3];
                return;
            }
            pbs[1].Image = imageList1.Images[0]; //done
            pbs[1].Refresh();
            log.Add("[OK]");

        Upload:
            log.Add("");
            log.Add("Retrieving local new points...");
            pbs[2].Image = imageList1.Images[4]; pbs[2].Refresh();  //working
            lbls[2].Enabled = true;
            if (!myBL.GetLocalNewsList(out localNews, out msg))
            {
                log.Add("Retrieving local news failed: " + msg);
                pbs[2].Image = imageList1.Images[3]; pbs[2].Refresh();
                goto Download;
            }
            pbs[2].Image = imageList1.Images[0]; //done
            pbs[2].Refresh();
            log.Add("[OK]" + "   Points found: " + localNews.Length);
            res3.Text = localNews.Length.ToString();

            if (localNews.Length > 0)
            {
                log.Add("");
                log.Add("Uploading local " + localNews.Length + " new points...");
                pbs[3].Image = imageList1.Images[4]; pbs[3].Refresh();   //working
                lbls[3].Enabled = true;
                myBL.UploadPoints(localNews, out msgLong, out uploaded, out errors, out uploadConflict);
                log.AddRange(msgLong);
                log.Add("Total: " + uploaded + " uploaded, " + errors + " errors");
                res4.Text = uploaded.ToString();

                if (errors == 0)
                {
                    pbs[3].Image = imageList1.Images[0]; //done
                }
                else
                {
                    pbs[3].Image = imageList1.Images[2]; //errors
                    allOk = false;
                }

            }
            else
            {
                log.Add("No points to upload.");
                pbs[3].Image = imageList1.Images[0]; //done
            }

        Download:
            pbs[3].Refresh();
            log.Add("");
            log.Add("Retrieving server's new points...");
            pbs[4].Image = imageList1.Images[4]; pbs[4].Refresh();   //working
            lbls[4].Enabled = true;
            if (!myBL.GetServerTime(out serverTime))
            {
                log.Add("Cannot get server time");
                pbs[4].Image = imageList1.Images[3];
                allOk = false;
                goto End;
            }

            if (!myBL.DownloadPoints(certificateTime, out serverNews, out msg))
            {
                log.Add("Retrieving server's news failed: " + msg);
                pbs[4].Image = imageList1.Images[3];
                allOk = false;
                goto End;
            }
            pbs[4].Image = imageList1.Images[0]; //done
            pbs[4].Refresh();
            log.Add("[OK]" + "   Points found: " + serverNews.Length);
            res5.Text = serverNews.Length.ToString();

            if (serverNews.Length == 0)
            {
                log.Add("No points to insert.");
                pbs[5].Image = imageList1.Images[0]; //done
                goto Certify;
            }

            log.Add("");
            log.Add("Inserting new points into local Db...");
            pbs[5].Image = imageList1.Images[4]; pbs[5].Refresh();   //working
            lbls[5].Enabled = true;
            if (!myBL.InsertPointsToLocal(serverNews, out msgLong, out downloaded, out errors, out downloadConflict))
            {
                log.Add("Error inserting points to local Db:");
                log.AddRange(msgLong);
                pbs[5].Image = imageList1.Images[3];
                allOk = false;
                goto End;
            }
            log.AddRange(msgLong);
            log.Add("Total: " + downloaded + " downloaded, " + errors + " errors");
            res6.Text = downloaded.ToString();

            if (errors == 0)
            {
                pbs[5].Image = imageList1.Images[0]; //done
            }
            else
            {
                pbs[5].Image = imageList1.Images[2]; //errors
                allOk = false;
            }

        Certify:
            if (!allOk)
            {
                log.Add("Process has not completed. Cannot write a new certificate.");
                goto End;
            }

            log.Add("");
            log.Add("Writing new certificate to local Db...");
            pbs[7].Image = imageList1.Images[1]; //web
            pbs[7].Refresh();

            lbls[7].Enabled = true;
            if (!myBL.InsertNewCertificate(serverTime, out msg))
            {
                log.Add("Certifing failed: " + msg);
                pbs[7].Image = imageList1.Images[3]; //error
                goto End;
            }
            pbs[7].Image = imageList1.Images[0]; //done
            pbs[7].Refresh();
            log.Add("[OK] " + serverTime);
            res7.Text = serverTime.ToString();


        End:
            log.Add("");
            log.Add("--End--");

            if (uploadConflict.Count > 0) TreatConflicts(uploadConflict);
        }

        private void TreatConflicts(List<int> conflicts)
        {
            List<string[]> points = new List<string[]>();
            foreach (int id in conflicts)
            {
                points.Add(GetLocalPointDetails(id));
            }

            ConflictsForm cf = new ConflictsForm(points.ToArray());
            if (cf.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            int renamed = 0; int deleted = 0;
            int ptid; string newName; string msg;
            foreach (DataGridViewRow row in cf.dataGridView1.Rows)
            {
                try
                {
                    ptid = Convert.ToInt32(row.Cells[0].Value);
                    newName = Convert.ToString(row.Cells[6].Value);
                    if (string.IsNullOrEmpty(newName)) continue;

                    if (row.Cells[5].Value == "שנה שם")
                    {
                        //שנה שם נקודה
                        if (myBL.RenamePoint(ptid, newName)) renamed++;
                    }
                    else
                    {
                        //מחק נקודה
                        if (myBL.DeletePoint(ptid, out msg)) deleted++;
                    }
                }
                catch
                {
                    continue;
                }
            }

            MessageBox.Show("Renamed: " + renamed + ",  Deleted: " + deleted);
        }

        private string[] GetLocalPointDetails(int id)
        {
            return this.myBL.GetLocalPointDetails(id);
        }

        private bool DownloadTree(out string msg, bool tryServer)
        {
            try
            {
                bool downloaded;
                BedekTreeAdapter.TreeAdapter ta = new BedekTreeAdapter.TreeAdapter();
                string time;
                ta.GetProjectsTree(tryServer, this.nodes, out time, out downloaded);
                this.treeLbl.Text = time;
                if (tryServer && downloaded) this.treeLbl.ForeColor = Color.FromArgb(0, 0, 192);
                else this.treeLbl.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

                msg = "";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            LogForm.LogForm lf = new LogForm.LogForm(this.log);
            lf.Show();
        }

        private void SyncWizard_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            this.DoSync();
        }
    }
}
