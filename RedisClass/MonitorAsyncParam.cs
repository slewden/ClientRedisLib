using System.Text;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Parameters for asynchrone call of then MONITOR REDIS server method
  /// </summary>
  public class MonitorAsyncParam
  {
    /// <summary>
    /// store the receive line
    /// </summary>
    private StringBuilder resultCommand;

    /// <summary>
    /// Redis Connector buffer
    /// </summary>
    private System.IO.BufferedStream bufferedStream;
  
    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="bufferedStream">RedisConnector stream</param>
    /// <param name="callBack">Call back method for notify the principal thread</param>
    public MonitorAsyncParam(System.IO.BufferedStream bufferedStream, EventMonitorHandler callBack)
    {
      this.Buffer = new byte[1];
      this.bufferedStream = bufferedStream;
      this.CallBack = callBack;
      this.resultCommand = new StringBuilder();
    }

    /// <summary>
    /// Read buffer
    /// </summary>
    public byte[] Buffer { get; private set; }

    /// <summary>
    /// Call back method to call
    /// </summary>
    public EventMonitorHandler CallBack { get; private set; }

    /// <summary>
    /// Add info to the receive line
    /// </summary>
    /// <param name="bytesRead">number of ready byte to read</param>
    /// <returns>the string read if it end with \n</returns>
    public string AddBytes(int bytesRead)
    {
      string text = Encoding.UTF8.GetString(this.Buffer, 0, bytesRead);
      this.resultCommand.Append(text);

      string totalString = this.resultCommand.ToString();
      int pos = totalString.IndexOf('\n');
      if (pos != -1)
      {
        string result = totalString.Substring(0, pos);
        string next = totalString.Length > pos ? totalString.Substring(pos + 1, totalString.Length - pos - 1) : string.Empty;
        this.resultCommand = new StringBuilder(next);
        return result.Replace("\r", string.Empty);
      }
      else
      { // no answer for the moment
        return string.Empty;
      }
    }
  }
}
