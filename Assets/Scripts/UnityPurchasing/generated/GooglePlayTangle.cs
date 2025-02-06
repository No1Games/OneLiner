// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("+tqt09dWoNUj5BIcD0u4xcmpiAg/SEMf9wpkOIu+oKD6KOetjl0Y/mKAUbUt+aP6oE2P/K6g/fcRekJDm2yoU8JRWMJSUehy6amYwudiqMXnQlPFADyNNn/SuYvUv2d7KwvtY+NgbmFR42BrY+NgYGHJQbtKSpi68xhJUch8O2bilfFGcXS3ulkmNI81UpeBzSI245eIzqzcWy4G3SRzx4fPgFEalzgFPsfAV7EmeYjbfKLi5+ZZHcx8I59Usu7bH1pwRNud2/HJq8Twwl1OzP26zy3oCSs4EGsAe1HjYENRbGdoS+cp55ZsYGBgZGFi/0BMFjJLhehZmtfC9bBWs1xFRin23aA8BmVaoVrcU/LrYmbyKC4IQfDMnI88Zss+ImNiYGFg");
        private static int[] order = new int[] { 3,8,12,5,9,8,6,10,13,10,12,13,12,13,14 };
        private static int key = 97;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
