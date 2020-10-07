using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GeoComTypes
{
    public static class GC_Consts
    {
        public static int
                GRC_TPS = 0x0000,	 // main return codes (identical to RC_SUP!!)
                GRC_SUP = 0x0000,   // supervisor task (identical to RCBETA!!)
                GRC_ANG = 0x0100,   // angle- and inclination
                GRC_ATA = 0x0200,   // automatic target acquisition
                GRC_EDM = 0x0300,   // electronic distance meter
                GRC_GMF = 0x0400,   // geodesy mathematics & formulas
                GRC_TMC = 0x0500,   // measurement & calc
                GRC_MEM = 0x0600,   // memory management
                GRC_MOT = 0x0700,   // motorization
                GRC_LDR = 0x0800,   // program loader
                GRC_BMM = 0x0900,   // basics of man machine interface
                GRC_TXT = 0x0A00,   // text management
                GRC_MMI = 0x0B00,   // man machine interface
                GRC_COM = 0x0C00,   // communication
                GRC_DBM = 0x0D00,   // data base management
                GRC_DEL = 0x0E00,   // dynamic event logging
                GRC_FIL = 0x0F00,   // file system
                GRC_CSV = 0x1000,   // central services
                GRC_CTL = 0x1100,   // controlling task
                GRC_STP = 0x1200,   // start + stop task
                GRC_DPL = 0x1300,   // data pool
                GRC_WIR = 0x1400,   // wi registration

                GRC_USR = 0x2000,   // user task
                GRC_ALT = 0x2100,   // alternate user task
                GRC_AUT = 0x2200,   // automatization
                GRC_AUS = 0x2300,   // alternative user
                GRC_BAP = 0x2400,   // basic applications
                GRC_SAP = 0x2500,   // system applications
                GRC_COD = 0x2600,   // standard code function
                GRC_BAS = 0x2700,   // GeoBasic interpreter
                GRC_IOS = 0x2800,   // Input-/ output- system
                GRC_CNF = 0x2900,   // configuration facilities

                GRC_CSX = 0x2C00,   // CSX (RCS1300, Escatec)
                GRC_RCS = 0x2D00,   // RCS 
                GRC_XIT = 0x2E00,   // XIT subsystem (Excite-Level, LIS)
                GRC_DNA = 0x2F00,   // DNA2 subsystem

                GRC_ICD = 0x3000,   // cal data management
                GRC_KDM = 0x3100,   // keyboard display module
                GRC_LOD = 0x3200,   // firmware loader
                GRC_FTR = 0x3300,   // file transfer
                GRC_TRK = 0x3400,   // tracker
                GRC_IMG = 0x3500,   // imaging, telescopic camera
                GRC_VNF = 0x3F00,   // reserved for new TPS1200 subsystem

                GRC_GPS = 0x4000,   // GPS subsystem
                GRC_TST = 0x4100,   // Test subsystem
                GRC_PTF = 0x4F00,   // reserved for new GPS1200 subsystem

                GRC_APP = 0x5000,   // offset for all applications

                GRC_RES = 0x7000,   // reserved code range 


               GRC_OK = GRC_TPS + 0,   // Function successfully completed.
               GRC_UNDEFINED = GRC_TPS + 1,   // Unknown error, result unspecified.
               GRC_IVPARAM = GRC_TPS + 2,   // Invalid parameter detected.\nResult unspecified.
               GRC_IVRESULT = GRC_TPS + 3,   // Invalid result.
               GRC_FATAL = GRC_TPS + 4,   // Fatal error.
               GRC_NOT_IMPL = GRC_TPS + 5,   // Not implemented yet.
               GRC_TIME_OUT = GRC_TPS + 6,   // Function execution timed out.\nResult unspecified.
               GRC_SET_INCOMPL = GRC_TPS + 7,   // Parameter setup for subsystem is incomplete.
               GRC_ABORT = GRC_TPS + 8,   // Function execution has been aborted.
               GRC_NOMEMORY = GRC_TPS + 9,   // Fatal error - not enough memory.
               GRC_NOTINIT = GRC_TPS + 10,   // Fatal error - subsystem not initialized.
               GRC_SHUT_DOWN = GRC_TPS + 12,   // Subsystem is down.
               GRC_SYSBUSY = GRC_TPS + 13,   // System busy/already in use of another process.\nCannot execute function.
               GRC_HWFAILURE = GRC_TPS + 14,   // Fatal error - hardware failure.
               GRC_ABORT_APPL = GRC_TPS + 15,   // Execution of application has been aborted (SHIFT-ESC).
               GRC_LOW_POWER = GRC_TPS + 16,   // Operation aborted - insufficient power supply level.
               GRC_IVVERSION = GRC_TPS + 17,   // Invalid version of file, ...
               GRC_BATT_EMPTY = GRC_TPS + 18,   // Battery empty
               GRC_NO_EVENT = GRC_TPS + 20,   // no event pending.
               GRC_OUT_OF_TEMP = GRC_TPS + 21,   // out of temperature range
               GRC_INSTRUMENT_TILT = GRC_TPS + 22,   // intrument tilting out of range
               GRC_COM_SETTING = GRC_TPS + 23,   // communication error
               GRC_NO_ACTION = GRC_TPS + 24,   // GRC_TYPE Input 'do no action'
               GRC_SLEEP_MODE = GRC_TPS + 25,   // Instr. run into the sleep mode
               GRC_NOTOK = GRC_TPS + 26,   // Function not successfully completed. 
               GRC_NA = GRC_TPS + 27,   // Not available 
               GRC_OVERFLOW = GRC_TPS + 28,   // Overflow error
               GRC_STOPPED = GRC_TPS + 29,   // System or subsystem has been stopped


              GRC_COM_ERO = GRC_COM + 0, //  Initiate Extended Runtime Operation (ERO).
              GRC_COM_CANT_ENCODE = GRC_COM + 1, //  Cannot encode arguments in client.
              GRC_COM_CANT_DECODE = GRC_COM + 2, //  Cannot decode results in client.
              GRC_COM_CANT_SEND = GRC_COM + 3, //  Hardware error while sending.
              GRC_COM_CANT_RECV = GRC_COM + 4, //  Hardware error while receiving.
              GRC_COM_TIMEDOUT = GRC_COM + 5, //  Request timed out.
              GRC_COM_WRONG_FORMAT = GRC_COM + 6, //  Packet format error.
              GRC_COM_VER_MISMATCH = GRC_COM + 7, //  Version mismatch between client and server.
              GRC_COM_CANT_DECODE_REQ = GRC_COM + 8, //  Cannot decode arguments in server.
              GRC_COM_PROC_UNAVAIL = GRC_COM + 9, //  Unknown RPC, procedure ID invalid.
              GRC_COM_CANT_ENCODE_REP = GRC_COM + 10, //  Cannot encode results in server.
              GRC_COM_SYSTEM_ERR = GRC_COM + 11, //  Unspecified generic system error.
              GRC_COM_UNKNOWN_HOST = GRC_COM + 12, //  (Unused error code)
              GRC_COM_FAILED = GRC_COM + 13, //  Unspecified error.
              GRC_COM_NO_BINARY = GRC_COM + 14, //  Binary protocol not available.
              GRC_COM_INTR = GRC_COM + 15, //  Call interrupted.
              GRC_COM_UNKNOWN_ADDR = GRC_COM + 16, //  (Unused error code)
              GRC_COM_NO_BROADCAST = GRC_COM + 17, //  (Unused error code)
              GRC_COM_REQUIRES_8DBITS = GRC_COM + 18, //  Protocol needs 8bit encoded chararacters.
              GRC_COM_UD_ERROR = GRC_COM + 19, //  (Unused error code)
              GRC_COM_LOST_REQ = GRC_COM + 20, //  (Unused error code)
              GRC_COM_TR_ID_MISMATCH = GRC_COM + 21, //  Transacation ID mismatch error.
              GRC_COM_NOT_GEOCOM = GRC_COM + 22, //  Protocol not recognizeable.
              GRC_COM_UNKNOWN_PORT = GRC_COM + 23, //  (WIN) Invalid port address.
              GRC_COM_ILLEGAL_TRPT_SELECTOR = GRC_COM + 24, //  (Unused error code)
              GRC_COM_TRPT_SELECTOR_IN_USE = GRC_COM + 25, //  (Unused error code)
              GRC_COM_INACTIVE_TRPT_SELECTOR = GRC_COM + 26, //  (Unused error code)
              GRC_COM_ERO_END = GRC_COM + 27, //  ERO is terminating.
              GRC_COM_OVERRUN = GRC_COM + 28, //  Internal error: data buffer overflow.
              GRC_COM_SRVR_RX_CHECKSUM_ERROR = GRC_COM + 29, //  Invalid checksum on server side received.
              GRC_COM_CLNT_RX_CHECKSUM_ERROR = GRC_COM + 30, //  Invalid checksum on client side received.
              GRC_COM_PORT_NOT_AVAILABLE = GRC_COM + 31, //  (WIN) Port not available.
              GRC_COM_PORT_NOT_OPEN = GRC_COM + 32, //  (WIN) Port not opened.
              GRC_COM_NO_PARTNER = GRC_COM + 33, //  (WIN) Unable to find TPS.
              GRC_COM_ERO_NOT_STARTED = GRC_COM + 34, //  Extended Runtime Operation could not be started.
              GRC_COM_CONS_REQ = GRC_COM + 35, //  Att to send cons reqs
              GRC_COM_SRVR_IS_SLEEPING = GRC_COM + 36, //  TPS has gone to sleep. Wait and try again.
              GRC_COM_SRVR_IS_OFF = GRC_COM + 37, //  TPS has shut down. Wait and try again.
              GRC_COM_NO_CHECKSUM = GRC_COM + 38, //  No checksum in ASCII protocol available.

              GRC_FTR_FILEACCESS = GRC_FTR + 0,  // File access error
              GRC_FTR_WRONGFILEBLOCKNUMBER = GRC_FTR + 1,  // block number was not the expected one
              GRC_FTR_NOTENOUGHSPACE = GRC_FTR + 2,  // not enough space on device to proceed uploading
              GRC_FTR_INVALIDINPUT = GRC_FTR + 3,  // Rename of file failed.
              GRC_FTR_MISSINGSETUP = GRC_FTR + 4,  // invalid parameter as input

              GRC_EDM_SYSTEM_ERR = GRC_EDM + 1, // Fatal EDM sensor error. See for the exact \nreason the original EDM sensor error number.\nIn the most cases a service problem.

             // Sensor user errors
             GRC_EDM_INVALID_COMMAND = GRC_EDM + 2, // Invalid command or unknown command,\nsee command syntax.
             GRC_EDM_BOOM_ERR = GRC_EDM + 3, // Boomerang error.
             GRC_EDM_SIGN_LOW_ERR = GRC_EDM + 4, // Received signal to low, prisma to far away,\nor natural barrier, bad environment, etc.  
             GRC_EDM_DIL_ERR = GRC_EDM + 5, // obsolete
             GRC_EDM_SIGN_HIGH_ERR = GRC_EDM + 6, // Received signal to strong, prisma \nto near, stranger light effect.
            // New TPS1200 sensor user errors
             GRC_EDM_TIMEOUT = GRC_EDM + 7, // Timeout, measuring time exceeded (signal too weak, beam interrupted,..)
             GRC_EDM_FLUKT_ERR = GRC_EDM + 8, // to much turbulences or distractions
             GRC_EDM_FMOT_ERR = GRC_EDM + 9, // filter motor defective

                // Subsystem errors 
             GRC_EDM_DEV_NOT_INSTALLED = GRC_EDM + 10, // Device like EGL, DL is not installed.
             GRC_EDM_NOT_FOUND = GRC_EDM + 11, // Search result invalid. For the exact explanation \nsee in the description of the called function.
             GRC_EDM_ERROR_RECEIVED = GRC_EDM + 12, // Communication ok, but an error\nreported from the EDM sensor.
             GRC_EDM_MISSING_SRVPWD = GRC_EDM + 13, // No service password is set.
             GRC_EDM_INVALID_ANSWER = GRC_EDM + 14, // Communication ok, but an unexpected\nanswer received. 
             GRC_EDM_SEND_ERR = GRC_EDM + 15, // Data send error, sending buffer is full.
             GRC_EDM_RECEIVE_ERR = GRC_EDM + 16, // Data receive error, like\nparity buffer overflow.
             GRC_EDM_INTERNAL_ERR = GRC_EDM + 17, // Internal EDM subsystem error.
             GRC_EDM_BUSY = GRC_EDM + 18, // Sensor is working already,\nabort current measuring first.
             GRC_EDM_NO_MEASACTIVITY = GRC_EDM + 19, // No measurement activity started.
             GRC_EDM_CHKSUM_ERR = GRC_EDM + 20, // Calculated checksum, resp. received data wrong\n(only in binary communication mode possible).
             GRC_EDM_INIT_OR_STOP_ERR = GRC_EDM + 21, // During start up or shut down phase an\nerror occured. It is saved in the DEL buffer.
             GRC_EDM_SRL_NOT_AVAILABLE = GRC_EDM + 22, // Red laser not available on this sensor HW.
             GRC_EDM_MEAS_ABORTED = GRC_EDM + 23, // Measurement will be aborted (will be used for the lasersecurity)

             // New TPS1200 sensor user error

             GRC_EDM_SLDR_TRANSFER_PENDING = GRC_EDM + 30, // Multiple OpenTransfer calls.
             GRC_EDM_SLDR_TRANSFER_ILLEGAL = GRC_EDM + 31, // No opentransfer happened.
             GRC_EDM_SLDR_DATA_ERROR = GRC_EDM + 32, // Unexpected data format received.
             GRC_EDM_SLDR_CHK_SUM_ERROR = GRC_EDM + 33, // Checksum error in transmitted data.
             GRC_EDM_SLDR_ADDR_ERROR = GRC_EDM + 34, // Address out of valid range.
             GRC_EDM_SLDR_INV_LOADFILE = GRC_EDM + 35, // Firmware file has invalid format.
             GRC_EDM_SLDR_UNSUPPORTED = GRC_EDM + 36, // Current (loaded) firmware doesn't support upload.
             GRC_EDM_UNKNOW_ERR = GRC_EDM + 40, // Undocumented error from the\nEDM sensor, should not occur.

             GRC_EDM_DISTRANGE_ERR = GRC_EDM + 50, // Out of distance range (dist too small or large)
             GRC_EDM_SIGNTONOISE_ERR = GRC_EDM + 51, // Signal to noise ratio too small
             GRC_EDM_NOISEHIGH_ERR = GRC_EDM + 52, // Noise to high
             GRC_EDM_PWD_NOTSET = GRC_EDM + 53, // Password is not set
             GRC_EDM_ACTION_NO_MORE_VALID = GRC_EDM + 54, // Elapsed time between prepare und start fast measurement for ATR to long
             GRC_EDM_MULTRG_ERR = GRC_EDM + 55, // Possibly more than one target (also a sensor error)
             GRC_EDM_MISSING_EE_CONSTS = GRC_EDM + 56, // eeprom consts are missing
             GRC_EDM_NOPRECISE = GRC_EDM + 57, // No precise measurement possible
             GRC_EDM_MEAS_DIST_NOT_ALLOWED = GRC_EDM + 58, // Measured distance is to big (not allowed)
             GRC_MOT_UNREADY = GRC_MOT + 0, //motorization is not ready
            //(1792)
            GRC_MOT_BUSY = GRC_MOT + 1, //motorization is handling another task
            //(1793)
            GRC_MOT_NOT_OCONST = GRC_MOT + 2, //motorization is not in velocity mode
            //(1794)
            GRC_MOT_NOT_CONFIG = GRC_MOT + 3, //motorization is in the wrong mode or busy
            //(1795)
            GRC_MOT_NOT_POSIT = GRC_MOT + 4, //motorization is not in posit mode
            //(1796)
            GRC_MOT_NOT_SERVICE = GRC_MOT + 5, //motorization is not in service mode
            //(1797)
            GRC_MOT_NOT_BUSY = GRC_MOT + 6, //motorization is handling no task
            //(1798)
            GRC_MOT_NOT_LOCK = GRC_MOT + 7, //motorization is not in tracking mode
            //(1799)
            GRC_MOT_NOT_SPIRAL = GRC_MOT + 8, //motorization is not in spiral mode
            //(1800)

          GRC_TMC_NO_FULL_CORRECTION = GRC_TMC + 3,       // Warning: measurment without full correction
          GRC_TMC_ACCURACY_GUARANTEE = GRC_TMC + 4,       // Info   : accuracy can not be guarantee

          GRC_TMC_ANGLE_OK = GRC_TMC + 5,    // Warning: only angle measurement valid
          GRC_TMC_ANGLE_NOT_FULL_CORR = GRC_TMC + 8,    // Warning: only angle measurement valid but without full correction
          GRC_TMC_ANGLE_NO_ACC_GUARANTY = GRC_TMC + 9,    // Info   : only angle measurement valid but accuracy can not be guarantee

          GRC_TMC_ANGLE_ERROR = GRC_TMC + 10,       // Error  : no angle measurement

          GRC_TMC_DIST_PPM = GRC_TMC + 11,       // Error  : wrong setting of PPM or MM on EDM
          GRC_TMC_DIST_ERROR = GRC_TMC + 12,       // Error  : distance measurement not done (no aim, etc.)
          GRC_TMC_BUSY = GRC_TMC + 13,       // Error  : system is busy (no measurement done)
          GRC_TMC_SIGNAL_ERROR = GRC_TMC + 14,       // Error  : no signal on EDM (only in signal mode)
            GRC_BMM_XFER_PENDING = GRC_BMM + 1,  // Loading process already opened
            GRC_BMM_NO_XFER_OPEN = GRC_BMM + 2,  // Transfer not opened
            GRC_BMM_UNKNOWN_CHARSET = GRC_BMM + 3,  // Unknown character set
            GRC_BMM_NOT_INSTALLED = GRC_BMM + 4,  // Display module not present
            GRC_BMM_ALREADY_EXIST = GRC_BMM + 5,  // Character set already exists
            GRC_BMM_CANT_DELETE = GRC_BMM + 6,  // Character set cannot be deleted
            GRC_BMM_MEM_ERROR = GRC_BMM + 7,  // Memory cannot be allocated
            GRC_BMM_CHARSET_USED = GRC_BMM + 8,  // Character set still used
            GRC_BMM_CHARSET_SAVED = GRC_BMM + 9,  // Charset cannot be deleted or is protected
            GRC_BMM_INVALID_ADR = GRC_BMM + 10, // Attempt to copy a character block\noutside the allocated memory
            GRC_BMM_CANCELANDADR_ERROR = GRC_BMM + 11, // Error during release of allocated memory
            GRC_BMM_INVALID_SIZE = GRC_BMM + 12, // Number of bytes specified in header\ndoes not match the bytes read
            GRC_BMM_CANCELANDINVSIZE_ERROR = GRC_BMM + 13, // Allocated memory could not be released
            GRC_BMM_ALL_GROUP_OCC = GRC_BMM + 14, // Max. number of character sets already loaded
            GRC_BMM_CANT_DEL_LAYERS = GRC_BMM + 15, // Layer cannot be deleted
            GRC_BMM_UNKNOWN_LAYER = GRC_BMM + 16, // Required layer does not exist
            GRC_BMM_INVALID_LAYERLEN = GRC_BMM + 17; // Layer length exceeds maximum
    }

    public enum BOOLE          // BOOLEan type
    {
        FALSE,
        TRUE
    };

    public enum COM_BAUD_RATE                             // Used for public customer interface
    {
        COM_BAUD_38400,                       //
        COM_BAUD_19200,                       //
        COM_BAUD_9600,                        //
        COM_BAUD_4800,                        //
        COM_BAUD_2400,                        //
        COM_BAUD_115200,                      //
        COM_BAUD_57600                        // 
    };

    public enum COM_PORT                            // Communications port selector (only used for DLL)
    {
        COM_1,                                // COM1 (Windows)
        COM_2,                                // COM2 (Windows)
        COM_3,                                // COM3 (Windows)
        COM_4,                                // COM4 (Windows)
        COM_5,
        COM_6,
        COM_7,
        COM_8,
        COM_9,
        COM_10,
        COM_11,
        COM_12,
        COM_13,
        COM_14,
        COM_15,
        COM_16,
        COM_17,
        COM_18,
        COM_19,
        COM_20,
        COM_21,
        COM_22,
        COM_23,
        COM_24,
        COM_USB,
        COM_TCP
    };

    public enum COM_FORMAT                           // Geocom format selector
    {
        COM_ASCII,                            //
        COM_BINARY                            //
    };

    public enum COM_TPS_STARTUP_MODE                          //  TPS startup selector
    {
        COM_TPS_STARTUP_LOCAL = 0,            //   boot into local mode of TPS
        COM_TPS_STARTUP_REMOTE = 1            //             remote mode of TPS
    } ;

    public enum COM_TPS_STOP_MODE                           //  TPS startup selector
    {
        COM_TPS_STOP_SHUT_DOWN,               //   shut down of TPS
        COM_TPS_STOP_SLEEP,                   //   put TPS into sleep mode
        COM_TPS_STOP_REBOOT                   //   reboot of TPS
    } ;

    public enum COM_TPS_STATUS                           //  Current state of server
    {
        COM_TPS_OFF,                          //  TPS is off
        COM_TPS_SLEEPING,                     //  TPS is in sleep mode
        COM_TPS_ONLINE,                       //  TPS is on & active, online mode
        COM_TPS_LOCAL,                        //  TPS is in local mode
        COM_TPS_UNKNOWN                       //  Unknown/Not initialized
    }  ;

    public struct DATE_TYPE
    {
        public short Year;       // year
        public Byte Month;      // month in year                1..12
        public Byte Day;		// day in month                 1..31
    };

    public struct TIME_TYPE
    {
        public Byte Hour;       // 24 hour per day  0..23
        public Byte Minute;     // minute           0..59
        public Byte Second;     // seconds          0..59
    };

    public struct DATIME
    {
        public DATE_TYPE Date;
        public TIME_TYPE Time;
    };

    public enum ON_OFF_TYPE        // on/off switch type
    {
        OFF,
        ON
    };

    public enum IMG_MEM_TYPE // MemDeviceType
    {
        IMG_INTERNAL_MEMORY = 0x0,     // internal memory module (optional)
        IMG_PC_CARD = 0x1,     // external pc card
    };

    public enum EDM_MODE
    { // Possible measuring programs, not each program will be
        // supported from the different sensor HW's
        EDM_MODE_NOT_USED,      // Init value
        EDM_SINGLE_TAPE,        // S.dist   (tape            , IR, f)
        EDM_SINGLE_STANDARD,    // S.dist   (standard   DIST , IR, g)
        EDM_SINGLE_FAST,        // S.dist   (coarse     DI   , IR, j)
        EDM_SINGLE_LRANGE,      // S.dist   (long range      , SR, o)
        EDM_SINGLE_SRANGE,      // S.dist   (short range     , SR, q)
        EDM_CONT_STANDARD,      // Trk dist (standard   TRK  , IR, h)
        EDM_CONT_DYNAMIC,       // Trk dist (dynamic         , IR, p)
        EDM_CONT_REFLESS,       // Trk dist (reflector less  , SR, r)
        EDM_CONT_FAST,          // Rpd dist (rapid trk  RTRK , IR, m)
        EDM_AVERAGE_IR,         // S.dist   (average DIL     , IR, g) 
        EDM_AVERAGE_SR,         // S.dist   (average DIL     , SR, q) 
        EDM_AVERAGE_LR,         // S.dist   (average DIL     , SR, o)
        EDM_PRECISE_IR,         // S.dist   (precise    DIST , IR, l) TS30/TM30 only
        EDM_PRECISE_TAPE        // S.dist   (prec tape  DIST , IR, u) TS30/TM30 only
    };

    public enum EDM_EGLINTENSITY_TYPE
    {// EGL intensity
        EDM_EGLINTEN_OFF,
        EDM_EGLINTEN_LOW,
        EDM_EGLINTEN_MID,
        EDM_EGLINTEN_HIGH
    };

    public enum EDM_MEASUREMENT_TYPE
    {// Possible EDM sensor measurements
        EDM_SIGNAL_MEASUREMENT = 1, // Inquiryflag for signal  measurement
        EDM_FREQ_MEASUREMENT = 2, //      "          frequency     "
        EDM_DIST_MEASUREMENT = 4, //      "          distance      "
        EDM_ANY_MEASUREMENT = 8  //      "          any           "
    };

    // controller type:
    public enum MOT_MODE
    {
        MOT_POSIT = 0,                // positioning controller
        MOT_OCONST = 1,                // angular velocity controller
        MOT_MANUPOS = 2,                // manual fine pointing
        MOT_LOCK = 3,                // target lock controller
        MOT_BREAK = 4,                // break controller
        MOT_SERVICE = 5,                // service mode (not used)
        MOT_SEARCH = 6,                // search controller
        MOT_NONE = 7,                // controller not configurated
    };

    // LockIn-Reglerstatus:
    public enum MOT_LOCK_STATUS
    {
        MOT_LOCKED_OUT,
        MOT_LOCKED_IN,
        MOT_PREDICTION
    };


    // deceleration modi:
    public enum MOT_STOPMODE
    {
        MOT_NORMAL,                     // standard brake acceleration
        MOT_SHUTDOWN                    // maximum brake acceleration
    };

    public struct MOT_COM_PAIR
    {
        public double[] adValue;
    }

    public enum AUT_POSMODE
    { AUT_NORMAL, AUT_PRECISE };          //Positionierungsmodi

    public enum AUT_ATRMODE
    { AUT_POSITION, AUT_TARGET };         //Positionierungsmodi der ATR

    public enum AUT_ADJMODE
    {
        AUT_NORM_MODE,                     //fineAdjust-Modi
        AUT_POINT_MODE,
        AUT_DEFINE_MODE
    };

    public enum TMC_FACE_DEF
    {
        TMC_FACE_NORMAL,          // Die Lage entspricht den Sensorwerten (V)
        TMC_FACE_TURN             // Spiegelung der Lage
    };


    public enum TMC_INCLINE_PRG
    {                            /* NEIGUNGSMESSUNG        ABHNGIG SETUP */
        TMC_MEA_INC = 0,     /* Sensor (Apriori Sigma)       ja       */
        TMC_AUTO_INC = 1,     /* Automatismus (Sensor/Ebene)  ja       */
        TMC_PLANE_INC = 2      /* Ebene  (Apriori Sigma)       ja       */


    };


    public enum TMC_MEASURE_PRG         /*      TMC                         INCLINE       */
    {                             /*   Messprogramm               Messmode Messzeit */
        TMC_STOP = 0, /* Stopt das Messprogramm         nein     nein   */
        TMC_DEF_DIST = 1, /* Default DIST-Messprogramm      ja       nein   */
        TMC_TRK_DIST = 2, /* Distanz-TRK Messprogramm       ja       nein   */
        TMC_CLEAR = 3, /* TMC_STOP mit Clear Data        nein     nein   */
        TMC_SIGNAL = 4, /* Signalmessungen (Testfkt.)     nein     nein   */


        TMC_DO_MEASURE = 6, /* (Re)start den Mess-Task        ja       nein   */


        TMC_RTRK_DIST = 8, /* Distanz-RTRK Messprogramm      ja       nein   */


        TMC_RED_TRK_DIST = 10,/* Distanz-RED-TRK Messprogramm   ja       nein   */
        TMC_FREQUENCY = 11 /* Frequenzmessung (Testfkt.)     nein     nein   */


    };


    public enum TMC_FACE
    {
        TMC_FACE_1,                             /* Lage 1 der Fernrohrs */
        TMC_FACE_2                              /* Lage 2 der Fernrohrs */
    };


    public enum TMC_AIM_TYPE
    {
        TMC_AIM_TYPE_USER = 0,         // Leica user (default = 34.4)
        TMC_AIM_TYPE_PRISM_ROUND,              // Standard prism
        TMC_AIM_TYPE_PRISM_MINI,               // Mini prism
        TMC_AIM_TYPE_PRISM_360,                // 360ר prism
        TMC_AIM_TYPE_TAPE,                     // Tape
        TMC_AIM_TYPE_REFLESS,                  // refless
        TMC_AIM_TYPE_JPMINI,                   // japanese mini prism (SMP222)
        TMC_AIM_TYPE_HDS_TAPE,                 // Cyra Tape
        TMC_AIM_TYPE_USER_TAPE,                // User Tape
        TMC_AIM_TYPE_GRZ121_ROUND,             // MA 360 GRZ121 prism
        TMC_AIM_TYPE_MA_MPR122                 // MA 360 power prism
    };

    public enum TMC_AIM_VALUE
    {
        TMC_AIM_VALUE_RELATIV = 0,         // Zieltypewertedarstellung relativ
        TMC_AIM_VALUE_ABSOLUT                  // Zieltypewertedarstellung absolut
    };

    public enum TMC_SYSTEM_DEDECT
    {
        THEODOLITE,                // Theodolite System
        TACHYMETER,                // Tachymeter System
        DIGITAL_LEVEL              // Digital Level system

    } ;

    public enum SUP_AUTO_POWER
    {
        AUTO_POWER_DISABLED,        // Keine Wirkung der Automatik
        AUTO_POWER_SLEEP,           // Automatik legt System schlafen
        AUTO_POWER_OFF              // Automatik schaltet System aus
    } ;

    public enum CSV_POWER_PATH
    {
        CSV_CURRENT_POWER,
        CSV_EXTERNAL_POWER,
        CSV_INTERNAL_POWER
    };

    /* definition of the prism type */
    /* Do not change enum values. They are used as array index. */
    public enum BAP_PRISMTYPE
    {
        BAP_PRISM_ROUND = 0, // prism type: round
        BAP_PRISM_MINI = 1, // prism type: mini
        BAP_PRISM_TAPE = 2, // prism type: tape
        BAP_PRISM_360 = 3, // prism type: 360
        BAP_PRISM_USER1 = 4, // prism type: user1 
        BAP_PRISM_USER2 = 5, // prism type: user2
        BAP_PRISM_USER3 = 6, // prism type: user3
        BAP_PRISM_360_MINI = 7, // prism type: 360 mini 
        BAP_PRISM_MINI_ZERO = 8, // prism type: mini zero 
        BAP_PRISM_USER = 9, // prism type: user
        BAP_PRISM_HDS_TAPE = 10, // prism type: tape cyra
        BAP_PRISM_GRZ121_ROUND = 11, // prism type: GRZ121 360 for machine guidance
        BAP_PRISM_MA_MPR122 = 12 // prism type: MPR122 360 for machine guidance
    } ;

    /* definiton of the reflector type */
    public enum BAP_REFLTYPE
    {
        BAP_REFL_UNDEF, // reflector not defined
        BAP_REFL_PRISM, // reflector prism
        BAP_REFL_TAPE   // reflector tape
    } ;

    public enum BAP_ATRSETTING
    {
        BAP_ATRSET_NORMAL,				// ATR is using no special flags or modes
        BAP_ATRSET_LOWVIS_ON,			// ATR low vis mode on
        BAP_ATRSET_LOWVIS_AON,			// ATR low vis mode always on
        BAP_ATRSET_SRANGE_ON,			// ATR high reflectivity mode on
        BAP_ATRSET_SRANGE_AON,			// ATR high reflectivity mode always on
    } ;


    public enum BAP_MEASURE_PRG
    {
        BAP_NO_MEAS,          // no measurements
        BAP_NO_DIST,          // distance measurement, only angles
        BAP_DEF_DIST,         // default distance measurement program
        BAP_TRK_DIST,         // TRK distance measurement program and angles
        BAP_RTRK_DIST,        // RTRK distance measurement program and angles
        BAP_CLEAR_DIST,       // clear distances 
        BAP_STOP_TRK,         // stop tracking
        BAP_RED_TRK_DIST      // tracking with red laser
    } ;

    //    /* definition of prism */
    //public struct BAP_PRISMDEF
    //{
    //   char         szName[BAP_PRISMNAME_LEN+1];
    //   double       dAddConst;
    //   BAP_REFLTYPE eReflType;
    //};


    /* definition of the distance measure program type */
    /* Do not change enum values. They are used as array index. */
    public enum BAP_USER_MEASPRG
    {
        BAP_SINGLE_REF_STANDARD = 0, 	// SINGLE = single measurement
        BAP_SINGLE_REF_FAST = 1, 	  	// REF = 	with reflector
        BAP_SINGLE_REF_VISIBLE = 2, 	// VISIBLE = with red laser
        BAP_SINGLE_RLESS_VISIBLE = 3,// RLESS = without reflector
        BAP_CONT_REF_STANDARD = 4, 	// CONT = continuous measurement
        BAP_CONT_REF_FAST = 5,
        BAP_CONT_RLESS_VISIBLE = 6,
        BAP_AVG_REF_STANDARD = 7, 		// AVG = average measurement
        BAP_AVG_REF_VISIBLE = 8,
        BAP_AVG_RLESS_VISIBLE = 9,
        BAP_CONT_REF_SYNCHRO = 10,     // SynchroTrack, IR
        BAP_SINGLE_REF_PRECISE = 11    // Precise, IR (TS30,TM30)
    } ;



    public enum BAP_TARGET_TYPE
    {
        BAP_REFL_USE,	// with reflector
        BAP_REFL_LESS	// without reflector
    };

    public struct TMC_STATION
    {
        public double dE0;                     /*            Standpunkt-Koordinate */
        public double dN0;                     /*            Standpunkt-Koordinate */
        public double dH0;                     /*            Standpunkt-Koordinate */

        public double dHi;                     /*           Instrumentenhoehe */
    };

    public struct TMC_INCLINE
    {
        public double dCrossIncline;                 /*             Querneigung */
        public double dLengthIncline;                /*           Laengsneigung */
        public double dAccuracyIncline;              /* Genauigkeit der Neigung */
        public long InclineTime;                  /*   Zeitpunkt der Messung */
    };

    public struct TMC_ATR
    {
        public double dHz;                   // Horizontaler Zielkorrektur
        public double dV;                    // Verticale Zielkorrektur
        public double dAtrAccuracy;          // Genauigkeit der Ablagemessung
        public long AtrTime;              // Zeitpunkt der Messung
    };

    public struct TMC_ANGLE
    {
        public double dHz;                   /*              Horizontalwinkel */
        public double dV;                    /*                Vertikalwinkel */
        public double dAngleAccuracy;        /*       Genauigkeit der Winkels */
        public long AngleTime;            /*         Zeitpunkt der Messung */

        public TMC_INCLINE Incline;          /*         Dazugehoerige Neigung */
        public TMC_FACE eFace;               /* Lageinformation des Fernrohrs */
    };

    public struct TMC_ANGLE_ALL
    {
        public double dHz;                   /*              Horizontalwinkel */
        public double dV;                    /*                Vertikalwinkel */
        public long AngleTime;            /*         Zeitpunkt der Messung */

        public double dCrossIncline;         /*                   Querneigung */
        public double dLengthIncline;        /*                 Laengsneigung */
        public long InclineTime;          /*         Zeitpunkt der Messung */
        public TMC_INCLINE_PRG IncMode;      /*                    level mode */

        public double dAtrHz;                /*         Hz-Ablagewinkel [rad] */
        public double dAtrV;                 /*       V-Ablagewinkel in [rad] */
        public long AtrTime;              /*         Zeitpunkt der Messung */
        public short AtrRetCode;           /*               Atr-Return-Code */
    };                    /* NOT USED */


    public struct TMC_HZ_V_ANG
    {
        public double dHz;                      /*           Horizontalwinkel */
        public double dV;                       /*             Vertikalwinkel */

    };                     /* NOT USED */


    public struct TMC_COORDINATE
    {
        public double dE;              /*                       E-Koordinaten */
        public double dN;              /*                       N-Koordinaten */
        public double dH;              /*                       H-Koordinaten */
        public long CoordTime;      /*               Zeitpunkt der Messung */

        public double dE_Cont;         /*       E-Koordinate (kontinuierlich) */
        public double dN_Cont;         /*       N-Koordinate (kontinuierlich) */
        public double dH_Cont;         /*       H-Koordinate (kontinuierlich) */
        public long CoordContTime;  /*               Zeitpunkt der Messung */
    };


    public struct TMC_EDM_SIGNAL
    {
        public double dSignalIntensity;       /*  Signalstaerke des EDM's in % */
        public long Time;                   /* Zeitpunkt der letzten Messung */
    };                                  /* NOT USED */

    public struct TMC_EDM_FREQUENCY
    {
        public double dFrequency;             // Frequenz des EDM's in Hz
        public long Time;                   // Zeitpunkt der letzten Messung
    };                   
}
