namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Interface to make analyse of unified protocol work
  /// </summary>
  public interface IReadUnifiedProtocol
  {
    /// <summary>
    /// Return the readxx last error
    /// </summary>
    string LastErrorText { get; }
    
    /// <summary>
    /// Read a bit
    /// </summary>
    /// <returns>the byte</returns>
    int ReadByte();

    /// <summary>
    /// Read a line. A line end with \n
    /// \r are filtered by this method
    /// </summary>
    /// <returns>The read string</returns>
    string ReadLine();

    /// <summary>
    /// Read lenght car and place it in retbuf return the number of char in retbuf
    /// </summary>
    /// <param name="retbuf">Read buffer to fill</param>
    /// <param name="lenght">Number of char to fill</param>
    /// <returns>Number of char filled</returns>
    int ReadAny(ref byte[] retbuf, int lenght);

    /// <summary>
    /// Construct an error message
    /// </summary>
    /// <param name="errorMsg">The message to format</param>
    /// <returns>Formatted Text</returns>
    string MakeErrorText(string errorMsg);
  }
}
