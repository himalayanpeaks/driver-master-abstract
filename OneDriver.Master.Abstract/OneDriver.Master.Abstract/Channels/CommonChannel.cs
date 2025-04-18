﻿using OneDriver.Framework.Module.Parameter;
using System.Windows.Input;

namespace OneDriver.Master.Abstract.Channels
{
    public class CommonChannel<TCommonSensorParameter>
                    : BaseChannelWithProcessData<CommonChannelParams<TCommonSensorParameter>,
                        CommonChannelProcessData<TCommonSensorParameter>>
                    where TCommonSensorParameter : CommonSensorParameter
    {
        private CommonChannelParams<TCommonSensorParameter> commonChannelParams;
        private CommonChannelProcessData<TCommonSensorParameter> commonChannelProcessData;

        public CommonChannel(CommonChannelParams<TCommonSensorParameter> commonChannelParams, CommonChannelProcessData<TCommonSensorParameter> commonChannelProcessData)
            : base(commonChannelParams, commonChannelProcessData)
        {
            this.commonChannelParams = commonChannelParams;
            this.commonChannelProcessData = commonChannelProcessData;
        }
    }
}