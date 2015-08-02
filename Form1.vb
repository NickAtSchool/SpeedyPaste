Imports System.Threading

Public Class Form1
    Private Declare Sub mouse_event Lib "user32.dll" (ByVal dwFlags As Integer, ByVal dx As Integer, ByVal dy As Integer, ByVal cButtons As Integer, ByVal dwExtraInfo As IntPtr)
    Private WithEvents kbHook As New KeyboardHook
    Private Shared threadStack As Stack = New Stack()

    Private Sub PerformSnipOperation()
        Dim snipImage As Image = SnippingTool.Snip()
        Dim result As Byte()
        Dim converter As New ImageConverter
        Dim ImageByteArr = converter.ConvertTo(snipImage, GetType(Byte()))
        Dim wc As System.Net.WebClient = New System.Net.WebClient()
        result = wc.UploadData("http://filesmelt.com/index.php", ImageByteArr)
        Dim str = System.Text.Encoding.Default.GetString(result)

        Dim file = System.IO.File.CreateText("asdf.txt")
        file.Write(str)
        file.Close()

        threadStack.Pop()
    End Sub

    Private Sub kbHook_KeyDown(ByVal Key As System.Windows.Forms.Keys) Handles kbHook.KeyDown
        ListBox1.Items.Add(Key.ToString())
        ListBox1.SelectedIndex = ListBox1.Items.Count - 1
        If Key = Keys.PrintScreen Then
            Dim toolThread As New Thread(AddressOf PerformSnipOperation)
            toolThread.Start()
            threadStack.Push(toolThread)
        ElseIf Key = Keys.Escape Then
            Try
                Dim mostRecentSnipper As Thread = threadStack.Pop()
                mostRecentSnipper.Abort()
            Catch
                'If the stack is empty, we won't be able to pop. So we ignore the fact that the user pressed enter
            End Try
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SnippingTool.Snip()
    End Sub
End Class
