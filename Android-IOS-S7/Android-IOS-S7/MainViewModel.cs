using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Android_IOS_S7
{
    class MainViewModel : BindableBase
    {
        private bool _db1Dbx40;
        private ushort _db1Dbw0;
        private ushort _db1Dbw2;


        private S7PlcService _plcService;



        public MainViewModel()
        {
            _plcService = new S7PlcService();
            _plcService.ValuesRefreshed += OnPlcValuesRefreshed;
            _plcService.Connect("192.168.0.102", 0, 1);
            WriteDb1Dbx40Commandtrue = new DelegateCommand(async () => await ExecuteWriteDb1Dbx40Commandtrue());
            WriteDb1Dbx40Commandfalse = new DelegateCommand(async () => await ExecuteWriteDb1Dbx40Commandfalse());
            WriteDb1Dbw0Command = new DelegateCommand(async () => await ExecuteWriteDb1Dbw0Command());
            WriteDb1Dbw2Command = new DelegateCommand(async () => await ExecuteWriteDb1Dbw2Command());

        }

        public bool Db1Dbx40
        {
            get => _db1Dbx40;
            set => SetProperty(ref _db1Dbx40, value);
        }


        public ushort Db1Dbw0
        {
            get => _db1Dbw0;
            set => SetProperty(ref _db1Dbw0, value);
        }

        public ushort Db1Dbw2
        {
            get => _db1Dbw2;
            set => SetProperty(ref _db1Dbw2, value);
        }


        public ICommand WriteDb1Dbx40Commandtrue { get; }
        public ICommand WriteDb1Dbx40Commandfalse { get; }

        public ICommand WriteDb1Dbw0Command { get; }
        public ICommand WriteDb1Dbw2Command { get; }




        private async Task ExecuteWriteDb1Dbx40Commandtrue()
        {
            await _plcService.WriteDb1Dbx40(true);
        }

        private async Task ExecuteWriteDb1Dbx40Commandfalse()
        {
            await _plcService.WriteDb1Dbx40(false);
        }


        private async Task ExecuteWriteDb1Dbw0Command()
        {
            await _plcService.WriteDb1Dbw0(Db1Dbw0);
        }

        private async Task ExecuteWriteDb1Dbw2Command()
        {
            await _plcService.WriteDb1Dbw2(Db1Dbw2);
        }


        private void OnPlcValuesRefreshed(object sender, EventArgs e)
        {
            Db1Dbx40 = _plcService.Db1Dbx40;
            Db1Dbw0 = _plcService.Db1Dbw0;
            Db1Dbw2 = _plcService.Db1Dbw2;
        }
    }
}
