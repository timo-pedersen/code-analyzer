using System;
using DotNetRemoting;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Alarm.Remoting;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Alarm
{
    [TestFixture]
    public class RemoteAlarmClientTest
    {
        [Test]
        public void NormalTimeTest()
        {
            // ARRANGE
            var syncClient = new NeoSyncClientTest();
            var remoteAlarmServerStateService = Substitute.For<IRemoteAlarmServerStateService>();
            var remoteAlarmClient = new RemoteAlarmClient("", 999, 999, syncClient, remoteAlarmServerStateService);
            var alarmguid = Guid.NewGuid();
            var activeTestTime = new DateTime(2016, 04, 01, 12, 30, 15);
            var acknowledgeTestTime = new DateTime(2017, 04, 01, 12, 30, 15);
            var inactiveTestTime = new DateTime(2018, 04, 01, 12, 30, 15);
            var normalTestTime = new DateTime(2019, 04, 01, 12, 30, 15);
            var alarmGroupName = "Yoda";
            var agi = new AlarmGroupInfo
            {
                Name = alarmGroupName
            };
            syncClient.FireDataReceivedHandler(new[] { agi });

            // ACT
            var alarmEvent1 = new AlarmEventInfo
            {
                AlarmGroupName = alarmGroupName,
                Id = alarmguid
            };
            syncClient.FireDataReceivedHandler(alarmEvent1);

            var alarmEvent2 = new AlarmEventInfo
            {
                ActiveTime = activeTestTime,
                AcknowledgeTime = acknowledgeTestTime,
                InActiveTime = inactiveTestTime,
                NormalTime = normalTestTime,
                AlarmGroupName = alarmGroupName,
                Id = alarmguid
            };
            syncClient.FireDataReceivedHandler(alarmEvent2);

            // ASSERT
            Assert.AreEqual(((IAlarmClient)remoteAlarmClient).AlarmEvents.Count, 1);
            var alarmEventLight = ((IAlarmClient)remoteAlarmClient).AlarmEvents[0] as AlarmEventLight;
            Assert.IsNotNull(alarmEventLight);
            Assert.AreEqual(alarmEventLight.ActiveTime, activeTestTime);
            Assert.AreEqual(alarmEventLight.AcknowledgeTime, acknowledgeTestTime);
            Assert.AreEqual(alarmEventLight.InActiveTime, inactiveTestTime);
            Assert.AreEqual(alarmEventLight.NormalTime, normalTestTime);
        }

        internal class NeoSyncClientTest : INeoSyncClient
        {
            internal void FireStatusHandler()
            {
                if (StatusHandler != null)
                    StatusHandler(new StatusMessage());
            }

            internal void FireDataReceivedHandler(object data)
            {
                if (DataReceivedHandler != null)
                    DataReceivedHandler(data);
            }

            public void RegisterLicenseCode(string p)
            {
            }

            public object this[string key]
            {
                get { return 0; }
                set { return; }
            }

            public int KeepConnectionAliveTime { get; set; }

            public int TimeBetweenReconnectAttempsMs { get; set; }

            public bool AutoReconnect { get; set; }

            public int MaxReconnectionAttemps { get; set; }

            public event ObjDelegate DataReceivedHandler;

            public event StatDelegate StatusHandler;

            public void Connect()
            { }

            public void Send(object co)
            {
                throw new NotImplementedException();
            }

            public void Disconnect()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }

}
