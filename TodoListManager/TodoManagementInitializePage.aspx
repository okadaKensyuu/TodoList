
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TodoManagementInitializePage.aspx.cs" Inherits="TodoListManager.TodoManagementInitializePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>初期化画面-Todoリスト</title>
</head>
<body style="width: 378px">
    <form id="form1" runat="server">
    <div>
    <h1>Todo初期化</h1>
        <asp:Label ID="Label1" runat="server" Text="Todoリストを初期化しますか？" Font-Bold="False" Font-Size="X-Large"></asp:Label>
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        　　<asp:Button ID="DoInitializeButton" runat="server" OnClick="DoInitializeButton_Click" Text="OK" Height="40px" Width="90px" Font-Bold="True" Font-Size="Large" />
        　　　　　　<asp:Button ID="ReturnButton" runat="server" OnClick="ReturnButton_Click" Text="戻る" Height="40px" Width="90px" Font-Bold="True" Font-Size="Large" />
    
    </div>
    </form>
</body>
</html>

