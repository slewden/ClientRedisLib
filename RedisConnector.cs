using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientRedisLib.RedisClass;

namespace ClientRedisLib
{
  /// <summary>
  /// A client for Redis Server
  /// Principal File.
  /// Public Methods mapping Server REDIS commands are in RedisConnector2.cs
  /// </summary>
  public partial class RedisConnector : IDisposable, IReadUnifiedProtocol
  {
    #region Const
    /// <summary>
    /// Redis default DB id
    /// </summary>
    public const int DEFAULTDB = 0;

    /// <summary>
    /// Redis Server Default Socket port
    /// </summary>
    public const int DEFAULTPORT = 6379;

    /// <summary>
    /// Redis Server Host name
    /// </summary>
    public const string DEFAULTHOST = "localhost";

    /// <summary>
    /// Redis DB ID min
    /// </summary>
    private const int MINDBID = 0;

    /// <summary>
    /// Redis db Id max
    /// </summary>
    private const int MAXDBID = 15;

    /// <summary>
    /// Add to all command line when the server in not in the good version
    /// </summary>
    private const string NOTIMPLEMENTEDINTHISVERSION = " [Not implemented on this server (need V2.6 at least)]";

    /// <summary>
    /// Problem when getting Number error message
    /// </summary>
    private const string CSTERRORNAN = "Answer return NAN";

    /// <summary>
    /// Value of then 2.4.xx version calculate to determinate the apropriate option
    /// </summary>
    private const int SERVER24 = 204;

    /// <summary>
    /// Value of then 2.6.xx version calculate to determinate the apropriate option
    /// </summary>
    private const int SERVER26 = 206;

    /// <summary>
    /// End of line for commands
    /// </summary>
    private readonly byte[] endOfLine = new[] { (byte)'\r', (byte)'\n' };

    /// <summary>
    /// Idle time out : default on redis is 300
    /// </summary>
    private readonly int idleTimeOutSecs = 240;
    #endregion

    #region private member
    /// <summary>
    /// Current db ID
    /// </summary>
    private int db = RedisConnector.DEFAULTDB;

    /// <summary>
    /// Memorize the version like that : [principal] * 100 + [Secondary]
    /// </summary>
    private int serverVersion = -1;

    #region Soket properties
    /// <summary>
    /// Connexion IP Soket
    /// </summary>
    private Socket socket;

    /// <summary>
    /// Port number for the client connexion.
    /// </summary>
    private int clientPort;

    /// <summary>
    /// Read buffer associated with soket
    /// </summary>
    private BufferedStream bufferStream = null;

    /// <summary>
    /// Redis Pipeline mode active if not null
    /// </summary>
    private RedisPipeline pipeline = null;

    /// <summary>
    /// Write command buffer
    /// </summary>
    private byte[] cmdBuffer = new byte[32 * 1024];

    /// <summary>
    /// Write command buffer index
    /// </summary>
    private int cmdBufferIndex = 0;

    /// <summary>
    /// Idle counter
    /// </summary>
    private long lastConnectedAtTimestamp;
    
    /// <summary>
    /// Connection event wainting envent when timeOut is set
    /// </summary>
    private ManualResetEvent connectDone = new ManualResetEvent(false);
    #endregion

    /// <summary>
    /// CallBack for monitor process
    /// </summary>
    private AsyncCallback processMonitorCallBack;

    /// <summary>
    /// Call back for subscribe and psubscribe methods
    /// </summary>
    private AsyncCallback processSubscribeCallBack;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor with default port and no password
    /// </summary>
    /// <param name="host">Server Redis Host</param>
    public RedisConnector(string host)
      : this(host, RedisConnector.DEFAULTPORT, string.Empty)
    {
    }

    /// <summary>
    /// Constructor with no password
    /// </summary>
    /// <param name="host">Server Redis Host</param>
    /// <param name="port">Server port</param>
    public RedisConnector(string host, int port)
      : this(host, port, string.Empty)
    {
    }

    /// <summary>
    /// Fully qualified constructor
    /// </summary>
    /// <param name="host">Server Redis Host</param>
    /// <param name="port">Server port</param>
    /// <param name="password">Password to connect</param>
    public RedisConnector(string host, int port, string password)
    {
      if (string.IsNullOrWhiteSpace(host))
      {
        throw new ArgumentException("Invalid Host name");
      }

      this.SendTimeout = -1;
      this.ReceiveTimeout = -1;

      this.Host = host;
      this.Port = port;
      this.Password = password;
      this.LastErrorText = string.Empty;
    }
    #endregion

    #region Public parameters
    /// <summary>
    /// Redis server host
    /// </summary>
    public string Host { get; private set; }

    /// <summary>
    /// Redis server port
    /// </summary>
    public int Port { get; private set; }

    /// <summary>
    /// redis server password
    /// </summary>
    public string Password { get; private set; }

    /// <summary>
    /// last error encountered
    /// </summary>
    public string LastErrorText { get; private set; }

    /// <summary>
    /// Last command launched
    /// </summary>
    public string LastCommandText { get; private set; }

    /// <summary>
    /// Socket send timeout for fine tunning (default set to -1)
    /// </summary>
    public int SendTimeout { get; set; }

    /// <summary>
    /// Socket receive timeout for fine tunning (default set to -1)
    /// </summary>
    public int ReceiveTimeout { get; set; }

    /// <summary>
    /// Connect to server Time out (default set to 0 = no wait)
    /// </summary>
    public int ConnectTimeout { get; set; }

    /// <summary>
    /// Return the string that represent the server version
    /// </summary>
    public string ServerVersionTxt { get; private set; }

    /// <summary>
    /// Return True if the Redis Server version is Higher or equal to 2.4
    /// Under this version the driver isn't be tested
    /// </summary>
    public bool ServerVersionIsHigherThan240
    {
      get
      {
        if (this.serverVersion < 0)
        {
          this.Info();
        }

        return this.serverVersion >= SERVER24;
      }
    }

    /// <summary>
    /// Return true if the server is in sentinel mode
    /// </summary>
    public bool ServerInSentinelMode { get; private set; }
    #endregion

    #region Public properties (wich send command to be filled)
    /// <summary>
    /// Get or set the database ID
    /// </summary>
    public int Db
    {
      get
      {
        return this.db;
      }

      set
      {
        this.Select(value);
      }
    }
    #endregion

    #region Static methods : Utilities
    /// <summary>
    /// Return a byte array for a char and arg lenght
    /// </summary>
    /// <param name="cmdPrefix">Prefix of the command * or $</param>
    /// <param name="lenght">Number of parameters or length</param>
    /// <returns>the datas</returns>
    public static byte[] GetCmdBytes(char cmdPrefix, int lenght)
    {
      string strLines = lenght.ToString();
      int strLinesLength = strLines.Length;

      // 1 + => cmdPrefix.lenght + lenght.Tostring.Lenght + 2 => \r\n
      int totalsize = 1 + strLinesLength + 2;

      byte[] cmdBytes = new byte[totalsize];
      cmdBytes[0] = (byte)cmdPrefix;

      for (int i = 0; i < strLinesLength; i++)
      {
        cmdBytes[i + 1] = (byte)strLines[i];
      }

      cmdBytes[1 + strLinesLength] = (byte)'\r';  // 0x0D; // \r
      cmdBytes[2 + strLinesLength] = (byte)'\n';  // 0x0A; // \n

      return cmdBytes;
    }

    /// <summary>
    /// Convert UTF8 to String
    /// </summary>
    /// <param name="bytes">the datas</param>
    /// <returns>the string</returns>
    public static string GetStringFromUtf8Bytes(byte[] bytes)
    {
      return bytes == null ? null : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
      /*
      if (bytes == null)
      {
        return null;
      }
      else
      {
        StringBuilder res = new StringBuilder();
        foreach (byte b in bytes)
        {
          res.Append((char)b);
        }

        return res.ToString();
      }
       */
    }

    /// <summary>
    /// Convert string To UTF8
    /// </summary>
    /// <param name="value">the string</param>
    /// <returns>the datas</returns>
    public static byte[] GetBytesUtf8FromString(string value)
    {
      return Encoding.UTF8.GetBytes(value);
    }
 
    /// <summary>
    /// Indicate if the socket is connected
    /// </summary>
    /// <param name="socket">the soket to test</param>
    /// <returns>True is is connected</returns>
    public static bool IsConnected(Socket socket)
    {
      try
      {
        return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
      }
      catch (SocketException)
      {
        return false;
      }
    }

    /// <summary>
    /// Convert a unix time to a DateTime
    /// </summary>
    /// <param name="unixTime">The number of seconds after unix Epoch</param>
    /// <returns>A dateTime in Local time</returns>
    public static DateTime GetDateTimeFromUnixTime(double unixTime)
    {
      if (unixTime.Equals(double.NaN))
      {
        return DateTime.MinValue;
      }
      else
      {
        DateTime unixEpochDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (unixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime)).ToLocalTime();
      }
    }

    /// <summary>
    /// Convert a DateTime to an unix time
    /// </summary>
    /// <param name="date">The date to convert</param>
    /// <returns>the number of second since January 1, 1970</returns>
    public static double GetUnixTimeFromDateTime(DateTime date)
    {
      DateTime unixEpochDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      TimeSpan ts = date.Subtract(unixEpochDateTimeUtc);
      return ts.TotalSeconds;
    }

    /// <summary>
    /// Convert a duration from microSeconds to TimeSpan
    /// </summary>
    /// <param name="value">duration in microseconds</param>
    /// <returns>the TimeSpan</returns>
    public static TimeSpan GetTimeSpanFromMicroSecString(string value)
    {
      long ts;
      if (long.TryParse(value, out ts))
      { // value est en MicroSecondes
        return new TimeSpan(ts * 10);
      }
      else
      {
        return TimeSpan.Zero;
      }
    }

    /// <summary>
    /// Convert a duration From MilliSeconds to TimeSpan
    /// </summary>
    /// <param name="value">duration in milliseconds</param>
    /// <returns>the TimeSpan</returns>
    public static TimeSpan GetTimeSpanFromMilliSecDouble(double value)
    {
      return new TimeSpan(Convert.ToInt64(value));
    }

    /// <summary>
    /// Convert a duration From Seconds to TimeSpan
    /// </summary>
    /// <param name="value">duration in seconds</param>
    /// <returns>the TimeSpan</returns>
    public static TimeSpan GetTimeSpanFromSecDouble(double value)
    {
      return TimeSpan.FromSeconds(Convert.ToInt64(value));
    }

    /// <summary>
    /// Get a double from a string
    /// </summary>
    /// <param name="value">the string to parse</param>
    /// <param name="defaultValue">if error return value</param>
    /// <returns>the double</returns>
    public static double GetDoubleFromString(string value, double defaultValue)
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        double n;
        if (double.TryParse(value.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator).Replace(",", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out n))
        {
          return n;
        }
      }

      return defaultValue;
    }

    /// <summary>
    /// Convert a double to string in order to send a argument to Redis Server (Culture invariant)
    /// </summary>
    /// <param name="value">Double to convert</param>
    /// <returns>the string (culture invariant)</returns>
    public static string GetStringFromDouble(double value)
    {
      return value.ToString().Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".").Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, string.Empty);
    }

    /// <summary>
    /// Get a long from a string
    /// </summary>
    /// <param name="value">the string to parse</param>
    /// <param name="defaultValue">if error return value</param>
    /// <returns>the long</returns>
    public static long GetLongFromString(string value, long defaultValue)
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        long n;
        if (long.TryParse(value, out n))
        {
          return n;
        }
      }

      return defaultValue;
    }

    /// <summary>
    /// Add the command string to the begining of the array argument
    /// </summary>
    /// <param name="command">string to add in first place</param>
    /// <param name="arguments">array of strings</param>
    /// <returns>One array with command first and arguments after</returns>
    public static string[] MergeStringBefore(string command, params string[] arguments)
    {
      string[] parameters = new string[arguments.Length + 1];
      parameters[0] = command;
      for (int i = 0; i < arguments.Length; i++)
      {
        parameters[i + 1] = arguments[i];
      }

      return parameters;
    }

    /// <summary>
    /// Add the command string to the begining of the array argument
    /// </summary>
    /// <param name="arguments">array of array of strings</param>
    /// <returns>One array with command first and arguments after</returns>
    public static string[] MergeStringBefore(params string[][] arguments)
    {
      int nb = 0;
      for (int i = 0; i < arguments.Length; i++)
      {
        nb += arguments[i].Length;
      }

      string[] parameters = new string[nb];
      int n = 0;
      for (int i = 0; i < arguments.Length; i++)
      {
        for (int j = 0; j < arguments[i].Length; j++)
        {
          parameters[n++] = arguments[i][j] ?? string.Empty;
        }
      }

      return parameters;
    }

    /// <summary>
    /// Add the command string to the begining of the array argument
    /// </summary>
    /// <param name="arguments">array of strings</param>
    /// <param name="fieldValue">first tuple</param>
    /// <param name="fieldsValues">array of Tuple key value</param>
    /// <returns>One array with command first and arguments after</returns>
    public static string[] MergeStringBefore(string[] arguments, Tuple<string, string> fieldValue, params Tuple<string, string>[] fieldsValues)
    {
      int nb = arguments.Length + (fieldsValues.Length * 2) + 2;
      string[] parameters = new string[nb];
      int n = 0;
      for (int i = 0; i < arguments.Length; i++)
      {
        parameters[n++] = arguments[i] ?? string.Empty;
      }

      parameters[n++] = fieldValue.Item1 ?? string.Empty;
      parameters[n++] = fieldValue.Item2 ?? string.Empty;
      for (int j = 0; j < fieldsValues.Length; j++)
      {
        parameters[n++] = fieldsValues[j].Item1 ?? string.Empty;
        parameters[n++] = fieldsValues[j].Item2 ?? string.Empty;
      }

      return parameters;
    }
    
    /// <summary>
    /// Add the command string to the begining of the array argument
    /// </summary>
    /// <param name="arguments">array of strings</param>
    /// <param name="fieldValue">first tuple</param>
    /// <param name="fieldsValues">array of Tuple key value</param>
    /// <returns>One array with command first and arguments after</returns>
    public static string[] MergeStringBeforeList(string cmd, string arguments, List<Tuple<string, string>> fieldsValues)
    {
      int nb = 2 + (fieldsValues.Count * 2);
      string[] parameters = new string[nb];
      int n = 0;
      parameters[n++] = cmd ?? string.Empty;
      parameters[n++] = arguments ?? string.Empty;

      for (int j = 0; j < fieldsValues.Count; j++)
      {
        parameters[n++] = fieldsValues[j].Item1 ?? string.Empty;
        parameters[n++] = fieldsValues[j].Item2 ?? string.Empty;
      }

      return parameters;
    }

    /// <summary>
    /// Add the command string to the begining of the array argument
    /// </summary>
    /// <param name="arguments">array of strings</param>
    /// <param name="fieldValue">first SortedSet</param>
    /// <param name="fieldsValues">array of SortedSet key value</param>
    /// <returns>One array with command first and arguments after</returns>
    public static string[] MergeStringBefore(string[] arguments, SortedSet fieldValue, params SortedSet[] fieldsValues)
    {
      int nb = arguments.Length + (fieldsValues.Length * 2) + 2;
      string[] parameters = new string[nb];
      int n = 0;
      for (int i = 0; i < arguments.Length; i++)
      {
        parameters[n++] = arguments[i] ?? string.Empty;
      }

      parameters[n++] = RedisConnector.GetStringFromDouble(fieldValue.Score) ?? string.Empty;
      parameters[n++] = fieldValue.Member ?? string.Empty;
      for (int j = 0; j < fieldsValues.Length; j++)
      {
        parameters[n++] = RedisConnector.GetStringFromDouble(fieldsValues[j].Score) ?? string.Empty;
        parameters[n++] = fieldsValues[j].Member ?? string.Empty;
      }

      return parameters;
    }
    #endregion

    #region IDisposable Methods
    /// <summary>
    /// Dispose the connection
    /// </summary>
    public void Dispose()
    {
      this.SafeConnectionClose();
    }
    #endregion

    #region IReadUnifiedProtocol Methods
    /// <summary>
    /// Read a bit from this.bStram
    /// </summary>
    /// <returns>the byte</returns>
    public int ReadByte()
    {
      if (this.bufferStream != null)
      {
        try
        {
          int c = this.bufferStream.ReadByte();
          while (c == '\n' || c == '\r')
          {
            c = this.bufferStream.ReadByte();
          }

          return c;
        }
        catch (Exception ex)
        {
          this.LastErrorText = ex.ToString();
          return -2;
        }
      }
      else
      {
        return -1;
      }
    }

    /// <summary>
    /// Read a line from bStream a line end with \n
    /// \r are filtered
    /// </summary>
    /// <returns>The read string</returns>
    public string ReadLine()
    {
      StringBuilder sb = new StringBuilder();

      if (this.bufferStream != null)
      {
        int c;
        while ((c = this.bufferStream.ReadByte()) != -1)
        {
          if (c == '\r')
          { // we ignore this char
            continue;
          }

          if (c == '\n')
          { // ok stop : we got a end of line
            break;
          }

          sb.Append((char)c);
        }
      }

      return sb.ToString();
    }

    /// <summary>
    /// Read lenght car and place it in retbuf return the number of char in retbuf
    /// </summary>
    /// <param name="retbuf">Read buffer to fill</param>
    /// <param name="lenght">Number of char to fill</param>
    /// <returns>Number of char filled</returns>
    public int ReadAny(ref byte[] retbuf, int lenght)
    {
      var offset = 0;
      while (lenght > 0)
      {
        var readCount = this.bufferStream.Read(retbuf, offset, lenght);
        if (readCount <= 0)
        {
          return -1;
        }
        else
        {
          offset += readCount;
          lenght -= readCount;
        }

        if (this.bufferStream.ReadByte() != '\r' || this.bufferStream.ReadByte() != '\n')
        {
          return -2;
        }
      }

      return lenght;
    }

    /// <summary>
    /// Construct an error message
    /// </summary>
    /// <param name="errorMsg">The message to format</param>
    /// <returns>Formatted Text</returns>
    public string MakeErrorText(string errorMsg)
    {
      return string.Format("Error Host : {0} Port : {1} : Client Port {2} : Command {3} = {4}", this.Host, this.Port, this.clientPort, this.LastCommandText, errorMsg);
    }
    #endregion

    /// <summary>
    /// Compute and return the server version
    /// </summary>
    /// <returns>return the server version in a string</returns>
    public string GetServerVersionText()
    {
      if (this.serverVersion <= 0)
      {
        this.Info(); // this command load the info and compute it
      }

      return this.ServerVersionTxt;
    }

    #region Servers versionning
    /// <summary>
    /// Get the server version launch an INFO command if needed
    /// </summary>
    /// <returns>the sever version</returns>
    protected int GetServerVersion()
    {
      if (this.serverVersion <= 0)
      {
        this.Info(); // this command load the info and compute it
      }

      return this.serverVersion;
    }

    /// <summary>
    /// Compute the server version from a string like "2.6.5.12" : 
    /// the only 2 first digits are used
    /// </summary>
    /// <param name="vers">The number of the version [princ] * 100 + [second]</param>
    protected void ComputeVersion(string vers)
    {
      if (!string.IsNullOrWhiteSpace(vers))
      {
        if (vers.IndexOf(".") != -1)
        {
          string[] nfo = vers.Split('.');
          int principal, second;
          if (int.TryParse(nfo[0], out principal) && int.TryParse(nfo[1], out second))
          {
            this.serverVersion = (principal * 100) + second;
          }
        }

        this.ServerVersionTxt = vers;
      }
    }
    #endregion

    #region Connection / disconnection
    /// <summary>
    /// Check server connection and connecte if needed
    /// </summary>
    /// <returns>Connected or not</returns>
    protected bool AssertConnectedSocket()
    {
      if (this.lastConnectedAtTimestamp > 0)
      {
        var now = Stopwatch.GetTimestamp();
        var elapsedSecs = (now - this.lastConnectedAtTimestamp) / Stopwatch.Frequency;

        if (this.socket == null || (elapsedSecs > this.idleTimeOutSecs && !RedisConnector.IsConnected(this.socket)))
        {
          return this.Reconnect();
        }

        this.lastConnectedAtTimestamp = now;
      }

      if (this.socket == null)
      { // First time (?)
        var previousDb = this.db;
        this.Connect();
        if (previousDb != RedisConnector.DEFAULTDB)
        { // Set the good db
          this.Db = previousDb;
        }
      }

      if (this.socket != null && !this.socket.Connected)
      { // Connexion loosed
        var previousDb = this.db;
        this.Connect();
        if (previousDb != RedisConnector.DEFAULTDB)
        { // Set the good db
          this.Db = previousDb;
        }
      }

      return this.socket != null;
    }

    /// <summary>
    /// Reconnect after an idle time or loose socket connexion
    /// </summary>
    /// <returns>Connected or not</returns>
    protected bool Reconnect()
    {
      var previousDb = this.db;
      this.SafeConnectionClose();
      this.Connect();

      if (previousDb != RedisConnector.DEFAULTDB)
      { // Connect sets db to 0 so we need to change db ID
        this.Db = previousDb;
      }

      return this.socket != null;
    }

    /// <summary>
    /// Connect the socket
    /// </summary>
    protected void Connect()
    {
      this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      {
        SendTimeout = this.SendTimeout,
        ReceiveTimeout = this.ReceiveTimeout
      };

      try
      {
        if (this.ConnectTimeout == 0)
        {
          this.socket.Connect(this.Host, this.Port);
        }
        else
        {
          IAsyncResult connectResult = this.socket.BeginConnect(this.Host, this.Port, new AsyncCallback(this.ConnectCallBack), this.socket);
          this.connectDone.WaitOne(this.ConnectTimeout, true);
        }

        if (!this.socket.Connected)
        {
          this.LastErrorText = this.MakeErrorText("Fail to creat soket");
          this.socket.Close();
          this.socket = null;
          return;
        }

        this.bufferStream = new BufferedStream(new NetworkStream(this.socket), 16 * 1024);

        if (!string.IsNullOrWhiteSpace(this.Password))
        { // Authentification : We don't use the Auth method here 
          this.SendCommand("AUTH", this.Password);
        }

        this.db = 0; // RAZ db to reflect the reality of the connection
        this.serverVersion = -1; // raz the version too
        var endpoint = this.socket.LocalEndPoint as IPEndPoint;
        this.clientPort = endpoint != null ? endpoint.Port : -1;
        this.LastCommandText = null;
        this.lastConnectedAtTimestamp = Stopwatch.GetTimestamp();
      }
      catch (SocketException ex)
      {
        if (this.socket != null)
        {
          this.socket.Close();
        }

        this.socket = null;
        this.LastErrorText = this.MakeErrorText(string.Format("could not connect to redis Instance {0}", ex.ToString()));
      }
    }

    /// <summary>
    /// Call back connection when a time out exists
    /// </summary>
    /// <param name="ar">Asynch soket argument</param>
    protected void ConnectCallBack(IAsyncResult ar)
    {
      try
      {
        Socket soket = (Socket)ar.AsyncState;
        soket.EndConnect(ar);
      }
      catch
      { 
      }
      finally
      {
        this.connectDone.Set();
      }
    }

    /// <summary>
    /// Close the old connexion is exists
    /// </summary>
    protected void SafeConnectionClose()
    {
      try
      {
        if (this.bufferStream != null)
        {
          this.bufferStream.Close();
        }
      }
      catch
      {
      }

      try
      {
        if (this.socket != null)
        {
          this.socket.Close();
        }
      }
      catch
      {
      }

      this.bufferStream = null;
      this.socket = null;
      this.lastConnectedAtTimestamp = 0;
      this.serverVersion = -1;
    }
    #endregion

    #region Unified protocol Send
    /// <summary>
    /// Send command an arguments and get response
    /// </summary>
    /// <param name="arguments">Commande and arguments</param>
    /// <returns>the response</returns>
    protected RedisReponse SendCommand(params string[] arguments)
    {
      if (this.SendACommand(arguments))
      {
        if (!this.IsDebugSegFault(arguments))
        { // the DEBUGSEGFAULT cmd does not return anything !!
          if (this.pipeline != null)
          {
            this.pipeline.CompleteBytesQueuedCommand(this.ReadAnswer);
            return RedisReponse.GetPipelinning();
          }
          else
          {
            RedisReponse answer = this.ReadAnswer();
            if (!answer.Success)
            {
              this.LastErrorText = answer.ErrorMessage;
            }

            return answer;
          }
        }
        else
        {
          return RedisReponse.GetError("Server down : crashed by the command DEBUG SEGFAULT", EErrorCode.ServerDown);
        }
      }
      else
      {
        return RedisReponse.GetError(this.MakeErrorText(this.LastErrorText), EErrorCode.UnknowError);
      }
    }

    /// <summary>
    /// Check if arguments is "DEBUG", "SEGFAULT"
    /// </summary>
    /// <param name="arguments">Arrays of arguments</param>
    /// <returns>True if first is DEBUG and Second is SEGFAULT</returns>
    protected bool IsDebugSegFault(string[] arguments)
    {
      if (arguments.Length != 2)
      {
        return false;
      }

      return arguments[0].ToUpper() == "DEBUG" && arguments[1].ToUpper() == "SEGFAULT";
    }

    /// <summary>
    /// Process sendind a command
    /// </summary>
    /// <param name="arguments">Command and arguments</param>
    /// <returns>True is success, if false lastError is updated</returns>
    protected bool SendACommand(params string[] arguments)
    {
      if (!this.AssertConnectedSocket())
      {
        return false;
      }

      this.LastCommandText = arguments.Aggregate((x, y) => x + " " + y);
      try
      {
        this.WriteToSendBuffer(arguments);
        if (this.pipeline == null)
        {
          this.FlushSendBuffer();
        }

        return true;
      }
      catch (SocketException ex)
      {
        this.LastErrorText = ex.ToString();
        return false;
      }
    }

    /// <summary>
    /// Write something like *[nb param]\r\n$[length param 1]\r\n[param 1]\r\n$[length param 2]\r\n[param 2]\r\n...
    /// </summary>
    /// <param name="arguments">Data to write</param>
    protected void WriteToSendBuffer(string[] arguments)
    {
      this.WriteToSendBuffer(RedisConnector.GetCmdBytes('*', arguments.Length));

      byte[] cmd;
      foreach (string argument in arguments)
      {
        // attention a voir cyrylique et autre + system non UTF8
        //// cmd = Encoding.Default.GetBytes(argument);// RedisConnector.GetBytesUtf8FromString(argument);
        cmd = RedisConnector.GetBytesUtf8FromString(argument);
        this.WriteToSendBuffer(RedisConnector.GetCmdBytes('$', cmd.Length));
        this.WriteToSendBuffer(cmd);
        this.WriteToSendBuffer(this.endOfLine);
      }
    }

    /// <summary>
    /// Write into the buffer
    /// </summary>
    /// <param name="datas">Data to write</param>
    protected void WriteToSendBuffer(byte[] datas)
    {
      if (this.cmdBufferIndex + datas.Length > this.cmdBuffer.Length)
      {
        int requiredLength = this.cmdBufferIndex + datas.Length;
        const int LOHTHRESHOLD = 85000 - 1;
        const int BREATHINGSPACETOREDUCEREALLOCATIONS = 32 * 1024;
        int newSize = LOHTHRESHOLD;
        if (requiredLength <= LOHTHRESHOLD)
        {
          if (requiredLength + BREATHINGSPACETOREDUCEREALLOCATIONS <= LOHTHRESHOLD)
          {
            newSize = requiredLength + BREATHINGSPACETOREDUCEREALLOCATIONS;
          }
        }
        else
        {
          newSize = requiredLength + BREATHINGSPACETOREDUCEREALLOCATIONS;
        }

        byte[] newLargerBuffer = new byte[newSize];
        Buffer.BlockCopy(this.cmdBuffer, 0, newLargerBuffer, 0, this.cmdBuffer.Length);
        this.cmdBuffer = newLargerBuffer;
      }

      Buffer.BlockCopy(datas, 0, this.cmdBuffer, this.cmdBufferIndex, datas.Length);
      this.cmdBufferIndex += datas.Length;
    }

    /// <summary>
    /// Write data on the socket
    /// </summary>
    protected void FlushSendBuffer()
    {
      this.socket.Send(this.cmdBuffer, this.cmdBufferIndex, SocketFlags.None);
      this.cmdBufferIndex = 0;
    }
    #endregion

    #region Unified Protocol Read
    /// <summary>
    /// Get datas from bStream and get Reponse
    /// </summary>
    /// <returns>the reponse</returns>
    protected RedisReponse ReadAnswer()
    {
      return UnifiedProtocolReader.Read(this);
    }
    #endregion

    #region Spécific methods Call backs for different functions
    /// <summary>
    /// Asynch method to Monitor function
    /// </summary>
    /// <param name="result">IAsynch info</param>
    protected void ProcessMonitor(IAsyncResult result)
    {
      MonitorAsyncParam param = result.AsyncState as MonitorAsyncParam;
      int bytesRead = this.bufferStream != null ? this.bufferStream.EndRead(result) : 0;
      string text;
      if (bytesRead > 0)
      {
        text = param.AddBytes(bytesRead);
        EventMonitorArgs e = new EventMonitorArgs(text, false);
        if (!string.IsNullOrWhiteSpace(text))
        {
          param.CallBack(e);
        }

        if (!e.CancelLoop)
        { // Do it Again
          this.bufferStream.BeginRead(param.Buffer, 0, param.Buffer.Length, this.processMonitorCallBack, param);
        }
      }
    }

    /// <summary>
    /// Asynch method to Subscribe/publish mechanism
    /// </summary>
    /// <param name="result">IAsynch info</param>
    protected void ProcessPublish(IAsyncResult result)
    {
      PublishAsyncParam param = result.AsyncState as PublishAsyncParam;

      int bytesRead = 0;
      if (this.bufferStream != null)
      {
        bytesRead = this.bufferStream.EndRead(result);
      }

      if (bytesRead > 0)
      {
        string[] inforamations = param.AddBytes(bytesRead);
        EventSubscribeArgs e = new EventSubscribeArgs(inforamations);
        if (inforamations != null)
        {
          param.CallBack(e);
        }

        if (!e.CanQuit)
        { // Do it Again
          this.bufferStream.BeginRead(param.Buffer, 0, param.Buffer.Length, this.processSubscribeCallBack, param);
        }
        else
        {
          this.processSubscribeCallBack = null;
        }
      }
      else
      {
        this.processSubscribeCallBack = null;
      }
    }
    #endregion

    #region Useful functions
    #region Conversions String To Dictionary
    /// <summary>
    /// Split source from model : property1[keyvalueSplitter]value1[propertySplitter]property2[keyvalueSplitter]value2... 
    /// To Dictionary([key], [value])
    /// </summary>
    /// <param name="source">string to split</param>
    /// <param name="propertySplitter">splitter char between properties</param>
    /// <param name="keyvalueSplitter">splitter char between key and value</param>
    /// <returns>the datas</returns>
    protected Dictionary<string, string> ExtractFromString(string source, char propertySplitter, char keyvalueSplitter)
    {
      Dictionary<string, string> result = new Dictionary<string, string>();
      if (!string.IsNullOrWhiteSpace(source) && source.IndexOf(propertySplitter) != -1)
      {
        string[] nfos = source.Split(propertySplitter);
        foreach (string nfo in nfos)
        {
          if (!string.IsNullOrWhiteSpace(nfo) && nfo.IndexOf(keyvalueSplitter) != -1)
          {
            string[] keyvalue = nfo.Split(keyvalueSplitter);
            result.Add(keyvalue[0], keyvalue[1]);
          }
        }
      }

      return result;
    }
    #endregion

    /// <summary>
    /// Unified Raise a error
    /// </summary>
    /// <param name="answer">The protocol answer</param>
    /// <param name="title">The title to add to the message</param>
    protected void ThrowErrorIfneeded(RedisReponse answer, string title)
    {
      if (answer != null)
      {
        if (!answer.Success)
        {
          string err = string.Empty;
          if (!string.IsNullOrWhiteSpace(title))
          {
            err += title;
          }

          if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(answer.ErrorMessage))
          {
            err += " : ";
          }

          if (!string.IsNullOrWhiteSpace(answer.ErrorMessage))
          {
            err += answer.ErrorMessage;
          }

          this.LastErrorText = err;
          throw new ArgumentException(err);
        }
      }
      else
      {
        this.LastErrorText = title;
        throw new ArgumentException(title);
      }
    }
    #endregion
  }
}
