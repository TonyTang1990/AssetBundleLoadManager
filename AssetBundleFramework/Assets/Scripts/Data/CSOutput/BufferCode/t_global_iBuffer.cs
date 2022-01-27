namespace xbuffer
{
    public static class t_global_iBuffer
    {
        public static t_global_i deserialize(byte[] buffer, ref uint offset)
        {

            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;

			// Key

			string _Key = stringBuffer.deserialize(buffer, ref offset);
			// Value

			int _Value = intBuffer.deserialize(buffer, ref offset);

			// value
			return new t_global_i() {
				Key = _Key,
				Value = _Value,
            };
        }

        public static void serialize(t_global_i value, XSteam steam)
        {

            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;

			// Key

			stringBuffer.serialize(value.Key, steam);
			// Value

			intBuffer.serialize(value.Value, steam);
        }
    }
}
