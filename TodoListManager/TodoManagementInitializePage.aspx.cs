
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TodoListManager
{
    /// <summary>
    /// Todo管理初期化画面(以下、初期化画面)
    /// </summary>
    public partial class TodoManagementInitializePage : System.Web.UI.Page
    {
        /// <summary>
        /// 初期化画面ロード時の処理
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
        protected void DoInitializeButton_Click(object sender, EventArgs e)
        {
            RemoveAllTodo();
            MoveMainPage();
        }
        /// <summary>
        /// 『戻る』ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ReturnButton_Click(object sender, EventArgs e)
        {
            MoveMainPage();
        }
        /// <summary>
        /// 全てのTodoをTodoリストから削除する処理
        /// </summary>
        public void RemoveAllTodo()
        {
            using (TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                context.Database.ExecuteSqlCommand("DELETE FROM TodoList");

            }
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

