using OneDriver.Framework.Module.Parameter;

namespace OneDriver.Master.Abstract.Channels
{
    public class CommonChannelParams<TSensorParameter> : BaseChannelParam
            where TSensorParameter : CommonSensorParameter
    {
        private int _deviceId; //Add
        private string _hashId = string.Empty;
        private List<TSensorParameter> _specificParameterCollection;
        private List<TSensorParameter> _standardParameterCollection;
        private List<TSensorParameter> _systemParameterCollection;
        private List<TSensorParameter> _commandCollection;
        public CommonChannelParams(string name) : base(name)
        {
            SpecificParameterCollection = new List<TSensorParameter>();
            StandardParameterCollection = new List<TSensorParameter>();
            SystemParameterCollection = new List<TSensorParameter>();
            CommandCollection = new List<TSensorParameter>();
        }
        public int DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        public string HashId => GetProperty<string>();

        public List<TSensorParameter> StandardParameterCollection
        {
            get => _standardParameterCollection;
            set => SetProperty(ref _standardParameterCollection, value);
        }

        public List<TSensorParameter> SpecificParameterCollection
        {
            get => _specificParameterCollection;
            set => SetProperty(ref _specificParameterCollection, value);
        }

        public List<TSensorParameter> SystemParameterCollection
        {
            get => _systemParameterCollection;
            set => SetProperty(ref _systemParameterCollection, value);
        }
        public List<TSensorParameter> CommandCollection
        {
            get => _commandCollection;
            set => SetProperty(ref _commandCollection, value);
        }
    }
}
