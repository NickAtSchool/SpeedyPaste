Imports System.Threading
Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Collections.Specialized

Public Class Form1
    Private Declare Sub mouse_event Lib "user32.dll" (ByVal dwFlags As Integer, ByVal dx As Integer, ByVal dy As Integer, ByVal cButtons As Integer, ByVal dwExtraInfo As IntPtr)
    Private WithEvents kbHook As New KeyboardHook
    Private Shared threadStack As Stack = New Stack()



    Public Shared Function UploadFilesToRemoteUrl(url As String, files As String(), logpath As String, nvc As NameValueCollection)

        Dim length As Long = 0
        Dim boundary As String = "----------------------------" + DateTime.Now.Ticks.ToString("x")

        Dim httpWebRequest2 As HttpWebRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)
        httpWebRequest2.ContentType = Convert.ToString("multipart/form-data; boundary=") & boundary
        httpWebRequest2.Method = "POST"
        httpWebRequest2.KeepAlive = True
        httpWebRequest2.Credentials = System.Net.CredentialCache.DefaultCredentials

        Dim memStream As Stream = New System.IO.MemoryStream()
        Dim boundarybytes As Byte() = System.Text.Encoding.ASCII.GetBytes((Convert.ToString(vbCr & vbLf & "--") & boundary) + vbCr & vbLf)
        Dim formdataTemplate As String = (Convert.ToString(vbCr & vbLf & "--") & boundary) + vbCr & vbLf & "Content-Disposition: form-data; name=""{0}"";" & vbCr & vbLf & vbCr & vbLf & "{1}"

        For Each key As String In nvc.Keys
            Dim formitem As String = String.Format(formdataTemplate, key, nvc(key))
            Dim formitembytes As Byte() = System.Text.Encoding.UTF8.GetBytes(formitem)
            memStream.Write(formitembytes, 0, formitembytes.Length)
        Next


        memStream.Write(boundarybytes, 0, boundarybytes.Length)
        Dim headerTemplate As String = "Content-Disposition: form-data; name=""{0}""; filename=""{1}""" & vbCr & vbLf & " Content-Type: application/octet-stream" & vbCr & vbLf & vbCr & vbLf
        For i As Integer = 0 To files.Length - 1

            'string header = string.Format(headerTemplate, "file" + i, files[i]);
            Dim header As String = String.Format(headerTemplate, "imgToUpload", files(i))
            Dim headerbytes As Byte() = System.Text.Encoding.UTF8.GetBytes(header)
            memStream.Write(headerbytes, 0, headerbytes.Length)
            Dim fileStream As New FileStream(files(i), FileMode.Open, FileAccess.Read)
            Dim buffer As Byte() = New Byte(1023) {}
            Dim bytesRead As Integer = 0
            While (True)
                bytesRead = fileStream.Read(buffer, 0, buffer.Length)
                If (bytesRead <> 0) Then
                    memStream.Write(buffer, 0, bytesRead)
                Else
                    Exit While
                End If
            End While


            memStream.Write(boundarybytes, 0, boundarybytes.Length)


            fileStream.Close()
        Next

        httpWebRequest2.ContentLength = memStream.Length

        Dim requestStream As Stream = httpWebRequest2.GetRequestStream()

        memStream.Position = 0
        Dim tempBuffer As Byte() = New Byte(memStream.Length - 1) {}
        memStream.Read(tempBuffer, 0, tempBuffer.Length)
        memStream.Close()
        requestStream.Write(tempBuffer, 0, tempBuffer.Length)
        requestStream.Close()


        Dim webResponse2 As WebResponse = httpWebRequest2.GetResponse()

        Dim stream2 As Stream = webResponse2.GetResponseStream()
        Dim reader2 As New StreamReader(stream2)
        Dim responseText = reader2.ReadToEnd()
        File.WriteAllText(logpath, responseText)

        'MessageBox.Show(reader2.ReadToEnd())

        webResponse2.Close()
        httpWebRequest2 = Nothing
        webResponse2 = Nothing

        Return responseText
    End Function

    'Private Function uploadImage(ByRef targetImage As Byte())
    '    Dim request As WebRequest = WebRequest.Create("http://unkk1eed8234.nick9321.koding.io/image")
    '    request.Credentials = CredentialCache.DefaultCredentials
    '    CType(request, HttpWebRequest).UserAgent = ".NET Framework Example Client"
    '    request.Method = "POST"
    '    request.ContentLength = targetImage.Length
    '    request.ContentType = "application/x-www-form-urlencoded"
    '    Dim dataStream As Stream = request.GetRequestStream()
    '    dataStream.Write(targetImage, 0, targetImage.Length)
    '    dataStream.Close()
    '    Dim response As WebResponse = request.GetResponse()

    '    Return response
    'End Function
    Private Sub PerformSnipOperation()
        Dim snipImage As Image = SnippingTool.Snip()
        Dim result As Byte()
        Dim converter As New ImageConverter
        Dim ImageByteArr = converter.ConvertTo(snipImage, GetType(Byte()))
        Dim image = System.IO.File.Create("tmpImage.png")
        image.Write(ImageByteArr, 0, ImageByteArr.length)
        image.Close()

        Dim nameCollection As NameValueCollection = New NameValueCollection()
        nameCollection.Add("name", "value")



        Dim Response As String = UploadFilesToRemoteUrl("http://unkk1eed8234.nick9321.koding.io/image", {"tmpImage.png"}, "asdf.html", nameCollection) ' uploadImage(Encoding.UTF8.GetBytes("anus")) 'ImageByteArr)


        'Dim wc As System.Net.WebClient = New System.Net.WebClient()
        'result = wc.UploadData("http://unkk1eed8234.nick9321.koding.io/image", ImageByteArr)
        'Dim str = System.Text.Encoding.Default.GetString(result)

        'Dim reader As New StreamReader(Response.GetResponseStream())
        '' Read the content.
        'Dim responseFromServer As String = reader.ReadToEnd()
        'Dim file = System.IO.File.CreateText("asdf.html")
        'file.Write(responseFromServer)
        'file.Close()
        'RichTextBox1.Text = str

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
