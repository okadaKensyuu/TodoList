
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TodoListManager
{
    /// <summary>
    /// Todo管理登録結果確認画面(以下、登録結果確認画面)
    /// </summary>
    public partial class TodoManagementEntryConfirmationPage : System.Web.UI.Page
    {
        /// <summary>
        /// 登録結果確認画面ロード時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 『OK』ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OKButton_Click(object sender, EventArgs e)
        {
            MoveMainPage();
        }
        /// <summary>
        /// メイン画面へ遷移する処理
        /// </summary>
        public void MoveMainPage()
        {
            Response.Redirect("~/TodoManagementMainPage.aspx");
        }
    }
}

