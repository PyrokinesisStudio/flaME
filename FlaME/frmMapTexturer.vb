﻿Public Class frmMapTexturer
#If MonoDevelop <> 0.0# Then
    Inherits Form
#End If

    Public Structure sLayerList
        Public Class clsLayer
            Public WithinLayer As Integer
            Public AvoidLayers() As Boolean
            Public Terrain As clsPainter.clsTerrain
            Public Terrainmap As clsBooleanMap
            Public HeightMin As Single
            Public HeightMax As Single
            Public SlopeMin As Single
            Public SlopeMax As Single
            'for generator only
            Public Scale As Single
            Public Density As Single
        End Class
        Public Layers() As clsLayer
        Public LayerCount As Integer

        Sub Layer_Insert(ByVal PositionNum As Integer, ByVal NewLayer As clsLayer)
            Dim A As Integer
            Dim B As Integer

            ReDim Preserve Layers(LayerCount)
            'shift the ones below down
            For A = LayerCount - 1 To PositionNum Step -1
                Layers(A + 1) = Layers(A)
            Next A
            'insert the new entry
            Layers(PositionNum) = NewLayer
            LayerCount += 1

            For A = 0 To LayerCount - 1
                If Layers(A).WithinLayer >= PositionNum Then
                    Layers(A).WithinLayer = Layers(A).WithinLayer + 1
                End If
                ReDim Preserve Layers(A).AvoidLayers(LayerCount - 1)
                For B = LayerCount - 2 To PositionNum Step -1
                    Layers(A).AvoidLayers(B + 1) = Layers(A).AvoidLayers(B)
                Next
                Layers(A).AvoidLayers(PositionNum) = False
            Next
        End Sub

        Sub Layer_Remove(ByVal Layer_Num As Integer)
            Dim A As Integer
            Dim B As Integer

            LayerCount = LayerCount - 1
            For A = Layer_Num To LayerCount - 1
                Layers(A) = Layers(A + 1)
            Next A
            ReDim Preserve Layers(LayerCount - 1)

            For A = 0 To LayerCount - 1
                If Layers(A).WithinLayer = Layer_Num Then
                    Layers(A).WithinLayer = -1
                ElseIf Layers(A).WithinLayer > Layer_Num Then
                    Layers(A).WithinLayer = Layers(A).WithinLayer - 1
                End If
                For B = Layer_Num To LayerCount - 1
                    Layers(A).AvoidLayers(B) = Layers(A).AvoidLayers(B + 1)
                Next
                ReDim Preserve Layers(A).AvoidLayers(LayerCount - 1)
            Next
        End Sub

        Sub Layer_Move(ByVal Layer_Num As Integer, ByVal Layer_Dest_Num As Integer)
            Dim Layer_Temp As clsLayer
            Dim boolTemp As Boolean
            Dim A As Integer
            Dim B As Integer

            If Layer_Dest_Num < Layer_Num Then
                'move the variables
                Layer_Temp = Layers(Layer_Num)
                For A = Layer_Num - 1 To Layer_Dest_Num Step -1
                    Layers(A + 1) = Layers(A)
                Next A
                Layers(Layer_Dest_Num) = Layer_Temp
                'update the layer nums
                For A = 0 To LayerCount - 1
                    If Layers(A).WithinLayer = Layer_Num Then
                        Layers(A).WithinLayer = Layer_Dest_Num
                    ElseIf Layers(A).WithinLayer >= Layer_Dest_Num And Layers(A).WithinLayer < Layer_Num Then
                        Layers(A).WithinLayer = Layers(A).WithinLayer + 1
                    End If
                    boolTemp = Layers(A).AvoidLayers(Layer_Num)
                    For B = Layer_Num - 1 To Layer_Dest_Num Step -1
                        Layers(A).AvoidLayers(B + 1) = Layers(A).AvoidLayers(B)
                    Next
                    Layers(A).AvoidLayers(Layer_Dest_Num) = boolTemp
                Next
            ElseIf Layer_Dest_Num > Layer_Num Then
                'move the variables
                Layer_Temp = Layers(Layer_Num)
                For A = Layer_Num To Layer_Dest_Num - 1
                    Layers(A) = Layers(A + 1)
                Next A
                Layers(Layer_Dest_Num) = Layer_Temp
                'update the layer nums
                For A = 0 To LayerCount - 1
                    If Layers(A).WithinLayer = Layer_Num Then
                        Layers(A).WithinLayer = Layer_Dest_Num
                    ElseIf Layers(A).WithinLayer > Layer_Num And Layers(A).WithinLayer <= Layer_Dest_Num Then
                        Layers(A).WithinLayer = Layers(A).WithinLayer - 1
                    End If
                    boolTemp = Layers(A).AvoidLayers(Layer_Num)
                    For B = Layer_Num To Layer_Dest_Num - 1
                        Layers(A).AvoidLayers(B) = Layers(A).AvoidLayers(B + 1)
                    Next
                    Layers(A).AvoidLayers(Layer_Dest_Num) = boolTemp
                Next
            End If
        End Sub
    End Structure
    Public LayerList As sLayerList

    Public Sub New()
        InitializeComponent() 'required for mono too

    End Sub

    Sub Map_Size_Refresh()
        Dim A As Integer

        For A = 0 To LayerList.LayerCount - 1
            LayerList.Layers(A).Terrainmap.Blank(Main_Map.TerrainSize.X + 1, Main_Map.TerrainSize.Y + 1)
        Next
    End Sub

    Sub cmbLayer_Terrain_Refresh(ByVal NewSelectedIndex As Integer)
        Dim A As Integer

        cboLayer_Terrain.Items.Clear()
        For A = 0 To Main_Map.Painter.TerrainCount - 1
            cboLayer_Terrain.Items.Add(Main_Map.Painter.Terrains(A).Name)
        Next A
        cboLayer_Terrain.SelectedIndex = NewSelectedIndex
    End Sub

    Sub cmbWithin_Refresh(ByVal NewSelectedIndex As Integer)
        Dim A As Integer

        cboWithin.Items.Clear()
        For A = 0 To LayerList.LayerCount - 1
            If LayerList.Layers(A).Terrain IsNot Nothing Then
                cboWithin.Items.Add(LayerList.Layers(A).Terrain.Name)
            Else
                cboWithin.Items.Add("Nothing")
            End If
        Next A
        cboWithin.SelectedIndex = NewSelectedIndex
    End Sub

    Sub lstLayer_Refresh(ByVal NewSelectedIndex As Integer)
        Dim A As Integer

        lstLayer.SelectedIndex = -1 'so that the items get disabled
        lstLayer.Items.Clear()
        For A = 0 To LayerList.LayerCount - 1
            If LayerList.Layers(A).Terrain IsNot Nothing Then
                lstLayer.Items.Add(LayerList.Layers(A).Terrain.Name)
            Else
                lstLayer.Items.Add("Nothing")
            End If
        Next A
        lstLayer.SelectedIndex = NewSelectedIndex

        Layer_Items_Refresh()
    End Sub

    Sub clstAvoid_Refresh(ByVal NewSelectedIndex As Integer)
        Dim A As Integer

        clstAvoid.Items.Clear()
        For A = 0 To LayerList.LayerCount - 1
            If LayerList.Layers(A).Terrain IsNot Nothing Then
                clstAvoid.Items.Add(LayerList.Layers(A).Terrain.Name, LayerList.Layers(lstLayer.SelectedIndex).AvoidLayers(A))
            Else
                clstAvoid.Items.Add("Nothing", LayerList.Layers(lstLayer.SelectedIndex).AvoidLayers(A))
            End If
        Next A
        clstAvoid.SelectedIndex = NewSelectedIndex
    End Sub

    Sub Layer_Items_Refresh()

        If lstLayer.SelectedIndex = -1 Then
            cboLayer_Terrain.Enabled = False
            cboLayer_Terrain.SelectedIndex = -1
            txtLayer_HeightMin.Enabled = False
            txtLayer_HeightMin.Text = ""
            txtLayer_HeightMax.Enabled = False
            txtLayer_HeightMax.Text = ""
            txtLayer_SlopeMin.Enabled = False
            txtLayer_SlopeMin.Text = ""
            txtLayer_SlopeMax.Enabled = False
            txtLayer_SlopeMax.Text = ""
            btnhmImport.Enabled = False
            picHeightmap.Visible = False
            btnTerrain_Rem.Enabled = False
            txtScale.Enabled = False
            txtDensity.Enabled = False
            btnGen.Enabled = False
            clstAvoid.Enabled = False
            clstAvoid.Items.Clear()
            cboWithin.Enabled = False
            btnWithinClear.Enabled = False
        Else
            cboLayer_Terrain.Enabled = False
            If LayerList.Layers(lstLayer.SelectedIndex).Terrain IsNot Nothing Then
                If LayerList.Layers(lstLayer.SelectedIndex).Terrain.Num >= 0 Then
                    cboLayer_Terrain.SelectedIndex = LayerList.Layers(lstLayer.SelectedIndex).Terrain.Num
                Else
                    cboLayer_Terrain.SelectedIndex = -1
                End If
            Else
                cboLayer_Terrain.SelectedIndex = -1
            End If
            cboLayer_Terrain.Enabled = True
            txtLayer_HeightMin.Enabled = False
            txtLayer_HeightMin.Text = CStr(LayerList.Layers(lstLayer.SelectedIndex).HeightMin)
            txtLayer_HeightMin.Enabled = True
            txtLayer_HeightMax.Enabled = False
            txtLayer_HeightMax.Text = CStr(LayerList.Layers(lstLayer.SelectedIndex).HeightMax)
            txtLayer_HeightMax.Enabled = True
            txtLayer_SlopeMin.Enabled = False
            txtLayer_SlopeMin.Text = CStr(Int(LayerList.Layers(lstLayer.SelectedIndex).SlopeMin / RadOf1Deg * 100.0# + 0.5#) / 100.0#)
            txtLayer_SlopeMin.Enabled = True
            txtLayer_SlopeMax.Enabled = False
            txtLayer_SlopeMax.Text = CStr(Int(LayerList.Layers(lstLayer.SelectedIndex).SlopeMax / RadOf1Deg * 100.0# + 0.5#) / 100.0#)
            txtLayer_SlopeMax.Enabled = True
            btnhmImport.Enabled = True
            btnTerrain_Rem.Enabled = True
            txtScale.Enabled = True
            txtDensity.Enabled = True
            btnGen.Enabled = True
            clstAvoid.Enabled = False
            clstAvoid_Refresh(-1)
            clstAvoid.Enabled = True
            cboWithin.Enabled = False
            cmbWithin_Refresh(LayerList.Layers(lstLayer.SelectedIndex).WithinLayer)
            cboWithin.Enabled = True
            btnWithinClear.Enabled = True
            bmDisplay()
        End If
    End Sub

    Private Sub cmbLayer_Terrain_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboLayer_Terrain.SelectedIndexChanged
        If Not cboLayer_Terrain.Enabled Then
            Exit Sub
        End If

        If cboLayer_Terrain.SelectedIndex >= 0 Then
            LayerList.Layers(lstLayer.SelectedIndex).Terrain = Main_Map.Painter.Terrains(cboLayer_Terrain.SelectedIndex)
            lstLayer_Refresh(lstLayer.SelectedIndex)
        End If
    End Sub

    Private Sub btnDo_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles btnDo.Click

        Main_Map.MapTexturer(LayerList)

        Main_Map.SectorAll_Set_Changed()
        Main_Map.SectorAll_GL_Update()

        Main_Map.UndoStepCreate("Entire Map Painter")

        frmMainInstance.View_DrawViewLater()
    End Sub

    Private Sub btnhmImport_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles btnhmImport.Click

        OpenFileDialog.FileName = ""
        OpenFileDialog.Filter = "Bitmap Images (*.bmp)|*.bmp|All Files (*.*)|*.*"
        If Not OpenFileDialog.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            Exit Sub
        End If

        Dim Result As sResult
        Dim hmA As clsHeightmap = New clsHeightmap

        Result = hmA.Load_Image(OpenFileDialog.FileName)
        If Not Result.Success Then
            MsgBox("There was a problem loading the selected file: " & Result.Problem(0))
            Exit Sub
        End If

        If Not (hmA.HeightData.SizeX = Main_Map.TerrainSize.X + 1 And hmA.HeightData.SizeY = Main_Map.TerrainSize.Y + 1) Then
            MsgBox("Heightmap sizes do not equal map size + 1.", MsgBoxStyle.OkOnly, "Error")
            Exit Sub
        End If
        LayerList.Layers(lstLayer.SelectedIndex).Terrainmap.Convert_Heightmap(hmA, 1275000L)

        bmDisplay()
    End Sub

    Private Sub lstLayer_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles lstLayer.SelectedIndexChanged
        If lstLayer.Enabled = False Then Exit Sub

        Layer_Items_Refresh()
    End Sub

    Private Sub lstLayer_KeyDown(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.KeyEventArgs) Handles lstLayer.KeyDown
        Dim NewLayer As sLayerList.clsLayer
        Dim Position_Num As Integer

        If eventArgs.KeyCode = 189 Then '-
            If Not (lstLayer.SelectedIndex = -1 Or lstLayer.SelectedIndex = 0) Then
                LayerList.Layer_Move(lstLayer.SelectedIndex, lstLayer.SelectedIndex - 1)
                lstLayer_Refresh(lstLayer.SelectedIndex - 1)
            End If
        ElseIf eventArgs.KeyCode = 187 Then  '+
            If Not (lstLayer.SelectedIndex = -1 Or lstLayer.SelectedIndex = LayerList.LayerCount - 1) Then
                LayerList.Layer_Move(lstLayer.SelectedIndex, lstLayer.SelectedIndex + 1)
                lstLayer_Refresh(lstLayer.SelectedIndex + 1)
            End If
        ElseIf eventArgs.KeyCode = System.Windows.Forms.Keys.Insert Then
            If lstLayer.SelectedIndex = -1 Then
                Position_Num = LayerList.LayerCount
            Else
                Position_Num = lstLayer.SelectedIndex + 1
            End If
            NewLayer = New sLayerList.clsLayer
            NewLayer.WithinLayer = -1
            NewLayer.Terrain = Nothing
            NewLayer.HeightMin = 0.0F
            NewLayer.HeightMax = 255.0F
            NewLayer.SlopeMin = 0
            NewLayer.SlopeMax = RadOf90Deg
            NewLayer.Terrainmap = New clsBooleanMap
            NewLayer.Terrainmap.Blank(Main_Map.TerrainSize.X + 1, Main_Map.TerrainSize.Y + 1)
            LayerList.Layer_Insert(Position_Num, NewLayer)
            lstLayer_Refresh(Position_Num)
        ElseIf eventArgs.KeyCode = System.Windows.Forms.Keys.Delete And eventArgs.Shift Then
            If Not lstLayer.SelectedIndex = -1 Then
                LayerList.Layer_Remove(lstLayer.SelectedIndex)
                lstLayer_Refresh(Math.Min(lstLayer.SelectedIndex, LayerList.LayerCount - 1))
            End If
        End If
    End Sub

    Private Sub txtLayer_HeightMax_TextChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLayer_HeightMax.TextChanged
        If Not txtLayer_HeightMax.Enabled Then Exit Sub

        LayerList.Layers(lstLayer.SelectedIndex).HeightMax = Val(txtLayer_HeightMax.Text)
    End Sub

    Private Sub txtLayer_HeightMin_TextChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLayer_HeightMin.TextChanged
        If Not txtLayer_HeightMin.Enabled Then Exit Sub

        LayerList.Layers(lstLayer.SelectedIndex).HeightMin = Val(txtLayer_HeightMin.Text)
    End Sub

    Private Sub txtLayer_SlopeMax_TextChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLayer_SlopeMax.TextChanged
        If Not txtLayer_SlopeMax.Enabled Then Exit Sub

        LayerList.Layers(lstLayer.SelectedIndex).SlopeMax = Val(txtLayer_SlopeMax.Text) * RadOf1Deg
    End Sub

    Private Sub txtLayer_SlopeMin_TextChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLayer_SlopeMin.TextChanged
        If Not txtLayer_SlopeMin.Enabled Then Exit Sub

        LayerList.Layers(lstLayer.SelectedIndex).SlopeMin = Val(txtLayer_SlopeMin.Text) * RadOf1Deg
    End Sub

    Sub bmDisplay()
        Dim X As Integer
        Dim Y As Integer
        Dim tmpBitmap As Bitmap = New Bitmap(Main_Map.TerrainSize.X + 1, Main_Map.TerrainSize.Y + 1, Imaging.PixelFormat.Format24bppRgb)

        For Y = 0 To Main_Map.TerrainSize.Y
            For X = 0 To Main_Map.TerrainSize.X
                If LayerList.Layers(lstLayer.SelectedIndex).Terrainmap.ValueData.Value(Y, X) Then
                    tmpBitmap.SetPixel(X, Y, Color.White)
                Else
                    tmpBitmap.SetPixel(X, Y, Color.Black)
                End If
            Next
        Next
        picHeightmap.Image = tmpBitmap
        picHeightmap.Visible = True
    End Sub

    Private Sub btnTerrain_Rem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTerrain_Rem.Click

        cboLayer_Terrain.SelectedIndex = -1
    End Sub

    Private Sub btnGen_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles btnGen.Click
        Dim Scale As Single
        Dim Density As Single

        Scale = Clamp(CSng(Val(txtScale.Text)), 0.0F, 8.0F)
        txtScale.Text = CStr(Scale)
        Density = Clamp(Val(txtDensity.Text) / 100.0#, 0.0#, 1.0#)
        txtDensity.Text = CStr(Density * 100.0#)
        LayerList.Layers(lstLayer.SelectedIndex).Terrainmap = Main_Map.GenerateTerrainMap(Scale, Density)

        bmDisplay()
    End Sub

    Private Sub btnTerrain_Rem_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTerrain_Rem.Click

        cboLayer_Terrain.SelectedIndex = -1
    End Sub

    Private Sub btnWithinClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnWithinClear.Click

        cboWithin.SelectedIndex = -1
    End Sub

    Private Sub clstAvoid_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles clstAvoid.ItemCheck

        LayerList.Layers(lstLayer.SelectedIndex).AvoidLayers(e.Index) = e.NewValue
    End Sub

    Private Sub cmbWithin_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboWithin.SelectedIndexChanged

        LayerList.Layers(lstLayer.SelectedIndex).WithinLayer = cboWithin.SelectedIndex
    End Sub

    Private Sub frmMapTexturer_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        e.Cancel = True
        Hide()
    End Sub

    Private Sub frmMapTexturer_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        cmbLayer_Terrain_Refresh(-1)

        Layer_Items_Refresh()
    End Sub

#If MonoDevelop <> 0.0# Then
    Private Sub InitializeComponent()
        Me.lstLayer = New System.Windows.Forms.ListBox()
        Me.btnDo = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnTerrain_Rem = New System.Windows.Forms.Button()
        Me.txtDensity = New System.Windows.Forms.TextBox()
        Me.txtScale = New System.Windows.Forms.TextBox()
        Me.btnGen = New System.Windows.Forms.Button()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.txtLayer_SlopeMax = New System.Windows.Forms.TextBox()
        Me.txtLayer_SlopeMin = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.txtLayer_HeightMax = New System.Windows.Forms.TextBox()
        Me.txtLayer_HeightMin = New System.Windows.Forms.TextBox()
        Me.btnhmImport = New System.Windows.Forms.Button()
        Me.picHeightmap = New System.Windows.Forms.PictureBox()
        Me.cboLayer_Terrain = New System.Windows.Forms.ComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.clstAvoid = New System.Windows.Forms.CheckedListBox()
        Me.cboWithin = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnWithinClear = New System.Windows.Forms.Button()
        Me.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()
        CType(Me.picHeightmap, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lstLayer
        '
        Me.lstLayer.BackColor = System.Drawing.SystemColors.Window
        Me.lstLayer.Cursor = System.Windows.Forms.Cursors.Default
        Me.lstLayer.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lstLayer.ItemHeight = 16
        Me.lstLayer.Location = New System.Drawing.Point(123, 15)
        Me.lstLayer.Margin = New System.Windows.Forms.Padding(4)
        Me.lstLayer.Name = "lstLayer"
        Me.lstLayer.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lstLayer.Size = New System.Drawing.Size(204, 228)
        Me.lstLayer.TabIndex = 37
        '
        'btnDo
        '
        Me.btnDo.BackColor = System.Drawing.SystemColors.Control
        Me.btnDo.Cursor = System.Windows.Forms.Cursors.Default
        Me.btnDo.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDo.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnDo.Location = New System.Drawing.Point(123, 505)
        Me.btnDo.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDo.Name = "btnDo"
        Me.btnDo.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.btnDo.Size = New System.Drawing.Size(183, 41)
        Me.btnDo.TabIndex = 33
        Me.btnDo.Text = "Perform"
        Me.btnDo.UseVisualStyleBackColor = False
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.SystemColors.Control
        Me.Label2.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label2.Location = New System.Drawing.Point(16, 15)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label2.Size = New System.Drawing.Size(108, 27)
        Me.Label2.TabIndex = 36
        Me.Label2.Text = "Terrain layers:"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'btnTerrain_Rem
        '
        Me.btnTerrain_Rem.Location = New System.Drawing.Point(575, 31)
        Me.btnTerrain_Rem.Margin = New System.Windows.Forms.Padding(4)
        Me.btnTerrain_Rem.Name = "btnTerrain_Rem"
        Me.btnTerrain_Rem.Size = New System.Drawing.Size(31, 27)
        Me.btnTerrain_Rem.TabIndex = 65
        Me.btnTerrain_Rem.Text = "X"
        Me.btnTerrain_Rem.UseVisualStyleBackColor = True
        '
        'txtDensity
        '
        Me.txtDensity.AcceptsReturn = True
        Me.txtDensity.BackColor = System.Drawing.SystemColors.Window
        Me.txtDensity.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtDensity.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtDensity.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtDensity.Location = New System.Drawing.Point(705, 50)
        Me.txtDensity.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDensity.MaxLength = 0
        Me.txtDensity.Name = "txtDensity"
        Me.txtDensity.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtDensity.Size = New System.Drawing.Size(43, 26)
        Me.txtDensity.TabIndex = 61
        Me.txtDensity.Text = "50"
        Me.txtDensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtScale
        '
        Me.txtScale.AcceptsReturn = True
        Me.txtScale.BackColor = System.Drawing.SystemColors.Window
        Me.txtScale.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtScale.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtScale.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtScale.Location = New System.Drawing.Point(705, 21)
        Me.txtScale.Margin = New System.Windows.Forms.Padding(4)
        Me.txtScale.MaxLength = 0
        Me.txtScale.Name = "txtScale"
        Me.txtScale.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtScale.Size = New System.Drawing.Size(43, 26)
        Me.txtScale.TabIndex = 59
        Me.txtScale.Text = "2.0"
        Me.txtScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'btnGen
        '
        Me.btnGen.BackColor = System.Drawing.SystemColors.Control
        Me.btnGen.Cursor = System.Windows.Forms.Cursors.Default
        Me.btnGen.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnGen.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnGen.Location = New System.Drawing.Point(633, 82)
        Me.btnGen.Margin = New System.Windows.Forms.Padding(4)
        Me.btnGen.Name = "btnGen"
        Me.btnGen.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.btnGen.Size = New System.Drawing.Size(175, 30)
        Me.btnGen.TabIndex = 58
        Me.btnGen.Text = "Generate Terrain Map"
        Me.btnGen.UseVisualStyleBackColor = False
        '
        'Label6
        '
        Me.Label6.BackColor = System.Drawing.SystemColors.Control
        Me.Label6.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label6.Location = New System.Drawing.Point(757, 50)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label6.Size = New System.Drawing.Size(65, 21)
        Me.Label6.TabIndex = 64
        Me.Label6.Text = "(0 - 100)"
        '
        'Label14
        '
        Me.Label14.BackColor = System.Drawing.SystemColors.Control
        Me.Label14.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label14.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label14.Location = New System.Drawing.Point(757, 21)
        Me.Label14.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label14.Name = "Label14"
        Me.Label14.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label14.Size = New System.Drawing.Size(77, 21)
        Me.Label14.TabIndex = 63
        Me.Label14.Text = "(1.0 - 8.0)"
        '
        'Label15
        '
        Me.Label15.BackColor = System.Drawing.SystemColors.Control
        Me.Label15.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label15.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label15.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label15.Location = New System.Drawing.Point(629, 50)
        Me.Label15.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label15.Name = "Label15"
        Me.Label15.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label15.Size = New System.Drawing.Size(68, 21)
        Me.Label15.TabIndex = 62
        Me.Label15.Text = "Density:"
        Me.Label15.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label16
        '
        Me.Label16.BackColor = System.Drawing.SystemColors.Control
        Me.Label16.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label16.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label16.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label16.Location = New System.Drawing.Point(629, 21)
        Me.Label16.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label16.Name = "Label16"
        Me.Label16.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label16.Size = New System.Drawing.Size(68, 21)
        Me.Label16.TabIndex = 60
        Me.Label16.Text = "Scale:"
        Me.Label16.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtLayer_SlopeMax
        '
        Me.txtLayer_SlopeMax.AcceptsReturn = True
        Me.txtLayer_SlopeMax.BackColor = System.Drawing.SystemColors.Window
        Me.txtLayer_SlopeMax.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtLayer_SlopeMax.Enabled = False
        Me.txtLayer_SlopeMax.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLayer_SlopeMax.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtLayer_SlopeMax.Location = New System.Drawing.Point(540, 134)
        Me.txtLayer_SlopeMax.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLayer_SlopeMax.MaxLength = 0
        Me.txtLayer_SlopeMax.Name = "txtLayer_SlopeMax"
        Me.txtLayer_SlopeMax.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtLayer_SlopeMax.Size = New System.Drawing.Size(53, 26)
        Me.txtLayer_SlopeMax.TabIndex = 55
        Me.txtLayer_SlopeMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtLayer_SlopeMin
        '
        Me.txtLayer_SlopeMin.AcceptsReturn = True
        Me.txtLayer_SlopeMin.BackColor = System.Drawing.SystemColors.Window
        Me.txtLayer_SlopeMin.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtLayer_SlopeMin.Enabled = False
        Me.txtLayer_SlopeMin.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLayer_SlopeMin.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtLayer_SlopeMin.Location = New System.Drawing.Point(477, 134)
        Me.txtLayer_SlopeMin.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLayer_SlopeMin.MaxLength = 0
        Me.txtLayer_SlopeMin.Name = "txtLayer_SlopeMin"
        Me.txtLayer_SlopeMin.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtLayer_SlopeMin.Size = New System.Drawing.Size(53, 26)
        Me.txtLayer_SlopeMin.TabIndex = 54
        Me.txtLayer_SlopeMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label13
        '
        Me.Label13.BackColor = System.Drawing.SystemColors.Control
        Me.Label13.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label13.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label13.Location = New System.Drawing.Point(413, 139)
        Me.Label13.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label13.Name = "Label13"
        Me.Label13.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label13.Size = New System.Drawing.Size(56, 21)
        Me.Label13.TabIndex = 53
        Me.Label13.Text = "Slope:"
        Me.Label13.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtLayer_HeightMax
        '
        Me.txtLayer_HeightMax.AcceptsReturn = True
        Me.txtLayer_HeightMax.BackColor = System.Drawing.SystemColors.Window
        Me.txtLayer_HeightMax.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtLayer_HeightMax.Enabled = False
        Me.txtLayer_HeightMax.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLayer_HeightMax.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtLayer_HeightMax.Location = New System.Drawing.Point(540, 95)
        Me.txtLayer_HeightMax.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLayer_HeightMax.MaxLength = 0
        Me.txtLayer_HeightMax.Name = "txtLayer_HeightMax"
        Me.txtLayer_HeightMax.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtLayer_HeightMax.Size = New System.Drawing.Size(53, 26)
        Me.txtLayer_HeightMax.TabIndex = 50
        Me.txtLayer_HeightMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtLayer_HeightMin
        '
        Me.txtLayer_HeightMin.AcceptsReturn = True
        Me.txtLayer_HeightMin.BackColor = System.Drawing.SystemColors.Window
        Me.txtLayer_HeightMin.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtLayer_HeightMin.Enabled = False
        Me.txtLayer_HeightMin.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLayer_HeightMin.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtLayer_HeightMin.Location = New System.Drawing.Point(477, 95)
        Me.txtLayer_HeightMin.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLayer_HeightMin.MaxLength = 0
        Me.txtLayer_HeightMin.Name = "txtLayer_HeightMin"
        Me.txtLayer_HeightMin.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtLayer_HeightMin.Size = New System.Drawing.Size(53, 26)
        Me.txtLayer_HeightMin.TabIndex = 49
        Me.txtLayer_HeightMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'btnhmImport
        '
        Me.btnhmImport.BackColor = System.Drawing.SystemColors.Control
        Me.btnhmImport.Cursor = System.Windows.Forms.Cursors.Default
        Me.btnhmImport.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnhmImport.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnhmImport.Location = New System.Drawing.Point(705, 117)
        Me.btnhmImport.Margin = New System.Windows.Forms.Padding(4)
        Me.btnhmImport.Name = "btnhmImport"
        Me.btnhmImport.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.btnhmImport.Size = New System.Drawing.Size(103, 32)
        Me.btnhmImport.TabIndex = 47
        Me.btnhmImport.Text = "Import .bmp"
        Me.btnhmImport.UseVisualStyleBackColor = False
        '
        'picHeightmap
        '
        Me.picHeightmap.BackColor = System.Drawing.SystemColors.Control
        Me.picHeightmap.Cursor = System.Windows.Forms.Cursors.Default
        Me.picHeightmap.ErrorImage = Nothing
        Me.picHeightmap.ForeColor = System.Drawing.SystemColors.ControlText
        Me.picHeightmap.InitialImage = Nothing
        Me.picHeightmap.Location = New System.Drawing.Point(415, 197)
        Me.picHeightmap.Margin = New System.Windows.Forms.Padding(0)
        Me.picHeightmap.Name = "picHeightmap"
        Me.picHeightmap.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.picHeightmap.Size = New System.Drawing.Size(256, 256)
        Me.picHeightmap.TabIndex = 46
        Me.picHeightmap.TabStop = False
        Me.picHeightmap.Visible = False
        '
        'cboLayer_Terrain
        '
        Me.cboLayer_Terrain.BackColor = System.Drawing.SystemColors.Window
        Me.cboLayer_Terrain.Cursor = System.Windows.Forms.Cursors.Default
        Me.cboLayer_Terrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboLayer_Terrain.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cboLayer_Terrain.ForeColor = System.Drawing.SystemColors.WindowText
        Me.cboLayer_Terrain.Location = New System.Drawing.Point(416, 31)
        Me.cboLayer_Terrain.Margin = New System.Windows.Forms.Padding(4)
        Me.cboLayer_Terrain.Name = "cboLayer_Terrain"
        Me.cboLayer_Terrain.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cboLayer_Terrain.Size = New System.Drawing.Size(149, 25)
        Me.cboLayer_Terrain.TabIndex = 44
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.BackColor = System.Drawing.SystemColors.Control
        Me.Label10.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label10.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label10.Location = New System.Drawing.Point(536, 75)
        Me.Label10.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label10.Name = "Label10"
        Me.Label10.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label10.Size = New System.Drawing.Size(37, 17)
        Me.Label10.TabIndex = 52
        Me.Label10.Text = "max:"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.BackColor = System.Drawing.SystemColors.Control
        Me.Label9.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label9.Location = New System.Drawing.Point(473, 75)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label9.Size = New System.Drawing.Size(34, 17)
        Me.Label9.TabIndex = 51
        Me.Label9.Text = "min:"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label8
        '
        Me.Label8.BackColor = System.Drawing.SystemColors.Control
        Me.Label8.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label8.Location = New System.Drawing.Point(413, 100)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label8.Size = New System.Drawing.Size(56, 21)
        Me.Label8.TabIndex = 48
        Me.Label8.Text = "Height:"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label4
        '
        Me.Label4.BackColor = System.Drawing.SystemColors.Control
        Me.Label4.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label4.Location = New System.Drawing.Point(352, 31)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label4.Size = New System.Drawing.Size(55, 21)
        Me.Label4.TabIndex = 45
        Me.Label4.Text = "Terrain:"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.SystemColors.Control
        Me.Label1.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label1.Location = New System.Drawing.Point(9, 335)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label1.Size = New System.Drawing.Size(117, 55)
        Me.Label1.TabIndex = 67
        Me.Label1.Text = "Dont place over:"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'clstAvoid
        '
        Me.clstAvoid.FormattingEnabled = True
        Me.clstAvoid.Location = New System.Drawing.Point(135, 335)
        Me.clstAvoid.Margin = New System.Windows.Forms.Padding(4)
        Me.clstAvoid.Name = "clstAvoid"
        Me.clstAvoid.Size = New System.Drawing.Size(169, 123)
        Me.clstAvoid.TabIndex = 68
        '
        'cboWithin
        '
        Me.cboWithin.BackColor = System.Drawing.SystemColors.Window
        Me.cboWithin.Cursor = System.Windows.Forms.Cursors.Default
        Me.cboWithin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboWithin.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cboWithin.ForeColor = System.Drawing.SystemColors.WindowText
        Me.cboWithin.Location = New System.Drawing.Point(135, 300)
        Me.cboWithin.Margin = New System.Windows.Forms.Padding(4)
        Me.cboWithin.Name = "cboWithin"
        Me.cboWithin.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cboWithin.Size = New System.Drawing.Size(149, 25)
        Me.cboWithin.TabIndex = 69
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.SystemColors.Control
        Me.Label3.Cursor = System.Windows.Forms.Cursors.Default
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label3.Location = New System.Drawing.Point(0, 300)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Label3.Size = New System.Drawing.Size(125, 27)
        Me.Label3.TabIndex = 70
        Me.Label3.Text = "Only place within:"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'btnWithinClear
        '
        Me.btnWithinClear.Location = New System.Drawing.Point(297, 300)
        Me.btnWithinClear.Margin = New System.Windows.Forms.Padding(4)
        Me.btnWithinClear.Name = "btnWithinClear"
        Me.btnWithinClear.Size = New System.Drawing.Size(31, 27)
        Me.btnWithinClear.TabIndex = 71
        Me.btnWithinClear.Text = "X"
        Me.btnWithinClear.UseVisualStyleBackColor = True
        '
        'frmMapTexturer
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(839, 560)
        Me.Controls.Add(Me.btnWithinClear)
        Me.Controls.Add(Me.cboWithin)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.clstAvoid)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnTerrain_Rem)
        Me.Controls.Add(Me.txtDensity)
        Me.Controls.Add(Me.txtScale)
        Me.Controls.Add(Me.btnGen)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label14)
        Me.Controls.Add(Me.Label15)
        Me.Controls.Add(Me.Label16)
        Me.Controls.Add(Me.txtLayer_SlopeMax)
        Me.Controls.Add(Me.txtLayer_SlopeMin)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.txtLayer_HeightMax)
        Me.Controls.Add(Me.txtLayer_HeightMin)
        Me.Controls.Add(Me.btnhmImport)
        Me.Controls.Add(Me.picHeightmap)
        Me.Controls.Add(Me.cboLayer_Terrain)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.lstLayer)
        Me.Controls.Add(Me.btnDo)
        Me.Controls.Add(Me.Label2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmMapTexturer"
        Me.Text = "Entire Map Painter"
        CType(Me.picHeightmap, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents lstLayer As System.Windows.Forms.ListBox
    Public WithEvents btnDo As System.Windows.Forms.Button
    Public WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents btnTerrain_Rem As System.Windows.Forms.Button
    Public WithEvents txtDensity As System.Windows.Forms.TextBox
    Public WithEvents txtScale As System.Windows.Forms.TextBox
    Public WithEvents btnGen As System.Windows.Forms.Button
    Public WithEvents Label6 As System.Windows.Forms.Label
    Public WithEvents Label14 As System.Windows.Forms.Label
    Public WithEvents Label15 As System.Windows.Forms.Label
    Public WithEvents Label16 As System.Windows.Forms.Label
    Public WithEvents txtLayer_SlopeMax As System.Windows.Forms.TextBox
    Public WithEvents txtLayer_SlopeMin As System.Windows.Forms.TextBox
    Public WithEvents Label13 As System.Windows.Forms.Label
    Public WithEvents txtLayer_HeightMax As System.Windows.Forms.TextBox
    Public WithEvents txtLayer_HeightMin As System.Windows.Forms.TextBox
    Public WithEvents btnhmImport As System.Windows.Forms.Button
    Public WithEvents picHeightmap As System.Windows.Forms.PictureBox
    Public WithEvents cboLayer_Terrain As System.Windows.Forms.ComboBox
    Public WithEvents Label10 As System.Windows.Forms.Label
    Public WithEvents Label9 As System.Windows.Forms.Label
    Public WithEvents Label8 As System.Windows.Forms.Label
    Public WithEvents Label4 As System.Windows.Forms.Label
    Public WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents clstAvoid As System.Windows.Forms.CheckedListBox
    Public WithEvents cboWithin As System.Windows.Forms.ComboBox
    Public WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents btnWithinClear As System.Windows.Forms.Button
    Friend WithEvents OpenFileDialog As System.Windows.Forms.OpenFileDialog
#End If
End Class