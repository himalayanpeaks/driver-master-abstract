using OneDriver.Framework.Base;
using OneDriver.Framework.Libs.Validator;
using OneDriver.Framework.Module;
using OneDriver.Framework.Module.Parameter;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using OneDriver.Master.Abstract.Contracts;
using DeviceDescriptor.Abstract.Variables;
using DeviceDescriptor.Abstract.Helper;

namespace OneDriver.Master.Abstract
{
    public abstract class CommonDevice<TDeviceParams, TSensorParam> :
        BaseDeviceWithChannels<TDeviceParams, DeviceVariables<TSensorParam>>, IMaster
        where TDeviceParams : CommonDeviceParams
        where TSensorParam : BasicVariable, new()
    {
        protected CommonDevice(TDeviceParams parameters, IValidator validator,
           ObservableCollection<BaseChannel<DeviceVariables<TSensorParam>>> elements)
           : base(parameters, validator, elements)
        {
            Parameters.PropertyChanged += Parameters_PropertyChanged;
            Parameters.PropertyChanging += Parameters_PropertyChanging;
        }

        public string[] GetAllParamsFromSensor()
        {
            var channelParams = Elements[Parameters.SelectedChannel].Parameters;

            var allNames = new List<string>();

            if (channelParams.SpecificVariableCollection != null)
                allNames.AddRange(channelParams.SpecificVariableCollection.Select(x => x.Name));

            if (channelParams.StandardVariableCollection != null)
                allNames.AddRange(channelParams.StandardVariableCollection.Select(x => x.Name));

            if (channelParams.SystemVariableCollection != null)
                allNames.AddRange(channelParams.SystemVariableCollection.Select(x => x.Name));

            return allNames.ToArray();
        }

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
        protected abstract int ReadParam(TSensorParam param);
        public Contracts.Definition.Error UpdateDataFromSensor()
        {
            if (Parameters.IsConnected == false)
            {
                Log.Error("Master not connected");
                return Contracts.Definition.Error.UptNotConnected;
            }

            foreach (var param in Elements[Parameters.SelectedChannel].Parameters.StandardVariableCollection)
                ReadParameterFromSensor(param);
            foreach (var param in Elements[Parameters.SelectedChannel].Parameters.SpecificVariableCollection)
                ReadParameterFromSensor(param);
            foreach (var param in Elements[Parameters.SelectedChannel].Parameters.SystemVariableCollection)
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
        private TSensorParam? FindCommand(string name) => 
            Elements[Parameters.SelectedChannel].Parameters.CommandCollection.FirstOrDefault(x => x.Name == name);


        private TSensorParam? FindParam(string name)
        {
            TSensorParam? parameter = default;

            if (Elements[Parameters.SelectedChannel].Parameters.SpecificVariableCollection != null)
                parameter = Elements[Parameters.SelectedChannel].Parameters.SpecificVariableCollection
                    .FirstOrDefault(x => x.Name == name);

            if (parameter == null && Elements[Parameters.SelectedChannel].Parameters.SystemVariableCollection != null)
                parameter = Elements[Parameters.SelectedChannel].Parameters.SystemVariableCollection
                    .FirstOrDefault(x => x.Name == name);

            if (parameter == null && Elements[Parameters.SelectedChannel].Parameters.StandardVariableCollection != null)
                parameter = Elements[Parameters.SelectedChannel].Parameters.StandardVariableCollection
                    .FirstOrDefault(x => x.Name == name);

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

        
        protected abstract int WriteParam(TSensorParam param);
        protected abstract int WriteCommand(TSensorParam command);
        protected abstract string GetErrorAsText(int errorCode);
    }
}
