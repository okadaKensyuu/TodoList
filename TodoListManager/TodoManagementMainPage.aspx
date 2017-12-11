
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TodoManagementMainPage.aspx.cs" Inherits="TodoListManager.TodoManagementMainPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>メイン画面-Todoリスト</title>
</head>
    <%--<body>--%>
    <form id="form1" runat="server">
    <div style="height: 377px; width: 1176px;">
    <h1>Todoリスト管理システム</h1>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="Black" BorderStyle="Ridge" BorderWidth="2px"
             CellPadding="3" CellSpacing="1" DataSourceID="TodoListDataSource" GridLines="None" AllowSorting="True" Font-Size="Large">
            <Columns>
                <asp:BoundField DataField="TODO名" HeaderText="TODO名" SortExpression="TODO名" >
                </asp:BoundField>
                <asp:BoundField DataField="追加日" HeaderText="追加日" SortExpression="追加日" DataFormatString="{0:MM/dd(ddd)}" >
                </asp:BoundField>
                <asp:BoundField DataField="追加者" HeaderText="追加者" SortExpression="追加者">
                </asp:BoundField>
            </Columns>
            <FooterStyle BackColor="#C6C3C6" ForeColor="Black" Wrap="False" />
            <HeaderStyle BackColor="#4A3C8C" Font-Bold="True" ForeColor="#E7E7FF" Font-Strikeout="False" />
            <PagerStyle BackColor="#C6C3C6" ForeColor="Black" HorizontalAlign="Right" />
            <RowStyle BackColor="#DEDFDE" ForeColor="Black" HorizontalAlign="Center" />
            <SelectedRowStyle BackColor="#9471DE" Font-Bold="True" ForeColor="White" />
            <SortedAscendingCellStyle BackColor="#F1F1F1" />
            <SortedAscendingHeaderStyle BackColor="#594B9C" />
            <SortedDescendingCellStyle BackColor="#CAC9C9" />
            <SortedDescendingHeaderStyle BackColor="#33276A" />
        </asp:GridView>
        <asp:SqlDataSource ID="TodoListDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:TodoList_ManagerConnectionString %>" SelectCommand="TodoListSelect" SelectCommandType="StoredProcedure"></asp:SqlDataSource>
        <br />
        <br />
        <asp:Button ID="TodoNewlyEntryButton" runat="server" OnClick="TodoNewlyEntryButton_Click" Text="Todo新規登録" Height="40px" Width="140px" Font-Bold="True" Font-Size="Large" />
    
        <br />
        <br />
            <asp:Button ID="TodoDetailDisplayButton" runat="server" OnClick="TodoDetailDisplayButton_Click" Text="Todo詳細表示" Height="40px" Width="140px" Font-Bold="True" Font-Size="Large" />
            &nbsp;
            <asp:Button ID="TodoListInitializeButton" runat="server" Text="Todoリスト初期化" OnClick="TodoListInitializeButton_Click" Height="40px" Width="180px" Font-Bold="True" Font-Size="Large" />
    
    </div>
    </form>
</body>
        <p style="height: 162px">
            &nbsp;</p>
    
</html>

