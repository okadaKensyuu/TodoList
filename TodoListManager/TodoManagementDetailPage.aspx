
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TodoManagementDetailPage.aspx.cs" Inherits="TodoListManager.TodoManagementDetailPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>詳細画面-Todoリスト</title>
    <style type="text/css">
        #form1 {
            width: 1167px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div style="height: 408px; width: 1171px;">
    <h1>Todo詳細表示</h1>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="Black" BorderStyle="Ridge" BorderWidth="2px" 
            CellPadding="3" CellSpacing="1" DataKeyNames="ID" DataSourceID="TodoListDataSource" GridLines="None" Font-Size="Large">
            <Columns>
                <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" SortExpression="ID">
                    <FooterStyle HorizontalAlign="Center" />
                     <HeaderStyle HorizontalAlign="Center" />
                    <ItemStyle HorizontalAlign="Center" Width="25px" />
                </asp:BoundField>
                <asp:BoundField DataField="TODO名" HeaderText="TODO名" SortExpression="TODO名" >
                    <FooterStyle HorizontalAlign="Center" />
                     <HeaderStyle HorizontalAlign="Center" />
                    <ItemStyle HorizontalAlign="Center" />
                </asp:BoundField>
                <asp:BoundField DataField="TODO内容" HeaderText="TODO内容" SortExpression="TODO内容" >
                <HeaderStyle HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Left" />
                </asp:BoundField>
                <asp:BoundField DataField="追加日" HeaderText="追加日" SortExpression="追加日" DataFormatString="{0:yyyy/MM/dd(ddd) HH:mm}" >
                    <FooterStyle HorizontalAlign="Center" />
                     <HeaderStyle HorizontalAlign="Center" />
                    <ItemStyle HorizontalAlign="Center" Width="170px" />
                </asp:BoundField>
                <asp:BoundField DataField="追加者" HeaderText="追加者" SortExpression="追加者" >
                    <FooterStyle HorizontalAlign="Center" />
                     <HeaderStyle HorizontalAlign="Center" />
                    <ItemStyle HorizontalAlign="Center" Width="80px" />
                </asp:BoundField>
            </Columns>
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
        <br />
        <br />
        <asp:RadioButtonList ID="RadioButtonList1" runat="server" AutoPostBack="True" OnSelectedIndexChanged="RadioButtonList1_SelectedIndexChanged" Font-Bold="False" Font-Size="Large" Height="30px">
            <asp:ListItem Value="1" Selected="True">最初に追加したTodo</asp:ListItem>
            <asp:ListItem Value="2">最後に追加したTodo</asp:ListItem>
        </asp:RadioButtonList>
        <br />
        <br />
        <asp:Button ID="RemoveButton" runat="server" OnClick="RemoveButton_Click" Text="削除" Height="40px" Width="90px" Font-Bold="True" Font-Size="Large" />
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Button ID="ReturnButton" runat="server" OnClick="ReturnButton_Click" Text="戻る" Height="40px" Width="90px" Font-Bold="True" Font-Size="Large" />
    
    </div>
        <asp:SqlDataSource ID="TodoListDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:TodoList_ManagerConnectionString2 %>" SelectCommand="TodoDetailSelect" SelectCommandType="StoredProcedure">
            <SelectParameters>
                <asp:ControlParameter ControlID="RadioButtonList1" Name="Number" PropertyName="SelectedValue" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>

