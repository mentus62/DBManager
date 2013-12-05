#region "Libraries"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.DataAccess;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.IO;
#endregion

public partial class _Default : System.Web.UI.Page
{
    #region "Variables"
        string SQL_Servers = @"Select * From sys.server";
        string SQL_Connection = @"Data Source=.\SQLEXPRESS; Initial Catalog=Workstations; Integrated Security=Yes;";
        string SQL_Query = "SELECT * FROM Workstations_List";
        const string _Local_SQL_Server = @"MH19189\SQLEXPRESS";
        const string ALL_DBs_n_Tables_FileName = "All_DBs_n_Tables.csv";
        const string All_DBs_Directory_name = "DBs";
        const int MAX_Tables_in_DB = 200;
        private SortDirection?[] Last_Sort
        {
            get
            {
                object d = ViewState["Last_Sort"];
                return (d == null ? null : (SortDirection?[])d);
            }
            set
            {
                ViewState["Last_Sort"] = value;
            }
        }
        private string[] DB_Dropdownlist
        {
            get
            {
                object d = ViewState["DB_DropdownList"];
                return (d == null ? null : (string[])d);
            }
            set
            {
                ViewState["DB_DropdownList"] = value;
            }
        }
        private string[] Table_Dropdownlist
        {
            get
            {
                object d = ViewState["Table_DropdownList"];
                return (d == null ? null : (string[])d);
            }
            set
            {
                ViewState["Table_DropdownList"] = value;
            }
        }
        private string SelectedTable
        {
            get
            {
                object d = ViewState["SelectedTable"];
                return (d == null ? null : (string)d);
            }
            set
            {
                ViewState["SelectedTable"] = value;
            }

        }
        private DataTable GridViewTable
        {
            get 
            {
                object d = Session["GridViewTable"];
                return (d == null ? null : (DataTable)d);
            }
            set 
            {
                Session["GridViewTable"] = value;
            }
        }
        private List<string[]> Changed_Items
        {
            get 
            {
                object d = ViewState["Changed_Items"];
                return (d == null) ? null : (List<string[]>)d;
            }
            set 
            {
                ViewState["Changed_Items"] = value;
            }
        }
        enum SQL_Command_type { ReadTable, Update, Delete, AddRow}
        private string Current_DB_name 
        {
            get
            {
                object d = ViewState["Current_DB_name"];
                return (d == null) ? null : (string)d;
            }
            set
            {
                ViewState["Current_DB_name"] = value;
            }
        }
        private string Current_Table_index 
        {
            get
            {
                object d = ViewState["Current_Table_name"];
                return (d == null) ? null : (string)d;
            }
            set
            {
                ViewState["Current_Table_name"] = value;
            }
        }
    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!Page.IsPostBack)
            {
                Start();
            }
            else 
            {
                
            }
        }
        catch (Exception ex) { Response.Write(ex.ToString()); }
    }

    private void Start()
    {
        // Initialization
        Select_Server.Items.Add(_Local_SQL_Server); // Server
        if (!Directory.Exists(All_DBs_Directory_name)) Directory.CreateDirectory(All_DBs_Directory_name); // Main Directory
        Changed_Items = new List<string[]>();

        // Getting the List of All DBs and Tables of the First DB
        string[,] _DB_n_DB1Tables_list = GetDBs_n_Tables(_Local_SQL_Server, ALL_DBs_n_Tables_FileName);

        // Fill DropDown Lists Variables
        DB_Dropdownlist = new string[_DB_n_DB1Tables_list.GetLength(1)];
        Table_Dropdownlist = new string[_DB_n_DB1Tables_list.GetLength(0)];
        for (int i = 0; i < _DB_n_DB1Tables_list.GetLength(1); i++) { DB_Dropdownlist[i] = _DB_n_DB1Tables_list[0, i]; }
        for (int i = 1; i < _DB_n_DB1Tables_list.GetLength(0); i++) { Table_Dropdownlist[i] = _DB_n_DB1Tables_list[i, 0]; }

        // Fill DB DropDown 
        Select_DB.Items.Clear();
        foreach (string s in DB_Dropdownlist) { Select_DB.Items.Add(s); }

        // Create DBs Directories
        foreach (string s in DB_Dropdownlist) { if (!Directory.Exists(All_DBs_Directory_name + @"\" + s)) Directory.CreateDirectory(All_DBs_Directory_name + @"\" + s); }

        DataTable _temp_table = ShowTable(DB_Dropdownlist[0], 1);
        GridViewTable = _temp_table;
    }

    #region "Get Server, DBs & Tables"
        private string[,] GetDBs_n_Tables(string Server, string CSV_Filename)
    {
        if (File.Exists(CSV_Filename) && !File_Older_Than_Hours(CSV_Filename, 24))
        {
            return CSV_To_Array2D(CSV_Filename);
        }
        else
        {
            //Get List of All DBs from Database
            DataTable _table = DB_To_Table(_Local_SQL_Server, "", "sys.databases");
            string[] db_array = _table.AsEnumerable().Select(row => row.Field<string>("name")).ToArray();

            // Fill Select_DB Items
            Select_DB.Items.Clear();
            foreach (string s in db_array) { Select_DB.Items.Add(s); }

            // Put Names of Tables in First Row of the DB&Tables List
            string[,] DBs = new string[MAX_Tables_in_DB, db_array.Length];
            for (int i = 0; i < db_array.Length; i++) { DBs[0, i] = db_array[i]; }

            // Get List of Tables of a All DBs & Put it in DB_List
            for (int a = 0; a < db_array.Length; a++)
            {
                string[] First_DB_Tables = Get_List_of_Tables(db_array[a]);
                for (int i = 0; i < First_DB_Tables.Length; i++) { DBs[i + 1, a] = First_DB_Tables[i]; }
            }
            Array2D_To_CSV(DBs, ALL_DBs_n_Tables_FileName);
            return DBs;
        }
    }

        private string[] Get_List_of_Tables(string DB)
    {
        DataTable _table1 = DB_To_Table(_Local_SQL_Server, Select_DB.SelectedItem.ToString(), "[" + DB + "].sys.tables");

        string[] table_array = _table1.AsEnumerable().Select(row => row.Field<string>("name")).ToArray();
        Select_Table.Items.Clear();
        foreach (string s in table_array) { Select_Table.Items.Add(s); }
        return table_array;
    }
    #endregion

    #region "DB, Table and Gridview"
        // Get the Table from DB Server
        DataTable DB_To_Table(string Server, string DB, string Table)
        {
            Build_SQL_Commands(Server, DB, Table, SQL_Command_type.ReadTable);
            try
            {
                using (SqlConnection dbconn = new SqlConnection(SQL_Connection))
                {
                    SqlCommand _sqlcomm = new SqlCommand(SQL_Query, dbconn);
                    using (SqlDataAdapter _dbdataadapter = new SqlDataAdapter(_sqlcomm))
                    {
                        DataTable _table = new DataTable();
                        _dbdataadapter.Fill(_table);
                        return _table;
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex);
                return new DataTable();
            }
        }

        // Display Gridview on the webpage
        #region "Display GridView"
            private void Table_To_Gridview(DataView _tableview)
            {
                DataTable _table = _tableview.ToTable();
                BindData(_table);
            }
            private void Table_To_Gridview(DataTable _table)
            {
                BindData(_table);
            }
            private void BindData(object _table)
            {
                DataTable Table = (DataTable)_table;
                if (Table.Rows.Count > 1)
                {
                    DataRow _Filter_Row = Table.NewRow();
                    Table.Rows.InsertAt(_Filter_Row,0);
                }
                GridView1.DataSource = Table;
                GridView1.DataBind();

                               
            }
        #endregion

        // Gets Tables of a specific DB in DB server
        private string[] Tables_in_DB(string Server, string DB)
            {
                DataTable _table1 = DB_To_Table(Server, DB, "[" + DB + "].sys.tables");
                string[] table_array = _table1.AsEnumerable().Select(row => row.Field<string>("name")).ToArray();
                return table_array;
            }

        // Gets the whole list of all DBs and tables and extracts the tables of a specific DB and puts it in the Dropdown control in the webpage.
        private void Fill_Table_Dropdown(string[,] _DBs_n_Tables, sbyte DB_index)
        {
            // FILL SELECT_TABLE DROPDOWN
            Table_Dropdownlist = new string[_DBs_n_Tables.GetLength(0)];
            Table_Dropdownlist[0] = _DBs_n_Tables[0, DB_index];
            for (int x = 1; x < _DBs_n_Tables.GetLength(0); x++)
            {
                Table_Dropdownlist[x] = _DBs_n_Tables[x, DB_index];
            }
            Select_Table.Items.Clear();
            for (int a = 1; a < Table_Dropdownlist.Length; a++) { if (Table_Dropdownlist[a] != "" && Table_Dropdownlist[a] != null) Select_Table.Items.Add(Table_Dropdownlist[a]); }
        }

        // Show the Specified Table
        // Gets list of all DBs and Tables from CSV file and reads the specified table from CSV file unless it doesn't exist or is older than 24h. 
        // Filters the table by the Filter string and Displays the table in Gridview.
        private DataTable ShowTable(string DB_name, int Table_index)
        {
            string[,] _DBs_n_Tables = GetDBs_n_Tables(_Local_SQL_Server, ALL_DBs_n_Tables_FileName);

            sbyte DB_index = 0;
            // Find Selected DB Index        
            for (sbyte y = 0; y < _DBs_n_Tables.GetLength(1); y++) { if (_DBs_n_Tables[0, y] == DB_name) DB_index = y; }

            Fill_Table_Dropdown(_DBs_n_Tables, DB_index);
            string _table_name = Table_Dropdownlist[Table_index];
            string _filename = All_DBs_Directory_name + @"\" + DB_name + @"\" + _table_name + ".csv";
            // If Table name exists in CSV File 
            if (_DBs_n_Tables[Table_index, DB_index] != "" && _DBs_n_Tables[Table_index, DB_index] != "")
            {
                if (!File.Exists(_filename) || File_Older_Than_Hours(_filename, 24))
                {
                    Table_to_CSV(DB_To_Table(_Local_SQL_Server, DB_name, _table_name), _filename, true);
                }
            }
            // If Table name is not in CSV File
            else
            {
                string[] list_of_tables = Tables_in_DB(_Local_SQL_Server, DB_name);
                if (list_of_tables.Length > 0)
                {
                    Tables_List_to_DB_n_Tables_List(list_of_tables, DB_index);
                    _DBs_n_Tables = CSV_To_Array2D(ALL_DBs_n_Tables_FileName);
                }
                else
                {
                    // NO TABLES AVAILABLE
                    Response.Write("No Data to Display");
                    return null;
                }
            }
            Fill_Table_Dropdown(_DBs_n_Tables, DB_index);

            if (!File.Exists(_filename)) return null;
            DataTable _temp_table = CSV_To_Table(txtFilter.Text, _filename);
            GridViewTable = _temp_table;

            Table_To_Gridview(_temp_table);

            // Set Last_Sort
            if (_temp_table != null)
            {
                Last_Sort = new SortDirection?[_temp_table.Columns.Count];
                Set_Last_Sort(_temp_table, 0, SortDirection.Ascending);
            }

            Current_DB_name = DB_name;
            Current_Table_index = Table_index.ToString();
            GridViewTable = _temp_table;

            return _temp_table;
        }

        #region "TABLE & CSV"
            private void Array2D_To_CSV(string[,] Array2D, string CSVFileName)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < Array2D.GetLength(0); x++)
                {
                    string[] Line = new string[Array2D.GetLength(1)];
                    for (int y = 0; y < Array2D.GetLength(1); y++)
                    {
                        Line[y] = Array2D[x, y];
                    }
                    sb.AppendLine(string.Join(",", Line));
                }
                File.WriteAllText(CSVFileName, sb.ToString());
            }

            private string[,] CSV_To_Array2D(string CSVFileName)
            {
                string[] csv_records = File.ReadAllLines(CSVFileName);
                string[,] DBs_n_Tables = new string[MAX_Tables_in_DB, csv_records[0].Split(',').Length];
                for (int i = 0; i < csv_records.Length; i++)
                {
                    for (int ii = 0; ii < csv_records[0].Split(',').Length; ii++)
                    {
                        DBs_n_Tables[i, ii] = csv_records[i].Split(',')[ii];
                    }
                }
                return DBs_n_Tables;
            }

            private DataTable CSV_To_Table(string filter, string CSVFileName)
            {

                DataTable _table = new DataTable();
                // READ ALL LINES
                string[] csv_records = File.ReadAllLines(CSVFileName);
                // FIND HEADERS
                string[] header = csv_records[1].Split(',');

                // Get Data Types
                string[] DataTypes = csv_records[0].Split(',');

                //if (_table.Rows.Count > 0)
                if (csv_records.Length > 1)
                {
                    // BUILD COLUMN NAMES
                    for (int i = 0; i < header.Length; i++) { _table.Columns.Add(header[i], Type.GetType(DataTypes[i])); }

                    // POPULATE ALL DATA
                    for (int index = 2; index < csv_records.Length; index++)
                    {
                        string[] record = csv_records[index].Split(',');
                        bool filter_string_exists = false;
                        for (int i = 0; i < record.Length; i++)
                        {
                            if (record[i].ToLower().Contains(filter.ToLower())) { filter_string_exists = true; }
                        }
                        if ((filter_string_exists) || (filter == ""))
                        {
                            _table.Rows.Add(record);
                        }
                    }
                }
                return _table;
            }

            private void Table_to_CSV(DataTable _dt, string CSVFileName, bool Inc_DataTypes)
            {
                // Insert Data Types into CSV File
                string[] ColumnTypes = new string[_dt.Columns.Count];
                for (int i = 0; i < _dt.Columns.Count; i++)
                {
                    ColumnTypes[i] = _dt.Columns[i].DataType.ToString();
                }
                StringBuilder sb = new StringBuilder();
                //Insert Data Types
                if (Inc_DataTypes) sb.AppendLine(string.Join(",", ColumnTypes));

                IEnumerable<string> _Columns_Name = _dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                // Insert Headers
                sb.AppendLine(string.Join(",", _Columns_Name));

                //Build Rows
                foreach (DataRow row in _dt.Rows)
                {
                    IEnumerable<string> _fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", _fields));
                }

                string[] _File_Path_Parts = CSVFileName.Split('\\');
                if (!Directory.Exists(_File_Path_Parts[0]) && _File_Path_Parts.Length > 1) { Directory.CreateDirectory(_File_Path_Parts[0]); }
                File.WriteAllText(CSVFileName, sb.ToString());
            }

            // Reads the list of tables from CSV file, Adds Tables of the newly read DB, and ptus it back into the CSV file
            private void Tables_List_to_DB_n_Tables_List(string[] list_of_tables, int DB_index)
            {
                string[,] _DBs_n_Tables = CSV_To_Array2D(ALL_DBs_n_Tables_FileName);
                for (int i = 0; i < list_of_tables.Length; i++)
                {
                    _DBs_n_Tables[i + 1, DB_index] = list_of_tables[i];
                }
                Array2D_To_CSV(_DBs_n_Tables, ALL_DBs_n_Tables_FileName);
            }

            private void Export_to_CSV()
            {
                
                DataTable _table = GridViewTable;
                Table_to_CSV(_table, Environment.GetEnvironmentVariable("TEMP") + @"\temp.csv", false);
                Response.ClearContent();
                Response.Clear();
                Response.ContentType = "text/plain";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + Environment.GetEnvironmentVariable("TEMP") + @"\temp.csv" + ";");
                Response.TransmitFile(@"C:\temp.csv");
                Response.Flush();
                Response.End();
            }
        #endregion
    #endregion  
    
    #region "Webpage Controls"
        protected void Select_DB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowTable(Select_DB.SelectedItem.Text, 1);
            Select_DB.SelectedItem.Selected = false;
            Select_DB.Items.FindByText(Table_Dropdownlist[0]).Selected = true;
        }
    
        protected void Select_Table_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedTable = Select_Table.SelectedItem.Text;
            ShowTable(Select_DB.SelectedItem.Text, Select_Table.SelectedIndex + 1);
            Select_DB.SelectedItem.Selected = false;
            Select_DB.Items.FindByText(Table_Dropdownlist[0]).Selected = true;
            Select_Table.SelectedItem.Selected = false;
            Select_Table.Items.FindByText(SelectedTable).Selected = true;
        }

        #region "Filtering"
            protected void txtFilter_TextChanged(object sender, EventArgs e)
            {
                // Cancel Row Editting
                if (GridView1.EditIndex > -1) GridView1.EditIndex = -1;

                GridViewTable = CSV_To_Table(txtFilter.Text, All_DBs_Directory_name + @"\" + Select_DB.SelectedItem.Text + @"\" + Select_Table.SelectedItem.Text + ".csv");
                Table_To_Gridview(GridViewTable);
            }

        #endregion

        #region "Sorting"
            
            protected void GridView1_Sorting(object sender, System.Web.UI.WebControls.GridViewSortEventArgs e)
            {
                Sort(e.SortExpression, true);
                //GridViewTable = (DataTable)GridView1.DataSource;
            }
            private DataTable Sort(string SortExp, bool Reverse)
            {
                // Cancel Row Editting
                GridView1_RowCancelingEdit(null, null);

                Table_To_Gridview(GridViewTable);
                DataTable _table = GridView1.DataSource as DataTable;
                DataView DB_view = new DataView(_table);

                int ColumnIndex = GetColumnIndexByName(_table, SortExp);

                // Reverse order or NOT
                SortDirection Direction1, Direction2;
                string DirectionString1, DirectionString2;
                if (Reverse)
                { Direction1 = SortDirection.Descending; Direction2 = SortDirection.Ascending; DirectionString1 = " DESC"; DirectionString2 = " ASC"; }
                else
                { Direction1 = SortDirection.Ascending; Direction2 = SortDirection.Descending; DirectionString1 = " ASC"; DirectionString2 = " DESC"; }

                if (Last_Sort[ColumnIndex] == SortDirection.Ascending)
                {
                    DB_view.Sort = SortExp + DirectionString1;
                    Set_Last_Sort(_table, ColumnIndex, Direction1);
                }
                else
                {
                    DB_view.Sort = SortExp + DirectionString2;
                    Set_Last_Sort(_table, ColumnIndex, Direction2);
                }

                Table_To_Gridview(DB_view);
                return DB_view.ToTable();
            }

            private void Set_Last_Sort(DataTable Table, int ColumnIndex, SortDirection Direction)
            {
                for (int index = 0; index < Last_Sort.Length; index++)
                {
                    if (index == ColumnIndex)
                    {
                        Last_Sort[index] = Direction;
                    }
                    else
                    {
                        Last_Sort[index] = null;
                    }
                }
            }

            private int GetColumnIndexByName(DataTable Table, string name)
            {
                string[] Headers = new string[Table.Columns.Count];

                for (int i = 0; i < Table.Columns.Count; i++)
                {
                    Headers[i] = Table.Columns[i].ColumnName;
                }

                foreach (string col in Headers)
                {
                    if (col.ToLower().Trim() == name.ToLower().Trim())
                    {
                        return Table.Columns.IndexOf(col.ToString());
                    }
                }

                return -1;
            }
        #endregion

        #region "Editting"
            
            protected void GridView1_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
            {
                int index=0;
                for (int i = 0; i < Last_Sort.Length; i++) { if (Last_Sort[i] != null) index = i; }
                
                DataTable _table;
                _table = Sort(GridViewTable.Columns[index].ColumnName, false);
                
                GridView1.EditIndex = e.NewEditIndex;
                GridViewTable = _table;
                BindData(_table);
            }
            protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
            {
                GridView1.EditIndex = -1;
                BindData(GridViewTable);
            }
            protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
            {
                DataTable _view_table = GridViewTable;
                GridView1.DataSource = _view_table;
                GridViewRow gridrow = GridView1.Rows[e.RowIndex]; 
                
                string[] Column_Names = new string[_view_table.Columns.Count];
                for (int x = 0; x < _view_table.Columns.Count; x++)
                {
                    Column_Names[x] = _view_table.Columns[x].ToString();
                }
                for (int i = 0; i < Column_Names.Length; i++)
                {
                    _view_table.Rows[gridrow.DataItemIndex][Column_Names[i]] = ((TextBox)(gridrow.Cells[i+2].Controls[0])).Text;
                }

                GridView1.EditIndex = -1;
                BindData(_view_table);

                UpdateChangedItemsArray(_view_table.Rows[gridrow.DataItemIndex], Select_DB.SelectedItem.Text, Select_Table.SelectedItem.Text);

                UpdateCSVFile(Current_Table_Path(), gridrow.DataItemIndex, _view_table);
            }
            private void UpdateChangedItemsArray(DataRow _row, string DB, string Table)
            {
                string[] _array = new string[_row.ItemArray.Count() + 2];
                _array[0] = DB;
                _array[1] = Table;
                for (int i = 0; i < _row.ItemArray.Count(); i++)
                {
                    _array[i + 2] = _row[i].ToString();
                }
                Changed_Items.Add(_array);
            }
            private void UpdateCSVFile(string CSV_Filename, int RowIndex, DataTable new_table)
            {
                DataTable _csv_table = CSV_To_Table("", CSV_Filename);
                for (int i = 0; i < new_table.Rows.Count; i++)
                {
                    if ((int)_csv_table.Rows[i][0] == (int)new_table.Rows[RowIndex][0])
                    {
                        for (int x = 0; x < new_table.Columns.Count; x++)
                        {
                            _csv_table.Rows[i][x] = new_table.Rows[RowIndex][x];
                        }
                    }
                }
                Table_to_CSV(_csv_table, CSV_Filename, true);
            }
            protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
            {
                HttpContext.Current.Response.Write(@"<SCRIPT type=""text/JavaScript"">confirm(""Are you sure you want to delete this row?"")</SCRIPT>");
            }

        #endregion

        #region "Buttons"
            protected void btnSaveDB_Click(object sender, EventArgs e)
        {
            //  Reads every item in Changed_Items, Updates DB server and removes the item at index 0
            while (Changed_Items.Count > 0)
            {
                SQL_Rows_Update();
                if (Changed_Items.Count > 0) { Changed_Items.RemoveAt(0); }
            }
        }

            protected void btnExport_to_CSV_Click(object sender, EventArgs e)
        {
            Export_to_CSV();
        }

            protected void btnRefresh_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(All_DBs_Directory_name)) Directory.Delete(All_DBs_Directory_name, true);
            if (File.Exists(ALL_DBs_n_Tables_FileName)) File.Delete(ALL_DBs_n_Tables_FileName);
            Start();
        }

            protected void btnB_Click(object sender, EventArgs e)
        {

        }
        #endregion
    #endregion

    #region "SQL Related!"
        // BUILD SQL COMMANDS
        private void Build_SQL_Commands(string Server, string DB, string Table, SQL_Command_type comm_type)
        {
            if (DB != "")
            {
                SQL_Connection = string.Format(@"Data Source={0}; Initial Catalog={1}; Integrated Security=Yes;", Server, DB);
            }
            else
            {
                SQL_Connection = string.Format(@"Data Source={0}; Integrated Security=Yes;", Server);
            }
            switch (comm_type)
            {
                case SQL_Command_type.ReadTable:
                    //if (Filter == "" || Filter == null)
                    {
                        SQL_Query = "SELECT * FROM " + Table; 
                    }
                    //else
                    { 
                        StringBuilder sb = new StringBuilder();


                    }

                break;

            
                case SQL_Command_type.Update:
                    string SQL_string_1 = string.Format("UPDATE {0} SET ", Changed_Items[0][1]) ;
                
                    string[,] _row = Row_n_Headers_from_CSV(Changed_Items[0][0], Changed_Items[0][1], Changed_Items[0][2]);
                    string[] values_strings = new string [_row.GetLength(1)-1];
                    for (int i = 1; i < _row.GetLength(1); i++)
                    {
                        values_strings[i-1] = "[" + _row[0, i] + "]='" + _row[1, i] + "'";
                    }
                    string SQL_string_2 = string.Join(",", values_strings);
                 
                    string SQL_string_last = " WHERE [" + _row[0, 0] + "]=" + Changed_Items[0][2] + ";" ;

                    SQL_Query = SQL_string_1 + SQL_string_2 + SQL_string_last;
                    break;
            
                case SQL_Command_type.AddRow:
                    break;
            
                case SQL_Command_type.Delete:
                    break;
            }
        }
    
        private void SQL_Rows_Update()
            {
                
            
                if (Changed_Items.Count > 0) Build_SQL_Commands(_Local_SQL_Server, Changed_Items[0][0], Changed_Items[0][1], SQL_Command_type.Update);
                try 
                {
                    using (SqlConnection _sql_conn = new SqlConnection(SQL_Connection))
                    { 
                        using (SqlCommand _sql_comm = _sql_conn.CreateCommand())
                        {
                            _sql_comm.CommandText = SQL_Query;
                            _sql_conn.Open();
                            _sql_comm.ExecuteNonQuery();
                            _sql_conn.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Response.Write(ex.ToString());
                }

            }

        // Get Headers of the table and the data in the row which is goign to be changed for building SQL Command
        private string[,] Row_n_Headers_from_CSV(string DB, string Table, string row_index)
        {
            string[,] _table = CSV_To_Array2D(All_DBs_Directory_name + @"\" + DB + @"\" + Table + ".csv");
            string[,] row = new string [2, _table.GetLength(1)];
        
            for (int i = 2; i < _table.GetLength(0) ; i++)
            {
                if (_table[i,0] == Changed_Items[0][2]) 
                {
                    for (int x = 0; x < row.GetLength(1); x++) { row[0, x] = _table[1, x]; }
                    for (int x = 0; x < row.GetLength(1); x++) { row[1, x] = _table[i, x]; }
                    return row;
                }
            }
            return null;
        }
    #endregion  

    #region "Misc"
        private bool File_Older_Than_Hours(string Filename, int Hours)
    {
        if (!File.Exists(Filename)) return true;
        if (DateTime.Compare(File.GetCreationTime(Filename).AddHours(Hours), DateTime.Now) < 0)
        {
            return true;
        }
        return false;
    }

        private string Current_Table_Path()
    {
        return All_DBs_Directory_name + @"\" + Select_DB.SelectedItem.Text + @"\" + Select_Table.SelectedItem.Text + ".csv";
    }
    #endregion  
        
        protected void GridView1_DataBound(object sender, EventArgs e)
        {
            DataTable Table = GridView1.DataSource as DataTable;
            TextBox[] FilterBoxs = new TextBox[Table.Columns.Count - 1];
            double width = 0;
            if (Table.Rows.Count > 1)
                for (int i = 0; i < Table.Columns.Count - 1; i++)
                {
                    FilterBoxs[i] = new TextBox();
                    GridView1.Rows[0].Cells[i + 3].Controls.Add(FilterBoxs[i]);
                }
        }
}