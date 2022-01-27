namespace xbuffer
{
    public static class t_global_sBuffer
    {
        public static t_global_s deserialize(byte[] buffer, ref uint offset)
        {

            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;

			// Key

			string _Key = stringBuffer.deserialize(buffer, ref offset);
			// Value

			string _Value = stringBuffer.deserialize(buffer, ref offset);

			// value
			return new t_global_s() {
				Key = _Key,
				Value = _Value,
            };
        }

        public static void serialize(t_global_s value, XSteam steam)
        {

            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;

			// Key

			stringBuffer.serialize(value.Key, steam);
			// Value

			stringBuffer.serialize(value.Value, steam);
        }
    }
}
