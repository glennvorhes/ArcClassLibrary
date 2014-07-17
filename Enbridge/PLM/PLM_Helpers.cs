using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enbridge.PLM
{
    /// <summary>
    /// Class methods to work with PLM report object and subobjects
    /// </summary>
    public static class PLM_Helpers
    {

        /// <summary>
        /// returns a nullable bool based on the passed in object
        /// no selection will return null
        /// yes or no defines the boolean as expected
        /// </summary>
        /// <param name="trueFalseObject"></param>
        /// <returns></returns>
        public static bool? trueFalseValue(Object trueFalseObject)
        {
            bool? returnValue;
            if (trueFalseObject == null)
            {
                returnValue = null;
            }
            else
            {
                switch (trueFalseObject.ToString())
                {
                    case "no":
                        returnValue = false;
                        break;
                    case "yes":
                        returnValue = true;
                        break;
                    default:
                        returnValue = null;
                        break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// get the value of the passed in object defining the selection
        /// if no selection has been made in the workflow, the object will be null
        /// return the selected value or 'null' if none has been selected
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string getComboBoxSelectedValue(Object obj)
        {
            string returnValue;
            if (obj == null)
            {
                returnValue = null;
            }
            else
            {
                returnValue = obj.ToString();
            }
            return returnValue;

        }

        public static double? convertStringToNullableDouble(string input)
        {
            double parseDoubleOutput;
            if (double.TryParse(input, out parseDoubleOutput))
            {
                return parseDoubleOutput;
            }
            else
            {
                return null;
            }
        }

        public static Object nullOneOrZeroFromNullableBool(bool? trueFalseNull)
        {
            switch (trueFalseNull)
            {
                case null:
                    return DBNull.Value;
                case true:
                    return 1;
                case false:
                    return 0;
                default:
                    throw new Exception("invalid value entered");
            }
        }


        public static Object nullOrNumberFromNullableDouble(double? input)
        {
            if (input == null)
            {
                return DBNull.Value;
            }
            else
            {
                return (double)input;
            }

        }


        public static Object nullOrStringFromString(string input, int characterLimit = 50)
        {
            if (input == null || input.Trim() == "")
            {
                return DBNull.Value;
            }
            else
            {
                if (input.Length > characterLimit)
                {
                    input = input.Substring(0, characterLimit);
                }
                return input;
            }

        }

        public static Object nullOrDateFromDate(DateTime dateTime)
        {
            if (DateTime.Compare(dateTime, DateTime.MinValue) == 0)
            {
                return DBNull.Value;
            }
            else
            {
                return dateTime;
            }

        }

        public static bool? processOneZeroToNullBool(Object databaseResult)
        {
            if (databaseResult == DBNull.Value)
            {
                return null;
            }
            else
            {
                int value = int.Parse(databaseResult.ToString());
                switch (value)
                {
                    case 1:
                        return true;
                    case 0:
                        return false;
                    default:
                        return null;
                }
            }
        }

        public static string processResultToString(Object readerRecord)
        {
            if (readerRecord == DBNull.Value)
            {
                return null;
            }
            else
            {
                return readerRecord.ToString();
            }
        }

        public static double? processResultToNullableDouble(Object readerRecord)
        {
            if (readerRecord == DBNull.Value)
            {
                return null;
            }
            else
            {
                double output;
                if (Double.TryParse(readerRecord.ToString(), out output))
                {
                    return output;
                }
                else
                {
                    return null;
                }
            }
        }

        public static double resultToDouble(Object readerRecord, double returnVal = 0)
        {
            if (readerRecord == DBNull.Value)
            {
                return returnVal;
            }
            else
            {
                double output;
                if (Double.TryParse(readerRecord.ToString(), out output))
                {
                    return output;
                }
                else
                {
                    return returnVal;
                }
            }
        }

        public static DateTime processResultToDate(Object readerRecord)
        {
            if (readerRecord == DBNull.Value)
            {
                return DateTime.MinValue;
            }
            else
            {
                DateTime output;
                if (DateTime.TryParse(readerRecord.ToString(), out output))
                {
                    return output;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }


    }
}
