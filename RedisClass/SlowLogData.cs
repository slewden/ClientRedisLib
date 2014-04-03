using System;

namespace ClientRedisLib
{
  /// <summary>
  /// Class to manipulate SLOWLOG Data informations 
  /// SlowLog are the most slow command run on the REDIS server
  /// this class expose all properties
  /// </summary>
  public class SlowLogData
  {
    /// <summary>
    /// Fill all properties from de receive datas for one SlowLog
    /// </summary>
    /// <param name="line">Line from RedisConnector</param>
    internal SlowLogData(string line)
    {
      string[] nfos = line.Split(new string[] { "<0>" }, StringSplitOptions.None);
      if (nfos.Length == 4)
      { // good size
        int n0;
        double n1;
        if (int.TryParse(nfos[0], out n0) && double.TryParse(nfos[1], out n1))
        {
          this.Index = n0;
          this.Date = RedisConnector.GetDateTimeFromUnixTime(n1);
        }

        this.Duration = RedisConnector.GetTimeSpanFromMicroSecString(nfos[2]);
        this.CommandLine = nfos[3].Replace("<1>", " ");
      }
      else
      { // wrong number of arguments: only the command line is filled
        this.Index = -1;
        this.Date = DateTime.MinValue;
        this.Duration = new TimeSpan();
        this.CommandLine = line.Replace("<0>", " ").Replace("<1>", " ");
      }
    }

    /// <summary>
    /// The index of the SlowLog
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The event date
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// The duration
    /// </summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>
    /// The command line
    /// </summary>
    public string CommandLine { get; private set; }

    /// <summary>
    /// Return the display string
    /// </summary>
    /// <returns>the string</returns>
    public override string ToString()
    {
      return string.Format("{0,2} : at {1:G} : {2} : {3}", this.Index, this.Date, this.Duration, this.CommandLine);
    }
  }
}
