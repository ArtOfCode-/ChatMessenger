using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPQMessenger.Helpers;

namespace EPQMessenger
{
    static class Protocol
    {
        /// <summary>
        /// The constant protocol version.
        /// </summary>
        public const string PROTOCOL_VERSION = "1.0";

        /// <summary>
        /// Contains all response codes and their text equivalents.
        /// </summary>
        public static Dictionary<int, string> StatusCodes = new Dictionary<int, string>
        {
            // 100 series: connections
            { 100, "Initiate" },
            { 101, "Successful Connection" },
            { 102, "Failed Connection" },
            { 103, "Dropping Connection" },
            { 104, "Server Shutdown" },
            { 105, "Kicked" },
            { 106, "Banned" },

            // 200 series: confirmations
            { 200, "OK" },
            { 201, "Received" },
            { 202, "Continue" },

            // 300 series: from client
            { 300, "Response" },
            { 301, "Disconnecting" },
            { 302, "Message" },

            // 400 series: client errors
            { 400, "Bad Request" },
            { 401, "Username Expected" },
            { 402, "Response Expected" },
            { 403, "Bad Format" },
            { 404, "No Question" },

            // 500 series: server errors
            { 500, "Server Error" }
        };

        /// <summary>
        /// When given a response code, constructs a full response string for it.
        /// </summary>
        /// <param name="code">The code for which to generate a response.</param>
        /// <returns>The string containing the response.</returns>
        public static string GetResponseFromCode(int code)
        {
            string status = StatusCodes.ContainsKey(code) ? StatusCodes[code] : null;
            if (status == null)
            {
                throw new InvalidStatusException(code);
            }
            return string.Format("CSP/{0} {1} {2}", PROTOCOL_VERSION, code.ToString(), status);
        }

        /// <summary>
        /// When given a valid CSP status response, returns the code contained in it.
        /// </summary>
        /// <param name="response">A valid CSP status response to parse.</param>
        /// <returns>The code contained within the reponse.</returns>
        public static int GetCodeFromResponse(string response)
        {
            if (!response.Contains(string.Format("CSP/{0}", PROTOCOL_VERSION)))
            {
                throw new FormatException("Response does not contain a CSP version header, or client is using an outdated "
                    + "protocol version.");
            }
            string[] parts = response.Split(' ');
            if (parts.Length < 3)
            {
                throw new FormatException("Response does not contain all required details.");
            }
            int code = -1;
            if (!int.TryParse(parts[1], out code) || code == 0)
            {
                throw new FormatException("Response code is not a valid number.");
            }
            return code;
        }

        /// <summary>
        /// When given a client response, retrieves from it just the response text.
        /// </summary>
        /// <param name="response">The full client response.</param>
        /// <returns>The simple response text, stripped of status headers.</returns>
        public static string[] GetClientResponse(string response)
        {
            int code = GetCodeFromResponse(response);
            if (code != 300 && code != 302)
            {
                throw new InvalidStatusException(GetCodeFromResponse(response), "This code does not imply a response.");
            }
            string[] lines = response.Split('\n');
            if (lines.Length < 2)
            {
                throw new FormatException("Not enough lines to contain a response.");
            }
            List<string> lineList = lines.ToList();
            lineList.RemoveAt(0);
            return lineList.ToArray();
        }
    }
}
