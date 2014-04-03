using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// This class store all published informations
  /// </summary>
  public class EventSubscribeArgs
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="informations">arrys of the informations returne by subcribe or psubscribe commands</param>
    public EventSubscribeArgs(string[] informations)
    {
      this.AnalyseMessage(informations);
    }

    /// <summary>
    /// Contains "message" or "pmessage" 
    /// </summary>
    public string MessageType { get; private set; }

    /// <summary>
    /// Pattern use if Psubscribe is used
    /// </summary>
    public string Pattern { get; private set; }

    /// <summary>
    /// Chanel use to publish
    /// </summary>
    public string Chanel { get; private set; }

    /// <summary>
    /// Publised message
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Explain if the message is the answer of the subscribe command 
    /// or a real message receive after a publish on the Redis server
    /// </summary>
    public int SubScribeIndex { get; private set; }

    /// <summary>
    /// Say if the subscribe mode can be quitted
    /// </summary>
    public bool CanQuit
    {
      get
      {
        return (this.MessageType.ToLower() == "unsubscribe" || this.MessageType.ToLower() == "punsubscribe") && this.Message == "0";
      }
    }

    /// <summary>
    /// Get the string that reprents this object
    /// </summary>
    /// <returns>the string</returns>
    public override string ToString()
    {
      if (string.IsNullOrWhiteSpace(this.MessageType))
      {
        return this.Message;
      }
      else if (this.SubScribeIndex > 0)
      {
        return string.Format("{0}ed to chanel : {1} at index {2}", this.MessageType, this.Chanel, this.Message);
      }
      else
      {
        return string.Format("type : {0}, chanel : {1}, pattern : {2}, message : {3}", this.MessageType, this.Chanel, this.Pattern, this.Message);
      }
    }

    /// <summary>
    /// Analyse the array to fill this object
    /// </summary>
    /// <param name="informations">arrys of the informations returne by subcribe or psubscribe commands</param>
    private void AnalyseMessage(string[] informations)
    {
      if (informations == null || informations.Length < 3 || informations.Length > 4)
      { // Error : clear all informations 
        this.MessageType = string.Empty;
        this.Pattern = string.Empty;
        this.Chanel = string.Empty;
        this.Message = informations == null ? string.Empty : informations.Aggregate((x, y) => x + ", " + y);
        this.SubScribeIndex = 0;
      }
      else
      {
        this.MessageType = informations[0];
        this.Message = informations[informations.Length - 1];
        this.Chanel = informations[informations.Length - 2];
        this.SubScribeIndex = 0;
        if (informations.Length == 4)
        {
          this.Pattern = informations[1];
        }
        else
        {
          this.Pattern = string.Empty;
          if (this.MessageType.ToLower() == "psubscribe" || this.MessageType == "subscribe")
          {
            double n = RedisConnector.GetDoubleFromString(this.Message, double.NaN);
            this.SubScribeIndex = n.Equals(double.NaN) ? 0 : Convert.ToInt32(n);
          }
        }
      }
    }
  }
}
