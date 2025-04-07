using OneDriver.Framework.Module.Parameter;

namespace OneDriver.Master.Abstract.Channels
{
    public class CommonChannelProcessData<TSensorParameter> : BaseProcessData
                where TSensorParameter : CommonSensorParameter

    {
        private List<TSensorParameter> _pdInCollection;

        public List<TSensorParameter> PdInCollection
        {
            get => _pdInCollection;
            set => SetProperty(ref _pdInCollection, value);
        }

        public CommonChannelProcessData()
        {
            PdInCollection = new List<TSensorParameter>();
        }
    }
}
