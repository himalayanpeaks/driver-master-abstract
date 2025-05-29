using DeviceDescriptor.Abstract;
using OneDriver.Framework.Module.Parameter;
namespace OneDriver.Master.Abstract.Channels
{
    public class CommonChannel<TBaseVariable>
                    : BaseChannel<BasicDescriptor<TBaseVariable>>
                    where TBaseVariable : DeviceDescriptor.Abstract.Variables.BasicVariable
    {
        public CommonChannel(BasicDescriptor<TBaseVariable> parameters) : base(parameters)
        {
        }
    }
}