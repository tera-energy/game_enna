// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("U/8CZiPrjKzE+ZesyGUzXsNWdMd6mde1rOFFoCXPW/7Xcbzn3+jBKUrWh0SL76QER10P7E/xZtiSCwno7O5WSO8QjpIupSZO8n87KtzYL/nB+FQIJK84Z6DWI8wPUnSIImE0e2bQSqyHgz5c3u9m1dhwvJdlzdhUMrG/sIAysbqyMrGxsAgm6HgLek3WCXykFE8TXpB/QY9uMuJEzY+0lXmsl9AnGBPKgGcEfz4dhdlRZmskIwkq6CXPmfhez96hja8x5WU86/yAMrGSgL22uZo2+DZHvbGxsbWws159pJJaL8gc7UpUhmRTFbolDhdfZAIf2kRAuTXBcaSJqfwW9vRojwpnU4/zjB+kjdy52XzC/1DBLK+sRSUqVrf+TL8edbKzsbCx");
        private static int[] order = new int[] { 10,6,9,9,12,11,6,11,8,13,10,13,12,13,14 };
        private static int key = 176;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
