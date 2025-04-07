using OneDriver.Framework.Module.Parameter;
using OneDriver.Master.Abstract.Contracts;

namespace OneDriver.Master.Abstract
{
    public class CommonDeviceParams : BaseDeviceParam
    {
        private int protocolId;
        private Definition.Mode _mode;

        public int ProtocolId
        {
            get => GetProperty(ref protocolId);
            set => SetProperty(ref protocolId, value);
        }
        public Definition.Mode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }
        private int _selectedChannel;

        public int SelectedChannel
        {
            get => _selectedChannel;
            internal set => SetProperty(ref _selectedChannel, value);
        }
        public CommonDeviceParams(string name) : base(name)
        {
        }
    }
}
