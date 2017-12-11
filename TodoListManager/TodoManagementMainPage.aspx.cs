
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TodoListManager
{
    /// <summary>
    /// Todo管理メイン画面(以下、メイン画面)
    /// </summary>
    public partial class TodoManagementMainPage : System.Web.UI.Page
    {
        //Todoリストに登録されているTodoの数
        int registeredToDoCount;
        /// <summary>
        /// メイン画面ロード時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            CheckTodoListContent();
        }
        /// <summary>
        /// 『Todo詳細表示』ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TodoDetailDisplayButton_Click(object sender, EventArgs e)
        {
            MoveTodoDetailPage();
        }
        /// <summary>
        /// 『Todo新規登録』ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TodoNewlyEntryButton_Click(object sender, EventArgs e)
        {
            MoveTodoEntryPage();
        }
        /// <summary>
        /// 『Todoリスト初期化』ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TodoListInitializeButton_Click(object sender, EventArgs e)
        {
            MoveTodoListInitializePage();
        }
        /// <summary>
        /// 詳細画面に遷移する処理
        /// </summary>
        public void MoveTodoDetailPage()
        {
            Response.Redirect("~/TodoManagementDetailPage.aspx");
            
        }
        /// <summary>
        /// 登録画面へ遷移する処理
        /// </summary>
        public void MoveTodoEntryPage()
        {
            Response.Redirect("~/TodoManagementEntryPage.aspx");
        }
        /// <summary>
        /// 初期化画面へ遷移する処理
        /// </summary>
        public void MoveTodoListInitializePage()
        {
            Response.Redirect("~/TodoManagementInitializePage.aspx");
        }
        /// <summary>
        /// Todoリストの中身を確認する処理
        /// </summary>
        public void CheckTodoListContent()
        {
            CheckCountRegisteredToDo();
            if (registeredToDoCount == 0)
            {
                TodoListContentEmptyOnProcess();
            }
            else if (registeredToDoCount >= 1)
            {
                TodoListContentEmptyNotOnProcess();
            }
        }
        /// <summary>
        /// 登録されているTodoの数を確認する処理
        /// </summary>
        /// <returns></returns>
        public int CheckCountRegisteredToDo()
        {
            using (TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                return registeredToDoCount = (context.TodoList.Select(b => b).Count());
            }
        }
        /// <summary>
        /// Todoリストが空の時の処理
        /// </summary>
        public void TodoListContentEmptyOnProcess()
        {
            TodoListInitializeButton.Enabled = false;
            TodoDetailDisplayButton.Enabled = false;
        }
        /// <summary>
        /// Todoリストが空ではない時の処理
        /// </summary>
        public void TodoListContentEmptyNotOnProcess()
        {
            TodoListInitializeButton.Enabled = true;
            TodoDetailDisplayButton.Enabled = true;
        }
    }
}


