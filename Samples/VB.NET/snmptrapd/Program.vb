﻿'
' Created by SharpDevelop.
' User: Administrator
' Date: 2010/4/25
' Time: 14:16
'
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Imports System.Net
Imports Lextm.SharpSnmpLib.Messaging
Imports Lextm.SharpSnmpLib.Pipeline
Imports Microsoft.Practices.Unity
Imports Microsoft.Practices.Unity.Configuration

Module Program
    Private m_Container As IUnityContainer

    Friend Property Container() As IUnityContainer
        Get
            Return m_Container
        End Get
        Private Set(ByVal value As IUnityContainer)
            m_Container = value
        End Set
    End Property

    Public Sub Main(ByVal args As String())
        If args.Length <> 0 Then
            Return
        End If

        Container = New UnityContainer().LoadConfiguration("snmptrapd")

        Dim trapv1 = Container.Resolve(Of TrapV1MessageHandler)("TrapV1Handler")
        AddHandler trapv1.MessageReceived, AddressOf WatcherTrapV1Received
        Dim trapv2 = Container.Resolve(Of TrapV2MessageHandler)("TrapV2Handler")
        AddHandler trapv2.MessageReceived, AddressOf WatcherTrapV2Received
        Dim inform = Container.Resolve(Of InformMessageHandler)("InformHandler")
        AddHandler inform.MessageReceived, AddressOf WatcherInformRequestReceived
        Using engine = Container.Resolve(Of SnmpEngine)()
            engine.Listener.AddBinding(New IPEndPoint(IPAddress.Any, 162))
            engine.Start()
            Console.WriteLine("#SNMP is available at http://sharpsnmplib.codeplex.com")
            Console.WriteLine("Press any key to stop . . . ")
            Console.Read()
            engine.[Stop]()
        End Using
    End Sub

    Private Sub WatcherInformRequestReceived(ByVal sender As Object, ByVal e As MessageReceivedEventArgs(Of InformRequestMessage))
        Console.WriteLine(e.Message)
    End Sub

    Private Sub WatcherTrapV2Received(ByVal sender As Object, ByVal e As MessageReceivedEventArgs(Of TrapV2Message))
        Console.WriteLine(e.Message)
    End Sub

    Private Sub WatcherTrapV1Received(ByVal sender As Object, ByVal e As MessageReceivedEventArgs(Of TrapV1Message))
        Console.WriteLine(e.Message)
    End Sub
End Module