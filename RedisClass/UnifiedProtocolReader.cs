using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Class to analyse the unified protocol and fill a Redisreponse
  /// </summary>
  public class UnifiedProtocolReader
  {
    /// <summary>
    /// The read method
    /// </summary>
    /// <param name="from">object that can read on a flux</param>
    /// <returns>the RedisReponse</returns>
    public static RedisReponse Read(IReadUnifiedProtocol from)
    {
      int c = from.ReadByte();
      if (c != -1)
      {
        if (c == '-')
        { // Error replies
          return UnifiedProtocolReader.ReadAnErrorReply(from);
        }
        else if (c == '$')
        { // Bulk replies
          return UnifiedProtocolReader.ReadABulkReply(from);
        }
        else if (c == '*')
        { // Multi-bulk replies
          return UnifiedProtocolReader.ReadAMultiBulkReply(from, 0);
        }
        else if (c == '+')
        { // Status reply 
          return UnifiedProtocolReader.ReadALineReply(from);
        }
        else if (c == ':')
        { // number reply
          return UnifiedProtocolReader.ReadANumberReply(from);
        }
        else if (c == -2)
        { // comunication error
          return RedisReponse.GetError(from.LastErrorText, EErrorCode.CommunicationError);
        }
        else
        { // unknow
          string msg = from.MakeErrorText("Unknow answer");
          return RedisReponse.GetError(msg, EErrorCode.UnknowError);
        }
      }
      else
      { // nothing received ??
        string msg = from.MakeErrorText("No answer received");
        return RedisReponse.GetError(msg, EErrorCode.NoAnswerReceived);
      }
    }

    /// <summary>
    /// Return a error reponse
    /// </summary>
    /// <param name="from">object that can read on a flux</param>
    /// <returns>the reponse</returns>
    protected static RedisReponse ReadAnErrorReply(IReadUnifiedProtocol from)
    {
      string infos = from.ReadLine();
      string msg = from.MakeErrorText(infos.StartsWith("ERR") ? infos.Substring(4) : infos);
      return RedisReponse.GetError(msg, EErrorCode.ServerError);
    }

    /// <summary>
    /// Read this reponse format   $[lenght]\r\n[lenght Datas]\r\n 
    /// </summary>
    /// <param name="from">object that can read on a flux</param>
    /// <returns>the reponse</returns>
    protected static RedisReponse ReadABulkReply(IReadUnifiedProtocol from)
    {
      string infos = from.ReadLine();
      int lenght;
      if (int.TryParse(infos, out lenght))
      {
        if (lenght <= 0)
        { // No size no answer
          return RedisReponse.GetEmpty();
        }
        else
        {
          var retbuf = new byte[lenght];
          int count = from.ReadAny(ref retbuf, lenght);
          if (count == -1)
          {
            return RedisReponse.GetError(from.MakeErrorText("Unexpected end of Stream"), EErrorCode.NoAnswerReceived);
          }
          else if (count == -2)
          {
            return RedisReponse.GetError(from.MakeErrorText("Invalid bulk reply termination"), EErrorCode.CommunicationError);
          }
          else
          {
            return RedisReponse.GetLine(RedisConnector.GetStringFromUtf8Bytes(retbuf));
          }
        }
      }
      else
      {
        string msg = from.MakeErrorText("Invalid bulk format : " + infos);
        return RedisReponse.GetError(msg, EErrorCode.CommunicationError);
      }
    }

    /// <summary>
    /// Read a multi-bulk reply *[nb param]\r\n[Bulk reply] * nb times
    /// </summary>
    /// <param name="from">object that can read on a flux</param>
    /// <param name="level">Recursivity level</param>
    /// <returns>the reponse</returns>
    protected static RedisReponse ReadAMultiBulkReply(IReadUnifiedProtocol from, int level)
    {
      string infos = from.ReadLine();
      int number;
      if (int.TryParse(infos, out number))
      {
        if (number < 0)
        { // No size no answer ==> nil
          return RedisReponse.GetEmpty();
        }
        else if (number == 0)
        { // 0 size = empty list
          return RedisReponse.GetEmptyList();
        }
        else
        {
          List<string> result = new List<string>();
          int c;
          RedisReponse answer;
          for (int i = 0; i < number; i++)
          {
            c = from.ReadByte();
            if (c == '$')
            {
              answer = UnifiedProtocolReader.ReadABulkReply(from);
              if (answer.Success && answer.Line != null)
              { // answer.Line may be string.Empty and this is valid
                result.Add(answer.Line);
              }
              else
              {
                return RedisReponse.GetError(from.MakeErrorText(string.Format("Invalid BulkReply at answer line {0} : {1}", i, answer.ErrorMessage)), EErrorCode.CommunicationError);
              }
            }
            else if (c == '*')
            {
              answer = UnifiedProtocolReader.ReadAMultiBulkReply(from, level + 1);
              if (answer.Success && answer.Datas != null)
              {
                result.Add(answer.MergeData(string.Format("<{0}>", level)));
              }
              else
              {
                return RedisReponse.GetError(from.MakeErrorText(string.Format("Invalid MultiBulkReply at answer line {0} : {1}", i, answer.ErrorMessage)), EErrorCode.CommunicationError);
              }
            }
            else if (c == '+')
            {
              answer = UnifiedProtocolReader.ReadALineReply(from);
              if (answer.Success && answer.Line != null)
              {
                result.Add(answer.Line);
              }
              else
              {
                return RedisReponse.GetError(from.MakeErrorText(string.Format("Invalid LineReply at answer line {0} : {1}", i, answer.ErrorMessage)), EErrorCode.CommunicationError);
              }
            }
            else if (c == ':')
            {
              answer = UnifiedProtocolReader.ReadANumberReply(from);
              if (answer.Success && answer.Number != double.NaN)
              {
                result.Add(answer.Number.ToString());
              }
              else
              {
                return RedisReponse.GetError(from.MakeErrorText(string.Format("Invalid NumberReply at answer line {0} : {1}", i, answer.ErrorMessage)), EErrorCode.CommunicationError);
              }
            }
            else
            {
              return RedisReponse.GetError(from.MakeErrorText(string.Format("Invalid multi-bulk format line {0}", i)), EErrorCode.CommunicationError);
            }
          }

          return RedisReponse.GetDatas(result);
        }
      }
      else
      {
        return RedisReponse.GetError(from.MakeErrorText("Invalid multi-bulk format : " + infos), EErrorCode.CommunicationError);
      }
    }
    
    /// <summary>
    /// Read a reponse line
    /// </summary>
    /// <param name="from">object that can read on a flux</param>
    /// <returns>the reponse</returns>
    protected static RedisReponse ReadALineReply(IReadUnifiedProtocol from)
    {
      string infos = from.ReadLine();
      return RedisReponse.GetLine(infos);
    }
    
    /// <summary>
    /// Read a reponse number
    /// </summary>
    /// <param name="from">object that can read on a flux</param>
    /// <returns>An answer with the number</returns>
    protected static RedisReponse ReadANumberReply(IReadUnifiedProtocol from)
    {
      string info = from.ReadLine();
      if (string.IsNullOrWhiteSpace(info))
      {
        return RedisReponse.GetError(from.MakeErrorText("Invalid number"), EErrorCode.CommunicationError);
      }
      else
      {
        return RedisReponse.GetNumber(info);
      }
    }
  }
}
