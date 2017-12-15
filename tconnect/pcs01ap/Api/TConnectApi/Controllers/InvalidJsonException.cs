using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TConnectApi.Controllers
{
    public class InvalidJsonException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InvalidJsonException()
        {
        }
    }
}