
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TodoListManager
{
    /// <summary>
    /// Todo管理詳細画面(以下、詳細画面)
    /// </summary>
    public partial class TodoManagementDetailPage : System.Web.UI.Page
    {
        int registeredToDoCount;
        /// <summary>
        /// 詳細画面ロード時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            CheckTodoListContent();
        }
        /// <summary>
        /// 『削除』ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void RemoveButton_Click(object sender, EventArgs e)
        {
            RemoveShowTodo();
            MoveMainPage();
        }
        /// <summary>
        /// 『戻るボタン押下時の処理』
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ReturnButton_Click(object sender, EventArgs e)
        {
            MoveMainPage();
        }
        /// <summary>
        /// GridViewに表示しているTodoをTodoリストから削除する処理
        /// </summary>
        public void RemoveShowTodo()
        {
            if (RadioButtonList1.SelectedItem.Value == "1")
            {
                RemoveTodoAddedFirst();
            }
            else if (RadioButtonList1.SelectedItem.Value == "2")
            {
                RemoveTodoAddedLast();
            }
        }
        /// <summary>
        /// 最初に追加されたTodoを削除する処理
        /// </summary>
        public void RemoveTodoAddedFirst()
        {
            using (TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                int todoAddedFirst = context.TodoList.Select(b => b.Todo_id).Min();
                var todoRemove = context.TodoList.Single(x => x.Todo_id == todoAddedFirst);
                context.TodoList.Remove(todoRemove);
                context.SaveChanges();
            }
        }
        /// <summary>
        /// 最後に追加されたTodoを削除する処理
        /// </summary>
        public void RemoveTodoAddedLast()
        {
            using (TodoList_ManagerEntities context = new TodoList_ManagerEntities())
            {
                int todoAddedLast = context.TodoList.Select(b => b.Todo_id).Max();
                var todoRemove = context.TodoList.Single(x => x.Todo_id == todoAddedLast);
                context.TodoList.Remove(todoRemove);
                context.SaveChanges();
            }
        }
        /// <summary>
        /// メイン画面へ遷移する処理
        /// </summary>
        public void MoveMainPage()
        {
            Response.Redirect("~/TodoManagementMainPage.aspx");
        }
        /// <summary>
        /// Todoリストの中身を確認する処理
        /// </summary>
        public void CheckTodoListContent()
        {
            CheckCountRegisteredToDo();
            if (registeredToDoCount == 1)
            {
                RadioButtonList1.Visible = false;
            }
            else if (registeredToDoCount > 1)
            {
                RadioButtonList1.Visible = true;
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
        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }  
    }
}

