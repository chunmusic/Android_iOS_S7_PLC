using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Sharp7;

namespace Android_IOS_S7
{
    public class S7PlcService
    {
        private readonly S7Client _client;
        private readonly System.Timers.Timer _timer;
        private DateTime _lastScanTime;

        private volatile object _locker = new object();

        public S7PlcService()
        {
            _client = new S7Client();
            _timer = new System.Timers.Timer();
            _timer.Interval = 100;
            _timer.Elapsed += OnTimerElapsed;
        }

        public ConnectionStates ConnectionState { get; private set; }

        public bool Db1Dbx40 { get; private set; }
        public ushort Db1Dbw0 { get; private set; }
        public ushort Db1Dbw2 { get; private set; }

        public TimeSpan ScanTime { get; private set; }

        public event EventHandler ValuesRefreshed;

        public void Connect(string ipAddress, int rack, int slot)
        {
            try
            {
                ConnectionState = ConnectionStates.Connecting;
                int result = _client.ConnectTo(ipAddress, rack, slot);
                if (result == 0)
                {
                    ConnectionState = ConnectionStates.Online;
                    _timer.Start();
                }
                else
                {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t Connection error: " + _client.ErrorText(result));
                    ConnectionState = ConnectionStates.Offline;
                }
                OnValuesRefreshed();
            }
            catch
            {
                ConnectionState = ConnectionStates.Offline;
                OnValuesRefreshed();
                throw;
            }
        }

        public void Disconnect()
        {
            if (_client.Connected)
            {
                _timer.Stop();
                _client.Disconnect();
                ConnectionState = ConnectionStates.Offline;
                OnValuesRefreshed();
            }
        }

        public async Task WriteDb1Dbx40(bool value)
        {
            await Task.Run(() =>
            {
                int writeResult = WriteBit("DB1.DBX4.0", value);
                if (writeResult != 0)
                {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t Write error: " + _client.ErrorText(writeResult));
                }
            });
        }


        public async Task WriteDb1Dbw0(ushort value)
        {
            await Task.Run(() =>
            {
                int writeResult = WriteInt("DB1.DBW0", value);
                if (writeResult != 0)
                {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t Write error: " + _client.ErrorText(writeResult));
                }
            });
        }

        public async Task WriteDb1Dbw2(ushort value)
        {
            await Task.Run(() =>
            {
                int writeResult = WriteInt("DB1.DBW2", value);
                if (writeResult != 0)
                {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t Write error: " + _client.ErrorText(writeResult));
                }
            });
        }







        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                ScanTime = DateTime.Now - _lastScanTime;
                RefreshValues();
                OnValuesRefreshed();
            }
            finally
            {
                _timer.Start();
            }
            _lastScanTime = DateTime.Now;
        }

        private void RefreshValues()
        {
            lock (_locker)
            {
                var buffer = new byte[1];
                int result = _client.DBRead(1, 4, buffer.Length, buffer);

                var buffer2 = new byte[4];
                int result2 = _client.DBRead(1, 0, buffer2.Length, buffer2);


                if (result == 0)
                {
                    Db1Dbx40 = S7.GetBitAt(buffer, 0, 0);
                }
                else
                {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t Read error: " + _client.ErrorText(result));
                }


                if (result2 == 0)
                {
                    Db1Dbw0 = S7.GetWordAt(buffer2, 0);
                    Db1Dbw2 = S7.GetWordAt(buffer2, 2);
                }
                else
                {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\t Read error: " + _client.ErrorText(result2));
                }




            }
        }

        /// <summary>
        /// Writes a bit at the specified address. Es.: DB1.DBX10.2 writes the bit in db 1, word 10, 3rd bit
        /// </summary>
        /// <param name="address">Es.: DB1.DBX10.2 writes the bit in db 1, word 10, 3rd bit</param>
        /// <param name="value">true or false</param>
        /// <returns></returns>
        private int WriteBit(string address, bool value)
        {
            var strings = address.Split('.');
            int db = Convert.ToInt32(strings[0].Replace("DB", ""));
            int pos = Convert.ToInt32(strings[1].Replace("DBX", ""));
            int bit = Convert.ToInt32(strings[2]);
            return WriteBit(db, pos, bit, value);
        }

        private int WriteInt(string address, ushort value)
        {
            var strings = address.Split('.');
            int db = Convert.ToInt32(strings[0].Replace("DB", ""));
            int pos = Convert.ToInt32(strings[1].Replace("DBW", ""));

            return WriteInt(db, pos, value);
        }

        private int WriteInt(int db, int pos, ushort value)
        {
            lock (_locker)
            {
                var buffer = new byte[2];
                S7.SetWordAt(buffer, pos, value);
                return _client.WriteArea(S7Consts.S7AreaDB, db, pos, buffer.Length, S7Consts.S7WLWord, buffer);
            }
        }


        private int WriteBit(int db, int pos, int bit, bool value)
        {
            lock (_locker)
            {
                var buffer = new byte[1];
                S7.SetBitAt(ref buffer, 0, 0, value);
                return _client.WriteArea(S7Consts.S7AreaDB, db, 32 + bit, buffer.Length, S7Consts.S7WLBit, buffer); //Starting at Db1.dbx4.0
            }
        }

        private void OnValuesRefreshed()
        {
            ValuesRefreshed?.Invoke(this, new EventArgs());
        }
    }

    public enum ConnectionStates
    {
        Offline,
        Connecting,
        Online
    }
}
