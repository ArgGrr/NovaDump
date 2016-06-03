Imports System.IO

Public Enum ChunkTypes As UInt32
    SHED = 1145391187
    PCFM = 1296450384
    SPAL = 1279348819
    SAMP = 1347240275

    FRAM = 1296126534
    EOFL = 1279676229

End Enum
Public Class Form1

    Private Sub ConvertFile(Filename As String)

        Dim outfile As String

        Dim chunkData() As Byte
        Dim chunkLen As UInt32
        Dim chunkType As ChunkTypes

        Dim FileSize As UInteger = 0
        Dim DataSize As UInteger = 0
        Dim DataOffset As Long

        Dim SampleRate As UInt32
        Dim Channels As UInt16
        Dim BitsPerChannel As UInt16

        If Not UInt32.TryParse(txtSampleRate.Text, SampleRate) Then
            SampleRate = 7937UI
        End If
        If Not UInt16.TryParse(txtChannels.Text, Channels) Then
            Channels = 2US
        End If
        If Not UInt16.TryParse(txtBits.Text, BitsPerChannel) Then
            BitsPerChannel = 8US
        End If

        outfile = Filename & ".wav" 'Path.ChangeExtension(filename, "wav")

        If File.Exists(outfile) Then File.Delete(outfile)


        Using bo As New BinaryWriter(New FileStream(outfile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            Using br As New BinaryReader(New FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read))

                'Write header..?
                bo.Write(1179011410UI)  'RIFF
                bo.Write(0UI)           'File Size, come back at end.

                bo.Write(1163280727UI)  'WAVE
                bo.Write(544501094UI)   'fmt 

                bo.Write(16UI)          'dwChunkSize
                bo.Write(1US)           'wFormatTag
                bo.Write(Channels)           'nChannels
                bo.Write(SampleRate)        'nSamplesPerSec
                bo.Write(CUInt(SampleRate * Channels))       'avgBytesperSec
                bo.Write(2US)           'nBlockAlign
                bo.Write(8US)           'wBitsPerSample

                bo.Write(1635017060UI)  'data
                DataOffset = bo.BaseStream.Position
                bo.Write(0UI)

                Do
                    chunkType = CType(br.ReadUInt32, ChunkTypes)
                    'Check type
                    chunkLen = br.ReadUInt32
                    DataSize += chunkLen

                    chunkData = br.ReadBytes(chunkLen)


                    Select Case chunkType
                        Case ChunkTypes.SAMP
                            bo.Write(chunkData)
                    End Select
                Loop While chunkType <> ChunkTypes.EOFL

                FileSize = CUInt(bo.BaseStream.Length)

                bo.Seek(4, SeekOrigin.Begin)
                bo.Write(CUInt(FileSize - 8))

                bo.Seek(40, SeekOrigin.Begin)
                bo.Write(CUInt(DataSize))
            End Using
        End Using
        Beep()
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs)

        Dim filename As String

        filename = txtSampleRate.Text

        If Not File.Exists(filename) Then Exit Sub

        ConvertFile(filename)
    End Sub

    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles Me.DragDrop

        Debug.Print(e.ToString)
        Dim files() As String

        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            files = CType(e.Data.GetData(DataFormats.FileDrop), String())

            For Each tmpstring As String In files
                If File.Exists(tmpstring) Then
                    ConvertFile(tmpstring)
                End If
            Next

        End If

    End Sub

    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles Me.DragEnter

        e.Effect = DragDropEffects.All
    End Sub

    Private Sub Form1_DragOver(sender As Object, e As DragEventArgs) Handles Me.DragOver
        e.Effect = DragDropEffects.All
    End Sub
End Class
