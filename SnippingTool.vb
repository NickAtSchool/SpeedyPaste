

'SnippingTool Code: Place this in a new form (set the StartUp Property to Manual)'
Public Class SnippingTool
    Private Shared _Screen As Screen

    Private Shared BitmapSize As Size

    Private Shared Graph As Graphics

    Private Structure MultiScreenSize
        Dim minX As Integer
        Dim minY As Integer
        Dim maxRight As Integer
        Dim maxBottom As Integer
    End Structure


    Private Shared Function FindMultiScreenSize() As MultiScreenSize

        Dim minX As Integer = Screen.AllScreens(0).Bounds.X
        Dim minY As Integer = Screen.AllScreens(0).Bounds.Y

        Dim maxRight As Integer = Screen.AllScreens(0).Bounds.Right
        Dim maxBottom As Integer = Screen.AllScreens(0).Bounds.Bottom

        For Each aScreen As Screen In Screen.AllScreens
            If aScreen.Bounds.X < minX Then
                minX = aScreen.Bounds.X
            End If

            If aScreen.Bounds.Y < minY Then
                minY = aScreen.Bounds.Y
            End If

            If aScreen.Bounds.Right > maxRight Then
                maxRight = aScreen.Bounds.Right
            End If

            If aScreen.Bounds.Bottom > maxBottom Then
                maxBottom = aScreen.Bounds.Bottom
            End If
        Next
        Dim m_MultiScreenSize As MultiScreenSize
        With m_MultiScreenSize
            .minX = minX
            .minY = minY
            .maxBottom = maxBottom
            .maxRight = maxRight
        End With
        Return m_MultiScreenSize
    End Function

    Public Shared Function Snip() As Image
        Dim m_MultiScreenSize As MultiScreenSize = FindMultiScreenSize()

        Dim bmp As New Bitmap(m_MultiScreenSize.maxRight - m_MultiScreenSize.minX, _
                              m_MultiScreenSize.maxBottom - m_MultiScreenSize.minY, _
                              System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
        Dim gr As Graphics = Graphics.FromImage(bmp)

        Graph = gr


        gr.SmoothingMode = Drawing2D.SmoothingMode.None
        BitmapSize = bmp.Size

        Using snipper = New SnippingTool(bmp)
            snipper.Location = New Point(m_MultiScreenSize.minX, m_MultiScreenSize.minY)
            'snipper.Show()
            'snipper.Focus()
            'snipper.TopMost = True
            If snipper.ShowDialog() = DialogResult.OK Then
                Return snipper.Image
                'System.Threading.Thread.Sleep(1000)
            End If
        End Using

        Return Nothing
    End Function


    Public Sub New(ByVal screenShot As Image)
        InitializeComponent()
        Me.BackgroundImage = screenShot
        Me.ShowInTaskbar = False
        Me.FormBorderStyle = FormBorderStyle.None

        Me.DoubleBuffered = True
    End Sub
    Public Property Image() As Image
        Get
            Return m_Image
        End Get
        Set(ByVal value As Image)
            m_Image = Value
        End Set
    End Property
    Private m_Image As Image


    Private rcSelect As New Rectangle()
    Private pntStart As Point

    Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
        ' Start the snip on mouse down'
        If e.Button <> MouseButtons.Left Then
            Return
        End If
        pntStart = e.Location
        rcSelect = New Rectangle(e.Location, New Size(0, 0))
        Me.Invalidate()
    End Sub
    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        ' Modify the selection on mouse move'
        If e.Button <> MouseButtons.Left Then
            Return
        End If
        Dim x1 As Integer = Math.Min(e.X, pntStart.X)
        Dim y1 As Integer = Math.Min(e.Y, pntStart.Y)
        Dim x2 As Integer = Math.Max(e.X, pntStart.X)
        Dim y2 As Integer = Math.Max(e.Y, pntStart.Y)
        rcSelect = New Rectangle(x1, y1, x2 - x1, y2 - y1)
        Me.Invalidate()
    End Sub


    Protected Overrides Sub OnMouseUp(ByVal e As MouseEventArgs)
        ' Complete the snip on mouse-up'
        If rcSelect.Width <= 0 OrElse rcSelect.Height <= 0 Then
            Return
        End If
        Image = New Bitmap(rcSelect.Width, rcSelect.Height)
        Using gr As Graphics = Graphics.FromImage(Image)
            gr.DrawImage(Me.BackgroundImage, New Rectangle(0, 0, Image.Width, Image.Height), _
                         rcSelect, GraphicsUnit.Pixel)
        End Using
        DialogResult = DialogResult.OK
    End Sub
    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        ' Draw the current selection'
        Using br As Brush = New SolidBrush(Color.FromArgb(200, Color.White))
            Dim x1 As Integer = rcSelect.X
            Dim x2 As Integer = rcSelect.X + rcSelect.Width
            Dim y1 As Integer = rcSelect.Y
            Dim y2 As Integer = rcSelect.Y + rcSelect.Height
            e.Graphics.FillRectangle(br, New Rectangle(0, 0, x1, Me.Height))
            e.Graphics.FillRectangle(br, New Rectangle(x2, 0, Me.Width - x2, Me.Height))
            e.Graphics.FillRectangle(br, New Rectangle(x1, 0, x2 - x1, y1))
            e.Graphics.FillRectangle(br, New Rectangle(x1, y2, x2 - x1, Me.Height - y2))
        End Using
        Using pen As New Pen(Color.LightGreen, 3)
            e.Graphics.DrawRectangle(pen, rcSelect)
        End Using
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        ' Allow canceling the snip with the Escape key'
        If keyData = Keys.Escape Then
            Me.DialogResult = DialogResult.Cancel
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        MyBase.OnLoad(e)
        Dim m_MultiScreenSize As MultiScreenSize = FindMultiScreenSize()
        Me.Size = New Size(m_MultiScreenSize.maxRight - m_MultiScreenSize.minX, _
                           m_MultiScreenSize.maxBottom - m_MultiScreenSize.minY)

        Graph.CopyFromScreen(m_MultiScreenSize.minX, m_MultiScreenSize.minY, 0, 0, BitmapSize)
        Me.TopMost = True
    End Sub
End Class

