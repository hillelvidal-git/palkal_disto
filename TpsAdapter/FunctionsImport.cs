using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GeoComTypes;
using System.Threading;
using Newtonsoft.Json;

namespace TpsAdapter
{
    partial class TpsAdapter
    {
        //public const string AppPath = @"C:\Program Fiels\WideField";
        //const string DllsPath = @"\Dlls";

        //public const string AppPath = @"C:\";

        //const string DllsPath = @"";

        //const string WrapperPath = AppPath + DllsPath + @"\GeoComWrap32.dll";
        const string WrapperPath = "GeoComWrap32.dll";

        [DllImport(WrapperPath, EntryPoint = "hv_COM_Init")]
        public static extern short hv_COM_Init();

        [DllImport(WrapperPath, EntryPoint = "hv_COM_End")]
        public static extern short hv_COM_End();

        [DllImport(WrapperPath, EntryPoint = "hv_COM_OpenConnection")]
        public static extern short hv_COM_OpenConnection(COM_PORT ePort, COM_BAUD_RATE eBaudRate, short nRetries);

        [DllImport(WrapperPath, EntryPoint = "hv_COM_CloseConnection")]
        public static extern short hv_COM_CloseConnection();

        [DllImport(WrapperPath, EntryPoint = "hv_COM_SetTimeOut")]
        public static extern short hv_COM_SetTimeOut(out short nTimeOutMs);

        [DllImport(WrapperPath, EntryPoint = "hv_COM_GetErrorText")]
        public static extern short hv_COM_GetErrorText(short nResult, out IntPtr szErrText);

        [DllImport(WrapperPath, EntryPoint = "hv_BAP_GetPrismType")]
        public static extern short hv_BAP_GetPrismType(out BAP_PRISMTYPE ePrismType);

        [DllImport(WrapperPath, EntryPoint = "hv_BAP_SetPrismType")]
        public static extern short hv_BAP_SetPrismType(BAP_PRISMTYPE ePrismType);


        [DllImport(WrapperPath, EntryPoint = "hv_BMM_BeepAlarm")]
        public static extern short hv_BMM_BeepAlarm();

        [DllImport(WrapperPath, EntryPoint = "hv_BMM_BeepNormal")]
        public static extern short hv_BMM_BeepNormal();


        [DllImport(WrapperPath, EntryPoint = "hv_COM_SwitchOnTPS")]
        public static extern short hv_COM_SwitchOnTPS(COM_TPS_STARTUP_MODE eOnMode);

        [DllImport(WrapperPath, EntryPoint = "hv_COM_SwitchOffTPS")]
        public static extern short hv_COM_SwitchOffTPS(COM_TPS_STOP_MODE eOffMode);

        [DllImport(WrapperPath, EntryPoint = "hv_COM_NullProc")]
        public static extern short hv_COM_NullProc();


        [DllImport(WrapperPath, EntryPoint = "hv_CSV_GetInstrumentNo")]
        public static extern short hv_CSV_GetInstrumentNo(out long SerialNo);

        [DllImport(WrapperPath, EntryPoint = "hv_CSV_GetInstrumentName")]
        public static extern short hv_CSV_GetInstrumentName(IntPtr Name);

        [DllImport(WrapperPath, EntryPoint = "hv_CSV_GetIntTemp")]
        public static extern short hv_CSV_GetIntTemp(out double Temp);

        [DllImport(WrapperPath, EntryPoint = "hv_CSV_SetDateTime")]
        public static extern short hv_CSV_SetDateTime(DATIME DateAndTime);

        [DllImport(WrapperPath, EntryPoint = "hv_EDM_Laserpointer")]
        public static extern short hv_EDM_Laserpointer(ON_OFF_TYPE eLaser);

        [DllImport(WrapperPath, EntryPoint = "hv_MOT_StartController")]
        public static extern short hv_MOT_StartController(MOT_MODE eControlMode);

        [DllImport(WrapperPath, EntryPoint = "hv_MOT_StopController")]
        public static extern short hv_MOT_StopController(MOT_STOPMODE eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_MOT_SetVelocity")]
        public static extern short hv_MOT_SetVelocity(MOT_COM_PAIR RefOmega); //צפויות בעיות כי מערכים זה לא עובר חלק!

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_SetOrientation")]
        public static extern short hv_TMC_SetOrientation(double HzOrientation);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetStation")]
        public static extern short hv_TMC_GetStation(out TMC_STATION Station);


        [DllImport(WrapperPath, EntryPoint = "hv_TMC_SetStation")]
        public static extern short hv_TMC_SetStation(TMC_STATION Station);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetFace")]
        public static extern short hv_TMC_GetFace(out TMC_FACE eFace);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetSignal")]
        public static extern short hv_TMC_GetSignal(out TMC_EDM_SIGNAL Signal);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetEdmMode")]
        public static extern short hv_TMC_GetEdmMode(out EDM_MODE eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_SetEdmMode")]
        public static extern short hv_TMC_SetEdmMode(EDM_MODE eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_DoMeasure")]
        public static extern short hv_TMC_DoMeasure(TMC_MEASURE_PRG eCommand, TMC_INCLINE_PRG eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetFullMeas")]
        public static extern short hv_TMC_GetFullMeas(long WaitTime, out double rdHzAngle, out double rdVAngle, out double rdAccuracyAngle, out double rdCrossIncl, out double rdLengthIncl, out double rdAccuracyIncl, out double rdSlopeDist, out double rdDistTime, TMC_INCLINE_PRG Mode);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetTiltStatus")]
        public static extern short hv_TMC_GetTiltStatus();

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetCoordinate")]
        public static extern short hv_TMC_GetCoordinate(long WaitTime, out TMC_COORDINATE Coordinate, TMC_INCLINE_PRG eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetSimpleCoord")]
        public static extern short hv_TMC_GetSimpleCoord(long WaitTime, out double dCoordE, out double dCoordN, out double dCoordH, TMC_INCLINE_PRG eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetSimpleMea")]
        public static extern short hv_TMC_GetSimpleMea(long WaitTime, out TMC_HZ_V_ANG OnlyAngle, out double dSlopeDistance, TMC_INCLINE_PRG eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_QuickDist")]
        public static extern short hv_TMC_QuickDist(out TMC_HZ_V_ANG OnlyAngle, out double dSlopeDistance);

        [DllImport(WrapperPath, EntryPoint = "hv_AUT_SetATRStatus")]
        public static extern short hv_AUT_SetATRStatus(ON_OFF_TYPE eOnOff);

        [DllImport(WrapperPath, EntryPoint = "hv_AUT_SetLockStatus")]
        public static extern short hv_AUT_SetLockStatus(ON_OFF_TYPE eOnOff);

        [DllImport(WrapperPath, EntryPoint = "hv_AUT_MakePositioning")]
        public static extern short hv_AUT_MakePositioning(double dHz, double dV, AUT_POSMODE ePOSMode, AUT_ATRMODE eATRMode, BOOLE bDummy);

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_GetAngle")]
        public static extern short hv_TMC_GetAngle(out TMC_HZ_V_ANG OnlyAngle, TMC_INCLINE_PRG eMode);

        [DllImport(WrapperPath, EntryPoint = "hv_AUT_FineAdjust")]
        public static extern short hv_AUT_FineAdjust(double dSrchHz, double dSrchV, BOOLE bDummy);

        [DllImport(WrapperPath, EntryPoint = "hv_AUT_LockIn")]
        public static extern short hv_AUT_LockIn();

        [DllImport(WrapperPath, EntryPoint = "hv_AUS_SetUserLockState")]
        public static extern short hv_AUS_SetUserLockState(ON_OFF_TYPE OnOff);

        [DllImport(WrapperPath, EntryPoint = "hv_CSV_CheckPower")]
        public static extern short hv_CSV_CheckPower(out short unCapacity, out CSV_POWER_PATH eActivePower, out CSV_POWER_PATH ePowerSuggest);

        [DllImport(WrapperPath, EntryPoint = "PASS_GetX")]
        public static extern double PASS_GetX();

        [DllImport(WrapperPath, EntryPoint = "PASS_GetY")]
        public static extern double PASS_GetY();

        [DllImport(WrapperPath, EntryPoint = "PASS_GetZ")]
        public static extern double PASS_GetZ();

        [DllImport(WrapperPath, EntryPoint = "hv_TMC_QuickDist2")]
        public static extern short hv_TMC_QuickDist2();

        [DllImport(WrapperPath, EntryPoint = "PASS_GetHZ")]
        public static extern double PASS_GetHZ();

        [DllImport(WrapperPath, EntryPoint = "PASS_GetV")]
        public static extern double PASS_GetV();

        [DllImport(WrapperPath, EntryPoint = "PASS_GetSlopeDist")]
        public static extern double PASS_GetSlopeDist();

        public Action<string> actReturned;

        public void cmdDisconnectTps()
        {
            commandList.Add(() =>
            {
                nLastResponse = hv_COM_CloseConnection();
            });
        }

        public void cmdRedLaser(bool on)
        {
            commandList.Add(() =>
            {
                ON_OFF_TYPE state = on ? ON_OFF_TYPE.ON : ON_OFF_TYPE.OFF;
                nLastResponse = hv_EDM_Laserpointer(state);
                //actReturned?.Invoke(new { cmd = "laser", val = on, res = nLastResponse});
                string json = @"{cmd : 'laser', val: '" + on.ToString() + "', res: " + nLastResponse + ", point: [] }";
                actReturned?.Invoke(json);
            });
        }

        public void cmdBeepAlarm()
        {
            commandList.Add(() =>
            {
                hv_BMM_BeepAlarm();
            });
        }
        public void cmdImportStation(bool now = false)
        {
            if (now)
            {
                hv_TMC_GetStation(out this.CurrentStation);
                string json = @"{cmd : 'station', val: 'ok', res: " + nLastResponse + ", point: [] }";
                actReturned?.Invoke(json);
                return;
            }
            else
            {
                commandList.Add(() =>
                {
                    hv_TMC_GetStation(out this.CurrentStation);
                    string json = @"{cmd : 'station', val: 'ok', res: " + nLastResponse + ", point: [] }";
                    actReturned?.Invoke(json);
                    return;
                });
            }
        }



        public void cmdMeasure(bool usePrism)
        {
            commandList.Add(() =>
            {
                string json;
                nLastResponse = usePrism ? hv_TMC_SetEdmMode(EDM_MODE.EDM_SINGLE_STANDARD) : hv_TMC_SetEdmMode(EDM_MODE.EDM_SINGLE_SRANGE);

                if (usePrism)
                {
                    nLastResponse = hv_AUS_SetUserLockState(ON_OFF_TYPE.ON);
                    nLastResponse = hv_AUT_FineAdjust(this.SearchRangeHz, this.SearchRangeV, BOOLE.FALSE);
                    if (nLastResponse != 0)
                    {
                        //actReturned?.Invoke(new { cmd = "measure", val = "prism failed", res = nLastResponse });
                        json = @"{cmd : 'measure', val: 'prism failed', res: " + nLastResponse + ", point: [] }";
                        actReturned?.Invoke(json);
                        return;
                    }
                }

                nLastResponse = hv_TMC_DoMeasure(TMC_MEASURE_PRG.TMC_DEF_DIST, TMC_INCLINE_PRG.TMC_AUTO_INC);
                if (nLastResponse != 0)
                {
                    //actReturned?.Invoke(new { cmd = "measure", val = "measure failed", res = nLastResponse });
                    json = @"{cmd : 'measure', val: 'measure failed', res: " + nLastResponse + ", point: [] }";
                    actReturned?.Invoke(json);
                    return;
                }

                nLastResponse = hv_TMC_GetSimpleCoord(5000, out double dummy1, out double dummy2, out double dummy3, TMC_INCLINE_PRG.TMC_AUTO_INC);
                // to complete the measurement, and clear data
                hv_TMC_DoMeasure(TMC_MEASURE_PRG.TMC_CLEAR, TMC_INCLINE_PRG.TMC_AUTO_INC);
                //שמור את נתוני המדידה
                double[] Point3d = new double[3];
                Point3d[0] = PASS_GetX();
                Point3d[1] = PASS_GetY();
                Point3d[2] = PASS_GetZ();

                json = @"{cmd : 'measure', val: 'ok', res: " + nLastResponse + ", point: [" + Point3d[0] + ", " + Point3d[1] + ", " + Point3d[2] + "] }";
                actReturned?.Invoke(json);
                actAlive?.Invoke();
            });
        }

        public void cmdSetPrismTrack(bool on)
        {
            commandList.Add(() =>
            {
                string json;
                if (on)
                {
                    nLastResponse = hv_AUS_SetUserLockState(ON_OFF_TYPE.ON);
                    nLastResponse = hv_AUT_FineAdjust(this.SearchRangeHz, this.SearchRangeV, BOOLE.FALSE); //חיפוש הפריזמה והתמקדות במרכזה
                    if (nLastResponse == 0)
                    {
                        actAlive?.Invoke();
                        nLastResponse = hv_AUT_LockIn(); //נסה לנעול את המכשיר על הפריזמה
                        if (nLastResponse == 0)
                        {
                            hv_BMM_BeepAlarm(); //השמע פצפוף לאות שהמכשיר ננעל
                            json = @"{cmd : 'lock on', val: 'ok', res: " + nLastResponse + ", point: [] }";
                        }
                        else
                        {
                            json = @"{cmd : 'lock on', val: 'lock failed', res: " + nLastResponse + ", point: [] }";
                        }
                    }
                    else
                    {
                        json = @"{cmd : 'lock on', val: 'fine adjust failed', res: " + nLastResponse + ", point: [] }";
                    }
                }
                else
                {
                    nLastResponse = hv_AUS_SetUserLockState(ON_OFF_TYPE.OFF);
                    json = @"{cmd : 'lock off', val: 'ok', res: " + nLastResponse + ", point: [] }";
                }

                actReturned?.Invoke(json);
            });
        }

        public void cmdPointAt(double Rel_HZ_Angle, double Rel_V_Angle)
        {
            commandList.Add(() =>
            {
                //בצע הצבעה לנקודה
                short nTimeOutMs = 4500; //קביעת זמן תגובה ארוך על מנת לאפשר את הנסיעה
                hv_COM_SetTimeOut(out nTimeOutMs);
                short nLastResponse = hv_AUT_MakePositioning(Rel_HZ_Angle, Rel_V_Angle, AUT_POSMODE.AUT_NORMAL, AUT_ATRMODE.AUT_POSITION, BOOLE.FALSE); ;
                bool ok = nLastResponse == 0;
                string json = @"{cmd : 'gotopoint', val: '" + ok.ToString() + "', res: " + nLastResponse + ", point: [] }";
                actReturned?.Invoke(json);
                if (ok) actAlive?.Invoke();
            });
        }

        public void cmdCheckTilt()
        {
            double[] Inclination = new double[3];

            //קריאת נתוני הפלס מהמכשיר לתוך העוטף
            nLastResponse = hv_TMC_GetTiltStatus();
            if ((nLastResponse == 0) || (nLastResponse == 1285))
            {
                actAlive?.Invoke();
                //קריאת נתוני הפלס מהעוטף
                Inclination[0] = PASS_GetX();
                Inclination[1] = PASS_GetY();
                Inclination[2] = PASS_GetZ();
                //Console.WriteLine("tilt data recieved: [" + Inclination[0] + ", " + Inclination[1] + ", " + Inclination[2] + "]");
            }
            else
            {
                //המדידה לא הצליחה
                Console.WriteLine("tilt data failed");
            }

            string json = @"{cmd : 'tilt', val: 'ok', res: " + nLastResponse + ", point: [" + Inclination[0] + ", " + Inclination[1] + ", " + Inclination[2] + "] }";
            actReturned?.Invoke(json);

        }
    }



}
