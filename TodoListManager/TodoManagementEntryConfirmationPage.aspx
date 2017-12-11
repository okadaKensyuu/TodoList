
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TodoManagementEntryConfirmationPage.aspx.cs" Inherits="TodoListManager.TodoManagementEntryConfirmationPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>登録結果確認画面-Todoリスト</title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="width: 1171px; height: 323px">
    <h1>TODO登録結果確認</h1>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="ID" DataSourceID="SqlDataSource1" 
            BackColor="White" BorderColor="Black" BorderStyle="Ridge" BorderWidth="2px" CellPadding="3" CellSpacing="1" GridLines="None" Font-Size="Large">
            <Columns>
                <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" SortExpression="ID" >
                <FooterStyle HorizontalAlign="Center" />
                <HeaderStyle HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Center" Width="25px" />
                </asp:BoundField>
                <asp:BoundField DataField="TODO名" HeaderText="TODO名" SortExpression="TODO名" >
                <HeaderStyle HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Center" />
                </asp:BoundField>
                <asp:BoundField DataField="TODO内容" HeaderText="TODO内容" SortExpression="TODO内容" >
                <HeaderStyle HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Left" />
                </asp:BoundField>
                <asp:BoundField DataField="追加日" HeaderText="追加日" SortExpression="追加日" DataFormatString="{0:yyyy/MM/dd(ddd) HH:mm}" >
                <HeaderStyle HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Center" Width="170px" />
                </asp:BoundField>
                <asp:BoundField DataField="追加者" HeaderText="追加者" SortExpression="追加者" >
                <HeaderStyle HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Center" Width="80px" />
                </asp:BoundField>
            </Columns>
            <EditRowStyle HorizontalAlign="Right" VerticalAlign="Middle" />
            <FooterStyle BackColor="#C6C3C6" ForeColor="Black" />
            <HeaderStyle BackColor="#4A3C8C" Font-Bold="True" ForeColor="#E7E7FF" />
            <PagerStyle BackColor="#C6C3C6" ForeColor="Black" HorizontalAlign="Right" />
            <RowStyle BackColor="#DEDFDE" ForeColor="Black" />
            <SelectedRowStyle BackColor="#9471DE" Font-Bold="True" ForeColor="White" />
            <SortedAscendingCellStyle BackColor="#F1F1F1" />
            <SortedAscendingHeaderStyle BackColor="#594B9C" />
            <SortedDescendingCellStyle BackColor="#CAC9C9" />
            <SortedDescendingHeaderStyle BackColor="#33276A" />
        </asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:TodoList_ManagerConnectionString3 %>" SelectCommand="MaxTodoSelect" SelectCommandType="StoredProcedure"></asp:SqlDataSource>
        <br />
        <asp:Button ID="OKButton" runat="server" OnClick="OKButton_Click" Text="OK" Height="40px" Width="90px" Font-Bold="True" Font-Size="Large" />
    
    </div>
    </form>
</body>
</html>

