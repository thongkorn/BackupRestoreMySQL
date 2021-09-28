#Region "ABOUT"
' / --------------------------------------------------------------------
' / Developer : Mr.Surapon Yodsanga (Thongkorn Tubtimkrob)
' / eMail : thongkorn@hotmail.com
' / URL: http://www.g2gnet.com (Khon Kaen - Thailand)
' / Facebook: https://www.facebook.com/g2gnet (For Thailand)
' / Facebook: https://www.facebook.com/commonindy (Worldwide)
' / More Info: http://www.g2gnet.com/webboard
' /
' / Purpose: Backup and Restore MySQL Server.
' / Microsoft Visual Basic .NET (2010) + MySQL Server.
' /
' / This is open source code under @Copyleft by Thongkorn Tubtimkrob.
' / You can modify and/or distribute without to inform the developer.
' / --------------------------------------------------------------------
#End Region

Imports MySql.Data.MySqlClient
Imports System.IO

Public Class frmMySQL
    '// Declare variable one time but use many times.
    Dim Conn As MySqlConnection
    Dim Cmd As MySqlCommand
    Dim DR As MySqlDataReader
    Dim DA As MySqlDataAdapter
    Dim strSQL As String
    '//
    Dim MyPathBackup As String = GetPath(Application.StartupPath + "Backup")
    ' / --------------------------------------------------------------------------------
    '// MySQL Server DBMS Connection Test with VB.NET (2010).
    Public Function ConnectMySQL(ByVal SERVER As String, ByVal UID As String, PWD As String, ByRef cmb As ComboBox, Optional ByVal DB As String = "") As Boolean
        Dim strCon As String = String.Empty
        '// Connect DataBase.
        If DB = "" Then
            strCon = _
                " Server=" & SERVER & "; " & _
                " User ID=" & UID & "; " & _
                " Password=" & PWD & "; " & _
                " Port=3306; " & _
                " CharSet=utf8; " & _
                " Connect Timeout=120000; " & _
                " Pooling = True; " & _
                " Persist Security Info=True; " & _
                " Connection Reset=False; "
        Else
            strCon = _
                " Server=" & SERVER & "; " & _
                " User ID=" & UID & "; " & _
                " Database=" & DB & "; " & _
                " Password=" & PWD & "; " & _
                " Port=3306; " & _
                " CharSet=utf8; " & _
                " Connect Timeout=120000; " & _
                " Pooling = True; " & _
                " Persist Security Info=True; " & _
                " Connection Reset=False; "
        End If
        Try
            Conn = New MySqlConnection
            Conn.ConnectionString = strCon
            '// Connect MySQL Server and Listing All DataBase.
            If DB = "" Then
                Conn.Open()
                Cmd = New MySqlCommand("SHOW DATABASES", Conn)
                DR = Cmd.ExecuteReader()
                If DR.HasRows Then
                    cmb.Items.Clear()
                    While DR.Read()
                        cmb.Items.Add(DR("Database").ToString)
                    End While
                    cmb.SelectedIndex = 0
                End If
                DR.Close()
                Cmd.Dispose()
                Conn.Close()
                Conn.Dispose()
                MessageBox.Show("Connect MySQL Server Complete.", "Report Status", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return True

                '//
            Else
                If Conn.State = ConnectionState.Open Then Conn.Close()
                Conn.Dispose()
                Return True
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return False
        End Try
    End Function

    '// START HERE.
    Private Sub frmMySQL_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Try
            '// Create Backup Folder if doesn't exist.
            If (Not System.IO.Directory.Exists(GetPath(Application.StartupPath) & "Backup")) Then System.IO.Directory.CreateDirectory(GetPath(Application.StartupPath) & "Backup")
            btnConnect.Enabled = True
            btnBackup.Enabled = False
            btnRestore.Enabled = False
            cmbDataBase.Enabled = False
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    '// Connect to MySQL Server and List all Databases.
    Private Sub btnConnect_Click(sender As System.Object, e As System.EventArgs) Handles btnConnect.Click
        Try
            '// Parameters: SERVER_NAME, DB_USERNAME, DB_PASSWORD, ComboBox Control Name, DB_NAME (Default = "")
            If Not ConnectMySQL(txtServer.Text.Trim, txtDBUserName.Text.Trim, txtDBPassword.Text.Trim, cmbDataBase) Then Return
            btnConnect.Enabled = False
            btnBackup.Enabled = True
            btnRestore.Enabled = True
            cmbDataBase.Enabled = True
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    '// Backup DataBase.
    Private Sub btnBackup_Click(sender As System.Object, e As System.EventArgs) Handles btnBackup.Click
        Dim DBFile As String = String.Empty
        Dim dlgSaveFile = New SaveFileDialog
        Try
            With dlgSaveFile
                .Title = "Save MySQL DataBase"
                .InitialDirectory = MyPathBackup
                .Filter = "SQL Dump File (*.sql)|*.sql"
                '// Filename: DBName-ddMMyyyy-HHmmss 
                '// Ex. dbfood-28092564-104812.sql
                .FileName = cmbDataBase.Text + "-" + DateTime.Now.ToString("ddMMyyyy-HHmmss") + ".sql"
                .RestoreDirectory = True
            End With
            If dlgSaveFile.ShowDialog = DialogResult.OK Then
                '// Parameters: SERVER_NAME, DB_USERNAME, DB_PASSWORD, ComboBox Control Name, DB_NAME
                If ConnectMySQL(txtServer.Text.Trim, txtDBUserName.Text.Trim, txtDBPassword.Text.Trim, cmbDataBase, cmbDataBase.Text) Then
                    DBFile = dlgSaveFile.FileName
                    Using Cmd = New MySqlCommand()
                        Using mb As MySqlBackup = New MySqlBackup(Cmd)
                            Cmd.Connection = Conn
                            Conn.Open()
                            mb.ExportToFile(DBFile)
                            Conn.Close()
                        End Using
                    End Using
                    MessageBox.Show("Backup Your MySQL DataBase Successfully!", "Backup MySQL DataBase", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    '// Restore DataBase.
    Private Sub btnRestore_Click(sender As System.Object, e As System.EventArgs) Handles btnRestore.Click
        Dim DBFile As String = String.Empty
        Dim dlgOpenFile = New OpenFileDialog
        Try
            With dlgOpenFile
                .Title = "Open MySQL DataBase"
                .InitialDirectory = MyPathBackup
                .Filter = "SQL Dump File (*.sql)|*.sql"
                .RestoreDirectory = True
            End With
            If dlgOpenFile.ShowDialog = DialogResult.OK Then
                Dim Result As Byte = MessageBox.Show("Are you sure to RESTORE into DataBase Name: " & cmbDataBase.Text & "?", "Confirm to Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                If Result = DialogResult.Yes Then
                    '// Parameters: SERVER_NAME, DB_USERNAME, DB_PASSWORD, ComboBox Control, DB_NAME (Optional)
                    If ConnectMySQL(txtServer.Text.Trim, txtDBUserName.Text.Trim, txtDBPassword.Text.Trim, cmbDataBase, cmbDataBase.Text) Then
                        DBFile = dlgOpenFile.FileName
                        Using Cmd = New MySqlCommand()
                            Using mb As MySqlBackup = New MySqlBackup(Cmd)
                                Cmd.Connection = Conn
                                Conn.Open()
                                mb.ImportFromFile(DBFile)
                                Conn.Close()
                            End Using
                        End Using
                        MessageBox.Show("Restore your MySQL DataBase Successfully!", "Restore MySQL DataBase", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    ' / --------------------------------------------------------------------------------
    ' / Get my project path
    Function GetPath(AppPath As String) As String
        '/ MessageBox.Show(AppPath);
        '/ Return Value
        GetPath = AppPath.ToLower.Replace("\bin\debug", "\").Replace("\bin\release", "\").Replace("\bin\x86\debug", "\")
        '// If not found folder then put the \ (BackSlash ASCII Code = 92) at the end.
        If Microsoft.VisualBasic.Right(GetPath, 1) <> Chr(92) Then GetPath = GetPath & Chr(92)
    End Function

    Private Sub btnExit_Click(sender As System.Object, e As System.EventArgs) Handles btnExit.Click
        Me.Close()
    End Sub

    Private Sub frmMySQL_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        Me.Dispose()
        GC.SuppressFinalize(Me)
        Application.Exit()
    End Sub
End Class
