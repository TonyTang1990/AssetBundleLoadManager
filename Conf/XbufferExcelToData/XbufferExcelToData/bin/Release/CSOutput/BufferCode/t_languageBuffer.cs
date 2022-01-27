namespace xbuffer
{
    public static class t_languageBuffer
    {
        public static t_language deserialize(byte[] buffer, ref uint offset)
        {

            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;

			// Id

			string _Id = stringBuffer.deserialize(buffer, ref offset);
			// string_value

			string _string_value = stringBuffer.deserialize(buffer, ref offset);

			// value
			return new t_language() {
				Id = _Id,
				string_value = _string_value,
            };
        }

        public static void serialize(t_language value, XSteam steam)
        {

            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;

			// Id

			stringBuffer.serialize(value.Id, steam);
			// string_value

			stringBuffer.serialize(value.string_value, steam);
        }
    }
}
