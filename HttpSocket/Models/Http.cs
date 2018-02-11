using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;


//Get request for text and image files , 200, 400, 404 reponses

namespace HttpSocket.Models
{
    public class Http
    { 
        const string format = "HTTP/1.1 {0}\r\nDate: {1}\r\nContent-Type: {2}\r\nServer: Apache/1.3.3.7\r\n\r\n";
        const string badRequest = "<html><head><title>Error</title></head><body><font size=\"26\">400 Bad Request</font></body></html>";
        const string notFound = "<html><head><title>Error</title></head><body><font size=\"26\">404 Not Found</font></body></html>";

        public static byte[] createResponse(int code, string httpReqeust)
        {
            string HttpDate = DateTime.Now.ToUniversalTime().ToString("r");
            string header = null;

            switch (code)
                {
                case 200:
                    {
                        string fileName = GetFileName(httpReqeust);
                        if (fileName.Length == 0)
                        {
                            return createResponse(400, null);
                        }

                        string fileType = GetFileType(fileName);
                        if (fileType.Length == 0)
                        {
                            return createResponse(400, null);
                        }

                        if (fileType == ".txt" || fileType == ".html")
                        {
                            header = "text/html";
                        }
                        if (fileType == ".png")
                        {
                            header = "image/png";
                        }
                        if (fileType == ".jpeg" || fileType == ".jpg")
                        {
                            header = "image/jpeg";
                        }
                        if (fileType == ".gif")
                        {
                            header = "image/gif";
                        }
                        return mergeArrays(Encoding.ASCII.GetBytes(string.Format(format, "200 OK", HttpDate, header)), getFile(fileName));
                    }
                case 400:
                    return Encoding.ASCII.GetBytes(string.Format(format, "400 Bad Request", HttpDate, "text/html") + badRequest);
                case 404:
                    return Encoding.ASCII.GetBytes(string.Format(format, "404 Not Found", HttpDate, "text/html") + notFound);
                default:
                    return null;
                }
        }

        public static string GetFileName(string httpReqeust)
        {
            return Regex.Match(httpReqeust, @"(?<=/).*(?=HTTP)").Value;
        }

        public static string GetFileType(string name)
        {
            return Regex.Match(name, @"\.txt|\.html|\.png|\.jpg|\.jpeg|\.gif").Value;   
        }

        public static byte[] getFile(string fileName)
        {
            byte[] fileBytes = null;
            try
            {
                FileStream stream = File.OpenRead(fileName);
                fileBytes = new byte[stream.Length];
                stream.Read(fileBytes, 0, fileBytes.Length);
                stream.Close();
            }
            catch(FileNotFoundException e)
            {
                return createResponse(404, null);
            }

            return fileBytes;
        }

        public static byte[] mergeArrays(byte[] firstArray, byte[] secondArray)
        {
            var newArray = new byte[firstArray.Length + secondArray.Length];
            firstArray.CopyTo(newArray, 0);
            secondArray.CopyTo(newArray, firstArray.Length);
            return newArray;
        }
    }
}
