using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ClientRedisLib.RedisClass
{
  /// <summary>
  /// Base StateBag to store every REDIS answer
  /// </summary>
  public class RedisReponse
  {
    /// <summary>
    /// Internal constructor
    /// </summary>
    /// <param name="status">True if no error, false else</param>
    private RedisReponse(bool status)
    {
      this.Success = status;
      this.Number = double.NaN;
      this.ErrorCode = 0;
    }

    #region Properties
    /// <summary>
    /// the reponse is an error ==> Errormessage will be filled
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Describe the error filled only if Succes = false
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// The code of the error
    /// </summary>
    public EErrorCode ErrorCode { get; private set; }

    /// <summary>
    /// if the answer is a single line : here it is,  else the value is null
    /// </summary>
    public string Line { get; private set; }

    /// <summary>
    /// If the answer is a number : here it is, else the value is double.NaN
    /// </summary>
    public double Number { get; private set; }

    /// <summary>
    /// If the answer is multiple line info : here it is,  else the value is null 
    /// </summary>
    public List<string> Datas { get; private set; }

    /// <summary>
    /// indicate when we are in pipeline mode
    /// </summary>
    public bool Pipelininng { get; private set; }
    #endregion

    #region static Constuctors
    /// <summary>
    /// Get an empty reponse
    /// </summary>
    /// <returns>the reponse</returns>
    public static RedisReponse GetEmpty()
    {
      return RedisReponse.GetLine(string.Empty);
    }

    /// <summary>
    /// Get an empty list Reponse
    /// </summary>
    /// <returns>the reponse</returns>
    public static RedisReponse GetEmptyList()
    {
      return RedisReponse.GetDatas(new List<string>());
    }

    /// <summary>
    /// Get a list reponse
    /// </summary>
    /// <param name="datas">the list of datas</param>
    /// <returns>the reponse</returns>
    public static RedisReponse GetDatas(List<string> datas)
    {
      RedisReponse rep = new RedisReponse(true);
      rep.Datas = datas;
      return rep;
    }

    /// <summary>
    /// Get an error reponse
    /// </summary>
    /// <param name="value">the error message</param>
    /// <param name="code">the error code</param>
    /// <returns>the reponse</returns>
    public static RedisReponse GetError(string value, EErrorCode code)
    {
      RedisReponse rep = new RedisReponse(false);
      rep.ErrorMessage = value;
      rep.ErrorCode = EErrorCode.UnknowError;
      return rep;
    }

    /// <summary>
    /// Get an unique reponse
    /// </summary>
    /// <param name="value">the message</param>
    /// <returns>the reponse</returns>
    public static RedisReponse GetLine(string value)
    {
      RedisReponse rep = new RedisReponse(true);
      rep.Line = value;
      return rep;
    }

    /// <summary>
    /// Get a number reponse
    /// </summary>
    /// <param name="value">the number</param>
    /// <returns>the reponse</returns>
    public static RedisReponse GetNumber(string value)
    {
      double n = RedisConnector.GetDoubleFromString(value, double.NaN);
      if (n.Equals(double.NaN))
      {
        return RedisReponse.GetLine(value);
      }
      else
      {
        RedisReponse rep = new RedisReponse(true);
        rep.Number = n;
        return rep;
      }
    }

    /// <summary>
    /// Get a pipeline reponse
    /// </summary>
    /// <returns>the reponse</returns>
    public static RedisReponse GetPipelinning()
    {
      RedisReponse rep = new RedisReponse(true);
      rep.Pipelininng = true;
      return rep;
    }
    #endregion

    /// <summary>
    /// Make a string from Datas
    /// </summary>
    /// <param name="splitter">char to separate every data</param>
    /// <returns>the string</returns>
    public string MergeData(string splitter)
    {
      if (this.Datas == null || this.Datas.Count == 0)
      {
        return string.Empty;
      }
      else
      {
        return this.Datas.Aggregate((x, y) => x + splitter + y);
      }
    }

    /// <summary>
    /// string to draw the answer
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
      if (this.Success)
      {
        if (!string.IsNullOrWhiteSpace(this.Line))
        {
          return "Line : " + this.Line;
        }
        else if (!this.Number.Equals(double.NaN))
        {
          return "Number : " + this.Number.ToString();
        }
        else if (this.Datas != null)
        {
          return string.Format("Datas {0} : {1}", this.Datas.Count, this.Datas.Aggregate((x, y) => string.Format("'{0}', '{1}'", x, y)));
        }
        else
        {
          return "Null";
        }
      }
      else
      {
        return "Error : " + this.ErrorMessage;
      }
    }
  }
}
