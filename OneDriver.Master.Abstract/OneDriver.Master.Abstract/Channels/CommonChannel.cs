using OneDriver.Framework.Module.Parameter;
using System.Windows.Input;

namespace OneDriver.Master.Abstract.Channels
{
    public class CommonChannel<TCommonSensorParameter>
        : BaseChannelWithProcessData<CommonChannelParams<TCommonSensorParameter>,
            CommonChannelProcessData<TCommonSensorParameter>>
        where TCommonSensorParameter : CommonSensorParameter
    {


        protected CommonChannel(CommonChannelParams<TCommonSensorParameter> parameters,
            CommonChannelProcessData<TCommonSensorParameter> processData) : base(parameters, processData)
        {

        }


    }
}