using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using ClosedXML.Excel;
using RadbitMqDemo.Extension;
using ServiceStack.Text;

namespace RadbitMqDemo.Global
{
    public class FunctionBase
    {
        /// <summary>
        ///     Convert folder path to general format.
        ///     Use slash instead of backslash and be sure it must end with slash character.
        ///     | Backslash = "\\" | Slash = "/" | FolderCurrent = "./"
        /// </summary>
        public static string ConvertToPathFormat(string value)
        {
            List<string> list = new List<string> { LocalFunctionEnum.Backslash, LocalFunctionEnum.Slash };
            return (value + (list.Contains(string.Empty + value[value.Length - 1])
                        ? string.Empty
                        : LocalFunctionEnum.Slash))
                .Replace(LocalFunctionEnum.Backslash, LocalFunctionEnum.Slash)
                .Replace(LocalFunctionEnum.FolderCurrent, string.Empty);
        }

        /// <summary>
        ///     Convert string data into InsensitiveDictionary
        /// </summary>
        public static InsensitiveDictionary<string> Deserialize(string data, string contentType = LocalFunctionEnum.JSON)
        {
            switch (contentType)
            {
                case LocalFunctionEnum.JSON:
                    return Deserialize<InsensitiveDictionary<string>>(data);

                case LocalFunctionEnum.XML:
                    return ParseXmlToDictionary<InsensitiveDictionary<string>>(data);

                default:
                    return null;
            }
        }

        /// <summary>
        ///     Convert string data into object
        /// </summary>
        public static T Deserialize<T>(string data, string contentType = LocalFunctionEnum.JSON) where T : class
        {
            switch (contentType)
            {
                case LocalFunctionEnum.JSON:
                    return JsonSerializer.DeserializeFromString(data, typeof(T)) as T;

                case LocalFunctionEnum.XML:
                    return XmlSerializer.DeserializeFromString(data, typeof(T)) as T;

                default:
                    return null;
            }
        }

        /// <summary>
        ///     Extract characters starting at the left side of text
        /// </summary>
        public static string ExtractLeft(string value, int length)
        {
            return value.Substring(0, length);
        }

        /// <summary>
        ///     Extract characters between start and end position
        /// </summary>
        public static string ExtractMiddle(string value, int startIndex, int endIndex)
        {
            return value.Remove(endIndex).Substring(startIndex);
        }

        /// <summary>
        ///     Extract characters starting at the right side of text
        /// </summary>
        public static string ExtractRight(string value, int length)
        {
            return value.Substring(value.Length - length);
        }

        /// <summary>
        ///     Mask card number
        /// </summary>
        public static string MaskCardNumber(string cardNumber)
        {
            return cardNumber.Length < 16
                ? cardNumber
                : $"{ExtractLeft(cardNumber, 6)}{MaskData(cardNumber.Length - 10)}{ExtractRight(cardNumber, 4)}";
        }

        /// <summary>
        ///     Mask sensitive data with *
        /// </summary>
        public static string MaskData(int length)
        {
            return new string('*', length);
        }

        /// <summary>
        ///     Use to mask sensitive fields | find field "CardNumber"
        /// </summary>
        public static string MaskSensitiveField(string field, string value)
        {
            switch (field?.ToUpper())
            {
                case LocalFunctionEnum.CardNumber:
                    return MaskCardNumber(value);
                case LocalFunctionEnum.Password:
                    return MaskData(value.Length);
                case LocalFunctionEnum.PinNumber:
                    return MaskData(value.Length);
                default:
                    return MaskData(value.Length);
            }
        }

        /// <summary>
        ///     Convert xml string data into Dictionary
        /// </summary>
        public static T ParseXmlToDictionary<T>(string xml) where T : IDictionary<string, string>
        {
            XDocument document = XDocument.Parse(xml);
            T dictionary = Activator.CreateInstance<T>();
            if (document.Root == null)
            {
                return dictionary;
            }

            foreach (XElement element in document.Root.Elements())
            {
                string keyName = element.Name.LocalName;
                if (dictionary.ContainsKey(keyName) == false)
                {
                    dictionary.Add(keyName, element.Value);
                }
            }
            return dictionary;
        }

        /// <summary>
        ///     Read file from disk
        /// </summary>
        public static string ReadFile(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            using (StreamReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        ///     Convert object to string data
        /// </summary>
        public static string Serialize<T>(T data, string contentType = LocalFunctionEnum.JSON) where T : class
        {
            switch (contentType)
            {
                case LocalFunctionEnum.JSON:
                    return JsonSerializer.SerializeToString(data);

                case LocalFunctionEnum.XML:
                    return XmlSerializer.SerializeToString(data);

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Convert IDictionary(string, object) to XML
        /// </summary>
        public static string SerializeToXML(IDictionary<string, object> dictionary, string root = "Content")
        {
            StringBuilder xml = new StringBuilder();
            xml.Append($"<{root}>");
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                IDictionary<string, string> value = pair.Value as IDictionary<string, string>;
                if (value != null)
                {
                    xml.Append(SerializeDictionaryToXML(value, true, pair.Key));
                }
                else
                {
                    List<Dictionary<string, string>> list = pair.Value as List<Dictionary<string, string>>;
                    xml.Append(list == null
                        ? $"<{pair.Key}>{pair.Value}</{pair.Key}>"
                        : SerializeListToXML(list, true, pair.Key));
                }
            }
            xml.Append($"</{root}>");
            return xml.ToString();
        }

        /// <summary>
        /// Convert IDictionary(string, string) to XML
        /// </summary>
        public static string SerializeDictionaryToXML(IDictionary<string, string> dictionary, bool isIncludeRoot = true,
            string root = "Root")
        {
            XElement element =
                new XElement(root, dictionary.Select(iterator => new XElement(iterator.Key, iterator.Value)));
            return GetInnerXML(element, isIncludeRoot);
        }

        /// <summary>
        /// Convert List(Dictionary(string, string)) to XML
        /// </summary>
        public static string SerializeListToXML(List<Dictionary<string, string>> list, bool isIncludeRoot = true,
            string root = "Root", string node = "Item")
        {
            XElement element =
                new XElement(root, list.Select(
                    item => new XElement(node, item.Select(iterator => new XElement(iterator.Key, iterator.Value)))));
            return GetInnerXML(element, isIncludeRoot);
        }

        public static string SerializeObjectToXML(object content)
        {
            Type type = content.GetType();
            StringBuilder xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xml.AppendLine(
                $@"<{type.Name} 
                    xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""
                    xmlns=""http://schemas.datacontract.org/2004/07/{type.Namespace}"">");
            foreach (PropertyInfo property in type.GetProperties())
            {
                string key = property.Name;
                string value = property.GetValue(content) + string.Empty;
                xml.AppendLine($"<{key}>{value}</{key}>");
            }
            xml.AppendLine($"</{type.Name}>");
            return xml.ToString();
        }

        /// <summary>
        /// Get CDATA string
        /// </summary>
        public static string GetCDataSection(string xml)
        {
            return $"<![CDATA[{xml}]]>";
        }

        /// <summary>
        /// Get InnerXML of XElement
        /// </summary>
        public static string GetInnerXML(XNode element, bool isIncludeRoot)
        {
            if (isIncludeRoot)
            {
                return element.ToString();
            }

            using (XmlReader reader = element.CreateReader())
            {
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }
        }
        public static string GetConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? string.Empty;
        }
        #region Format
        /// <summary>
        /// Format Money | use pattern: "#,##0"
        /// </summary>
        public static string FormatDecimal(string value)
        {
            decimal result;
            return decimal.TryParse(value, out result)
                ? result.ToString("#,##0", CultureInfo.InvariantCulture)
                : value;
        }
        /// <summary>
        /// Format Date | Format Date "yyyyMMdd" Dispaly--> "dd/MM/yyyy"| Date Time: "yyyyMMddHHmmss" Display--> "dd/MM/yyyy HH:mm:ss"
        /// </summary>
        public static string FormatDate(string value)
        {
            DateTime date;
            bool isDateFormat = value.Length == "yyyyMMdd".Length;
            return DateTime.TryParseExact(value, isDateFormat ? "yyyyMMdd" : "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                ? date.ToString(isDateFormat ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm:ss")
                : value;
        }
        /// <summary>
        /// Format Card Number for view (add white space)
        /// </summary>
        public static string CardNumFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            if (input.Trim().Length < 10) return input;

            input = input.Trim();
            int numCut = (input.Length >= 16 ? 4 : 3);
            string prefix = input.Substring(0, 4);
            string lastToken = input.Substring(4, input.Length - 4);

            string outToken = prefix;

            while (lastToken != string.Empty)
            {

                if (lastToken.Length > 3)
                {
                    var proccessToken = lastToken.Substring(0, numCut);
                    outToken = $"{outToken} {proccessToken}";

                    lastToken = lastToken.Substring(numCut, lastToken.Length - numCut);
                }
                else
                {
                    outToken = $"{outToken} {lastToken}";

                    lastToken = string.Empty;

                }

            }
            return outToken;
        }
        #endregion

        public static void PostToUrl(string url, NameValueCollection data = null)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.Clear();

            StringBuilder s = new StringBuilder();
            s.Append("<html>");
            s.AppendFormat("<body onload='document.forms[\"form\"].submit()'>");
            s.AppendFormat("<form name='form' action='{0}' method='post'>", url);
            if (data != null)
            {
                foreach (string key in data)
                {
                    s.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", key, data[key]);
                }
            }
            else
            {
                s.Append("<input type='hidden' name='STBServicePost' value='SacombankEPAYWebsite' />");
            }
            s.Append("</form></body></html>");
            response.Write(s.ToString());
            //response.End();
            HttpContext.Current.Response.Flush(); // Sends all currently buffered output to the client.
            HttpContext.Current.Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
            HttpContext.Current.ApplicationInstance.CompleteRequest();

        }
        public static string GetAbsoluteUrl(string absoluteUrl)
        {
            return (HttpContext.Current.Request.ApplicationPath + absoluteUrl.Replace("~", string.Empty))
                .Replace(LocalFunctionEnum.DoubleSlash, LocalFunctionEnum.Slash);
        }
        #region Import
        /// <summary>
        /// separator ','
        /// </summary>
        public static List<T> ImportCSV<T>(string filePath, char separator = ',') where T : class
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                return ImportCSV<T>(stream, separator);
            }
        }
        /// <summary>
        /// separator ','
        /// </summary>
        public static List<T> ImportCSV<T>(Stream stream, char separator = ',') where T : class
        {
            Type type = typeof(T);
            List<T> listResult = new List<T>();
            List<string> listFields = new List<string>();
            int row = 0;

            using (StreamReader reader = new StreamReader(stream))
            {
                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string[] listData = line.Split(separator);

                    // Read header
                    if (row == 0)
                    {
                        row++;
                        listFields = listData.ToList();
                        continue;
                    }

                    // Read data
                    T instance = (T)Activator.CreateInstance(type);
                    for (int i = 0; i < listFields.Count; i++)
                    {
                        string field = listFields[i];
                        if (string.IsNullOrWhiteSpace(field))
                        {
                            continue;
                        }
                        type.GetField(field)?.SetValue(instance, listData[i]);
                    }
                    listResult.Add(instance);
                }
            }

            return listResult;
        }
        #endregion

        #region Export
        public static byte[] ExportToExcel(DataTable dtResult)
        {
            XLWorkbook workbook = new XLWorkbook();
            workbook.Worksheets.Add(dtResult, "Sheet");

            MemoryStream memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            memoryStream.Close();

            return bytes;
        }
        #endregion

        #region MASK INFO
        public static string MaskCardNo(string cardNo)
        {
            return cardNo.Length < 16
                ? cardNo
                : $"{Left(cardNo, 6)}-{Right(cardNo, 4)}";
        }

        public static string MaskEmail(string value)
        {
            return value.Length < 5
                ? value
                : value.Substring(0, 4) + "*";
        }

        public static string MaskMobile(string value)
        {
            return value.Length < 5
                ? value
                : new string('*', value.Length - 5) + value.Substring(value.Length - 5);
        }
        #endregion
        #region string Utilities
        public static string Left(string value, int length)
        {
            return value.Substring(0, length);
        }

        public static string Right(string value, int length)
        {
            return value.Substring(value.Length - length);
        }
        /// <summary>
        /// Replace String with Vietnamese to eng
        /// </summary>
        public static string RemoveDiacritics(string stIn)
        {
            stIn = stIn.Replace('Đ', 'D');  // Check type Unicode
            stIn = stIn.Replace('Ð', 'D');  // Check type VNI
            stIn = stIn.Replace("|", "");
            stIn = stIn.Replace("  ", " ");
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char t in stFormD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(t);
                if (uc != UnicodeCategory.NonSpacingMark) { sb.Append(t); }
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
        #endregion
        #region Validation Info
        /// <summary>
        /// Check string is Base64String
        /// </summary>
        public static bool IsBase64String(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }
        /// <summary>
        /// check string is cardnumvber
        /// </summary>
        public static bool IsCreditCardValid(string cardNumber)
        {
            const string allowed = "0123456789";
            int i;
            var cleanNumber = new StringBuilder();
            for (i = 0; i < cardNumber.Length; i++)
            {
                if (allowed.IndexOf(cardNumber.Substring(i, 1), StringComparison.Ordinal) >= 0)
                    cleanNumber.Append(cardNumber.Substring(i, 1));
            }
            if (cleanNumber.Length < 13 || cleanNumber.Length > 16) return false;
            for (i = cleanNumber.Length + 1; i <= 16; i++)
                cleanNumber.Insert(0, "0");
            int total = 0;
            string number = cleanNumber.ToString();
            for (i = 1; i <= 16; i++)
            {
                var multiplier = 1 + (i % 2);
                var digit = int.Parse(number.Substring(i - 1, 1));
                var sum = digit * multiplier;
                if (sum > 9)
                    sum -= 9;
                total += sum;
            }
            return (total % 10 == 0);

        }
        /// <summary>
        /// Verification Crdcode sacombank
        /// </summary>
        public static bool IsSacombankCardCode(string crdCde)
        {
            const string allowed = "0123456789";
            int i;
            var cleanNumber = new StringBuilder();
            for (i = 0; i < crdCde.Length; i++)
            {
                if (allowed.IndexOf(crdCde.Substring(i, 1), StringComparison.Ordinal) >= 0)
                    cleanNumber.Append(crdCde.Substring(i, 1));
            }
            if (cleanNumber.Length != 10) return false;
            for (i = cleanNumber.Length + 1; i <= 10; i++)
                cleanNumber.Insert(0, "0");
            int total = 0;
            string number = cleanNumber.ToString();
            for (i = 1; i <= 10; i++)
            {
                var multiplier = 1 + (i % 2);
                var digit = int.Parse(number.Substring(i - 1, 1));
                var sum = digit * multiplier;
                if (sum > 9)
                    sum -= 9;
                total += sum;
            }
            return (total % 10 == 0);
        }
        public static bool IsEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase);
        }
        public static bool IsEmail(string email, bool required)
        {
            if (required)
            {
                return email.Length != 0 && IsEmail(email);
            }
            return email.Length == 0 || IsEmail(email);
        }
        public static bool IsName(string s)
        {
            var name = (s.Trim().Length > 0 ? RemoveDiacritics(s) : string.Empty);
            var regex = new Regex(@"^[a-zA-Z\s]+$");
            return regex.IsMatch(name);
        }
        public static bool IsName(string name, bool required, int minLength, int maxLength)
        {
            if (!required) return name.Length == 0 || IsName(name);
            if (name.Length == 0) return false;

            return name.Length >= minLength && name.Length <= maxLength && IsName(name);
        }
        public static bool IsText(string s)
        {
            var text = (s.Trim().Length > 0 ? RemoveDiacritics(s) : string.Empty);
            var regex = new Regex(@"^[a-zA-Z\s.,-_0123456789():+/?]+$");
            return regex.IsMatch(text);
        }
        public static bool IsText(string text, bool required, int minLength, int maxLength)
        {
            if (!required) return text.Length == 0 || IsText(text);
            if (text.Length == 0) return false;
            return text.Length >= minLength && text.Length <= maxLength && IsText(text);
        }
        public static bool IsMobileNumber(string s)
        {
            return Regex.IsMatch(s, @"^([0|\+[0-9]{1,5})?([1-9][0-9]{8,9})$");
        }
        public static bool IsNumber(string s)
        {
            try
            {
                int number;
                bool result = int.TryParse(s, out number);
                return result;
            }
            catch { return false; }
        }
        public static bool IsNumber(string num, int minLength, int maxLength)
        {
            return (!string.IsNullOrEmpty(num) &&
                    num.Length >= minLength &&
                    num.Length <= maxLength &&
                    IsNumber(num));
        }
        public static bool IsNumber(string mobileno, bool required)
        {
            if (required)
            {
                return mobileno.Length != 0 && IsMobileNumber(mobileno);
            }
            return mobileno.Length == 0 || IsMobileNumber(mobileno);
        }
        public static bool IsCustomerID(string s)
        {
            return Regex.IsMatch(s, @"^([a-z,A-Z,0-9]){1,15}$");
        }
        public static bool IsCustomerID(string custId, bool required)
        {
            if (required)
            {
                return custId.Length != 0 && IsCustomerID(custId);
            }
            return custId.Length == 0 || IsCustomerID(custId);
        }
        /// <summary>
        /// Check PIN (PWD IS NUMBER)
        /// </summary>
        public static bool IsPIN(string iPIN, int iPINLength)
        {
            decimal outPIN;
            var isNumber = decimal.TryParse(iPIN, out outPIN);
            var countPwd = iPIN.Trim().Length;
            if (!isNumber) return false;
            return countPwd == iPINLength;
        }
        #endregion
        #region Dictionary Utitlities
        public static void AddDictionary(Dictionary<string, string> dic, string key, string value)
        {
            if (dic == null) dic = new Dictionary<string, string>();
            if (dic.ContainsKey(key))
            {
                dic.Remove(key);

            }
            dic.Add(key, value);
        }
        public static string GetDictionary(Dictionary<string, string> dic, string key)
        {
            if (dic == null) return string.Empty;
            if (dic.Count == 0) return string.Empty;
            return dic.ContainsKey(key) ? dic[key] : string.Empty;
        }
        public static Dictionary<string, string> GetDictianrDictionaryByValueColl(NameValueCollection resp)
        {
            var responseData = new Dictionary<string, string>();
            for (var i = 0; i < resp.AllKeys.Length - 1; i++)
            {
                var nextKey = resp.AllKeys[i];
                if (nextKey.Substring(0, 2) == "__") continue;
                responseData.Add(nextKey, resp[i]);
            }
            return responseData;
        }
        #endregion
    }

    class LocalFunctionEnum
    {
        public const string JSON = "application/json";
        public const string XML = "application/xml";
        public const string CardNumber = "CARDNUMBER";
        public const string Password = "PASSWORD";
        public const string PinNumber = "PINNUMBER";

        public const string Backslash = "\\";
        public const string Slash = "/";
        public const string DoubleSlash = "/";
        public const string FolderCurrent = "./";

    }
}
