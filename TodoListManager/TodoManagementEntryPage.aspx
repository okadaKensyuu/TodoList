
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TodoManagementEntryPage.aspx.cs" Inherits="TodoListManager.TodoManagementEntryPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>登録画面-Todoリスト</title>
    <style type="text/css">
        #form1 {
            height: 106px;
            width: 734px;
        }
    </style>
</head>
<body style="height: 294px; width: 736px;">
    <form id="form1" runat="server">
        <h1>Todo新規登録</h1>
        <asp:Label ID="Label2" runat="server" Text="Todo名" Font-Bold="True" Font-Size="Large" Height="20px"></asp:Label>
        　　<asp:TextBox ID="TextBox_Title" runat="server" Height="20px" Width="300px"></asp:TextBox>
        <br />
        <asp:Label ID="Label4" runat="server" Text="Todo内容" Font-Bold="True" Font-Size="Large" Height="20px"></asp:Label>
        　<asp:TextBox ID="TextBox_Contexts" runat="server" Height="20px" Width="300px"></asp:TextBox>
        <br />
        <asp:Label ID="Label5" runat="server" Text="追加者" Font-Bold="True" Font-Size="Large" Height="20px"></asp:Label>
        　 　<asp:TextBox ID="TextBox_Member" runat="server" Height="20px" Width="300px"></asp:TextBox>
        <br />
        <br />
        <asp:Label ID="MessageLabel" runat="server" ForeColor="Red" Text="※ 入力されていない項目があります。全ての項目を埋めてから登録ボタンを押してください。" Visible="False" Height="43px" Width="710px"></asp:Label>
        <p>
            <asp:Button ID="EntryDecisionButton" runat="server" OnClick="EntryDecisionButton_Click" Text="登録" Height="40px" Width="90px" Font-Bold="True" Font-Size="Large" />
            　　　　　　　<asp:Button ID="ReturnButton" runat="server" OnClick="ReturnButton_Click" Text="戻る" Height="40px" Width="90px" Font-Bold="True" Font-Size="Large" />
        </p>
    </form>
</body>
</html>

