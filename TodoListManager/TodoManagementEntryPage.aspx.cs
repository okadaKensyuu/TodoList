
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TodoListManager
{
    /// <summary>
    /// Todo管理登録画面(以下、登録画面)
    /// </summary>
    public partial class TodoManagementEntryPage : System.Web.UI.Page
    {
        int newRegister_TodoId;
        int registeredToDoCount;
        /// <summary>
        /// 登録画面ロード時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 『登録』ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EntryDecisionButton_Click(object sender, EventArgs e)
        {
            InputedContentRegisterableStateJudge();
        }
        /// <summary>
        /// 入力した内容が登録可能な状態か判定する処理
        /// </summary>
        public void InputedContentRegisterableStateJudge()
        {
            if ((TextBox_Title.Text != "" && TextBox_Contexts.Text != "") && TextBox_Member.Text != "")
            {
                MessageLabel.Visible = false;
                RegisterInputedToDo();
                EntryDecisionButton.Enabled = false;
                MoveEntryConfirmationPage();
            }
            MessageLabel.Visible = true;
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
        /// 入力したTodoをTodoリストに登録する処理
        /// </summary>
        public void RegisterInputedToDo()
        {
            DateTime toDay = DateTime.Now;
            using(TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                CheckTodoListContent();               
                context.TodoList.Add(new TodoList
                {
                    Todo_id = newRegister_TodoId,
                    Todo_title = TextBox_Title.Text,
                    Add_date = toDay,
                    Todo_contents = TextBox_Contexts.Text,
                    Adding_member = TextBox_Member.Text
                });
               context.SaveChanges();
            }
        }
        /// <summary>
        /// Todoリストの中身を確認する処理
        /// </summary>
        public  void CheckTodoListContent()
        {
            using (TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                CheckCountRegisteredToDo();

                //int currentMax_TodoId;
                if (registeredToDoCount == 0)
                {
                    TodoListContentEmptyOnProcess();                   

                } else if (registeredToDoCount >= 1)
                {
                    TodoListContentEmptyNotOnProcess();
                }
            } 
        }
        /// <summary>
        /// Todoリストが空の時の処理
        /// </summary>
        public void TodoListContentEmptyOnProcess()
        {
           newRegister_TodoId = 1;
        }
        /// <summary>
        /// Todoリストが空ではないときの処理
        /// </summary>
        public void TodoListContentEmptyNotOnProcess()
        {
            using (TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                newRegister_TodoId = (context.TodoList.Select(b => b.Todo_id).Max()) + 1;
            }
        }
        /// <summary>
        /// Todoリストに登録されているTodoの数を確認する処理
        /// </summary>
        /// <returns></returns>
        public int CheckCountRegisteredToDo()
        {
            using(TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                return registeredToDoCount = (context.TodoList.Select(b => b).Count());
            }
        }
        /// <summary>
        /// 登録結果確認画面へ遷移する処理
        /// </summary>
        public void MoveEntryConfirmationPage()
        {
            Response.Redirect("~/TodoManagementEntryConfirmationPage.aspx");
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

