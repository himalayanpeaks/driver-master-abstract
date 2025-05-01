using OneDriver.Framework.Base;
using OneDriver.Framework.Libs.DeviceDescriptor;
using OneDriver.Framework.Libs.Validator;
using OneDriver.Framework.Module;
using OneDriver.Framework.Module.Parameter;
using OneDriver.Master.Abstract.Channels;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ParameterTool.NSwagClass.Generator.Interface;
using System.Text.RegularExpressions;
using OneDriver.Master.Abstract.Contracts;
using DataType = OneDriver.Helper.Definitions.DataType;
using static OneDriver.Master.Abstract.Contracts.Definition;
using OneDriver.Helper;

namespace OneDriver.Master.Abstract
{
    public abstract class CommonDevice<TDeviceParams, TSensorParam> :
        BaseDeviceWithChannelsPd<TDeviceParams, CommonChannelParams<TSensorParam>, 
            CommonChannelProcessData<TSensorParam>>, IMaster
        where TDeviceParams : CommonDeviceParams
        where TSensorParam : CommonSensorParameter, new()
    {
        protected CommonDevice(TDeviceParams parameters, IValidator validator, 
            ObservableCollection<BaseChannelWithProcessData<CommonChannelParams<TSensorParam>,
            CommonChannelProcessData<TSensorParam>>> elements, IDeviceDescriptor parameterDatabank) 
            : base(parameters, validator, elements)
        {
            ParameterDatabank = parameterDatabank;
            Parameters.PropertyChanged += Parameters_PropertyChanged;
            Parameters.PropertyChanging += Parameters_PropertyChanging;
        }

        public string?[] GetAllParamsFromSensor()
        {
            return (Elements[Parameters.SelectedChannel].Parameters.SpecificParameterCollection.Select(x => x.Name)).ToArray();

        }

        public Contracts.Definition.Error LoadDataFromPdb(string server, int deviceId, int protocolId)
        {
            List<ParameterDetailsResponse> ret;
            try
            {
                ret = ParameterDatabank.ReadData(server, deviceId, protocolId);
                Elements[Parameters.SelectedChannel].Parameters.DeviceId = deviceId;
                ConvertAbstractData(ret, Elements[Parameters.SelectedChannel].Parameters);
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is AggregateException
            )
            {
                Log.Error("Check for serverId, deviceId, protocolId");
            }
            return Contracts.Definition.Error.NoError;
        }

        public Contracts.Definition.Error LoadDataFromPdb(string server, int protocolId, out string? hashId)
        {
            hashId = Elements[Parameters.SelectedChannel].Parameters.HashId;
            if(string.IsNullOrEmpty(hashId))
                return Contracts.Definition.Error.HasdIdNotFound;

            try
            {
                var ret = ParameterDatabank.ReadData(server, hashId, protocolId);
                ConvertAbstractData(ret, Elements[Parameters.SelectedChannel].Parameters);
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is AggregateException
            )
            {
                Log.Error("Check for serverId, hashId, protocolId");
            }
            return Contracts.Definition.Error.NoError;
        }
        private void ConvertAbstractData(List<ParameterDetailsResponse> dataFromDb, 
            CommonChannelParams<TSensorParam> sensor)
        {
            foreach (var paramDetail in dataFromDb)
            {
                string? min = null; string? max = null;
                var valid = new List<string?>();
                try
                {
                    if (!object.Equals(paramDetail.AllowedValues, null))
                        foreach (var paramDetailAllowedValue in paramDetail.AllowedValues)
                        {
                            if (!string.IsNullOrEmpty(paramDetailAllowedValue.Min.Value) &&
                                !string.IsNullOrEmpty(paramDetailAllowedValue.Max.Value))
                            {
                                min = paramDetailAllowedValue.Min.Value.ToString();
                                max = paramDetailAllowedValue.Max.Value.ToString();
                            }
                            if (!string.IsNullOrEmpty(paramDetailAllowedValue.Min.Value) &&
                                string.IsNullOrEmpty(paramDetailAllowedValue.Max.Value))
                            {
                                valid.Add(paramDetailAllowedValue.Min.Value.ToString());
                            }
                        }
                    var protocol = paramDetail.Protocols.First();
                    DataType type = (DataType)Enum.Parse(typeof(DataType), Regex.Replace(paramDetail.DataType, @"\d+", ""));
                    CommonSensorParameter local = new CommonSensorParameter(paramDetail.ParameterName, (int)protocol.Index,
                        (Definition.AccessType)Enum.Parse(typeof(Definition.AccessType), protocol.Accesses.First(x => x.Role == "RnD").Access),
                        type, protocol.ArrayCount, protocol.BitLength, protocol.OffSet, null,
                        paramDetail.DefaultValue.Value, min, max, String.Join(";", valid.ToArray()));
                    local.PropertyChanging += Parameters_PropertyChanging;
                    local.PropertyChanged += Parameters_PropertyChanged;

                    AddData(paramDetail, local);

                }
                catch (Exception e)
                {
                    Log.Error(paramDetail.ParameterName + " invalid");
                }
            }
        }

        protected abstract void AddData(ParameterDetailsResponse paramDetail, CommonSensorParameter commonParameter);
        protected IDeviceDescriptor ParameterDatabank { get; set; }
        /// <summary>
        /// Write here the validation of a param before its new value of a param is accepted 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Parameters_PropertyChanging(object sender, PropertyValidationEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Parameters.ProtocolId):
                    break;
            }
        }

        private void Parameters_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Parameters.ProtocolId):
                    break;
            }
        }

        public Contracts.Definition.Error SelectSensorAtPort(int portNumber)
        {
            if (portNumber < Elements.Count)
                Parameters.SelectedChannel = portNumber;
            else
            {
                Log.Error(portNumber + " doesn't exist");
                return Contracts.Definition.Error.ChannelError;
            }
            return Contracts.Definition.Error.NoError;
        }

        public abstract int ConnectSensor();
        public abstract int DisconnectSensor();

        public Contracts.Definition.Error UpdateDataFromSensor()
        {
            if (ParameterDatabank == null)
            {
                Log.Error("Database not connected");
                return Contracts.Definition.Error.DatabaseNotConnected;
            }

            if (Parameters.IsConnected == false)
            {
                Log.Error("UPT not connected");
                return Contracts.Definition.Error.UptNotConnected;
            }

            foreach (var param in Elements[Parameters.SelectedChannel].Parameters.StandardParameterCollection)
                ReadParameterFromSensor(param);
            foreach (var param in Elements[Parameters.SelectedChannel].Parameters.SpecificParameterCollection)
                ReadParameterFromSensor(param);
            foreach (var param in Elements[Parameters.SelectedChannel].Parameters.SystemParameterCollection)
                ReadParameterFromSensor(param);

            return Contracts.Definition.Error.NoError;
        }

        public void UpdateDataFromAllSensors()
        {
            if (this.Elements.Count > 1)
                if (Parameters.IsConnected)
                    DisconnectSensor();
            for(int i = 0; i < Elements.Count; i++)
            {
                if (Parameters.IsConnected == false)
                {
                    DisconnectSensor();
                    SelectSensorAtPort(i++);
                    ConnectSensor();
                }
                UpdateDataFromSensor();
            }
        }

        public int ReadParameterFromSensor(string name, out string? value)
        {
            TSensorParam? foundParam = FindParam(name);
            value = null;
            if (foundParam == null)
                return (int)Contracts.Definition.Error.ParameterNotFound;
            int err = ReadParameterFromSensor(foundParam);
            if (err == 0)
                value = foundParam.Value;
            return err;
        }

        public int ReadParameterFromSensor<T>(string name, out T? value)
        {
            value = default(T);
            int err = ReadParameterFromSensor(name, out var readValue);

            if (err == 0 && DataConverter.ConvertTo(readValue, out value))
                return 0;
            return err != 0 ? err : (int)DataConverter.DataError.UnsupportedDataType;
        }
        private TSensorParam? FindCommand(string name) => Elements[Parameters.SelectedChannel].Parameters.CommandCollection
                .FirstOrDefault(x => x.Name == name);


        private TSensorParam FindParam(string name)
        {
            TSensorParam? parameter = new TSensorParam();

            if (!Object.Equals((Elements[Parameters.SelectedChannel].Parameters.SpecificParameterCollection.Find(x => x.Name == name)), null))
                parameter = Elements[Parameters.SelectedChannel].Parameters.SpecificParameterCollection.Find(x => x.Name == name);
            if (!Object.Equals((Elements[Parameters.SelectedChannel].Parameters.SystemParameterCollection.Find(x => x.Name == name)), null))
                parameter = Elements[Parameters.SelectedChannel].Parameters.SystemParameterCollection.Find(x => x.Name == name);
            if (!Object.Equals((Elements[Parameters.SelectedChannel].Parameters.StandardParameterCollection.Find(x => x.Name == name)), null))
                parameter = Elements[Parameters.SelectedChannel].Parameters.StandardParameterCollection.Find(x => x.Name == name);

            return parameter;
        }

        public int WriteParameterToSensor(string name, string value)
        {
            if (DataConverter.ConvertTo<TSensorParam>(value, out var toWriteValue) == true)
                return WriteParameterToSensor(name, toWriteValue);
            return (int)DataConverter.DataError.InValidData;
        }

        public int WriteParameterToSensor<T>(string name, T value)
        {
            if (DataConverter.ConvertTo(value, out var toWriteValue))
                return WriteParameterToSensor(name, toWriteValue);
            return (int)DataConverter.DataError.InValidData;
        }

        public int WriteCommandToSensor(string name, string value)
        {
            TSensorParam? foundCommand = FindCommand(name);
            if (foundCommand == null)
            {
                Log.Error("Command not found: " + name);
                return (int)Contracts.Definition.Error.CommandNotFound;
            }

            if (!DataConverter.ConvertTo(value, out var toWriteValue))
            {
                Log.Error("Invalid data for command: " + name);
                return (int)DataConverter.DataError.InValidData;
            }

            foundCommand.Value = toWriteValue;
            return WriteCommandToSensor(foundCommand);
        }
        internal int WriteCommandToSensor(TSensorParam command)
        {
            int err = 0;
            try
            {
                if((err = WriteCommand(command)) !=0)
                    Log.Error("Error in write command: " + GetErrorMessage(err));
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return err;
        }
        public int WriteCommandToSensor<T>(string name, T value)
        {
            if (DataConverter.ConvertTo<T>(value, out var toWriteValue) == true)
                return WriteCommandToSensor(name, toWriteValue);
            else
                return (int)DataConverter.DataError.InValidData;
        }
        public string GetErrorMessage(int errorCode)
        {
            if (Enum.IsDefined(typeof(Contracts.Definition.Error), errorCode))
                return ((Contracts.Definition.Error)errorCode).ToString();
            if (Enum.IsDefined(typeof(DataConverter.DataError), errorCode))
                return ((DataConverter.DataError)errorCode).ToString();
            return GetErrorAsText(errorCode);
        }
        
        private int ReadParameterFromSensor(TSensorParam parameter)
        {
            int err = 0;
            try
            {
                if((err = ReadParam(parameter)) != 0)
                    Log.Error("Error in read: " + GetErrorMessage(err));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            return err;
        }

        protected abstract int ReadParam(TSensorParam param);
        protected abstract int WriteParam(TSensorParam param);
        protected abstract int WriteCommand(TSensorParam command);
        protected abstract string GetErrorAsText(int errorCode);
    }
}
