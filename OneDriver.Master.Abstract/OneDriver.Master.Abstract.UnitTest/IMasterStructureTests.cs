using OneDriver.Master.Abstract.Contracts;

namespace OneDriver.Master.Abstract.UnitTest
{
    public class IMasterStructureTests
    {
        [Fact]
        public void IMaster_InterfaceSignature_ShouldNotChange()
        {
            var expected = new List<string>
            {
                "OneDriver.Master.Abstract.Contracts.Definition+Error SelectSensorAtPort(Int32)",
                "Int32 ConnectSensor()",
                "Int32 DisconnectSensor()",
                "OneDriver.Master.Abstract.Contracts.Definition+Error UpdateDataFromSensor()",
                "Void UpdateDataFromAllSensors()",
                "Int32 ReadParameterFromSensor(System.String, System.String ByRef)",
                "Int32 ReadParameterFromSensor[T](System.String, T ByRef)",
                "Int32 WriteParameterToSensor(System.String, System.String)",
                "Int32 WriteParameterToSensor[T](System.String, T)",
                "Int32 WriteCommandToSensor(System.String, System.String)",
                "Int32 WriteCommandToSensor[T](System.String, T)",
                "System.String GetErrorMessage(Int32)",
                "System.String[] GetAllParamsFromSensor()",
                "OneDriver.Master.Abstract.Contracts.Definition+Error LoadDataFromPdb(System.String, Int32, Int32)",
                "OneDriver.Master.Abstract.Contracts.Definition+Error LoadDataFromPdb(System.String, Int32, System.String ByRef)"
            };

            var actual = typeof(IMaster).GetMethods().Select(m => m.ToString()).ToList();
            foreach (var exp in expected)
                Assert.Contains(exp, actual);
        }
    }
}
