namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class MultiCallResult
    {
        public bool AuthSuccessful { get; private set; }
        public DedimaniaWarningsAndTTRReply WarningsAndTTRReply { get; private set; }

        public static MultiCallResult Parse(object[] multiCallResult)
        {
            if (multiCallResult == null || multiCallResult.Length < 3)
                return null;

            return new MultiCallResult { AuthSuccessful = CheckAuthResultOfMultiCall(multiCallResult), WarningsAndTTRReply = DedimaniaWarningsAndTTRReply.Parse(multiCallResult[multiCallResult.Length - 1]) };
        }

        private static bool CheckAuthResultOfMultiCall(object[] multiCallResult)
        {
            return (multiCallResult != null && multiCallResult.Length > 1 && multiCallResult[0].GetType() == typeof(bool[]) && ((bool[])multiCallResult[0]).Length == 1 && ((bool[])multiCallResult[0])[0]);
        }
    }
}
