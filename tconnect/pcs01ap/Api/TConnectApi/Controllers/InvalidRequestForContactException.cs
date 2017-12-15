using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TConnectApi.Controllers
{
    public class InvalidRequestForContactException : Exception
    {
        public string InvalidItem { get { return this.invalidItem; } }
        /// <summary>
        /// 不正な要素
        /// </summary>
        private string invalidItem;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="invalidItem">不正な要素</param>
        public InvalidRequestForContactException(string invalidItem)
        {
            this.invalidItem = invalidItem;
        }
    }
}