using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIGigaChatImage_Vinokurov.Response
{
    public class ResponseToken
    {
        /// <summary>
        /// Token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// Cpow korpa token wcrekaer
        /// </summary>
        public string expires_at { get; set; }
    }
}
