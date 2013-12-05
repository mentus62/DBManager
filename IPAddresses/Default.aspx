<%@ Import Namespace="System.Web.UI.HtmlControls" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <style type="text/css">
        #Select_Server {
            margin-bottom: 0px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <h2>
        RMH Radiology 
        IP Addresses Database</h2>
        <h3>
            Select Server: 
            <asp:DropDownList ID="Select_Server" runat="server">
            </asp:DropDownList>
            &nbsp;&nbsp;&nbsp; 
            Select Database: 
            <asp:DropDownList ID="Select_DB" runat="server" AutoPostBack="True" 
                onselectedindexchanged="Select_DB_SelectedIndexChanged">
            </asp:DropDownList>
            &nbsp; 
            Select Table:
            <asp:DropDownList ID="Select_Table" runat="server" AutoPostBack="True" 
                onselectedindexchanged="Select_Table_SelectedIndexChanged">
            </asp:DropDownList>
        </h3>
        <h3>
            &nbsp;&nbsp;
            <font>Filter: 
        <asp:TextBox ID="txtFilter" runat="server" AutoPostBack="true"
            ontextchanged="txtFilter_TextChanged"></asp:TextBox>
        </font>
                <!--  -->
        </h3>
        <h2>
            <asp:ImageButton ID="btnB" src="Icons\Blank.jpg" runat="server" onclick="btnB_Click" 
                Text="B" Height="0px" Width="0px" style="Visibility: Hidden"/>
            <asp:ImageButton ID="btnRefresh" src="Icons\Refresh.jpg" runat="server" onclick="btnRefresh_Click" 
                Text="Refresh" AlternateText="Refresh" Height="31px" Width="31px" 
                ToolTip="Reload from Database" />&nbsp;
            
            <asp:ImageButton ID="btnSaveDB" src="Icons\Save.jpg" runat="server" Text="Save to Database" 
                width="31px" onclick="btnSaveDB_Click" AlternateText="Save to Database" 
                Height="31px" ToolTip="Save changes to Database" />&nbsp;

            <asp:ImageButton ID="btnExport_to_CSV" src="Icons\CSV.jpg" runat="server" 
                onclick="btnExport_to_CSV_Click" Text="Export to CSV File" 
                AlternateText="Export to CSV File" Height="31px" Width="31
                px" ToolTip="Export Current View into CSV File" />
            <br />
                
    </h2>
        
        <asp:GridView ID="GridView1" runat="server" CellPadding="4" ForeColor="#333333" 
            GridLines="None" AllowSorting="True" onrowediting="GridView1_RowEditing" 
            onsorting="GridView1_Sorting" HorizontalAlign="Left" 
            onrowcancelingedit="GridView1_RowCancelingEdit" 
            onrowdeleting="GridView1_RowDeleting" onrowupdating="GridView1_RowUpdating" 
            PageSize="20" EmptyDataText="No Data to Display." 
            ondatabound="GridView1_DataBound">
            <AlternatingRowStyle BackColor="White" />
            <Columns>
                <asp:CommandField ShowEditButton="True" ShowHeader="True" />
                <asp:CommandField ShowDeleteButton="True" />
            </Columns>
            <EditRowStyle BackColor="#2461BF" />
            <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" 
                HorizontalAlign="Left" />
            <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#EFF3FB" Wrap="False" />
            <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#F5F7FB" />
            <SortedAscendingHeaderStyle BackColor="#6D95E1" />
            <SortedDescendingCellStyle BackColor="#E9EBEF" />
            <SortedDescendingHeaderStyle BackColor="#4870BE" />
        </asp:GridView>
    </div>
    </form>
</body>
</html>

