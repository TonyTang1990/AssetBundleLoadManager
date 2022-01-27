namespace xbuffer
{
    public static class t_global_bBuffer
    {
        public static t_global_b deserialize(byte[] buffer, ref uint offset)
        {

            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;

			// Key

			string _Key = stringBuffer.deserialize(buffer, ref offset);
			// Value

			bool _Value = boolBuffer.deserialize(buffer, ref offset);

			// value
			return new t_global_b() {
				Key = _Key,
				Value = _Value,
            };
        }

        public static void serialize(t_global_b value, XSteam steam)
        {

            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;

			// Key

			stringBuffer.serialize(value.Key, steam);
			// Value

			boolBuffer.serialize(value.Value, steam);
        }
    }
}
