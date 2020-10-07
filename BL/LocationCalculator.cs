using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WideFieldBL
{
    public class TargetData
    {
        public int Number;
        public int BsNumber;
        public double[] Position;
        public double HzDist;
        public double HzAngle;
        public double VDist;
        public double VAngle;
        public double Evaluation;
        public int UserEvaluation;
    }

    public class Station
    {
        public double[] Position;
        public double AngleDeviation;
        public double Evaluation;

        public Station(double[] pos)
        {
            this.Position = new double[] { pos[0], pos[1], pos[2] };
        }

        public double EvaluateStation(List<TargetData> Targets)
        {
            this.Evaluation = 0;
            foreach (TargetData t in Targets)
            {
                //מחשבים את הסטייה שבין המרחק הנמדד למטרה ובין המרחק התיאורטי בין התחנה למטרה
                //את הסטייה הזו מורידים מדירוג התחנה
                this.Evaluation -= Math.Abs(1 - (t.HzDist / this.DistTo(t.Position)));
            }
            this.Evaluation /= Targets.Count;
            return this.Evaluation;
        }

        private double DistTo(double[] p)
        {
            //מחזיר את המרחק בין התחנה לבין נקודה נתונה
            //החישוב הוא במישור בלבד ואין התייחסות לציר השלישי
            return Math.Sqrt(Math.Pow(this.Position[0] - p[0], 2) + Math.Pow(this.Position[1] - p[1], 2));
        }

    }

    class LocationCalculator
    {
        double AngleMaxDev = 0.0005;
        double StationsMaxDist = 0.015;
        internal List<string> log;

        public double GetAngleCorrection(Station St, List<TargetData> Targets)
        {
            //הערה: הזויות המחושבות כאן מתחילות מהכיוון החיובי של ציר האיקס, וגדלות נגד כיוון השעון

            log.Add("");
            log.Add("Angle Correction:");
            log.Add("=================");

            double dx, dy, CalcAngle, firstMeas, tCorrection;
            double Correction = 0;
            foreach (TargetData t in Targets)
            {
                dx = t.Position[0] - St.Position[0];
                dy = t.Position[1] - St.Position[1];
                CalcAngle = Math.Atan2(dy, dx);
                firstMeas = Math.PI / 2 - t.HzAngle;
                tCorrection = CalcAngle - t.HzAngle; //ההפרש שבין הזוית המדודה לזוית המחושבת
                if (tCorrection >= Math.PI) tCorrection -= 2 * Math.PI;

                while (tCorrection < 0)
                    tCorrection += Math.PI * 2;
                while (tCorrection > Math.PI * 2)
                    tCorrection -= Math.PI * 2;

                log.Add("Target: BS_" + t.BsNumber.ToString());
                log.Add(">>> Measured: " + firstMeas.ToString("0.00000") + "  |  Converted: " + t.HzAngle.ToString("0.00000") + "   |   Calc.: " + CalcAngle.ToString("0.00000") + "   |   Correction: " + tCorrection.ToString("0.00000"));
                Correction += tCorrection;
            }

            log.Add("Average Correction: " + (Correction / Targets.Count).ToString("0.00000"));
            return Correction / Targets.Count; //מחזיר את התיקון הממוצע
        }

        public Station CalculateBestStation(List<TargetData> Targets)
        {
            log = new List<string>();
            log.Add("Locating By Resection. Start time: " + DateTime.Now.ToString());
            log.Add("========================================");
            log.Add("");
            log.Add(Targets.Count.ToString() + " Targets:");
            log.Add("----------");
            foreach (TargetData t in Targets)
                log.Add(t.BsNumber.ToString() + ", HzAngle: " + t.HzAngle.ToString("0.00000") + ", HzDist: " + t.HzDist.ToString("0.00000"));

            //חישוב כל התחנות האפשריות על פי זוגות של מטרות
            List<Station> Stations = new List<Station>();
            log.Add("");
            log.Add("Stations:");
            log.Add("---------");

            for (int i = 0; i < Targets.Count; i++)
                for (int j = 0; j < i; j++)
                {
                    try
                    {
                        log.Add("");
                        log.Add("By BS_" + Targets[j].BsNumber.ToString() + " and BS_" + Targets[i].BsNumber.ToString());
                        log.Add("~~~~~~~~~~~~~");
                        Stations.Add(GetStationBy2Targets(Targets[j], Targets[i]));
                        log.Add("---> Station No. " + (Stations.Count - 1).ToString() + " Calculated");
                        log.Add("------> AngleDev.: " + Stations[Stations.Count - 1].AngleDeviation.ToString("0.00000"));
                    }
                    catch (Exception se)
                    {
                        log.Add("---> Error: " + se.Message);
                        //TODO: Handle Error
                    }
                }

            if (Stations.Count == 0)
            {
                log.Add("No Valid Station. Locating fail******");
                throw new Exception("No Valid Station");
            }

            //בדיקה שהתחנות האפשריות אינן רחוקות מדי זו מזו
            double gap;
            log.Add("");
            log.Add("Distances Between Stations:");
            log.Add("-----------------------");

            for (int i = 0; i < Stations.Count; i++)
                for (int j = 0; j < i; j++)
                {
                    gap = GetDistBetween(Stations[i].Position, Stations[j].Position);
                    log.Add("Between St. " + i.ToString() + " and St. " + j.ToString() + " ---> " + gap.ToString("0.00000"));
                    if (gap >= this.StationsMaxDist)
                    {
                        log.Add("Over Gap. Locating fail******");
                        throw new Exception("Stations Not Consistent. Gap = " + gap.ToString("0.00000"));
                    }
                }

            //הערכת הדיוק של התחנות השונות על פי כלל המטרות שנמדדו
            Station BestStation = Stations[0];
            BestStation.EvaluateStation(Targets);
            double stEval;

            log.Add("");
            log.Add("Stations Evaluation:");
            log.Add("-----------------------");

            int si = 0;
            foreach (Station station in Stations)
            {
                stEval = station.EvaluateStation(Targets);
                log.Add("St. No. " + si.ToString() + " >> Value = " + stEval.ToString("0.00000"));
                if (stEval > BestStation.Evaluation)
                {
                    log.Add("$$$ Best Station Meanwhile $$$");
                    BestStation = station;
                }
                si++;
            }

            if (BestStation.Evaluation == 0)
            {
                log.Add("Best Station value is PERFECT");
                return BestStation;
            }

            Station SimpleMeanStation = GetMeanStation(Stations);
            SimpleMeanStation.EvaluateStation(Targets);
            log.Add("");
            log.Add("Mean Station: " + SimpleMeanStation.Evaluation.ToString("0.00000"));

            Station WeightedMeanStation = GetWeightedMeanStation(Stations);
            WeightedMeanStation.EvaluateStation(Targets);
            log.Add("WeightedMean Station: " + WeightedMeanStation.Evaluation.ToString("0.00000"));

            Station Final;

            //החזרת התחנה בעלת הדירוג הטוב ביותר: או אחת התחנות שחושבו, או ממוצע, או ממוצע משוקלל
            //TODO: להבין האם ניתן להוריד אפשרויות
            if (BestStation.Evaluation > SimpleMeanStation.Evaluation)
                if (BestStation.Evaluation > WeightedMeanStation.Evaluation) Final = BestStation;
                else Final = WeightedMeanStation;
            else
                if (SimpleMeanStation.Evaluation > WeightedMeanStation.Evaluation) Final = SimpleMeanStation;
                else Final = WeightedMeanStation;

            log.Add("Final Station: "
                + Final.Position[0].ToString("0.00000") + ", "
                + Final.Position[1].ToString("0.00000") + ", "
                + Final.Position[2].ToString("0.00000"));
            return Final;
        }

        private double GetDistBetween(double[] p1, double[] p2)
        {
            return Math.Sqrt(Math.Pow(p1[0] - p2[0], 2) + Math.Pow(p1[1] - p2[1], 2));
        }

        private Station GetWeightedMeanStation(List<Station> Stations)
        {
            Station mean = new Station(new double[] { 0, 0, 0 });

            double weight;
            double totalWeight = 0;
            foreach (Station st in Stations)
                totalWeight += 0 - (1 / st.Evaluation);

            foreach (Station st in Stations)
            {
                //חישוב משקלה היחסי של התחנה
                weight = 0 - (1 / st.Evaluation);

                mean.Position[0] += weight * st.Position[0];
                mean.Position[1] += weight * st.Position[1];
                mean.Position[2] += weight * st.Position[2];
            }

            mean.Position[0] /= totalWeight;
            mean.Position[1] /= totalWeight;
            mean.Position[2] /= totalWeight;

            return mean;
        }

        private Station GetMeanStation(List<Station> Stations)
        {
            Station mean = new Station(new double[] { 0, 0, 0 });

            foreach (Station st in Stations)
            {
                mean.Position[0] += st.Position[0];
                mean.Position[1] += st.Position[1];
                mean.Position[2] += st.Position[2];
            }

            mean.Position[0] /= Stations.Count;
            mean.Position[1] /= Stations.Count;
            mean.Position[2] /= Stations.Count;

            return mean;
        }

        private Station GetStationBy2Targets(TargetData target1, TargetData target2)
        {
            //קבלת שתי נקודות החיתוך של המעגלים
            double[] intersect1, intersect2;
            Get2CirclesIntersections(target1, target2, out intersect1, out intersect2);

            log.Add("Intersection No. 1:");
            log.Add(intersect1[0].ToString("0.00000") + ",");
            log.Add(intersect1[1].ToString("0.00000") + ",");
            log.Add(intersect1[2].ToString("0.00000"));
            log.Add("Intersection No. 2:");
            log.Add(intersect2[0].ToString("0.00000") + ",");
            log.Add(intersect2[1].ToString("0.00000") + ",");
            log.Add(intersect2[2].ToString("0.00000"));

            //בחירת הנקודה המתאימה על פי הזוויות
            return DecideByAngles(target1, target2, intersect1, intersect2);

        }

        private Station DecideByAngles(TargetData target1, TargetData target2, double[] st1, double[] st2)
        {
            double[] selectedSt;
            double selectedAngle;

            //מציאת הזוית שנמדדה בין שתי המטרות הסדורות
            //הזוית מוגדרת כגדלה נגד כיוון השעון
            //בנוסף אנו מחשבים רק זויות חיוביות
            double t1t2Ang = target2.HzAngle - target1.HzAngle;
            if (t1t2Ang < 0)
                t1t2Ang += Math.PI * 2;

            //מציאת הזוית בין שתי המטרות מנקודת מבט הפתרון הראשון
            double st1Ang = GetAngle(st1, target1.Position, target2.Position);

            //מציאת הזוית בין שתי המטרות מנקודת מבט הפתרון השני
            double st2Ang = GetAngle(st2, target1.Position, target2.Position);

            //בחירת הפתרון על פי הזוית
            //הפתרון שיבחר הוא זה שנמצא באותו צד עם הזוית שנמדדה
            if (Math.Sign(t1t2Ang - Math.PI) == Math.Sign(st1Ang - Math.PI))
            {
                selectedSt = st1;
                selectedAngle = st1Ang;
                log.Add("...............Selected: #1");
            }
            else
            {
                selectedSt = st2;
                selectedAngle = st2Ang;
                log.Add("...............Selected: #2");
            }

            Station SelectedStation = new Station(selectedSt);

            //בדיקת הדיוק שהושג בזוית
            SelectedStation.AngleDeviation = Math.Abs(1 - (selectedAngle / t1t2Ang));
            if (SelectedStation.AngleDeviation > this.AngleMaxDev)
            {
                //הורד את דירוג המטרות, מכיוון שכנראה אחת מהן לא נמדדה באופן מדויק
                target1.Evaluation -= 1;
                target2.Evaluation -= 1;
                throw new Exception("Angle Deviation Too high: " + SelectedStation.AngleDeviation.ToString("0.00000"));
            }

            return SelectedStation;
        }

        private double GetAngle(double[] C, double[] A, double[] B)
        {
            //מציאת הזוית בין א ל- ב
            //הזוית מוגדרת כגדלה נגד כיוון השעון
            //בנוסף אנו מחשבים רק זויות חיוביות
            //סדר הנקודות משנה בהחלט
            //========================================

            //מציאת גודל הזויות לכל נקודה
            double a = Math.Atan2(A[1] - C[1], A[0] - C[0]);
            double b = Math.Atan2(B[1] - C[1], B[0] - C[0]);

            //TODO: תיקון סימן
            if (b > a) return b - a;
            else return Math.PI * 2 + b - a;
        }


        private void Get2CirclesIntersections(TargetData target1, TargetData target2, out double[] intersect1, out double[] intersect2)
        {
            double[] P0 = target1.Position;
            double r0 = target1.HzDist;
            double[] P1 = target2.Position;
            double r1 = target2.HzDist;
            double d, a, h;
            double[] P2;

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

                throw new Exception("No Solution");

            //חישוב ערכי ביניים לצורך הפתרון
            a = (Math.Pow(r0, 2) - Math.Pow(r1, 2) + Math.Pow(d, 2)) / (2 * d);
            h = Math.Sqrt(Math.Pow(r0, 2) - Math.Pow(a, 2));

            P2 = new double[]
            {
                P0[0] + (a / d) * (P1[0] - P0[0]),
                P0[1] + (a / d) * (P1[1] - P0[1])
            };

            //מצא את גובה התחנה, על ידי ממוצע מדידות המטרות
            double stZ = ((target1.Position[2] + target2.Position[2]) - (target1.VDist + target2.VDist)) / 2;

            //חישוב נקודות החיתוך
            intersect1 = new double[]
            {
                P2[0] + (h / d) * (P1[1] - P0[1]),
                P2[1] - (h / d) * (P1[0] - P0[0]),
                stZ
            };

            intersect2 = new double[]
            {
                P2[0] - (h / d) * (P1[1] - P0[1]),
                P2[1] + (h / d) * (P1[0] - P0[0]),
                stZ
            };
        }
    }
}
