using System;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Store the time info return by the command TIME on a REDIS server
  /// The TIME command returns the current server time as a two items lists : 
  /// a Unix timestamp and the amount of microseconds already elapsed in the 
  /// current second. 
  /// Basically the interface is very similar to the one of the gettimeofday system call.
  /// </summary>
  public class PrecisionTime
  {
    /// <summary>
    /// The constructor : parse original datas to cast values
    /// </summary>
    /// <param name="date">The date</param>
    /// <param name="microSecond">The duration in micro seconds</param>
    public PrecisionTime(string date, string microSecond)
    {
      double dateDouble = RedisConnector.GetDoubleFromString(date, double.NaN);
      if (dateDouble != double.NaN)
      {
        this.Date = RedisConnector.GetDateTimeFromUnixTime(dateDouble);
      }
      else
      {
        this.Date = DateTime.MinValue;
      }

      this.MicroSeconds = RedisConnector.GetLongFromString(microSecond, -1);
    }

    /// <summary>
    /// Get an empty PrecisionTime : use to return a error
    /// </summary>
    public static PrecisionTime Empty
    {
      get
      {
        return new PrecisionTime(string.Empty, string.Empty);
      }
    }

    /// <summary>
    /// Get an PrecisionTime with the current DateTime : used on Server before the 2.6 version who don't have the TIME command
    /// </summary>
    public static PrecisionTime Now
    {
      get
      {
        return new PrecisionTime(DateTime.Now.ToString("G"), "0");
      }
    }

    /// <summary>
    /// The date
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// The duration in micro seconds
    /// </summary>
    public long MicroSeconds { get; private set; }

    /// <summary>
    /// Indicate if the object is filled or not
    /// </summary>
    public bool IsEmpty
    { 
      get
      {
        return this.Date == DateTime.MinValue || this.MicroSeconds < 0;
      }
    }

    /// <summary>
    /// Return the string of this object
    /// </summary>
    /// <returns>the string</returns>
    public override string ToString()
    {
      if (this.IsEmpty)
      { // empty 
        return string.Empty;
      }
      else
      { // not empty
        return string.Format("{0:G}.{1}µs", this.Date, this.MicroSeconds);
      }
    }
  }
}