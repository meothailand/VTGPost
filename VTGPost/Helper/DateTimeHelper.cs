using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VTGPost.Helper
{
    public class DateTimeHelper
    {
        public enum dateFormat
        {
            ddMMyyyy,
            MMddyyyy,
            yyyyMMdd,
            ddMMMyyyyHHmm
        };

        private static Dictionary<string, int> _months = new Dictionary<string, int>
                                            {
                                                {"january", 1},
                                                {"february", 2},
                                                {"march", 3},
                                                {"april", 4},
                                                {"may", 5},
                                                {"june", 6},
                                                {"july", 7},
                                                {"august", 8},
                                                {"september", 9},
                                                {"october", 10},
                                                {"november", 11},
                                                {"december", 12}
                                            }; 

        public static DateTime ConvertDatetimeStringToDateTime(string dtString, dateFormat format, char separator)
        {
            var values = dtString.Split(separator);
            
            switch (format)
            {
                case dateFormat.ddMMyyyy:
                    return new DateTime(int.Parse(values[2]), int.Parse(values[1]), int.Parse(values[0]));

                case dateFormat.MMddyyyy:
                    return new DateTime(int.Parse(values[2]), int.Parse(values[0]), int.Parse(values[1]));

                case dateFormat.yyyyMMdd:
                    return new DateTime(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
                
                case dateFormat.ddMMMyyyyHHmm:
                    var date = dtString.Substring(0, dtString.IndexOf('-'));
                    var time = dtString.Substring(dtString.IndexOf('-') + 1, dtString.Length - dtString.IndexOf('-') - 1);
                    var dateValues = date.Split(' ');
                    var timeValues = time.Split(':');

                    return new DateTime(int.Parse(dateValues[2]), _months[dateValues[1].ToLower()], int.Parse(dateValues[0]), int.Parse(timeValues[0]), int.Parse(timeValues[1]), 0);

                default:
                    throw new Exception("Date format is incorrect.");
            }
        }
    }
}