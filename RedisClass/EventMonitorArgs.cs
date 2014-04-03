using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// This class store and type a row send by the MONITOR (or SYNC !) REDIS server command
  /// </summary>
  public class EventMonitorArgs
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="message">Message receive to parse</param>
    /// <param name="cancel">Return param to say when monitoring stop</param>
    public EventMonitorArgs(string message, bool cancel)
    {
      this.CancelLoop = cancel;
      this.AnalyseMessage(message);
    }

    /// <summary>
    /// Internal constructor for insert Trace management command
    /// </summary>
    /// <param name="command">The text of the command trace</param>
    private EventMonitorArgs(string command)
    {
      this.Command = command;
      this.Date = DateTime.Now;
      this.IPAdress = string.Empty;
      this.BaseId = -1;
      this.Port = string.Empty;
    }

    /// <summary>
    /// Property set to true by the final client to say stop monitoring
    /// </summary>
    public bool CancelLoop { get; set; }

    /// <summary>
    /// Date of the monitor event (DateTime.MinValue if not present)
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// the database Id (if present else -1)
    /// </summary>
    public int BaseId { get; private set; }

    /// <summary>
    /// All command and parameters
    /// </summary>
    public string Command { get; private set; }

    /// <summary>
    /// IP adresse + port of the client if present (else string.Empty)
    /// </summary>
    public string IPAdress { get; private set; }

    /// <summary>
    /// The port used by the client if present (else string.Empty)
    /// </summary>
    public string Port { get; private set; }
    
    /// <summary>
    /// Get an Event Montior : For Tace purpose
    /// </summary>
    /// <param name="command">The trace action like "Trace start", "Trace Pause" or "Trace stop"</param>
    /// <returns>an Event Montior</returns>
    public static EventMonitorArgs Trace(string command)
    {
      return new EventMonitorArgs(command);
    }

    /// <summary>
    /// String to display informations
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
      if (this.Date != DateTime.MinValue)
      {
        if (string.IsNullOrWhiteSpace(this.IPAdress))
        {
          return string.Format("{0:G} : {1}", this.Date, this.Command);
        }
        else
        {
          return string.Format("{0:G} - {1} - {2}", this.Date, this.IPAdress, this.Command);
        }
      }
      else
      {
        return this.Command;
      }
    }

    /// <summary>
    /// From the string receive fill all properties
    /// </summary>
    /// <param name="message">the string receive by the Redis Server</param>
    private void AnalyseMessage(string message)
    {
      this.Date = DateTime.MinValue;
      this.IPAdress = string.Empty;
      this.Port = string.Empty;
      this.BaseId = -1;
      this.Command = string.Empty;

      if (string.IsNullOrWhiteSpace(message))
      { // No datas !
        return;
      }
      else if (!message.StartsWith("+"))
      { // Strange datas ??
        this.Command = message;
        return;
      }
      else if (message.IndexOf(' ') == -1)
      { // One information like the "+OK" at the begining
        this.Command = message.Substring(1);  // without the + at the begining
        return;
      }
      else
      { // something like : timestamp [Id_base Ip_adresse] "Commands" "argument"
        string[] nfo = message.Split(' ');
        double ts = RedisConnector.GetDoubleFromString(nfo[0], double.NaN);
        if (ts != double.NaN)
        { // Date is filled
          this.Date = RedisConnector.GetDateTimeFromUnixTime(ts);
          message = message.Replace(nfo[0], string.Empty);
        }

        if (nfo[1].StartsWith("["))
        { // Base Id
          int id = -1;
          if (int.TryParse(nfo[1].Replace("[", string.Empty).Replace("]", string.Empty), out id))
          {
            this.BaseId = id;
          }
          else
          {
            this.IPAdress = nfo[1].Replace("[", string.Empty).Replace("]", string.Empty);
          }

          message = message.Replace(nfo[1], string.Empty);
        }

        if (nfo.Length > 3 && nfo[2].EndsWith("]"))
        { 
          this.IPAdress = nfo[2].Replace("[", string.Empty).Replace("]", string.Empty);
          message = message.Replace(nfo[2], string.Empty);
        }

        if (this.IPAdress.IndexOf(':') != -1)
        {
          string[] nfos = this.IPAdress.Split(':');
          this.IPAdress = nfos[0];
          this.Port = nfos[1];
        }
        else
        {
          this.Port = string.Empty;
        }

        this.Command = message.Trim();
      }
    }
  }
}
