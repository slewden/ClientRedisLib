using System.Text;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Parameters for asynchrone call of then MONITOR REDIS server method
  /// </summary>
  public class PublishAsyncParam : IReadUnifiedProtocol
  {
    /// <summary>
    /// store the receive line
    /// </summary>
    private string resultCommand;

    /// <summary>
    /// Current position in resultCommand;
    /// </summary>
    private int indexer;

    /// <summary>
    /// Redis Connector buffer
    /// </summary>
    private System.IO.BufferedStream bufferedStream;
  
    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="bufferedStream">RedisConnector stream</param>
    /// <param name="callBack">Call back method for notify the principal thread</param>
    public PublishAsyncParam(System.IO.BufferedStream bufferedStream, EventSubscribeHandeler callBack)
    {
      this.Buffer = new byte[1];
      this.bufferedStream = bufferedStream;
      this.CallBack = callBack;
      this.resultCommand = string.Empty;
      this.indexer = 0;
    }

    /// <summary>
    /// return the la error read message
    /// </summary>
    public string LastErrorText { get; private set; }
    
    /// <summary>
    /// Read buffer
    /// </summary>
    public byte[] Buffer { get; private set; }

    /// <summary>
    /// Call back method to call
    /// </summary>
    public EventSubscribeHandeler CallBack { get; private set; }

    /// <summary>
    /// Add info to the receive line
    /// </summary>
    /// <param name="bytesRead">number of ready byte to read</param>
    /// <returns>the string read if it end with \n</returns>
    public string[] AddBytes(int bytesRead)
    {
      string text = Encoding.UTF8.GetString(this.Buffer, 0, bytesRead);
      this.resultCommand += text;
      this.indexer = 0;
      RedisReponse rep = UnifiedProtocolReader.Read(this);
      if (rep.ErrorCode == EErrorCode.None)
      {
        this.resultCommand = this.resultCommand.Substring(this.indexer, this.resultCommand.Length - this.indexer);
        this.indexer = 0;
        return rep.Datas.ToArray();
      }
      else
      {
        return null;
      }
    }

    #region IReadUnifiedProtocol Membres
    /// <summary>
    /// Read a bit from this.Buffer
    /// </summary>
    /// <returns>the byte</returns>
    public int ReadByte()
    {
      if (!string.IsNullOrWhiteSpace(this.resultCommand))
      {
        if (this.indexer < this.resultCommand.Length)
        {
          int n = this.resultCommand[this.indexer++];
          while ((n == '\r' || n == '\n') && this.indexer < this.resultCommand.Length)
          {
            n = this.resultCommand[this.indexer++];
          }

          return n;
        }
        else
        {
          this.LastErrorText = "Index out of bounds";
          return -2;
        }
      }
      else
      {
        return -1;
      }
    }

    /// <summary>
    /// Read a line from this.buffer a line end with \n
    /// \r are filtered
    /// </summary>
    /// <returns>The read string</returns>
    public string ReadLine()
    {
      if (!string.IsNullOrWhiteSpace(this.resultCommand))
      {
        if (this.indexer < this.resultCommand.Length)
        {
          int pos = this.resultCommand.IndexOf("\n", this.indexer);
          if (pos != -1)
          {
            string res = this.resultCommand.Substring(this.indexer, pos - this.indexer);
            this.indexer = pos + 1;
            return res.Replace("\r", string.Empty);
          }
          else
          {
            return string.Empty;
          }
        }
        else
        {
          this.LastErrorText = "Index out of bounds";
          return string.Empty;
        }
      }
      else
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// Read lenght car and place it in retbuf return the number of char in retbuf
    /// </summary>
    /// <param name="retbuf">Read buffer to fill</param>
    /// <param name="lenght">Number of char to fill</param>
    /// <returns>Number of char filled</returns>
    public int ReadAny(ref byte[] retbuf, int lenght)
    {
      if (!string.IsNullOrWhiteSpace(this.resultCommand))
      {
        if (this.indexer + lenght < this.resultCommand.Length)
        {
          string res = this.resultCommand.Substring(this.indexer, lenght);
          int i = 0;
          byte[] bytes = RedisConnector.GetBytesUtf8FromString(res.Replace("\r", string.Empty));
          for (; i < retbuf.Length && i < bytes.Length; i++)
          {
            retbuf[i] = bytes[i];
          }

          this.indexer += lenght + 1;
          return i;
        }
        else
        {
          this.LastErrorText = "Index out of bounds";
          return -1;
        }
      }
      else
      {
        return -1;
      }
    }

    /// <summary>
    /// Create a custom error message
    /// </summary>
    /// <param name="errorMsg">input error messagase</param>
    /// <returns>output error messagase</returns>
    public string MakeErrorText(string errorMsg)
    {
      return errorMsg;
    }
    #endregion
  }
}
