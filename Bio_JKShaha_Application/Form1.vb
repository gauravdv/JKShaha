Imports System.Data.SqlClient
Imports System.IO
Imports MySql.Data.MySqlClient

Public Class Form1

    Dim _TxtCountLimit As String

    Dim DatabaseName As String = "db_jksc_andheri"
    Dim server As String = "79.143.188.228"
    Dim userName As String = "root"
    Dim password As String = "JK_db)sh@h1("
    Dim test As String = ""

    Private Declare Function device_connect Lib "SL_FRONTIER.dll" (ByVal i As String, ByVal j As Integer) As Integer
    Private Declare Function device_close Lib "SL_FRONTIER.dll" () As Integer
    Private Declare Function get_cardlist Lib "SL_FRONTIER.dll" () As String
    Private Declare Function get_fingerdata Lib "SL_FRONTIER.dll" (ByVal STR As String) As String
    Private Declare Function delete_finger_data Lib "SL_FRONTIER.dll" (ByVal STR As String) As Integer ' Delete

    Dim MysqlConn As New MySqlConnection

    Dim _DeviceNumber As String
    Dim _BatchNumber As String
    Dim list_DeviceIp As New List(Of String) 'Get All ip
    Dim list_DeviceInfo As New List(Of Tuple(Of String, String, String))

    Dim List_BatchId As New List(Of String)()
    Dim List_StudentId As New List(Of String)()

    Dim List_StudentAllId As New List(Of String)()
    Dim Student_100 As New List(Of String)()

    Dim BatchId As String

    Dim SQLTable_BatchId As New Data.DataTable '---datatable

    'Connet To Database
    Public Sub DataBase_connect()

        If Not MysqlConn Is Nothing Then MysqlConn.Close()
        MysqlConn.ConnectionString = String.Format("server={0}; user id={1}; password={2}; database={3}; pooling=false; default command timeout=0", server, userName, password, DatabaseName)
        Try
            MysqlConn.Open()
            'MsgBox("Connected")
            MysqlConn.Close()
            Read_txtFile()
        Catch ex As Exception
            MsgBox("DataBase Connection Errors Found Contact To The Naresh - Data Voice" + ex.Message)
            Application.Exit()
            End
        End Try

    End Sub

    'Read Txt File
    Public Sub Read_txtFile()
        Dim txtFile_Name As String = "C:\JkshahDevice\devices.txt"
        Dim TextLine As String
        Dim ipArray() As String
        Dim _DeviceIp As String

        ' Dim list_DeviceInfo As New List(Of Dictionary(Of String, String))()

        If System.IO.File.Exists(txtFile_Name) = True Then

            Dim objReader As New System.IO.StreamReader(txtFile_Name)

            Do While objReader.Peek() <> -1
                TextLine = objReader.ReadLine()

                If Not TextLine = "" Then
                    Dim Device_ip As String() = Split(TextLine, " ")
                    ipArray = Split(TextLine, " ")
                    _DeviceIp = ipArray(0)
                    _DeviceNumber = ipArray(1)

                    list_DeviceIp.Add(_DeviceIp.ToString()) ' Get All ip
                    'list_DeviceInfo.Add(New Dictionary(Of String, String)() From {{_DeviceIp.ToString(), _DeviceNumber.ToString()}})
                    list_DeviceInfo.Add(New Tuple(Of String, String, String)(_DeviceIp.ToString(), _DeviceNumber.ToString(), "")) ' Get All Ip With Device No
                    'MsgBox(vbCrLf & "Find Device =  " + _DeviceIp)
                Else
                End If
            Loop
            objReader.Close()
            'Console.ReadKey(True)
        Else
            MsgBox(vbCrLf & "File Does Not Exist Contact To The Naresh - Data Voice")
            Application.Exit()
            End
        End If
        'Console.Write(vbCrLf & "Press any key to continue...")
        'Console.ReadKey(True)

        If list_DeviceIp IsNot Nothing AndAlso list_DeviceIp.Count = 0 Then
        Else
            getAllStudentId()
        End If
    End Sub

    'get All Student Id
    Public Sub getAllStudentId()
        Dim Sql As String
        Dim dbcomm As MySqlCommand
        Dim dbread As MySqlDataReader

        Dim SQLda As New MySqlDataAdapter
        Dim CurrentDate As String = System.DateTime.Now.ToString("yyyy/MM/dd")

        Dim txtFile_Name As String = "C:\JkshahDevice\DeleteRecord.txt"
        Dim TextLine As String
        Dim ipArray() As String
        Dim _TxtDate As String

        If Not MysqlConn Is Nothing Then MysqlConn.Close()
        MysqlConn.Open()
        If Not _DeviceNumber = "" Then

            If System.IO.File.Exists(txtFile_Name) = True Then

                Dim objReader As New System.IO.StreamReader(txtFile_Name)

                Do While objReader.Peek() <> -1
                    TextLine = objReader.ReadLine()

                    'If Not TextLine = "" Then
                    Dim Device_ip As String() = Split(TextLine, " ")
                    ipArray = Split(TextLine, " ")
                    _TxtDate = ipArray(0)

                    If _TxtDate = CurrentDate Then
                        _TxtCountLimit = ipArray(1)
                    Else
                        _TxtCountLimit = 0
                    End If
                    'Else
                    'End If
                Loop
                'Console.ReadKey(True)
                objReader.Close()
            Else
                MsgBox(vbCrLf & "File Does Not Exist")
            End If


            Sql = "SELECT studentId
                        FROM tblBatchMst a, tblStudentBatchDtls b, tbl_branch_device c
                        WHERE fldc_deleted =  'N'
                        AND a.batchId = b.batchId
                        AND a.completionDate <  '" + CurrentDate + "'
                        AND a.branchId = fldi_branch_id
                        AND c.fldi_id =  '" + _DeviceNumber + "' 
                        AND studentId 
                        NOT IN (
                    SELECT studentId
                        FROM tblBatchMst a, tblStudentBatchDtls b, tbl_branch_device c
                        WHERE
                        a.batchId = b.batchId
                        AND a.branchId = fldi_branch_id
                        AND c.fldi_id =  '" + _DeviceNumber + "' 
                        AND a.completionDate >  '" + CurrentDate + "'
                        )LIMIT  " + _TxtCountLimit + ", 50000 "

            'Sql = "SELECT studentId
            '            FROM tblBatchMst a, tblStudentBatchDtls b, tbl_branch_device c
            '            WHERE fldc_deleted =  'N'
            '            AND a.batchId = b.batchId
            '            AND completionDate < '" + CurrentDate + "'
            '            AND a.branchId = fldi_branch_id
            '            AND c.fldi_id =   '" + _DeviceNumber + "' "

            Try

                dbcomm = New MySqlCommand(Sql, MysqlConn)
                dbread = dbcomm.ExecuteReader()
                If (dbread.HasRows()) Then
                    While dbread.Read()
                        List_StudentAllId.Add(dbread("studentId").ToString())
                        'MessageBox.Show("Get All Student ID")
                    End While
                Else
                    'MsgBox("Device Number Not Found!")
                End If
                dbread.Close()

            Catch ex As Exception
                MsgBox("Error in collecting data from Database. Contact To The Naresh - Data Voice Error is :" & ex.Message)
                Application.Exit()
                End
            End Try
            MysqlConn.Close()
        End If

    End Sub

    'Connect To Device
    Public Sub Connect_Device()
        Dim j As Integer
        Dim i As Integer = 0
        Dim Count_Device As Integer = list_DeviceInfo.Count - 1
        Dim arrayDevices(Count_Device) As String

        If Not (List_StudentAllId Is Nothing) Then
            For Each value As Tuple(Of String, String, String) In list_DeviceInfo
                Dim _DeviceIp As String = value.Item1
                Dim _DeviceNo As String = value.Item2
                Dim _Port As Integer = 1085

                Try
                    j = device_connect(_DeviceIp, _Port)

                    If (j = 0) Then
                        'Rtxt_.Text = vbCrLf & "Device Connect Sucessfully " + _DeviceIp
                        'MsgBox(vbCrLf & "Device Connect Sucessfully " + _DeviceIp)
                        arrayDevices(i) = "Connect".ToString()
                        device_close()
                    Else
                        'Rtxt_.Text = vbCrLf & "Device Can't Connect Sucessfully" + _DeviceIp
                        arrayDevices(i) = "NotConnect".ToString()
                        'MsgBox("Device Can't Connect Sucessfully" + _DeviceIp + " Contact to Naresh - DataVoice")
                        device_close()
                    End If

                Catch ex As Exception
                    arrayDevices(i) = "NotConnect".ToString()
                    MsgBox("Contact To The Naresh - Data Voice, Device Can't Connect, Wrong IP address format" + vbCrLf + ex.Message, MsgBoxStyle.Information)
                    Application.Exit()
                    End
                End Try

                i += 1
            Next


            'Check All Devices Are Connect Or Not
            If arrayDevices.Contains("NotConnect") Then
                MsgBox(vbCrLf & "All Devices Are Not Connected, Contact To The Naresh - Data Voice")
                Application.Exit()
                End
            Else
                'MsgBox(vbCrLf & "All Devices Are Connected")
                Delete_StudentDetail()
                'get_BranchId()
            End If

        Else
            MsgBox("Not Batch or Student to Delete")
        End If

        'Console.Write(vbCrLf & "Press any key to continue...")
        'Console.ReadKey(True)
    End Sub

    'Delete Student Detail
    Public Sub Delete_StudentDetail()
        Dim CurrentDate As String = System.DateTime.Now.ToString("yyyy/MM/dd")
        Dim strFile As String = "C:\JkshahDevice\DeleteRecord.txt"
        Dim _CountSundet As Integer
        _CountSundet = _TxtCountLimit
        Dim j As Integer
        Dim i As Integer
        Dim ii As Integer
        Dim _Start As Integer = 0
        Dim _End As Integer = 100
        Dim Count_Device As Integer = (List_StudentAllId.Count - 1) / 100

        For i = 0 To Count_Device
            Student_100.Clear()
            For ii = _Start To _End - 1
                Student_100.Add(List_StudentAllId(ii).ToString())
            Next

            For Each value As Tuple(Of String, String, String) In list_DeviceInfo
                Dim _DeviceIp As String = value.Item1
                Dim _DeviceNo As String = value.Item2
                Dim _Port As Integer = 1085

                Try
                    j = device_connect(_DeviceIp, _Port)
                    If (j = 0) Then

                    End If
                Catch ex As Exception
                    MsgBox("Wrong IP address format" + vbCrLf + ex.Message, MsgBoxStyle.Information)
                End Try

                For Each value_StudentId As String In Student_100
                    Dim Each_Student As String = value_StudentId
                    Dim k As Integer
                    Dim Card As String

                    Each_Student = Each_Student.Remove(0, 1)

                    Try
                        k = delete_finger_data(Each_Student)     ' card number must be with eight digit

                        If k = 0 Then
                            'Rtxt_.Text = "Delete in Process.........."
                            'MsgBox("Delete in Process..........")
                        Else
                            MsgBox("Check time Format, Not Changed Successfully or disconnect device, Contact to Naresh : Data Voice")
                            Application.Exit()
                            End
                        End If
                    Catch ex As Exception
                        device_close()
                        AllFunction()
                        'If System.IO.File.Exists(strFile) = True Then
                        '    Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                        '    Dim objWriter As New System.IO.StreamWriter(strFile)
                        '    objWriter.Write(WriteLine)
                        '    objWriter.Close()
                        '    AllFunction()
                        'End If
                    End Try

                    Try
                        Card = String.Format("{0:00000000}", Convert.ToInt32(Each_Student.Trim()))
                    Catch ex As Exception
                        Card = Each_Student.Trim()
                    End Try

                    Try
                        If (Trim(Card) <> "") Then
                            k = delete_finger_data(Card)     ' card number must be with eight digit
                            If k = 0 Then
                                'Rtxt_.Text = "Delete in Process.........."
                                'MsgBox("Delete in Process..........")
                            Else
                                'MsgBox("Check time Format, Not Changed Successfully")
                            End If
                        End If
                    Catch ex As Exception
                        device_close()
                        AllFunction()
                        'If System.IO.File.Exists(strFile) = True Then
                        '    Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                        '    Dim objWriter As New System.IO.StreamWriter(strFile)
                        '    objWriter.Write(WriteLine)
                        '    objWriter.Close()
                        '    AllFunction()
                        'End If
                    End Try
                Next

            Next
            _CountSundet += 100
            If System.IO.File.Exists(strFile) = True Then
                Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                Dim objWriter As New System.IO.StreamWriter(strFile)
                objWriter.Write(WriteLine)
                objWriter.Close()
            End If
            _Start = (_Start + 100)
            _End = _End + 100
        Next

        MsgBox("StudentId Deleted successfully From All the Devices")


    End Sub

    Public Sub Delete_StudentDetail3()
        Dim CurrentDate As String = System.DateTime.Now.ToString("yyyy/MM/dd")
        Dim strFile As String = "C:\JkshahDevice\DeleteRecord.txt"

        Dim j As Integer
        Dim i As Integer = 0
        Dim Count_Device As Integer = list_DeviceInfo.Count - 1
        Dim arrayDevices(Count_Device) As String
        Dim _CountSundet As Integer
        _CountSundet = _TxtCountLimit

        For Each value_StudentId As String In List_StudentAllId

            Dim Each_Student As String = value_StudentId
            Dim k As Integer
            Dim Card As String


            Each_Student = Each_Student.Remove(0, 1)

            For Each value As Tuple(Of String, String, String) In list_DeviceInfo
                Dim _DeviceIp As String = value.Item1
                Dim _DeviceNo As String = value.Item2
                Dim _Port As Integer = 1085

                Try
                    j = device_connect(_DeviceIp, _Port)
                    If (j = 0) Then

                        Try
                            k = delete_finger_data(Each_Student)     ' card number must be with eight digit

                            If k = 0 Then
                                'Rtxt_.Text = "Delete in Process.........."
                                'MsgBox("Delete in Process..........")
                            Else
                                'MsgBox("Check time Format, Not Changed Successfully")
                            End If
                        Catch ex As Exception
                            device_close()
                            If System.IO.File.Exists(strFile) = True Then
                                Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                                Dim objWriter As New System.IO.StreamWriter(strFile)
                                objWriter.Write(WriteLine)
                                objWriter.Close()
                                AllFunction()
                            End If
                        End Try

                        Try
                            Card = String.Format("{0:00000000}", Convert.ToInt32(Each_Student.Trim()))
                        Catch ex As Exception
                            Card = Each_Student.Trim()
                        End Try

                        Try
                            If (Trim(Card) <> "") Then
                                k = delete_finger_data(Card)     ' card number must be with eight digit
                                If k = 0 Then
                                    'Rtxt_.Text = "Delete in Process.........."
                                    'MsgBox("Delete in Process..........")
                                Else
                                    'MsgBox("Check time Format, Not Changed Successfully")
                                End If
                            End If
                        Catch ex As Exception
                            device_close()
                            If System.IO.File.Exists(strFile) = True Then
                                Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                                Dim objWriter As New System.IO.StreamWriter(strFile)
                                objWriter.Write(WriteLine)
                                objWriter.Close()
                                AllFunction()
                            End If
                        End Try

                    End If
                Catch ex As Exception
                    MsgBox("Wrong IP address format" + vbCrLf + ex.Message, MsgBoxStyle.Information)
                End Try
            Next
            If System.IO.File.Exists(strFile) = True Then
                Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                Dim objWriter As New System.IO.StreamWriter(strFile)
                objWriter.Write(WriteLine)
                objWriter.Close()
            End If

            _CountSundet += 1
        Next

        MsgBox("StudentId Deleted successfully From All the Devices")


    End Sub

    Public Sub Delete_StudentDetail2()
        Dim CurrentDate As String = System.DateTime.Now.ToString("yyyy/MM/dd")

        Dim j As Integer
        Dim i As Integer = 0
        Dim Count_Device As Integer = list_DeviceInfo.Count - 1
        Dim arrayDevices(Count_Device) As String

        'MysqlConn.Open()
        'Dim Sql As String
        'Dim dbcomm As MySqlCommand

        'Dim Batch_Id As String = 2
        'Sql = "UPDATE tblBatchMst SET fldc_deleted =  'Y' WHERE batchId = '" + Batch_Id + "' "
        'Try
        '    MysqlConn.Open()
        '    dbcomm = New MySqlCommand(Sql, MysqlConn)
        '    dbcomm.ExecuteNonQuery()
        'Catch ex As Exception
        '    MsgBox(ex.Message)
        'End Try
        'MysqlConn.Close()


        For Each value As Tuple(Of String, String, String) In list_DeviceInfo
            Dim _DeviceIp As String = value.Item1
            Dim _DeviceNo As String = value.Item2
            Dim _Port As Integer = 1085
            Dim _CountSundet As Integer
            _CountSundet = _TxtCountLimit
            '_CountSundet = 0
            Dim strFile As String = "C:\JkshahDevice\DeleteRecord.txt"

            Try
                j = device_connect(_DeviceIp, _Port)

                If (j = 0) Then
                    'Rtxt_.Text = vbCrLf & "Device Connect Sucessfully " + _DeviceIp
                    'MsgBox(vbCrLf & "Device Connect Sucessfully " + _DeviceIp)
                    arrayDevices(i) = "Connect".ToString()


                    'Delete Student into Devices
                    For Each value_StudentId As String In List_StudentAllId
                        Dim Each_Student As String = value_StudentId
                        'Dim Each_Student As String = "S32917"
                        Dim k As Integer
                        Dim Card As String
                        'If Each_Student = "S194711" Then
                        '    MsgBox("Got it")
                        'End If

                        Each_Student = Each_Student.Remove(0, 1)

                        Try
                            k = delete_finger_data(Each_Student)     ' card number must be with eight digit

                            If k = 0 Then
                                'Rtxt_.Text = "Delete in Process.........."
                                'MsgBox("Delete in Process..........")
                            Else
                                'MsgBox("Check time Format, Not Changed Successfully")
                            End If
                        Catch ex As Exception
                            device_close()
                            If System.IO.File.Exists(strFile) = True Then
                                Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                                Dim objWriter As New System.IO.StreamWriter(strFile)
                                objWriter.Write(WriteLine)
                                objWriter.Close()
                                AllFunction()
                            End If
                        End Try


                        Try
                            Card = String.Format("{0:00000000}", Convert.ToInt32(Each_Student.Trim()))
                        Catch ex As Exception
                            Card = Each_Student.Trim()
                        End Try

                        Try
                            If (Trim(Card) <> "") Then
                                k = delete_finger_data(Card)     ' card number must be with eight digit

                                If k = 0 Then
                                    'Rtxt_.Text = "Delete in Process.........."
                                    'MsgBox("Delete in Process..........")
                                Else
                                    'MsgBox("Check time Format, Not Changed Successfully")

                                End If

                            End If
                        Catch ex As Exception
                            device_close()
                            If System.IO.File.Exists(strFile) = True Then
                                Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                                Dim objWriter As New System.IO.StreamWriter(strFile)
                                objWriter.Write(WriteLine)
                                objWriter.Close()
                                AllFunction()
                            End If
                        End Try

                        If System.IO.File.Exists(strFile) = True Then
                            Dim WriteLine As String = CurrentDate + " " + CStr(_CountSundet)
                            Dim objWriter As New System.IO.StreamWriter(strFile)
                            objWriter.Write(WriteLine)
                            objWriter.Close()
                        End If

                        _CountSundet += 1
                    Next

                    device_close()
                Else

                    arrayDevices(i) = "NotConnect".ToString()
                    'Rtxt_.Text = vbCrLf & "Device Can't Connect Sucessfully " + _DeviceIp
                    MsgBox("Device Can't Connect Sucessfully" + _DeviceIp + " Contact to Naresh - DataVoice")

                End If

            Catch ex As Exception
                MsgBox("Wrong IP address format" + vbCrLf + ex.Message, MsgBoxStyle.Information)
            End Try

            i += 1
        Next

        'Rtxt_.Text = vbCrLf & "StudentId Deleted successfully From All the Devices "
        MsgBox("StudentId Deleted successfully From All the Devices")
        'MysqlConn.Open()
        'Dim Sql As String
        'Dim dbcomm As MySqlCommand

        'Sql = "UPDATE tblBatchMst SET fldc_deleted =  'Y' WHERE batchId = '" + Batch_Id + "' "
        'Try
        '    MysqlConn.Open()
        '    dbcomm = New MySqlCommand(Sql, MysqlConn)
        '    dbcomm.ExecuteNonQuery()
        'Catch ex As Exception
        '    MsgBox(ex.Message)
        'End Try
        'MysqlConn.Close()

    End Sub

    '''''''Old *****************************************************************************
    '''''''get Branch Batch to deleted
    ''''''Public Sub get_BranchId()
    ''''''    Dim Sql As String
    ''''''    Dim dbcomm As MySqlCommand

    ''''''    Dim SQLda As New MySqlDataAdapter
    ''''''    Dim CurrentDate As String = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
    ''''''    Dim dbread As MySqlDataReader

    ''''''    'If Not MysqlConn Is Nothing Then MysqlConn.Close()
    ''''''    If Not _DeviceNumber = "" Then
    ''''''        Sql = "SELECT a.batchId As BatchNumber
    ''''''                    FROM tblBatchMst a, tbl_branch_device c
    ''''''                    WHERE fldc_deleted =  'N' AND completionDate < '" + CurrentDate + "' AND a.branchId = fldi_branch_id
    ''''''                            AND c.fldi_id =  '" + _DeviceNumber + "' "
    ''''''        Try

    ''''''            'MysqlConn.Open()
    ''''''            dbcomm = New MySqlCommand(Sql, MysqlConn)
    ''''''            'SQLda = New MySqlDataAdapter(Sql, conn) '-- datatable
    ''''''            'SQLda.Fill(SQLTable_BatchId)'-----datatable
    ''''''            dbread = dbcomm.ExecuteReader()
    ''''''            If (dbread.HasRows()) Then
    ''''''                While dbread.Read()
    ''''''                    List_BatchId.Add(dbread("BatchNumber").ToString())
    ''''''                End While
    ''''''            Else
    ''''''                'MsgBox("Device Number No t Found!")
    ''''''            End If
    ''''''            dbread.Close()

    ''''''        Catch ex As Exception
    ''''''            MsgBox("Error in collecting data from Database. Error is :" & ex.Message)
    ''''''        End Try
    ''''''        'MysqlConn.Close()

    ''''''    End If
    ''''''End Sub

    '''''''get Student id
    ''''''Public Sub get_StudentId()
    ''''''    Dim dbread1 As MySqlDataReader
    ''''''    Dim Sql As String
    ''''''    Dim dbcomm1 As MySqlCommand
    ''''''    Dim DatabaseName As String = "db_jksc_andheri"
    ''''''    Dim server As String = "79.143.188.228"
    ''''''    Dim userName As String = "root"
    ''''''    Dim password As String = "JK_db)sh@h1("

    ''''''    Dim StudentId As String


    ''''''    For Each value As String In List_BatchId
    ''''''        BatchId = value
    ''''''        'BatchId = 32
    ''''''        'If Not conn Is Nothing Then conn.Close()
    ''''''        'MysqlConn.Open()
    ''''''        Sql = "SELECT studentId FROM tblStudentBatchDtls WHERE batchId = '" + BatchId + "'"
    ''''''        Try

    ''''''            'MySQl_Conn2.Open()
    ''''''            dbcomm1 = New MySqlCommand(Sql, MysqlConn)
    ''''''            dbread1 = dbcomm1.ExecuteReader()
    ''''''            Dim s As String
    ''''''            If (dbread1.HasRows()) Then
    ''''''                Do While dbread1.Read
    ''''''                    ' get the data here
    ''''''                    StudentId = dbread1("studentId")
    ''''''                    'List_StudentId.Add(dbread1("BatchNumber").ToString())
    ''''''                    'DeleteCardNo(dbread("studentId"))
    ''''''                Loop
    ''''''            Else
    ''''''                'MsgBox("No Students Found!")
    ''''''            End If
    ''''''            dbread1.Close()
    ''''''        Catch ex As Exception
    ''''''            MsgBox("Error in collecting data from Database. Error is :" & ex.Message)
    ''''''        End Try
    ''''''        'MysqlConn.Close()

    ''''''    Next
    ''''''End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Application.Exit()
        'End
        AllFunction()
    End Sub

    Private Sub AllFunction()
        DataBase_connect()
        'Read_txtFile()
        Connect_Device()
    End Sub

End Class
